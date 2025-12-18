using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization.Formatters;
using UnityEngine;

// ------------------------------------------------------
// Interface
// ------------------------------------------------------
public interface IFadeable
{
    void FadeIn(float duration);
    void FadeOut(float duration);
}

// ------------------------------------------------------
// GroupFader: attach on the *parent* GameObject (group root)
// - Fades all child Renderers' materials (alpha)
// - On FadeOut complete -> SetActive(false)
// - On FadeIn start      -> SetActive(true)
// ------------------------------------------------------
[DisallowMultipleComponent]
public class GroupFader : MonoBehaviour, IFadeable
{
    [Header("Default Settings")]
    [SerializeField] private float defaultDuration = 1.0f;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Cache Children On Awake")]
    [Tooltip("If true, cache child renderers at Awake; otherwise refresh at fade start.")]
    [SerializeField] private bool cacheOnAwake = true;

    private readonly List<Renderer> _renderers = new List<Renderer>();
    private readonly List<CanvasGroup> _canvasGroups = new List<CanvasGroup>();
    private Coroutine _fadeRoutine;

    void Awake()
    {
        if (cacheOnAwake)
            RefreshTargets();
    }

    // ------------------------------------------------------
    // IFadeable API
    // ------------------------------------------------------
    public void FadeIn(float duration) => StartFade(1f, duration <= 0f ? defaultDuration : duration, true);
    public void FadeOut(float duration) => StartFade(0f, duration <= 0f ? defaultDuration : duration, false);

    // ------------------------------------------------------
    // Public helpers for external use
    // ------------------------------------------------------
    public void FadeIn()  => FadeIn(defaultDuration);
    public void FadeOut() => FadeOut(defaultDuration);

    // ------------------------------------------------------
    // Core
    // ------------------------------------------------------
    private void StartFade(float targetAlpha, float duration, bool isFadeIn)
    {
        // If group is inactive and we need to fade in, activate it first
        if (isFadeIn && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            SetTransparentAll();
            SetAlphaAll(0f);
        }

        // (Re)collect targets if not cached
        if (!cacheOnAwake)
            RefreshTargets();

        // Stop previous fade
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        // Set transparent when fading out
        if (!isFadeIn)
            SetTransparentAll();

        // If fading in from inactive, many children may still be alpha=0; that's fine
        _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, duration, isFadeIn));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, bool isFadeIn)
    {   
        float startAlpha = GetCurrentAlphaOrDefault(isFadeIn ? 0f : 1f);
        float time = 0f;

        // Ensure renderers/groups exist
        if (_renderers.Count == 0 && _canvasGroups.Count == 0)
            RefreshTargets();

        // Guard: if still nothing to fade, just toggle active as requested
        if (_renderers.Count == 0 && _canvasGroups.Count == 0)
        {
            if (!isFadeIn) gameObject.SetActive(false);
            yield break;
        }
        
        // Smooth fade
        while (time < duration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            time += dt;
            float t = Mathf.Clamp01(time / duration);
            float a = Mathf.Lerp(startAlpha, targetAlpha, t);
            SetAlphaAll(a);
            yield return null;
        }

        // Final snap
        SetAlphaAll(targetAlpha);

        if (Mathf.Approximately(targetAlpha, 1f))
            SetOpaqueAll();
    
        // Finalize active state
        if (Mathf.Approximately(targetAlpha, 0f))
            gameObject.SetActive(false);

        _fadeRoutine = null;
    }

    // ------------------------------------------------------
    // Utilities
    // ------------------------------------------------------
    private void RefreshTargets()
    {
        _renderers.Clear();
        _canvasGroups.Clear();

        // includeInactive: true so we can prep even if children are disabled under active parent
        GetComponentsInChildren(true, _renderers);
        GetComponentsInChildren(true, _canvasGroups);

        // Optionally: remove our own Renderer/CanvasGroup if present but undesired
        // (Usually fine to keep them.)
    }

    private float GetCurrentAlphaOrDefault(float fallback)
    {
        if (_canvasGroups.Count > 0) return Mathf.Clamp01(_canvasGroups[0].alpha);

        foreach (var r in _renderers)
        {
            if (!RendererIsValid(r)) continue;
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i]; if (!m) continue;

                if (m.HasProperty("_BaseColor"))
                    return Mathf.Clamp01(m.GetColor("_BaseColor").a);
                if (m.HasProperty("_Color"))
                    return Mathf.Clamp01(m.color.a);
            }
        }
        return Mathf.Clamp01(fallback);
    }


    private void SetAlphaAll(float a)
    {
        // CanvasGroup
        for (int i = 0; i < _canvasGroups.Count; i++)
            if (_canvasGroups[i]) _canvasGroups[i].alpha = a;

        // Renderers
        for (int rIdx = 0; rIdx < _renderers.Count; rIdx++)
        {
            var r = _renderers[rIdx];
            if (!RendererIsValid(r)) continue;

            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i]; if (!m) continue;

                if (m.HasProperty("_BaseColor"))
                {
                    var c = m.GetColor("_BaseColor");
                    c.a = a;
                    m.SetColor("_BaseColor", c);
                }
                else if (m.HasProperty("_Color"))
                {
                    var c = m.color;
                    c.a = a;
                    m.color = c;
                }
            }
        }
    }


    private void SetTransparentAll()
    {
        foreach (var r in _renderers)
        {
            if (!RendererIsValid(r)) continue;
            foreach (var m in r.materials)
                SetMaterialRenderMode(m, true);
        }
    }

    private void SetOpaqueAll()
    {
        foreach (var r in _renderers)
        {
            if (!RendererIsValid(r)) continue;
            foreach (var m in r.materials)
                SetMaterialRenderMode(m, false);
        }
    }

    private void SetMaterialRenderMode(Material mat, bool transparent)
    {
        bool isWall = mat.name.Contains("Wall");

        if (transparent)
        {
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            if (!isWall)
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + 100;
        }
        else
        {
            mat.SetFloat("_Surface", 0); // Opaque
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
        }
    }

    
    private bool RendererIsValid(Renderer r)
    {
        if (r == null) return false;
        // Skip hidden/Editor-only or non-visible? Usually not necessary.
        return true;
    }
    
}
