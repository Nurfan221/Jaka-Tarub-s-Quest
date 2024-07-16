using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenUI : MonoBehaviour
{
    public static LoadingScreenUI Instance;
    [SerializeField] TMP_Text loadingText;
    [SerializeField] TMP_Text tipsText;

    [SerializeField] string[] tips;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void LoadScene(int i)
    {
        Debug.Log("LOAD SCENE");
        tipsText.text = "Tips: \n" + tips[Random.Range(0, tips.Length)];
        transform.GetChild(0).gameObject.SetActive(true);
        StartCoroutine(LoadingScene(i));
    }

    IEnumerator LoadingScene(int i)
    {
        Debug.Log("LOADING SCENE");
        AsyncOperation loadLevel = SceneManager.LoadSceneAsync(i);
        string loading = "Loading...";
        int index = 7;

        while (!loadLevel.isDone)
        {
            if (index > 9)
                index = 7;
            loadingText.text = loading[..index];
            index++;
            yield return new WaitForSeconds(.1f);
        }
        transform.GetChild(0).gameObject.SetActive(false);
        Debug.Log("DONE");
    }
}
