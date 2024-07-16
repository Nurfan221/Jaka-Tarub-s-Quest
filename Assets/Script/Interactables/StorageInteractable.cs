using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageInteractable : Interactable
{
    public List<Item> Items = new();
    [SerializeField] StorageUI storageUI;
    protected override void Interact()
    {
        storageUI.OpenStorage(this, Items);
    }
}
