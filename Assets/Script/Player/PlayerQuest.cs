
using System.Collections.Generic;

using UnityEngine;


public class PlayerQuest : MonoBehaviour
{

    // Ini adalah "pengeras suara" yang bisa didengar oleh skrip lain.
    // Ia akan menyiarkan sebuah string (nama lokasi) saat pemain masuk ke sebuah lokasi.
    public static event System.Action<Transform> OnPlayerEnteredLocation;


    public Dialogues dialogueInLocation;
    public bool mainQuestInLocation = false;


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
        // Cek apakah objek yang kita masuki punya skrip LocationConfiguration
        LocationConfiguration locationConfig = other.GetComponent<LocationConfiguration>();

        if (locationConfig != null)
        {
            // "Teriakkan" atau siarkan Transform dari objek trigger itu sendiri (other.transform)
            Debug.Log($"Pemain masuk ke lokasi: {other.name}");
            OnPlayerEnteredLocation?.Invoke(other.transform);
        }
  
    }


}
