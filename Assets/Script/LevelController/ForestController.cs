using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestController : MonoBehaviour
{
    public static ForestController Instance;


    Transform player;
    [SerializeField] Transform playerSpawnSpot;

    public GameObject FirstSpawner;
    bool firstSpawnerDefeated;
    [SerializeField] Dialogues danauKetenanganDialogue;
    public Transform DanauKetenangan;
    public Transform VillagePortal;


    public GameObject QUEST_GagangPedang;
    public GameObject QUEST_Perisai;
    public GameObject QUEST_Armor;
    public GameObject QUEST_Buku;
    public GameObject QUEST_Tongkat;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameController.Instance.ResumeGame();

        if (!GameController.NewGame)
            GameController.Instance.LoadGame();

        // player = GameObject.FindGameObjectWithTag("Player").transform;
        // if (!GameEventSystem.Instance.DoneFirstNarration)
        // {
        //     DialogueSystem.Instance.StartFirstDialogue();
        // }
        else
        {
            if (GameController.Instance.fromPortal)
                player.position = playerSpawnSpot.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!firstSpawnerDefeated)
        {
            FirstSpawner.GetComponent<Enemy_Spawner>().CanSpawn = false;
            if (FirstSpawner.GetComponent<Enemy_Spawner>().enemies.Count == 0)
            {
                firstSpawnerDefeated = true;
                // DialogueSystem.Instance.StartDialogue(danauKetenanganDialogue);
            }
        }
    }
}
