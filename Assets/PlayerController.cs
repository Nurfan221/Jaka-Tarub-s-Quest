using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Singleton "Otak"
    public static PlayerController Instance { get; private set; }

    // Variabel untuk menyimpan koneksi ke "Tubuh" yang aktif saat ini.
    // Properti publik agar skrip lain bisa melihat, tapi hanya kelas ini yang bisa mengubah.
    // Sekarang ia hanya menyimpan SATU referensi ke "paket" Player yang aktif.
    public Player ActivePlayer { get; private set; }
    public Rigidbody2D ActivePlayerRigidbody { get; private set; }
    public Animator ActivePlayerAnimator { get; private set; }
    public Vector2 MovementDirection { get; private set; }

    public PlayerData_SO playerData;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // Fungsi ini akan dipanggil oleh setiap Player_Movement baru yang muncul.
    public void RegisterPlayer(Player player)
    {
        this.ActivePlayer = player;
        Debug.Log($"PlayerController: Paket Player '{player.gameObject.name}' telah terdaftar.");
    }

    // Fungsi Unregister juga diubah
    public void UnregisterPlayer(Player player)
    {
        if (this.ActivePlayer == player)
        {
            this.ActivePlayer = null;
        }
    }

    // --- CONTOH FUNGSI PERINTAH ---

    public void HandleMovement(Vector2 direction)
    {
        // Cek apakah ada player aktif, lalu akses departemen gerakannya.
        if (ActivePlayer != null)
        {
            // "Manajer Hotel, tolong suruh departemen gerakan untuk bergerak."
            ActivePlayer.Movement.SetMovementDirection(direction);
        }
    }

    public void HandleDash()
    {
        ActivePlayer.Movement.TriggerDash();
    }

    public void HandleAttack()
    {
        if (ActivePlayer != null)
        {
            // "Manajer Hotel, tolong suruh departemen aksi untuk menyerang."
            //ActivePlayer.Action.PerformAttack();
        }
    }

    public void HandleSpendStamina(float useStamina)
    {
        ActivePlayer.Health.SpendStamina(useStamina);
    }
}