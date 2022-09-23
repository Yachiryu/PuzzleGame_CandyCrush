using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int cordenadaX; // xIndex
    public int cordenadaY; // yIndex
    
    public Board boardFac; // m_board
    
    public bool yaSeEjecuto = false; // m_isMoving
    
    public TipoMovimiento tipoDeMovimiento; // Interpolation
    public TipoFicha tipoFicha; // MatchValue

    // public float tiempoMovimiento2;
   // public AnimationCurve curve;

    public void Coordenadas(int x , int y) // setCoord
    {
        cordenadaX = x;
        cordenadaY = y;
    }

    internal void Init(Board board)
    {
        boardFac = board;
    }


    internal void MoverPieza(int x,int y, float tiempoMovimiento) // Move
    {
        if (!yaSeEjecuto)
        {
            StartCoroutine(MovePiece(x, y, tiempoMovimiento));
        }
    }

    IEnumerator MovePiece(int destX, int destY, float timeToMove) // MoveRoutine
    {
        Vector2 startPosition = transform.position;
        bool alcanzoElDestino = false; // reacedDestination
        float tiempoTomado = 0f; // elapsedTime
        yaSeEjecuto = true;

        while (!alcanzoElDestino)
        {
            if (Vector2.Distance(transform.position, new Vector2(destX, destY)) < 0.01f)
            {
                alcanzoElDestino = true;

                if (boardFac != null)
                {
                    boardFac.PlaceGamePiece(this, destX, destY);
                }
                break;
            }

            tiempoTomado += Time.deltaTime;
            float t = Mathf.Clamp(tiempoTomado / timeToMove, 0f, 1f);

            switch (tipoDeMovimiento) // Interpolation
            {
                case TipoMovimiento.Linear:

                    break;

                case TipoMovimiento.EaseOut:
                    t = Mathf.Sin(t * Mathf.PI * .5F);
                    break;

                case TipoMovimiento.EseIn:
                    t = 1 - Mathf.Cos(t * Mathf.PI * .5f);
                    break;

                case TipoMovimiento.SmoothStep:
                    t = t * t * (3 - 2 * t);
                    break;

                case TipoMovimiento.SmootherStep:
                    t = t * t * t * (t * (t * 6 - 15) + 10);
                    break;   
            }

            transform.position = Vector2.Lerp(startPosition, new Vector2(destX, destY), t);
            yield return null;
        }

        yaSeEjecuto = false;

    }
    public enum TipoMovimiento // InterType
    {
        Linear,
        EaseOut,
        EseIn,
        SmoothStep,
        SmootherStep
    }
    public enum TipoFicha // MatchValue
    {
        Naranja,
        Manzana,
        Pera,
        Durazno,
        Granada,
        Limon,
        Sandia,
        Aguacate,
        Morazul,
        Pina,
    }
}

