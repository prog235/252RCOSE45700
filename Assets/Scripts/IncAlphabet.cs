using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class IncAlphabet : MonoBehaviour
{
    public int cur;
    private string pattern = "ABEFH";
    private TextMeshProUGUI numText;
    public event Action OnAlpChanged;


    void Start()
    {
        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(OnClick);
        numText = GetComponentInChildren<TextMeshProUGUI>();
        cur = 0;
        numText.text = pattern[cur].ToString();
    }


    private void OnClick()
    {
        cur += 1;
        if (cur == 5) cur = 0;
        numText.text = pattern[cur].ToString();
        OnAlpChanged?.Invoke();
        AudioManager.Instance.PlayButton();
    }
}
