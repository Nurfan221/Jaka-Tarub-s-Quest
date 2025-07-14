using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlantContainer : MonoBehaviour
{

    public List<GameObject> plantObject;

    private void OnEnable()
    {
        TimeManager.OnDayChanged += HandleNewDay;
    }
    private void OnDisable()
    {
        TimeManager.OnDayChanged -= HandleNewDay;
    }
    
    private void HandleNewDay()
    {
        HitungPertumbuhanPohon();
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
