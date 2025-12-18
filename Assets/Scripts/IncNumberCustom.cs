using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class IncNumberCustom : MonoBehaviour
{
    public int cur;
    private TextMeshProUGUI numText;
    public event Action<int> OnNumberChanged;
    private int index;
    public int max;

    void Awake()
    {
        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(OnClick);
        numText = GetComponentInChildren<TextMeshProUGUI>();
        index = GetIndexFromName(gameObject.name) - 1;
        cur = int.Parse(numText.text);
        numText.text = cur.ToString();   
    }


    private void OnClick()
    {
        cur += 1;
        if (cur == max) cur = 0;
        numText.text = cur.ToString();
        OnNumberChanged?.Invoke(index);
        AudioManager.Instance.PlayClockButton();
    }

    private int GetIndexFromName(string name)
    {
        string numStr = name.Replace("Number", "");
        return int.Parse(numStr);
    }
}
