using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject[] rooms;
    public int curRoom = 0;
    private int pre;

    void Awake()
    {
        if (Instance != null && Instance != this)
            {
                Destroy(gameObject); 
                return;
            }
        Instance = this;

        for(int i = 0; i < rooms.Length; i++)
        {
            if (i == curRoom) rooms[i].SetActive(true);
            else rooms[i].SetActive(false);
        }
    }

    public void SwitchRoom(int idx)
    {
        pre = curRoom;
        curRoom = idx;
        rooms[curRoom].SetActive(true);
        CameraManager.Instance.SwitchTo(curRoom);
        rooms[pre].SetActive(false);
    }
}
