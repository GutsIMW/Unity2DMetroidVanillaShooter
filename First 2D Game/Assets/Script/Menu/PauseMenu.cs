using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private static bool gameIsPaused = false; // Indicate if the game is paused or not
    private static bool isInOptions = false; // Indicate if we are in the options menu (in game, not in the main menu)
    private static bool isInGameOver = false; // Indicate if we are in the game over
    public GameObject PauseMenuUI;
    public GameObject OptionsMenuUI;
    public LevelLoader levelLoader;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(!gameIsPaused)
            {
                Pause();
            }
            else if(isInOptions)
            {
                OptionsMenuUI.SetActive(false);
                PauseMenuUI.SetActive(true);
                SetIsInOptions(false);
            }
            else if(!isInGameOver)
            {
                Resume();
            }
            //else do nothing (we should be in gameOver so just do nothing)
        }
    }

    public static bool GetGameIsPaused() { return gameIsPaused; }
    public static bool GetIsInOptions() { return isInOptions; }
    public static bool GetIsInGameOver() { return isInGameOver;}
    public static void SetGameIsPaused(bool val) { gameIsPaused = val; }
    public static void SetIsInOptions(bool val) { isInOptions = val; }
    public static void SetIsInGameOver(bool val)
    {
        isInGameOver = val;
        gameIsPaused = val;
    }

    public void Resume()
    {
        PauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    void Pause()
    {
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    public void LoadMenu()
    {
        // We need to set Time.timeScale to 1f and gameIsPaused to false cause we're no more in game, we're now going to the menu. This avoids problems when going to the menu and then returning in game
        Time.timeScale = 1f;
        gameIsPaused = false;
        levelLoader.LoadMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
