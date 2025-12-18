using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;      // controls whole overlay alpha
    [SerializeField] private RawImage noiseImage;          // animated static frames
    [SerializeField] private RawImage scanlineImage;       // subtle scanlines
    [SerializeField] private Image whiteFlashImage;        // 1-frame flash

    [Header("Noise Frames (Texture2D)")]
    [SerializeField] private Texture2D[] noiseFrames;

    [Header("Timing (Memory cut style)")]
    [SerializeField] private float fadeInDuration = 0.10f;     // faster
    [SerializeField] private float peakHoldDuration = 0.08f;   // short hold at full static
    [SerializeField] private float fadeOutDuration = 0.18f;

    [Header("Noise Animation")]
    [SerializeField] private float fps = 24f;

    [Header("Scanlines")]
    [SerializeField] private float scanlineScrollSpeed = 0.25f;
    [Range(0f, 1f)]
    [SerializeField] private float scanlineAlpha = 0.12f;

    [Header("Jitter (subtle)")]
    [SerializeField] private float jitterPixels = 6f;          // small UI shake
    [SerializeField] private float jitterFrequency = 28f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip staticSfx;
    [Range(0f, 1f)]
    [SerializeField] private float staticVolume = 0.65f;

    private bool isTransitioning = false;
    private Coroutine noiseRoutine;
    private Coroutine jitterRoutine;

    private RectTransform noiseRect;
    private Vector2 noiseBaseAnchoredPos;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;


        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (scanlineImage != null)
        {
            var c = scanlineImage.color;
            c.a = scanlineAlpha;
            scanlineImage.color = c;
        }

        if (whiteFlashImage != null)
        {
            var c = whiteFlashImage.color;
            c.a = 0f;
            whiteFlashImage.color = c;
        }

        if (noiseImage != null)
        {
            noiseRect = noiseImage.GetComponent<RectTransform>();
            noiseBaseAnchoredPos = noiseRect.anchoredPosition;
        }
    }

    public void LoadSceneWithStatic(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;

        StartNoise();
        StartJitter();
        PlayStaticSfx();

        // Fade in quickly
        yield return FadeCanvas(0f, 1f, fadeInDuration);

        // 1-frame white flash at peak
        yield return WhiteFlashOneFrame();

        // Begin async load, keep control
        bool loadedCallbackFired = false;
        SceneManager.sceneLoaded += OnSceneLoaded;

        void OnSceneLoaded(Scene s, LoadSceneMode mode)
        {
            if (s.name == sceneName)
                loadedCallbackFired = true;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        // Peak hold (very short)
        if (peakHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(peakHoldDuration);

        // Wait until ready (0.9f = "ready to activate")
        while (op.progress < 0.9f)
            yield return null;

        // Activate while still fully covered by static
        op.allowSceneActivation = true;

        // 1) Wait for sceneLoaded event (new scene actually entered)
        while (!loadedCallbackFired)
            yield return null;

        // 2) Give the new scene a couple frames to render & run initial Awake/Start
        //    (This is the 핵심: hitch가 있어도 오버레이가 가려줌)
        yield return null;
        yield return new WaitForEndOfFrame();
        yield return null;

        // Clean up event
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Fade out static overlay (now the scene should already be "there")
        yield return FadeCanvas(1f, 0f, fadeOutDuration);

        StopJitter();
        StopNoise();
        ResetNoisePosition();

        isTransitioning = false;
    }

    private IEnumerator WhiteFlashOneFrame()
    {
        if (whiteFlashImage == null)
            yield break;

        var c = whiteFlashImage.color;

        // show
        c.a = 0.9f;
        whiteFlashImage.color = c;

        // one frame
        yield return null;

        // hide quickly
        c.a = 0f;
        whiteFlashImage.color = c;
    }

    private void StartNoise()
    {
        if (noiseRoutine != null) StopCoroutine(noiseRoutine);
        noiseRoutine = StartCoroutine(NoiseAnimationRoutine());
    }

    private void StopNoise()
    {
        if (noiseRoutine != null)
        {
            StopCoroutine(noiseRoutine);
            noiseRoutine = null;
        }
    }

    private IEnumerator NoiseAnimationRoutine()
    {
        if (noiseFrames == null || noiseFrames.Length == 0 || noiseImage == null)
            yield break;

        float interval = (fps <= 0f) ? 0.05f : 1f / fps;
        int idx = 0;

        float scanT = 0f;

        while (true)
        {
            // static frame swap
            noiseImage.texture = noiseFrames[idx];
            idx = (idx + 1) % noiseFrames.Length;

            // scanline subtle scroll
            if (scanlineImage != null)
            {
                scanT += scanlineScrollSpeed * interval;
                scanlineImage.uvRect = new Rect(0f, scanT, 1f, 1f);
            }

            yield return new WaitForSecondsRealtime(interval);
        }
    }

    private void StartJitter()
    {
        if (jitterRoutine != null) StopCoroutine(jitterRoutine);
        if (noiseRect == null) return;
        jitterRoutine = StartCoroutine(JitterRoutine());
    }

    private void StopJitter()
    {
        if (jitterRoutine != null)
        {
            StopCoroutine(jitterRoutine);
            jitterRoutine = null;
        }
    }

    private IEnumerator JitterRoutine()
    {
        if (noiseRect == null) yield break;

        float interval = 1f / Mathf.Max(1f, jitterFrequency);

        while (true)
        {
            float x = Random.Range(-jitterPixels, jitterPixels);
            float y = Random.Range(-jitterPixels, jitterPixels);
            noiseRect.anchoredPosition = noiseBaseAnchoredPos + new Vector2(x, y);
            yield return new WaitForSecondsRealtime(interval);
        }
    }

    private void ResetNoisePosition()
    {
        if (noiseRect != null)
            noiseRect.anchoredPosition = noiseBaseAnchoredPos;
    }

    private void PlayStaticSfx()
    {
        if (audioSource == null || staticSfx == null) return;
        audioSource.clip = staticSfx;
        audioSource.loop = false;
        audioSource.volume = staticVolume;
        audioSource.Play();
    }

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float t = 0f;
        canvasGroup.alpha = from;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
