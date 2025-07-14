using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Properti publik untuk menyimpan referensi ke semua "departemen"-nya.
    // Skrip lain akan mengakses komponen player melalui ini.
    public Player_Movement Movement { get; private set; }
    public Player_Action Action { get; private set; }
    public Player_Inventory Inventory { get; private set; }
    public Player_Health Health { get; private set; }
    
    // Tambahkan komponen penting lainnya di sini

    private void Awake()
    {
        // Saat "Manajer Hotel" ini bangun, ia langsung mencari semua kepala departemennya.
        Movement = GetComponent<Player_Movement>();
        Action = GetComponent<Player_Action>();
        Inventory = GetComponent<Player_Inventory>();
        Health = GetComponent<Player_Health>();
    }

    private void Start()
    {
        // Setelah semua departemen siap, Manajer Hotel ini melapor ke "Kantor Pusat".
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.RegisterPlayer(this);
        }
        else
        {
            Debug.LogError("Player tidak bisa menemukan PlayerController untuk mendaftar!");
        }
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.UnregisterPlayer(this);
        }
    }


}