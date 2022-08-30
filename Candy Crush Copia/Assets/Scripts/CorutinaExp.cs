using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorutinaExp : MonoBehaviour
{
    public int conteoR;
    public int conteoR2;
    private void Start()
    {
        StartCoroutine(enumerator());
    }
    IEnumerator enumerator()
    {
        Debug.Log("Hello World");
        yield return new WaitForSeconds(conteoR);
        Debug.Log("Adios");
        yield return new WaitForSeconds(conteoR2);
        Debug.Log("Hello World");

    }

   /* void Update()
    {
        if (Input.GetKeyDown ("e"))
        {
            StartCoroutine(enumerator());
        }
    }*/
}
