using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] Button LoadGameButton;

    private void Update()
    {
        if (PlayerPrefs.GetInt("HaveSaved") == 99)
        {
            LoadGameButton.interactable = true;
        }
        else
        {
            LoadGameButton.interactable = false;
        }
    }

    public void PlayClickSound()
    {
        SoundManager.Instance.PlaySound("Click");
    }

    public void StartGame(bool newGame)
    {
        Debug.Log("START GAME");
        //GameController.isNewGame = newGame;
        //LoadingScreenUI.Instance.LoadScene(newGame ? 1 : GameController.LatestMap);
        //StopAllCoroutines();
        //StartCoroutine(DelayedLoadScene(GameController.LatestMap, 0.5f)); // delay 0.5 detik
    }

    private IEnumerator DelayedLoadScene(int sceneIndex, float delay)
    {
        Debug.Log("DELAYED LOAD SCENE " + delay);
        yield return new WaitForSeconds(delay);
        Debug.Log("AFTER DELAY");
        Debug.Log(LoadingScreenUI.Instance);
        //LoadingScreenUI.Instance.LoadScene(sceneIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("APPLICATION QUIT");
    }
}
