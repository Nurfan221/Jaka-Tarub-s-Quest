using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInteractable : Interactable
{
    [SerializeField] ShopUI shopUI;

    protected override void Interact()
    {
        shopUI.OpenShop();
    }
}
