using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
//using UnityEngine.XR.Interaction.Toolkit;


public class STT_HF_OpenAI : MonoBehaviour
{
    [SerializeField]
    private string HF_INF_API_KEY;
    const string STT_API_URI = "https://api-inference.huggingface.co/models/openai/whisper-tiny"; //POST URL

    [SerializeField]
    private LLM_Groq llmGroq;

    MemoryStream stream; //global varable that used for coroutine data pass(because coroutine cannot return)


    void Update()
    {
        // Start recording when the 'R' key is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartSpeaking();
        }

        // Stop recording when the 'R' key is released
        if (Input.GetKeyUp(KeyCode.R))
        {
            Microphone.End(null);
        }
    }

    public void StartSpeaking()
    {
        stream = new MemoryStream();

        AudioSource aud = GetComponent<AudioSource>();
        Debug.Log("Start Recording");
        aud.clip = Microphone.Start(null, false, 30, 11025);

        StartCoroutine(RecordAudio(aud.clip));
    }

    IEnumerator RecordAudio(AudioClip clip)
    {
        while (Microphone.IsRecording(null))
        {
            yield return null;
        }
        Debug.Log("Done Recording!");
        AudioSource aud = GetComponent<AudioSource>();
        ConvertClipToWav(aud.clip);

        StartCoroutine(STT());

    } 
    //STT reads from the global varable "stream" it doeesn't have input
    IEnumerator STT()
    {
        SpeechToTextData sttData = new SpeechToTextData();
        UnityWebRequest request = new UnityWebRequest(STT_API_URI, "POST");
        request.uploadHandler = new UploadHandlerRaw(stream.GetBuffer());
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + HF_INF_API_KEY);

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            SpeechToTextData sttResponse = JsonUtility.FromJson<SpeechToTextData>(responseText);
            Debug.Log(sttResponse.text);

            if (llmGroq) llmGroq.TextToLLM(sttResponse.text);
        }
        else Debug.Log("STT API Request Faild: " + request.error);


    }


    //Json Handler
    [System.Serializable]
    public class SpeechToTextData
    {
        public string text;
    }



    Stream ConvertClipToWav(AudioClip clip)
    {
        var data = new float[clip.samples * clip.channels];
        clip.GetData(data, 0);

        if (stream!=null) stream.Dispose();         //Cleanup
        stream = new MemoryStream();                //Start with a clean stream

        var bitsPerSample = (ushort)16;
        var chunkID = "RIFF";
        var format = "WAVE";
        var subChunk1ID = "fmt ";
        var subChunk1Size = (uint)16;
        var audioFormat = (ushort)1;
        var numChannels = (ushort)clip.channels;
        var sampleRate = (uint)clip.frequency;
        var byteRate = (uint)(sampleRate * clip.channels * bitsPerSample / 8);  // SampleRate * NumChannels * BitsPerSample/8
        var blockAlign = (ushort)(numChannels * bitsPerSample / 8); // NumChannels * BitsPerSample/8
        var subChunk2ID = "data";
        var subChunk2Size = (uint)(data.Length * clip.channels * bitsPerSample / 8); // NumSamples * NumChannels * BitsPerSample/8
        var chunkSize = (uint)(36 + subChunk2Size); // 36 + SubChunk2Size

        WriteString(stream, chunkID);
        WriteUInt(stream, chunkSize);
        WriteString(stream, format);
        WriteString(stream, subChunk1ID);
        WriteUInt(stream, subChunk1Size);
        WriteShort(stream, audioFormat);
        WriteShort(stream, numChannels);
        WriteUInt(stream, sampleRate);
        WriteUInt(stream, byteRate);
        WriteShort(stream, blockAlign);
        WriteShort(stream, bitsPerSample);
        WriteString(stream, subChunk2ID);
        WriteUInt(stream, subChunk2Size);

        foreach (var sample in data)
        {
            // De-normalize the samples to 16 bits.
            var deNormalizedSample = (short)0;
            if (sample > 0)
            {
                var temp = sample * short.MaxValue;
                if (temp > short.MaxValue)
                    temp = short.MaxValue;
                deNormalizedSample = (short)temp;
            }
            if (sample < 0)
            {
                var temp = sample * (-short.MinValue);
                if (temp < short.MinValue)
                    temp = short.MinValue;
                deNormalizedSample = (short)temp;
            }
            WriteShort(stream, (ushort)deNormalizedSample);
        }

        return stream;
    }

        //Helper functions to send data into the stream
    private void WriteUInt(Stream stream, uint data)
    {
        stream.WriteByte((byte)(data & 0xFF));
        stream.WriteByte((byte)((data >> 8) & 0xFF));
        stream.WriteByte((byte)((data >> 16) & 0xFF));
        stream.WriteByte((byte)((data >> 24) & 0xFF));
    }

    private void WriteShort(Stream stream, ushort data)
    {
        stream.WriteByte((byte)(data & 0xFF));
        stream.WriteByte((byte)((data >> 8) & 0xFF));
    }

    private void WriteString(Stream stream, string value)
    {
        foreach (var character in value)
            stream.WriteByte((byte)character);
    }

}







