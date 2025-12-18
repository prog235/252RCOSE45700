using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Hotspot closeup;
    [SerializeField] private Hotspot switchRoom;
    [SerializeField] private GameObject obstacles;
    

    void OnEnable()
    {
        InventoryManager.Instance.OnItemUsed += ActivateDoor;
    }


    void Awake()
    {
        switchRoom.gameObject.SetActive(false);
    }


    void ActivateDoor(ItemSO item)
    {
        if (item.itemName == "Crowbar")
        {
            if (obstacles != null) obstacles.SetActive(false);

            switchRoom.gameObject.SetActive(true);
            closeup.gameObject.SetActive(false);
            CameraManager.Instance.BackToRoom();
            AudioManager.Instance.PlayWoodFall();
        }
        else if (item.itemName.Contains("Key1"))
        {
            switchRoom.gameObject.SetActive(true);
            closeup.gameObject.SetActive(false);
            CameraManager.Instance.BackToRoom();
            AudioManager.Instance.PlayDoorUnlock();
        }
    }

    void OnDisable()
    {
        InventoryManager.Instance.OnItemUsed -= ActivateDoor;
    }
}
