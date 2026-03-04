public class PlantInteractable : Interactable
{
    public PlantSeed plantSeed; // Changed to public to be accessible

    private void Start()
    {
        plantSeed = GetComponent<PlantSeed>();
    }

    private void Update()
    {
        // if (plantSeed.isReadyToHarvest)
        // {
        //     promptMessage = "Panen Tanaman";
        // }
        // else
        // {
        //     promptMessage = "";
        // }
    }

    protected override void Interact()
    {
        if (plantSeed.isReadyToHarvest)
        {
            plantSeed.Harvest();
        }else if(plantSeed.isPlantDie)
        {
            SoundManager.Instance.PlaySound(SoundName.MulungSfx);
            ItemData tanamanLayu = new ItemData("TanamanLayu", 1, ItemQuality.Normal, 0);
            ItemPool.Instance.AddItem(tanamanLayu);
            FarmTile.Instance.OnPlantHarvested(plantSeed.UniqueID);
            Destroy(gameObject);
        }
    }
}
