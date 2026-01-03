using UnityEngine;
using UnityEngine.Tilemaps;
public enum SurfaceType
{
    Dirt,
    Grass,
    Stone,
    Wood,
    Water
}
public class FootstepController : MonoBehaviour
{
    [Header("Tilemap References")]
    public Tilemap tilemaps;
    public TileSoundLibrary soundLibrary;

    [Header("Settings")]
    public float hearingDistance = 15f; // Jarak maksimal suara terdengar (sesuaikan dengan ukuran layar)

    public void TriggerFootstep()
    {

        // Ambil posisi Kamera Utama
        Vector3 cameraPos = Camera.main.transform.position;
        // Ambil posisi Karakter ini
        Vector3 myPos = transform.position;

        // Hitung Jarak (Abaikan sumbu Z karena ini game 2D)
        cameraPos.z = 0;
        myPos.z = 0;

        float distance = Vector3.Distance(myPos, cameraPos);

        //  Kalau kejauhan, BATALKAN suara.
        if (distance > hearingDistance)
        {
            return; // Stop di sini, jangan mainkan suara
        }
        // (Dikurangi sedikit Y-nya agar pas di kaki, bukan di pusat badan)
        Vector3 footPosition = transform.position + new Vector3(0, -0.1f, 0);

        // Cek Tile apa yang ada di posisi itu
        SurfaceType surface = GetSurfaceAtPosition(footPosition);

        // Mainkan Suara via SoundManager
        PlaySoundForSurface(surface);
    }

    private SurfaceType GetSurfaceAtPosition(Vector3 worldPos)
    {
        Vector3Int gridPos = tilemaps.WorldToCell(worldPos);
        TileBase tile = tilemaps.GetTile(gridPos);

        if (tile != null)
        {
            return soundLibrary.GetSurfaceType(tile);
        }

        return SurfaceType.Dirt; 
    }

    private void PlaySoundForSurface(SurfaceType surface)
    {
        switch (surface)
        {
            case SurfaceType.Grass:
                SoundManager.Instance.PlaySound(SoundName.StepGrass, true); // True = Random Pitch
                break;
            case SurfaceType.Stone:
                SoundManager.Instance.PlaySound(SoundName.StepStone, true);
                break;
            case SurfaceType.Wood:
                SoundManager.Instance.PlaySound(SoundName.StepWood, true);
                break;
            default:
                SoundManager.Instance.PlaySound(SoundName.StepDirt, true);
                break;
        }
    }
}