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
        startPos = transform.position;
        endPos = new Vector3(startPos.x, startPos.y + 1, 0);
    }

    void Update()
    {
        transform.position = Vector3.Lerp(startPos, endPos, time); ;
    }
}
