using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitGameButton : MonoBehaviour
{
    //Kilépés a játékból

    public void QuitGame()
    {
        Application.Quit();
    }
}
