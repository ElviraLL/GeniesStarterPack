using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class TTS_SF_Simba : MonoBehaviour
{
    //Variables
    [SerializeField] 
    private string SPEECHFY_API_KEY;

    private enum SelectVoice
    {
        US_Henry, US_Carly, US_Kyle, US_Kristy, US_Oliver, US_Tasha, US_Joe, US_Lisa,
        US_George, US_Emily, US_Rob, GB_Russell, GB_Benjamin, GB_Michael, AU_KIM, IN_Ankit, IN_Arun,
        GB_Carol, GB_Helen, US_Julie, AU_Linda, US_Mark, US_Nick, NG_Elijah, GB_Beverly, GB_Collin,
        US_Erin, US_Jack, US_Jesse, US_Keenan, US_Lindsey, US_Monica, GB_Phil, GB_Declan, US_Stacy,
        GB_Archie, US_Evelyn, GB_Freddy, GB_Harper, US_Jacob, US_James, US_Mason, US_Victoria
    }

    [SerializeField]
    private SelectVoice selectVoice;

    const string TTS_API_URI = "https://api.sws.speechify.com/v1/audio/stream"; //POST URI, streaming API
    private string sfVoice;

    Animator avtAnimator;



    // Start is called before the first frame update
    void Start()
    {
        avtAnimator = GetComponent<Animator>();

        sfVoice = selectVoice.ToString().Substring(3);
        Debug.Log("Selected Voice: " + sfVoice);

        //Debug
        // Say("Testing testing, Hello Ray, this is Jingwen speaking, please answer");

    }
    
    public void Say(string textInput)
    {
        StartCoroutine(PlayTTS(textInput));
    }

    IEnumerator PlayTTS(string mesg)
    {
        //JSON
        TextToSpeechData ttsData = new TextToSpeechData();
        ttsData.input = SimpleCleanText(mesg);
        ttsData.voice_id = sfVoice;
        string jsonPrompt = JsonUtility.ToJson(ttsData);

        //Web Request
        UnityWebRequest request = new UnityWebRequest(TTS_API_URI, "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPrompt));
        request.downloadHandler = new DownloadHandlerAudioClip(TTS_API_URI, AudioType.MPEG);

        //Headers
        request.SetRequestHeader("content-type", "application/json");
        request.SetRequestHeader("accept", "audio/mpeg");
        request.SetRequestHeader("Authorization", SPEECHFY_API_KEY);

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            avtAnimator.SetBool("isTalking", true);
            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            GetComponent<AudioSource>().PlayOneShot(clip);
            StartCoroutine(WaitForTalkingFinished());
        }
        else Debug.Log("TTS API Request Failed: " + request.error);
        
        
    }

    IEnumerator WaitForTalkingFinished()
    {
        while (GetComponent<AudioSource>().isPlaying)
        {
            yield return null;
        }
        avtAnimator.SetBool("isTalking", false);
    }

    //JSON support class
    [Serializable]
    public class TextToSpeechData
    {
        public string input;
        public string voice_id;
    }

    string SimpleCleanText(string msg)
    {
        string result = "";
        for (int i = 0; i < msg.Length; i++)
        {

            switch (msg[i])
            {
                case '+':
                    result += " plus ";
                    break;
                case ':':
                    result += ", ";
                    break;
                case '*':
                    result += ", ";
                    break;
                case '=':
                    result += " equals ";
                    break;
                case '-':
                    result += " ";
                    break;
                case '#':
                    result += " hash ";
                    break;
                case '&':
                    result += " and ";
                    break;
                case 'I':
                    if ((i < msg.Length + 2) && (msg[i + 1] == '\'') && msg[i + 2] == 'm')
                    {
                        result += "I am";
                        i += 2;
                    }
                    else result += 'I';
                    break;
                case 'e':
                    if ((i < msg.Length + 2) && (msg[i + 1] == '\'') && msg[i + 2] == 's')
                    {
                        result += "ee is";
                        i += 2;
                    }
                    else result += 'e';
                    break;
                default:
                    result += msg[i];       //simply pass on everything else
                    break;
            }
        }
        return result;
    }
}
