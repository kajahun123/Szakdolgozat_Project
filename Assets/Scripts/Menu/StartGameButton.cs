using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameButton : MonoBehaviour
{
    //GameScene ind�t�sa
    public int gameStartScene;

    public void StartGame() {
        SceneManager.LoadScene(gameStartScene);
    }
}
