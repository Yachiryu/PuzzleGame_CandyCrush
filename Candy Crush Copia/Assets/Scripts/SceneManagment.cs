using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagment : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Juego Principal");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Saliendo...");
    }

    public void SelectLevel()
    {
        //SceneManager.LoadScene();
    }

}
