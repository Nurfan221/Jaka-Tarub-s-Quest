using UnityEngine;


public class PlayerQuest : MonoBehaviour
{

    [SerializeField] QuestManager questManager;
    public GameObject objekMainQuest;
    public int indexLocation;
    public bool inLocation = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Pastikan hanya memicu sekali
        if (!inLocation)
        {
            // Cek apakah objek yang masuk adalah
            if (other.gameObject == objekMainQuest)
            {
                // Panggil fungsi playMainLocationQuest dengan index yang sesuai
                questManager.playMainLocationQuest(indexLocation);

                // Tampilkan log untuk debugging
                Debug.Log($"Objek {objekMainQuest.name} telah memasuki lokasi! Memanggil quest index {indexLocation}");

                // Set agar fungsi tidak bisa dipanggil lagi
                inLocation = true;
            }
            

        }

        

    }
 

}
