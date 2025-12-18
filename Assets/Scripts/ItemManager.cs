using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool CanAdd(ItemSO item)
    {
        if (item == null) return false;

        return true;
    }

    public bool CanUse(ItemSO item, ItemSO target)
    {
        if (item.target_id != target.id) return false;

        return true;
    }
}
