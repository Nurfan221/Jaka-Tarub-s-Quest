using UnityEngine;

public class VillageController : MonoBehaviour
{
    public static VillageController Instance;

    Transform player;
    [SerializeField] Transform playerSpawnSpot;


    public Transform WargaDesaTransform;
    public Transform KakRenTransform;
    public Transform TanamanCabaiTransform;
    public Transform TengahKota;
    public Transform SumberBandit;

    public GameObject PedangKakRen;


    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameController.Instance.ResumeGame();

        player = GameObject.FindGameObjectWithTag("Player").transform;
        //if (GameController.Instance.fromPortal)
        //    player.position = playerSpawnSpot.position;

        //if (!GameController.NewGame)
        //    GameController.Instance.LoadGame();



        // if (!GameEventSystem.Instance.DoneDialogue_FirstDesaWarga && GameEventSystem.Instance.DoneDialogue_DanauPertamaKeDesa)
        // {
        //     Player_Direction.Instance.Target = WargaDesaTransform;
        // }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
