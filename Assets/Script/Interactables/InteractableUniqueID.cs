using UnityEngine;

public class InteractableUniqueID : UniqueIdentifiableObject
{ 
    
    //  Implementasi dari Kontrak IUniqueIdentifiable 
    public EnvironmentHardnessLevel hardnessLevel;
    public TypeObject typeObject;
    public TypePlant typePlant;
    public ArahObject arahObject;
    public EnvironmentType environmentType;

    #region Unique ID Implementation

    public override string GetObjectType()
    {
        // Berikan kategori umum untuk objek ini.
        return typeObject.ToString();
    }

    public override EnvironmentHardnessLevel GetHardness()
    {
        // Ambil nilai dari variabel yang bisa diatur di Inspector.
        return hardnessLevel;
    }

    public override string GetBaseName()
    {
        // Ambil nama dasar dari variabel yang bisa diatur di Inspector.
        if (typePlant == TypePlant.None && arahObject != ArahObject.None)
        {
            return arahObject.ToString();
        }
        else
        {
            return typePlant.ToString();
        }

    }

    public override string GetVariantName()
    {
        return environmentType.ToString();

    }

    #endregion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
