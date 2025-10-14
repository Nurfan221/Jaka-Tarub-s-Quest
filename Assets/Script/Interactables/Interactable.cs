using UnityEngine;

// Mengubah kelas menjadi 'abstract'
public abstract class Interactable : MonoBehaviour
{
    [Tooltip("Pesan yang akan ditampilkan saat pemain melihat objek ini.")]
    public string promptMessage;

    // Fungsi ini akan menjadi satu-satunya titik masuk untuk interaksi.
    public void BaseInteract()
    {
        Interact();
    }

    protected abstract void Interact();
}
