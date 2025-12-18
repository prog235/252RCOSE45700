using UnityEngine;

public class Drawer : MonoBehaviour
{
    [SerializeField] private GameObject closed;
    [SerializeField] private GameObject opened;

    void OnEnable()
    {
        InventoryManager.Instance.OnItemUsed += HandleDrawer;
    }


    void Awake()
    {
        closed.SetActive(true);
        opened.SetActive(false);
    }

    private void HandleDrawer(ItemSO item)
    {
        UnityEngine.Debug.Log("HandleDrawer");
        if (item.itemName == "Key2")
        {
            closed.SetActive(false);
            opened.SetActive(true);
            AudioManager.Instance.PlayDoorUnlock();
            AudioManager.Instance.PlayDrawer();
        }
    }

    void OnDisable()
    {
        InventoryManager.Instance.OnItemUsed -= HandleDrawer;
    }

}
