using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items")]
public class ItemSO : ScriptableObject
{
    public string id;
    public string itemName;
    public string target_id; 
    public Sprite icon;
    public GameObject windowPrefab;
}
