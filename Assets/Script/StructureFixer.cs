using UnityEngine;

public class StructureFixer : MonoBehaviour
{
    [Header("Pengaturan PopUp")]
    public AnimationCurve popUpCurve; // Masukkan kurva di inspector prefab
    public float startYOffset = -2f;

    void Awake()
    {
        // Cek apakah SpriteRenderer masih nempel di Parent
        SpriteRenderer parentSR = GetComponent<SpriteRenderer>();

        if (parentSR != null)
        {
            //  Buat Child baru bernama "Visual"
            GameObject visualChild = new GameObject("Visual");

            // Set Child menjadi anak dari objek ini (Parent)
            visualChild.transform.SetParent(this.transform);
            visualChild.transform.localPosition = Vector3.zero;
            visualChild.transform.localScale = Vector3.one;

            // Pindahkan Data Sprite ke Child
            SpriteRenderer childSR = visualChild.AddComponent<SpriteRenderer>();
            childSR.sprite = parentSR.sprite;
            childSR.color = parentSR.color;
            childSR.sortingLayerID = parentSR.sortingLayerID;
            childSR.sortingOrder = parentSR.sortingOrder;
            childSR.material = parentSR.material;

            // Hapus SpriteRenderer dari Parent (agar tidak double)
            Destroy(parentSR);

            // Tambahkan Script PopUpAnimation ke Child
            PopUpAnimation popUp = visualChild.AddComponent<PopUpAnimation>();

            // Set settingan PopUp via code
            popUp.popUpCurve = this.popUpCurve;
            popUp.startYOffset = this.startYOffset;

            // (Script PopUpAnimation akan otomatis menjalankan logika Start-nya sendiri)
        }
    }
}