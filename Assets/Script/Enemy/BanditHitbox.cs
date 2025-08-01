using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BanditHitbox : MonoBehaviour
{
    [SerializeField] QuestManager questManager;
    public Enemy_Bandit bandit; // Referensi ke skrip utama Bandit
    public int damageHit = 10; // Default damage
    public Player_Health playerHealth;
    public float jedaSerangan = 3f; // Jeda antar serangan
    public bool isBanditQuest = false;
    private bool playerDiHitbox = false; // Untuk memastikan serangan tidak dobel

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player terkena hitbox! Bandit berhenti mengejar.");
            playerHealth = other.GetComponent<Player_Health>();

            bandit.isMoving = false; // Hentikan gerakan bandit
            bandit.StopChasing(); // Hentikan pengejaran dengan aman

            if (!bandit.isAttacking)
            {
                bandit.isAttacking = true;
                playerDiHitbox = true;
                StartCoroutine(Serang()); // Mulai serangan
            }
        }
    }

    private IEnumerator Serang()
    {
        while (bandit.isAttacking && playerDiHitbox)
        {
            bandit.isMoving = false;
            bandit.rb.linearVelocity = Vector2.zero; // Hentikan pergerakan

            bandit.PlayActionAnimation("Sword");

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageHit, transform.position); // Kirim posisi musuh sebagai parameter
                Debug.Log("Menyerang Player! Damage: " + damageHit);

                if (isBanditQuest)
                {
                    int sekaratThreshold = (int)(PlayerController.Instance.playerData.maxHealth * 0.3f);
                    //if (PlayerController.Instance.playerData.health <= sekaratThreshold && !questManager.playerSekaratSudahDiproses)
                    //{
                    //    Debug.Log("Player sekarat! Memulai event kekalahan...");
                    //    questManager.playerSekaratSudahDiproses = true;
                    //    //questManager.currentMainQuest.currentQuestState = MainQuest1State.Sekarat;
                    //    //questManager.NextQuestState();
                    //}
                }
            }


            yield return new WaitForSeconds(jedaSerangan);
            

            // Jika player keluar dari hitbox, hentikan serangan
            if (!playerDiHitbox)
            {
                Debug.Log("Player keluar dari hitbox, hentikan serangan.");
                bandit.isAttacking = false;
                yield break; // Hentikan coroutine
            }

            playerDiHitbox = false;


        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player keluar dari hitbox! Bandit kembali mengejar.");

            bandit.isAttacking = false;
            playerDiHitbox = false;

            // Tunggu sebentar sebelum mulai mengejar kembali
            StartCoroutine(KembaliMengejar());
        }
    }

    private IEnumerator KembaliMengejar()
    {
        yield return new WaitForSeconds(1.5f); // Tunggu sebelum mulai mengejar kembali
        if (!bandit.isAttacking)
        {
            bandit.StartChasing();
        }
    }




}
