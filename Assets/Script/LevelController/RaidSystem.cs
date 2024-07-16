using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RaidSystem : MonoBehaviour
{
    public static RaidSystem Instance;

    [SerializeField] Transform spawners;

    [SerializeField] bool raidStart = false;
    [SerializeField] bool raiding = false;
    [SerializeField] bool winStatus = false;

    [SerializeField] float raidTimeLimit = 5 * 60;
    float raidTimer;

    [SerializeField] float gracePeriod = 5;
    float periodTimer;

    bool currentlySpawning;
    [SerializeField] int totalWave;
    [SerializeField] int currentWave;
    [SerializeField] int enemiesNumber;
    [SerializeField] int enemiesNumbers;

    [SerializeField] int currentLoot;

    [SerializeField] List<GameObject> uniqueSpawners;

    [Header("UI STUFF")]
    [SerializeField] GameObject raidUI;
    [SerializeField] TMP_Text raidTimerText;
    [SerializeField] Image raidProgress;
    [SerializeField] GameObject raidEndUI;
    [SerializeField] TMP_Text raidEndText;
    [SerializeField] TMP_Text raidEndDescText;
    [SerializeField] Button raidEndButton;

    // sound 
    private bool hasPlayedWinSound = false;



    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        if (GameController.Instance.supposedRaid)
        {
            StartRaid(2, 500);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (raidStart)
        {
            if (raiding)
            {
                raidTimer += Time.deltaTime;
                raidTimerText.text = (raidTimer / 60).ToString("F0") + ":" + (int)(raidTimer % 60);
                if (raidTimer > raidTimeLimit)
                {
                    RaidEnd(false);
                }

                if (!currentlySpawning)
                {
                    // Always check enemy number
                    enemiesNumber = GetCurrentNumber();
                    raidProgress.fillAmount = (float)enemiesNumber / enemiesNumbers;

                    // If enemy all died, go to next wave or end raid
                    if (enemiesNumber <= 0)
                    {
                        // If there are no next wave, raid finished
                        if (currentWave >= totalWave)
                        {
                            Debug.Log("ALL ENEMIES DEAD AND WAVES END");
                            RaidEnd(true);
                        }
                        else
                            NextWave();
                    }
                }
            }
            else
            {
                if (winStatus)
                {
                    // Win
                    raidEndText.color = Color.yellow;
                    raidEndText.text = "Raid Complete";
                    raidEndDescText.text = "Gained Coins: " + currentLoot;

                    // Give Money
                    GameEconomy.Instance.GainMoney(currentLoot);
                    currentLoot = 0;

                    // Mainkan sound "WinRaid"
                    if (!hasPlayedWinSound)
                    {
                        if (SoundManager.Instance != null)
                            SoundManager.Instance.PlaySound("WinRaid");
                        hasPlayedWinSound = true;
                    }
                }
                else
                {
                    // Lost
                    raidEndText.color = Color.red;
                    raidEndText.text = "The Bandits Have Finished Looting the Goods";
                    raidEndDescText.text = "Coins: -" + currentLoot;

                    // Lost money
                    GameEconomy.Instance.LostMoney(currentLoot);
                }
            }
        }
    }

    [ContextMenu("Start Raid")]
    void StartRaidFromMenu()
    {

        StartRaid(1, 100);
    }

    public void StartRaid(int totalWave, int theLoot, UnityAction afterAction = null)
    {
        currentlySpawning = true;
        raidStart = true;
        raiding = true;

        Player_Quest.Instance.SetQuest("Bunuh semua bandit");

        raidUI.SetActive(true);
        SetNumberTotal(enemiesNumber);
        foreach (Transform child in spawners)
        {
            if (child.gameObject.activeInHierarchy)
                enemiesNumbers += enemiesNumber;
        }

        StartCoroutine(EnableSpawners());

        currentWave = 1;
        this.totalWave = totalWave;

        currentLoot += theLoot;

        SetActionAfter(afterAction);
    }

    public void StartRaid(bool endOfStory, UnityAction afterAction = null)
    {
        foreach (GameObject spawner in uniqueSpawners)
        {
            spawner.SetActive(true);
        }
        StartRaid(5, 500, () =>
        {
            afterAction?.Invoke();
            EndOfStoryRaid();
        });
    }

    void EndOfStoryRaid()
    {
        foreach (GameObject spawner in uniqueSpawners)
        {
            spawner.SetActive(false);
        }
    }

    public void SetActionAfter(UnityAction afterAction = null)
    {
        raidEndButton.onClick.RemoveAllListeners();
        if (afterAction != null)
            raidEndButton.onClick.AddListener(afterAction);
    }

    void NextWave()
    {
        currentlySpawning = true;
        // Wait a sec before next wave
        periodTimer += Time.deltaTime;
        if (periodTimer > gracePeriod)
        {
            periodTimer = 0;
            currentWave++;
            enemiesNumbers = totalWave * 5;
            SetNumberTotal(enemiesNumbers);
            StartCoroutine(EnableSpawners());
        }
    }

    void RaidEnd(bool win)
    {
        raiding = false;
        winStatus = win;
        raidEndUI.SetActive(true);
    }

    IEnumerator EnableSpawners()
    {
        Debug.Log("spawning");
        foreach (Transform spawner in spawners)
        {
            spawner.GetComponent<Enemy_Spawner>().CanSpawn = true;
        }
        yield return new WaitForSeconds(1f);
        foreach (Transform spawner in spawners)
        {
            spawner.GetComponent<Enemy_Spawner>().CanSpawn = false;
        }
        Debug.Log("not spawning");
        currentlySpawning = false;
    }

    int GetCurrentNumber()
    {
        int total = 0;
        foreach (Transform spawner in spawners)
        {
            total += spawner.GetComponent<Enemy_Spawner>().enemies.Count;
        }
        return total;
    }

    void SetNumberTotal(int numbers)
    {
        foreach (Transform spawner in spawners)
        {
            spawner.GetComponent<Enemy_Spawner>().spawnCount = numbers;
        }
    }
}
