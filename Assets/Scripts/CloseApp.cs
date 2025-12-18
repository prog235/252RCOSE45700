using UnityEngine;
using UnityEngine.UI;

public class CloseApp : MonoBehaviour
{
    [SerializeField] Button btn;
    private DeviceUI device;
    
    void Awake()
    {
        device = GetComponentInParent<DeviceUI>();
        btn.onClick.AddListener(Close);
    }


    public void Close()
    {
        device.Close();
    }
}
