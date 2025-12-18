using UnityEngine;
using System.Collections.Generic;
using System;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance {get; private set;}
    [SerializeField] private int maxTruth = 1;
    public int truthSum = 0;
    public float truthLevel = 0f;
    public bool gifted = false;
    public event Action<int> OnTruthChanged;

    
    Dictionary<string, int> truthTable = new Dictionary<string, int>()
    {
        { "LivingRoomMemo1", 5 },
        { "TV", 5 },
        { "Memory1", 5 },
        { "JamesPhoto", 5 },
        { "Sheet", 5 },
        { "JamesDiary", 20 },
        { "SonAndDad", 5 },
        { "Award", 5 },
        { "TomDiary", 30 },
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

        Instance = this;
    }

    void OnEnable()
    {
        InventoryManager.Instance.OnItemUsed += GiftHandler;
    }

    public void AddTruthByKey(string key)
    {
        truthSum += truthTable[key];
        truthLevel = truthSum / maxTruth;
        Debug.Log($"Truth {truthTable[key]} Added. Current Truth = {truthSum}");
        OnTruthChanged?.Invoke(truthSum);
    }

    public void AddTruthByNum(int i)
    {
        truthSum += i;
        truthLevel = truthSum / maxTruth;
        Debug.Log($"Truth {i} Added. Current Truth = {truthSum}");
        OnTruthChanged?.Invoke(truthSum);
    }

    private void GiftHandler(ItemSO item)
    {
        if (item.itemName == "Gift") gifted = true;
    }
}
