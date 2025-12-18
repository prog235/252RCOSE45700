using UnityEngine;
using UnityEngine.UI;
using System;

public class LeverToggle : MonoBehaviour
{
    public event Action<int> OnLeverChanged;
    public bool isUp = false;
    private int index;
    private RectTransform rect;

    [SerializeField] private float moveDistance = 105f; // 위아래 이동 거리
    [SerializeField] private float moveSpeed = 0.1f;

    private void Awake()
    {
        index = GetIndexFromName(gameObject.name) - 1;
        rect = GetComponent<RectTransform>();
        GetComponent<Button>().onClick.AddListener(ToggleLever);
    }

    private void ToggleLever()
    {
        isUp = !isUp;
        StopAllCoroutines();
        StartCoroutine(MoveLever(isUp ? -moveDistance : moveDistance));
        OnLeverChanged?.Invoke(index);
    }

    private System.Collections.IEnumerator MoveLever(float targetOffset)
    {
        Vector2 start = rect.anchoredPosition;
        Vector2 end = new Vector2(start.x, start.y + targetOffset);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveSpeed;
            rect.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }
    }

    private int GetIndexFromName(string name)
    {
        string numStr = name.Replace("Lever", "");
        return int.Parse(numStr);
    }

}
