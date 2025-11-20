using UnityEngine;

public class HitboxSensor : MonoBehaviour
{
    // Referensi ke Script Bos (NPC Utama)
    public NPCBehavior npcBehavior;

    private void Awake()
    {
        // Jika lupa drag-drop di inspector, cari otomatis di parent
        if (npcBehavior == null)
        {
            npcBehavior = GetComponentInParent<NPCBehavior>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Saat Hitbox menabrak sesuatu (misal Pintu),
        // Lapor ke script NPCBehavior!
        if (npcBehavior != null)
        {
            npcBehavior.OnHitboxTriggerEnter(collision);
        }
    }
}