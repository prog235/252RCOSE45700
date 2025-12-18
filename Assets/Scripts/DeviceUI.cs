using UnityEngine;
using UnityEngine.UI;

public class DeviceUI : MonoBehaviour
{
    private Transform cur;
    public Transform initial;
    public Transform main;
    private Transform app;

    void Awake()
    {
        if (cur == null)
        {
            cur = initial;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
            initial.gameObject.SetActive(true);
        }
    }

    public void Goto(Transform next)
    {        
        next.gameObject.SetActive(true);
        if (cur.name != "Main")
        {
            cur.gameObject.SetActive(false);
        }
        cur = next;
    }


    public void Close()
    {
        cur.gameObject.SetActive(false);
        cur = main;
    }

}
