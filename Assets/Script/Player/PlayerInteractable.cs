using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Untuk EventSystem

public class PlayerInteractable : MonoBehaviour
{
    [SerializeField] private LayerMask interactablesLayer; // Layer objek interaktif
    [SerializeField] private float interactRadius = 1.5f;  // Radius interaksi
    private Interactable currentInteractable; // Objek interaktif yang sedang didekati
    public List<Interactable> interactableList = new List<Interactable>(); // Daftar semua interaksi dalam radius

    private void Start()
    {
        // Hubungkan promptButton agar bisa dipanggil
        PlayerUI.Instance.promptButton.onClick.AddListener(() =>
        {
            if (currentInteractable != null)
            {
                Debug.Log("Interaksi dipanggil melalui promptButton: " + currentInteractable.name);
                currentInteractable.BaseInteract();
            }
        });
    }

    private void Update()
    {
        // Hapus objek yang sudah dihancurkan (null) dari daftar interaktif
        interactableList.RemoveAll(interactable => interactable == null);

        // Pilih objek interaktif terdekat (jika ada banyak)
        SelectClosestInteractable();
    }

    private void SelectClosestInteractable()
    {
        if (interactableList.Count == 0)
        {
            currentInteractable = null;
            PlayerUI.Instance.SetPromptText("");
            return;
        }

        float closestDistance = Mathf.Infinity;
        Interactable closestInteractable = null;

        foreach (var interactable in interactableList)
        {
            float distance = Vector2.Distance(transform.position, interactable.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = interactable;
            }
        }

        if (closestInteractable != currentInteractable)
        {
            currentInteractable = closestInteractable;
            PlayerUI.Instance.SetPromptText("Tekan untuk " + currentInteractable.promptMessage);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null && !interactableList.Contains(interactable))
        {
            interactableList.Add(interactable);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null && interactableList.Contains(interactable))
        {
            interactableList.Remove(interactable);
        }

        if (interactableList.Count == 0)
        {
            PlayerUI.Instance.SetPromptText("");
            currentInteractable = null;
        }
    }
}
