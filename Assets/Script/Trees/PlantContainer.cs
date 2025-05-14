using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlantContainer : MonoBehaviour
{

    public List<GameObject> plantObject;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HitungPertumbuhanPohon()
    {
        // Buat salinan agar aman saat list aslinya dimodifikasi
        List<GameObject> salinanPlant = new List<GameObject>(plantObject);

        foreach (var prefabObject in salinanPlant)
        {
            if (prefabObject == null) continue;

            TreeBehavior treeBehavior = prefabObject.GetComponent<TreeBehavior>();
            if (treeBehavior != null && treeBehavior.currentStage != GrowthTree.MaturePlant)
            {
                treeBehavior.PertumbuhanPohon();
            }
        }
    }

}
