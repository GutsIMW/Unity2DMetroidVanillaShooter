using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;

    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void LoadMenu()
    {
        StartCoroutine(LoadLevel(0));
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("Start"); // Active le trigger de l'animator qui déclenche la prochaine animation
        yield return new WaitForSeconds(transition.GetCurrentAnimatorClipInfo(0)[0].clip.length); // Atttend jusqu'à la fin de l'animation
        SceneManager.LoadScene(levelIndex); // Charge la scene suivante
    }
}
