using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int cordenadaX;
    public int cordenadaY;

    public float tiempoMovimiento2;
    public bool yaSeEjecuto = true;
    public AnimationCurve curve;

    public Board board;

    public TipoMovimiento tipoDeMovimiento;
    public TipoFicha tipoFicha;


    private void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoverPieza(new Vector3((int)transform.position.x, (int)transform.position.y + 1, 0), tiempoMovimiento2);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoverPieza(new Vector3((int)transform.position.x, (int)transform.position.y - 1, 0), tiempoMovimiento2);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoverPieza(new Vector3((int)transform.position.x - 1, (int)transform.position.y, 0), tiempoMovimiento2);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoverPieza(new Vector3((int)transform.position.x + 1, (int)transform.position.y, 0), tiempoMovimiento2);
        }
        */
    }

    public void Coordenadas(int x , int y)
    {
        cordenadaX = x;
        cordenadaY = y;
    }


    public void MoverPieza(int x,int y, float tiempoMovimiento)
    {
        if (yaSeEjecuto == true)
        {
            StartCoroutine(MovePiece(new Vector3(x, y), tiempoMovimiento));
        }
    }

    IEnumerator MovePiece(Vector3 posicionFinal, float tiempoMovimiento)
    {
        yaSeEjecuto = false;
        bool llegoAlPunto = false;
        Vector3 posicionInicial = new Vector3((int)transform.position.x, (int)transform.position.y, 0);
        float tiempoTranscurrido = 0;
       
        while (!llegoAlPunto)
        {
            if (Vector3.Distance(transform.position, posicionFinal)< 0.01f)
            {
                llegoAlPunto = true;
                yaSeEjecuto = true;
                board.PiezaPosicion(this, (int)posicionFinal.x, (int)posicionFinal.y);
                transform.position = new Vector3((int) posicionFinal.x,(int) posicionFinal.y);
                break;
            }
            
            float t = tiempoTranscurrido / tiempoMovimiento;

            switch (tipoDeMovimiento)
            {
                case TipoMovimiento.Lineal:
                    t = curve.Evaluate(t);

                    break;
                case TipoMovimiento.Entrada:
                    t = 1 - Mathf.Cos(t * Mathf.PI * .5f);

                    break;
                case TipoMovimiento.Salida:

                    break;
                case TipoMovimiento.Suavisado:
                    t = t * t * (3 - 2 * t);
                    break;
                case TipoMovimiento.MasSuavisado:
                    t = t * t * t * (t * (t * 6 - 15) + 10);
                    break;
            }

            transform.position = Vector3.Lerp(posicionInicial, posicionFinal, t);
            tiempoTranscurrido += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

    }
    public enum TipoFicha
    {
        Naranja,
        Manzana,
        Mango,
        Melon,
        Granada,
        Limon2,
        limon,
        Sandia,
    }
    public enum TipoMovimiento
    {
        Lineal,
        Entrada,
        Salida,
        Suavisado,
        MasSuavisado,
    }
}

