using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CellphoneButton : MonoBehaviour
{
    [Range(0, 9)]
    [SerializeField] private int digit = 0;   // this button's number

    private CellphonePuzzle ownerPuzzle;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);
    }

    /// <summary>
    /// Called from CellphonePuzzle to inject reference.
    /// </summary>
    public void Init(CellphonePuzzle puzzle)
    {
        ownerPuzzle = puzzle;
    }

    private void HandleClick()
    {
        if (ownerPuzzle == null)
        {
            Debug.LogWarning($"CellphoneButton {name}: ownerPuzzle is null.");
            return;
        }

        // send digit to puzzle
        ownerPuzzle.OnDigitPressed(digit);
    }
}
