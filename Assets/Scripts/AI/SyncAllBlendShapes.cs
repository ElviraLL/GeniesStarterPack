using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncAllBlendShapes : MonoBehaviour
{
    
    [SerializeField]
    GameObject body_geo, innerMouth_geo, eyebrow_geo;

    SkinnedMeshRenderer body_SMR, innerMouth_SMR, eyebrow_SMR;
    Mesh body_Mesh, innerMouth_Mesh, eyebrow_Mesh;
    Animator avtAnimator;
    // Start is called before the first frame update
    void Start()
    {
        avtAnimator = GetComponent<Animator>();

        body_SMR = body_geo.GetComponent<SkinnedMeshRenderer>();
        innerMouth_SMR = innerMouth_geo.GetComponent<SkinnedMeshRenderer>();
        eyebrow_SMR = eyebrow_geo.GetComponent<SkinnedMeshRenderer>();
        
        body_Mesh = body_SMR.sharedMesh;
        innerMouth_Mesh = innerMouth_SMR.sharedMesh;
        eyebrow_Mesh = eyebrow_SMR.sharedMesh;
        // for (int i = 0; i < eyebrow_Mesh.blendShapeCount; i++){
        //     Debug.Log($"blend shape innermouth {i}, {eyebrow_Mesh.GetBlendShapeName(i)}");
        // }

    }


    // Update is called once per frame
    void Update()
    {
        if (avtAnimator.GetBool("isTalking"))
        {
            float v = body_SMR.GetBlendShapeWeight(24); //jaw open
            innerMouth_SMR.SetBlendShapeWeight(2, v); //jaw open

            for (int i = 18; i < 20; i++ )
            {
                eyebrow_SMR.SetBlendShapeWeight(i, (float)(0.2 * v)); //eyebrow up
            }
            eyebrow_SMR.SetBlendShapeWeight(0, (float)(0.2 * v)); //eye blink
            eyebrow_SMR.SetBlendShapeWeight(2, (float)(0.2 * v));

        }
    }
}
