using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerangkapBehavior : MonoBehaviour
{
    public bool isfull; // Apakah perangkap penuh

    public GameObject[] hewanBuruan; // Daftar prefab hewan yang bisa ditangkap
    public GameObject animalInTrap;  // Hewan yang tertangkap
    public GameObject imageAnimalInTrap; // GameObject di atas perangkap untuk menampilkan sprite hewan
    [SerializeField] private PrefabItemBehavior prefabItemBehavior;
    [SerializeField] private Player_Inventory player_Inventory;

    private PlayerData_SO stats;
    private void Awake()
    {


        // Ambil "Papan Pengumuman" dari Otak dan simpan ke jalan pintas kita.
        if (PlayerController.Instance != null)
        {
            stats = PlayerController.Instance.playerData;
        }
        else
        {
            Debug.LogError("PlayerController.Instance tidak ditemukan saat Awake!");
        }
    }
    void Start()
    {
        imageAnimalInTrap.gameObject.SetActive(false);
        // Berlangganan ke event OnDayChanged di TimeManager
        TimeManager.OnDayChanged += OnDayChanged;
    }

    void Update()
    {

    }


    private void OnDestroy()
    {
        // Unsubscribe dari event OnDayChanged untuk mencegah error saat pohon dihancurkan
        TimeManager.OnDayChanged -= OnDayChanged;
    }

    private void OnDayChanged(int currentDay)
    {
        Debug.Log($"Pohon menerima perubahan hari, Hari ke-{currentDay}");
        GetAnimalToTrap();
        Debug.Log("pohon tumbuh");
    }

    // Fungsi untuk menangkap hewan
    public void GetAnimalToTrap()
    {
        if (!isfull) // Perangkap kosong, bisa menangkap hewan
        {
            prefabItemBehavior.health -= 1;
            imageAnimalInTrap.gameObject.SetActive(true);
            // Memilih hewan secara acak dari daftar hewanBuruan
            int randomIndex = Random.Range(0, hewanBuruan.Length);
            animalInTrap = hewanBuruan[randomIndex];

            //Mendapatkan SpriteRenderer dari GameObject imageAnimalInTrap
            SpriteRenderer hewanImage = imageAnimalInTrap.GetComponent<SpriteRenderer>();

            if (hewanImage != null && animalInTrap != null)
            {
                //Mendapatkan Sprite dari hewan yang tertangkap
                Sprite spriteHewan = animalInTrap.GetComponent<SpriteRenderer>().sprite;

                if (spriteHewan != null)
                {
                    hewanImage.sprite = spriteHewan;
                    Debug.Log("Sprite hewan di perangkap diperbarui ke: " + spriteHewan.name);
                }
                else
                {
                    Debug.LogWarning("Animal tidak memiliki sprite.");
                }
            }
            else
            {
                Debug.LogWarning("SpriteRenderer tidak ditemukan di imageAnimalInTrap.");
            }

            isfull = true; // Perangkap sekarang penuh
        }
        else
        {
            Debug.Log("Perangkap sudah penuh, tidak bisa menangkap hewan baru.");
        }
    }

    //Fungsi untuk melepaskan hewan dari perangkap
    public void TakeAnimal()
    {
        if (isfull) // Perangkap penuh, hewan bisa dilepas
        {
            // Hapus sprite dari GameObject imageAnimalInTrap
            SpriteRenderer hewanImage = imageAnimalInTrap.GetComponent<SpriteRenderer>();

            if (hewanImage != null)
            {
                //Player_Inventory.Instance.AddItem(ItemPool.Instance.GetItem(animalInTrap.gameObject.name));
            }
            else
            {
                Debug.LogWarning("Komponen SpriteRenderer pada imageAnimalInTrap tidak ditemukan.");
            }

            //string nameAnimal = animalInTrap.gameObject.name;
            imageAnimalInTrap.gameObject.SetActive(false);
            //Debug.Log("Hewan " + animalInTrap.name + " dilepas dari perangkap.");

            animalInTrap = null; // Hapus hewan dari perangkap
            isfull = false; // Set perangkap kosong lagi
            if (prefabItemBehavior.health <=0)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log("Perangkap kosong, tidak ada hewan yang bisa dilepas.");
        }
    }


    public void TakePerangkap()
    {
        ItemDropInteractable itemDropInteractable = gameObject.GetComponent<ItemDropInteractable>();
        Item itemPerangkap = itemDropInteractable.item;
        itemPerangkap.health = prefabItemBehavior.health;

        bool itemFound = false; // Menyimpan status apakah item sudah ditemukan

        for (int i = 0; i < stats.itemList.Count; i++)
        {
            if (stats.itemList[i].itemName == itemPerangkap.itemName)
            {
                //stats.itemList[i].stackCount += 1; // Menambahkan jumlah stack
                itemFound = true;
                break; // Keluar dari loop setelah item ditemukan
            }
        }

        // Jika item tidak ditemukan, tambahkan item baru ke dalam inventory
        if (!itemFound)
        {
            stats.itemList.Add(itemPerangkap);
        }

        Destroy(gameObject);
    }



}
