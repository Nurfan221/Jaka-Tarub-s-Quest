using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Farming : MonoBehaviour
{
    public Seed currentSeed; // Menyimpan referensi ke tanaman yang saat ini berada di dekat pemain

    private void Update()
    {
        // Jika pemain menekan tombol 'X'
        if (Input.GetKeyDown(KeyCode.X) && currentSeed != null )
        {
            Debug.Log("Tombol X ditekan, mencoba memanen tanaman.");
            currentSeed.Harvest();
        }

        if(Input.GetKeyDown(KeyCode.C) && currentSeed.siram == true){
            Debug.Log("Tombol C ditekan, mencoba Menyiram tanaman.");
            currentSeed.Siram();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Jika pemain mendekati tanaman
        if (other.CompareTag("Seed"))
        {
            Seed seed = other.GetComponent<Seed>();
            if (seed != null)
            {
                currentSeed = seed;
                Debug.Log("Pemain berada di dekat tanaman.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Jika pemain meninggalkan tanaman
        if (other.CompareTag("Seed"))
        {
            Seed seed = other.GetComponent<Seed>();
            if (seed != null && seed == currentSeed)
            {
                currentSeed = null;
                Debug.Log("Pemain meninggalkan tanaman.");
            }
        }
    }
}