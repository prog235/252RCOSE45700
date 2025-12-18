using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform slotParent; 
    [SerializeField] private GameObject slotPrefab; 
    private List<GameObject> slotInstances = new();

    private void OnEnable()
    {
        InventoryManager.Instance.OnItemAdded += HandleItemAdded;
        InventoryManager.Instance.OnItemUsed += HandleItemUsed;
        InventoryManager.Instance.OnItemSelected += HandleItemSelected;
        RefreshUI();
    }

    private void OnDisable()
    {
        InventoryManager.Instance.OnItemAdded -= HandleItemAdded;
        InventoryManager.Instance.OnItemUsed -= HandleItemUsed;
        InventoryManager.Instance.OnItemSelected -= HandleItemSelected;
    }

    private void HandleItemAdded(ItemSO item)
    {
        RefreshUI();
    }

    private void HandleItemUsed(ItemSO item)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        foreach (var slot in slotInstances)
            Destroy(slot);
        slotInstances.Clear();

        foreach (var item in InventoryManager.Instance.GetItems())
        {
            GameObject slotGO = Instantiate(slotPrefab, slotParent);
            InvenSlot slotUI = slotGO.GetComponent<InvenSlot>();
            slotUI.SetItem(item); 
            slotInstances.Add(slotGO);
        }
    }

    private void HandleItemSelected(ItemSO selected)
    {
        foreach (var slot in slotInstances)
        {
            InvenSlot slotUI = slot.GetComponent<InvenSlot>();
            slotUI.SetOutline(selected);
        }
    }
}

