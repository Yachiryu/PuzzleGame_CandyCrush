using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public void MainMenu() 
    {
        SceneManager.LoadScene("Main Menu");
    }
    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Saliendo...");
    } 
    public void YouWin()
    {
        SceneManager.LoadScene("You_Win"); 
    }

    public void SelectLevel()
    {
        SceneManager.LoadScene("Level Selection");
    }
    public void Level_1()
    {
        SceneManager.LoadScene("Nivel_1");
    }
    public void Level_2()
    {
        SceneManager.LoadScene("Nivel_2");
    }
    public void Level_3()
    {
        SceneManager.LoadScene("Nivel_3");
    }

    public void Level_4()
    {
        SceneManager.LoadScene("Nivel_4");
    }

    public void Level_5()
    {
        SceneManager.LoadScene("Nivel_5");
    }

}
