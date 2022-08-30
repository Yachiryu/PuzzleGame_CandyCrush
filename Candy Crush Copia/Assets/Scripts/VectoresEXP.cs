using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectoresEXP : MonoBehaviour
{
    Vector3 startPos;
    Vector3 endPos;
   [Range(0f,1f)] float time;
    void Start()
    {

    }

    void Update()
    {
        transform.position = Vector3.Lerp(startPos, endPos, time); ;
    }
}
