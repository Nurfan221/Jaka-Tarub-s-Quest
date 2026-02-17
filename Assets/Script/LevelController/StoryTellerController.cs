using UnityEngine;
using UnityEngine.UI; // Untuk Image UI
using UnityEngine.Events; // Jika ingin menggunakan Event Unity

public class StoryTellerController: MonoBehaviour
{
    public static StoryTellerController Instance { get; private set; }

    [Header("Data Cerita")]
    public OptionalCerita prologueData; // Masukkan data cerita disini via Inspector

    private void Awake()
    {
        // Logika Singleton standar
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
    private void OnEnable()
    {
        DialogueSystem.OnDialogueEnded += OnSegmentFinished; // Daftar ke event dialog selesai
    }

    private void OnDisable()
    {
        DialogueSystem.OnDialogueEnded -= OnSegmentFinished; // Un-daftar saat objek mati
    }

    // Variable Private untuk melacak posisi cerita
    private int currentIndex = 0;

    void Start()
    {
        // Mulai cerita saat game dimulai (atau panggil fungsi ini dari tombol)
    }

    public void StartStory(OptionalCerita cerita)
    {
        prologueData = cerita; // Set data cerita yang ingin dimainkan
        currentIndex = 0;
        PlayStorySegment();
    }

    // Fungsi Utama: Memainkan 1 potongan cerita berdasarkan Index
    private void PlayStorySegment()
    {
        // 1. Cek apakah cerita sudah habis?
        if (currentIndex > prologueData.contentOptionalCerita.Count)
        {
            EndStory();
            return;
        }

        Debug.Log($"Memainkan Cerita Bagian ke-{currentIndex}");

        // 2. Ganti Gambar (Sprite)
        if (prologueData.contentOptionalCerita.Count > currentIndex)
        {
            QuestManager.Instance.HandleContentStory(prologueData.contentOptionalCerita[currentIndex].gambarCerita);
        }

        // 3. Putar Audio (Voice Over)
        if (prologueData.contentOptionalCerita.Count > currentIndex)
        {
            if (prologueData.contentOptionalCerita[currentIndex].audioCerita != null)
            {
                SoundManager.Instance.PlayAudioClip(prologueData.contentOptionalCerita[currentIndex].audioCerita, 1);
            }
        }

        if (prologueData.contentOptionalCerita.Count > currentIndex)
        {
            if (prologueData.contentOptionalCerita[currentIndex].dialogCerita != null)
            {
                Dialogues currentDialog = prologueData.contentOptionalCerita[currentIndex].dialogCerita;

                DialogueSystem.Instance.HandlePlayDialogue(currentDialog, false);
            }
        }


        // 4. Mulai Dialog
        // DI SINI KUNCINYA: Kita harus memberitahu Dialogue Manager 
        // "Hei, kalau dialog ini selesai, panggil fungsi 'OnSegmentFinished' ya!"


        // Asumsi fungsi di DialogueManager Anda bisa menerima "Action" (Callback) saat selesai
        // Jika belum ada, lihat poin nomor 3 di bawah untuk cara memodifikasinya.
    }

    // Fungsi ini dipanggil OTOMATIS ketika Dialog selesai
    public void OnSegmentFinished()
    {
        Debug.Log($"Bagian ke-{currentIndex} selesai. Lanjut ke bagian berikutnya.");

        // Naikkan index ke 1
        currentIndex++;

        // Panggil fungsi play lagi (Looping logic)
        PlayStorySegment();

        if (currentIndex > prologueData.contentOptionalCerita.Count)
        {
            currentIndex = 0; // Reset jika sudah selesai semua cerita
        }
    }

    private void EndStory()
    {
        Debug.Log("Seluruh Prolog Selesai! Pindah ke Gameplay...");
        // Disini logika pindah scene atau memunculkan karakter player
        // SceneManager.LoadScene("GameplayScene");
    }
}