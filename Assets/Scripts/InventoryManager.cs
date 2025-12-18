using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    private List<ItemSO> items = new();
    public ItemSO selected;

    // events for UI
    public event Action<ItemSO> OnItemAdded;
    public event Action<ItemSO> OnItemUsed; 
    public event Action<ItemSO> OnItemSelected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }
        Instance = this; 
    }

    public List<ItemSO> GetItems()
    {
        return items;
    }

    public void AddItem(ItemSO item)
    {   
        items.Add(item);
        OnItemAdded?.Invoke(item);
        AudioManager.Instance.PlayItemPickup();
    }

    public void UseItem()
    {   
        UnityEngine.Debug.Log($"{selected.itemName} used");
        items.Remove(selected);
        OnItemUsed?.Invoke(selected);
        selected = null;
    }

    public void SelectItem(ItemSO item)
    {
        if (selected == item) selected = null;
        else selected = item;

        OnItemSelected?.Invoke(selected);
    }
}
