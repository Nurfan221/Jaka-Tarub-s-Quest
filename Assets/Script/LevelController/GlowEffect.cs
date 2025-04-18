using UnityEngine;

public class GlowParticleEffect : MonoBehaviour
{
    private ParticleSystem glowingParticles; // Menyimpan referensi Particle System

    void Start()
    {
        // Mendapatkan komponen Particle System dari objek ini
        glowingParticles = GetComponentInChildren<ParticleSystem>();

        // Memastikan Particle System sudah diatur dengan benar
        if (glowingParticles != null)
        {
            // Memulai animasi Particle System (jika belum berjalan)
            glowingParticles.Play();
        }
        else
        {
            Debug.LogWarning("Particle System tidak ditemukan pada objek!");
        }
    }

    // Fungsi untuk memulai efek particle jika diperlukan (misalnya ketika objek diambil)
    public void StartGlowEffect()
    {
        if (glowingParticles != null)
        {
            glowingParticles.Play(); // Memulai particle effect
        }
    }

    // Fungsi untuk menghentikan efek particle jika diperlukan (misalnya ketika objek hilang)
    public void StopGlowEffect()
    {
        if (glowingParticles != null)
        {
            glowingParticles.Stop(); // Menghentikan particle effect
        }
    }
}
