using UnityEngine;

public class SceneBGMManager : MonoBehaviour
{
    [SerializeField] private string bgmName;

    private void Start()
    {
        if (SoundManager.Instance != null)
        {
            //SoundManager.Instance.PlayBGM(bgmName);
        }
    }
}
