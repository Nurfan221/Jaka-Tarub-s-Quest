using UnityEngine;

public class EnvironmentInteractable : Interactable
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Interact()
    {
        EnvironmentBehavior environmentBehavior = gameObject.GetComponent<EnvironmentBehavior>();
        if (environmentBehavior != null && environmentBehavior.itemDrop != null)
        {
            //Player_Inventory.Instance.AddItem(ItemPool.Instance.GetItem(environmentBehavior.itemDrop.itemName));


            EnvironmentManager environmentManager = environmentBehavior.plantsContainer.gameObject.GetComponent<EnvironmentManager>();

            if (environmentManager != null)
            {
                foreach (var cekLokasiObjek in environmentManager.environmentList)
                {
                    if (gameObject.transform.position == cekLokasiObjek.objectPosition && environmentBehavior.nameEnvironment == cekLokasiObjek.prefabName)
                    {
                        cekLokasiObjek.isGrowing = false;
                    }
                }
            }
            Destroy(gameObject);
        }
    }
}
