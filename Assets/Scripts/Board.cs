using UnityEngine;

public class Board : MonoBehaviour
{
    void OnEnable()
    {
        InventoryManager.Instance.OnItemUsed += HandleLetters;
    }

    private void HandleLetters(ItemSO letter)
    {
        string letterName = letter.itemName;
        Transform target = transform.parent.parent.Find(letterName);
        target.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        InventoryManager.Instance.OnItemUsed -= HandleLetters;
    }
}
