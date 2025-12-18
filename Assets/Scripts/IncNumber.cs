using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class IncNumber : MonoBehaviour
{
    public int cur;
    private TextMeshProUGUI numText;
    public event Action<int> OnNumberChanged;
    private int index;

    void Awake()
    {
        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(OnClick);
        numText = GetComponentInChildren<TextMeshProUGUI>();
        index = GetIndexFromName(gameObject.name) - 1;
        cur = 0;
        numText.text = cur.ToString();   
    }


    private void OnClick()
    {
        cur += 1;
        if (cur == 10) cur = 0;
        numText.text = cur.ToString();
        OnNumberChanged?.Invoke(index);
        AudioManager.Instance.PlayButton();
    }

    private int GetIndexFromName(string name)
    {
        string numStr = name.Replace("Number", "");
        return int.Parse(numStr);
    }
}
