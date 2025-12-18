using UnityEngine;

public class EraserPuzzle : MonoBehaviour
{
    [SerializeField] GameObject paperAfter;
    void Awake()
    {
        paperAfter.SetActive(false);
    }

    void OnEnable()
    {
        InventoryManager.Instance.OnItemUsed += HandleState;
    }

    // Update is called once per frame
    private void HandleState(ItemSO item)
    {
        if (item.itemName == "Eraser")
        {
            paperAfter.SetActive(true);
            AudioManager.Instance.PlayEraser();
        }
    }
}
