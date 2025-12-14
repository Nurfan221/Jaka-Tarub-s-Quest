using UnityEngine;

public class BanditDetection : MonoBehaviour
{
    public Enemy_Bandit bandit; // Referensi ke skrip utama Bandit

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Target Terdeteksi! Kejar " + other.name);

            if (bandit != null)
            {
                bandit.SetTarget(other.transform); // Panggil fungsi kejar di Enemy_Bandit
            }
            else
            {
                Debug.LogError("Bandit tidak di-assign di Inspector!");
            }
        }
    }


    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Player"))
    //    {
    //        Debug.Log("Target Keluar dari Jangkauan! " + collision.name);
    //        bandit.SetTarget(collision.transform); // Set target menjadi null agar berhenti mengejar

    //        // Panggil animasi berdasarkan arah pergerakan
    //        bandit.SetWalkAnimation(bandit.moveDirection.y > 0.1f, bandit.moveDirection.y < -0.1f, bandit.moveDirection.x > 0.1f, bandit.moveDirection.x < -0.1f);
    //        // Coba temukan BanditHitbox
    //        BanditHitbox bH = bandit.GetComponentInParent<BanditHitbox>();
    //        if (bH != null)
    //        {
    //            BanditHitbox banditHitbox = bH.gameObject.GetComponent<BanditHitbox>();
    //            if (banditHitbox != null)
    //            {
    //                banditHitbox.isAttacking = false; // Hentikan serangan jika ditemukan
    //            }

    //        }

    //    }
    //}

}
