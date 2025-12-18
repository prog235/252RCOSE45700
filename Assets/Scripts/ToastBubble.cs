using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

[DisallowMultipleComponent]
public class ToastBubble : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text messageText;     // Child TMP
    [SerializeField] private CanvasGroup canvasGroup;  // Fade controller
    [SerializeField] private RectTransform rect;       // For slight move

    [Header("Timing (seconds)")]
    [SerializeField] private float fadeIn = 0.15f;
    [SerializeField] private float hold = 1.20f;
    [SerializeField] private float fadeOut = 0.25f;

    [Header("Motion")]
    [SerializeField] private float appearOffsetY = 8f; // Move up while fading
    [SerializeField] private float disappearOffsetY = 8f;

    private Vector2 _baseAnchoredPos;

    private Coroutine _fadeCoroutine;

    private void Reset()
    {
        // Try to auto-assign references on add
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (messageText == null) messageText = GetComponentInChildren<TMP_Text>(true);
    }

    private void Awake()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _baseAnchoredPos = rect.anchoredPosition;

        // Initialize invisible
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Play toast: set text, fade in, hold, fade out, then destroy.
    /// </summary>
    public void Play(string msg)
    {
        if (messageText != null) messageText.text = msg;
        StopAllCoroutines();
        StartCoroutine(CoPlay());
    }

    private IEnumerator CoPlay()
    {
        // Fade In
        float t = 0f;
        rect.anchoredPosition = _baseAnchoredPos + new Vector2(0f, -appearOffsetY);
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeIn);
            canvasGroup.alpha = k;
            rect.anchoredPosition = Vector2.Lerp(
                _baseAnchoredPos + new Vector2(0f, -appearOffsetY),
                _baseAnchoredPos,
                k
            );
            yield return null;
        }
        canvasGroup.alpha = 1f;
        rect.anchoredPosition = _baseAnchoredPos;

        // Hold
        t = 0f;
        while (t < hold)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // Fade Out
        t = 0f;
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, disappearOffsetY);
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeOut);
            canvasGroup.alpha = 1f - k;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, k);
            yield return null;
        }

        // Destroy after animation
        Destroy(gameObject);
    }

    public void AppendText(string token)
    {
        if (messageText.text == "New Text") messageText.text = "";
        if (messageText != null) messageText.text += token;

        // FadeIn 시작, 한 번만
        if (_fadeCoroutine == null)
        {
            _fadeCoroutine = StartCoroutine(CoFadeIn());
        }
    }

    public void Finish()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);
        StartCoroutine(CoFadeOut());
    }

    private IEnumerator CoFadeIn()
    {
        float t = 0f;
        rect.anchoredPosition = _baseAnchoredPos + new Vector2(0f, -appearOffsetY);
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeIn);
            canvasGroup.alpha = k;
            rect.anchoredPosition = Vector2.Lerp(
                _baseAnchoredPos + new Vector2(0f, -appearOffsetY),
                _baseAnchoredPos,
                k
            );
            yield return null;
        }
        canvasGroup.alpha = 1f;
        rect.anchoredPosition = _baseAnchoredPos;
    }

    private IEnumerator CoFadeOut()
    {
        float t = 0f;
        while (t < hold + 1.0)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        t = 0f;
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, disappearOffsetY);
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeOut);
            canvasGroup.alpha = 1f - k;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, k);
            yield return null;
        }
        Destroy(gameObject);
    }
}
