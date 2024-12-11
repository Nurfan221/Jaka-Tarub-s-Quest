using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerangkapBehavior : MonoBehaviour
{
    public bool isfull; // Apakah perangkap penuh

    public GameObject[] hewanBuruan; // Daftar prefab hewan yang bisa ditangkap
    public GameObject animalInTrap;  // Hewan yang tertangkap
    public GameObject imageAnimalInTrap; // GameObject di atas perangkap untuk menampilkan sprite hewan
    public float Health = 1;


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
            Health -= 1;
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
                Player_Inventory.Instance.AddItem(ItemPool.Instance.GetItem(animalInTrap.gameObject.name));
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
            if (Health <=0)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log("Perangkap kosong, tidak ada hewan yang bisa dilepas.");
        }
    }

    



}
