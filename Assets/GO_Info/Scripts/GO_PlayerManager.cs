using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_PlayerManager : MonoBehaviour
{
    [SerializeField]
    private string nickName;
    [SerializeField]
    private int lives = 3;
    [SerializeField]
    public int currentLevel = 1;

    public void LoseLife()
    {
        lives--;
        if (lives <= 0)
        {
            ResetGame();
        }
    }

    private void ResetGame()
    {
        currentLevel = 0;
        lives = 3;
    }
}
