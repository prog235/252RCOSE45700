using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CellphonePuzzle : MonoBehaviour
{
    [Header("Circle indicators (4 dots above keypad)")]
    [SerializeField] private Image[] circles;    
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private Color filledColor = Color.white;

    [Header("Correct Code Settings")]
    [SerializeField] private int[] correctCode = { 3, 3, 2, 8 };

    private int[] inputBuffer = new int[4]; // current 4-digit input
    private int inputIndex = 0;
    private bool isLocked = false; // block input while checking/resetting
    private DeviceUI device;
    [SerializeField] Transform next;

    private void Awake()
    {
        // Find all button scripts in children and register this puzzle as their owner
        device = GetComponentInParent<DeviceUI>();
        var buttons = GetComponentsInChildren<CellphoneButton>(true);
        foreach (var btn in buttons)
        {
            btn.Init(this);
        }

        ResetInput();
    }

    /// <summary>
    /// Called from CellphoneButton when a digit is pressed.
    /// </summary>
    public void OnDigitPressed(int digit)
    {
        if (isLocked) return;
        if (inputIndex >= 4) return;

        inputBuffer[inputIndex] = digit;
        UpdateCircles(inputIndex, true);
        inputIndex++;

        if (inputIndex >= 4)
        {
            // once 4 digits entered, check answer
            StartCoroutine(CheckCodeAndHandle());
        }
    }

    /// <summary>
    /// Check if current inputBuffer equals correctCode.
    /// </summary>
    private bool IsCodeCorrect()
    {
        if (correctCode.Length != 4) return false;

        for (int i = 0; i < 4; i++)
        {
            if (inputBuffer[i] != correctCode[i])
                return false;
        }
        StateManager.Instance.AddTruthByNum(20);
        return true;
    }

    private IEnumerator CheckCodeAndHandle()
    {
        isLocked = true;

        // small delay for UX (optional)
        yield return new WaitForSeconds(0.1f);

        if (IsCodeCorrect())
        {
            device.Goto(next);
        }
        else
        {
            ResetInput();
        }

        isLocked = false;
    }

    /// <summary>
    /// Clear buffer and circles.
    /// </summary>
    public void ResetInput()
    {
        inputIndex = 0;

        for (int i = 0; i < inputBuffer.Length; i++)
            inputBuffer[i] = 0;

        // reset circle UI
        if (circles != null)
        {
            for (int i = 0; i < circles.Length; i++)
            {
                if (circles[i] != null)
                {
                    circles[i].color = emptyColor;
                }
            }
        }
    }

    /// <summary>
    /// Update a single circle color.
    /// </summary>
    private void UpdateCircles(int index, bool filled)
    {
        if (circles == null) return;
        if (index < 0 || index >= circles.Length) return;
        if (circles[index] == null) return;

        circles[index].color = filled ? filledColor : emptyColor;
    }
}
