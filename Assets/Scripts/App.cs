using UnityEngine;
using UnityEngine.UI;

public class App : MonoBehaviour
{
    [SerializeField] Button btn;
    [SerializeField] Transform next;
    private DeviceUI device;

    void Awake()
    {
        device = GetComponentInParent<DeviceUI>();
        btn.onClick.AddListener(Execute);
    }


    public void Execute()
    {
        device.Goto(next);
    }
}
