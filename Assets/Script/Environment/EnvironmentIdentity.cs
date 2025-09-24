using UnityEngine;

public class EnvironmentIdentity : UniqueIdentifiableObject
{

    // Ini berguna untuk "memaksa" pembuatan ulang ID secara manual.
    public TypeObject envType;
    public EnvironmentHardnessLevel environmentHardnessLevel;
    StorageInteractable storageInteractable;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    #region Unique ID Implementation

    public override string GetObjectType()
    {
        // Berikan kategori umum untuk objek ini.
        return envType.ToString();
    }

    public override EnvironmentHardnessLevel GetHardness()
    {
        // Ambil nilai dari variabel yang bisa diatur di Inspector.
        return environmentHardnessLevel;
    }

    public override string GetBaseName()
    {
        // Ambil nama dasar dari variabel yang bisa diatur di Inspector.
        return envType.ToString();
    }

    public override string GetVariantName()
    {
        return "";
    }

    #endregion

    void Start()
    {
        storageInteractable = GetComponent<StorageInteractable>();
        if (storageInteractable != null)
        {
            storageInteractable.uniqueID = this.UniqueID;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
