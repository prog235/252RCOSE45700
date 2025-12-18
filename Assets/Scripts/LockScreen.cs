using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LockScreen : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Transform main;
    private DeviceUI device;
    void Awake()
    {
        device = GetComponentInParent<DeviceUI>();
        inputField.onSubmit.AddListener(_ => IsCorrect());
    }

    // Update is called once per frame
    public void IsCorrect()
    {
        string msg = inputField.text.Trim();

        if (string.IsNullOrEmpty(msg)) return;

        string upper = msg.ToUpper();
        if (upper == "POSE@1707")
        {
            device.Goto(main);
            StateManager.Instance.AddTruthByNum(10);
            AudioManager.Instance.PlayPC();
        }
    }
}
