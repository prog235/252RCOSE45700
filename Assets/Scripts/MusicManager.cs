using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public string name;
        public AudioSource source;
        public float threshold;      // truthLevel이 이 값 이상이면 활성화
        public float targetVolume = 1f;
        public float fadeSeconds = 4f;
    }

    [Header("BGM Layers (all same length, loopable)")]
    [SerializeField] private List<Layer> layers = new List<Layer>();

    [Header("Sync")]
    [SerializeField] private bool startAllLayersMutedOnAwake = true;

    private readonly HashSet<int> _activated = new HashSet<int>();

    private void Awake()
    {
        if (startAllLayersMutedOnAwake)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                var s = layers[i].source;
                if (s == null) continue;

                s.loop = true;
                s.volume = 0f;

                // 싱크를 위해 전부 동시에 재생 시작
                if (!s.isPlaying) s.Play();
            }
        }
    }

    private void Start()
    {
        if (StateManager.Instance != null)
        {
            StateManager.Instance.OnTruthChanged += HandleTruthChanged;
            // 시작 시점 truthLevel 기준으로도 반영
            HandleTruthChanged(StateManager.Instance.truthSum);
        }
    }

    private void OnDisable()
    {
        if (StateManager.Instance != null)
        {
            StateManager.Instance.OnTruthChanged -= HandleTruthChanged;
        }
    }

    private void HandleTruthChanged(int truthLevel)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            var layer = layers[i];
            if (layer.source == null) continue;

            // 이미 켠 레이어는 다시 처리하지 않음
            if (_activated.Contains(i)) continue;

            if (truthLevel >= layer.threshold)
            {
                _activated.Add(i);

                // 혹시 아직 재생 안 하고 있었다면 시작 (안전장치)
                if (!layer.source.isPlaying)
                {
                    layer.source.loop = true;
                    layer.source.volume = 0f;
                    layer.source.Play();
                }

                StartCoroutine(FadeIn(layer.source, layer.targetVolume, layer.fadeSeconds));
            }
        }
    }

    private IEnumerator FadeIn(AudioSource src, float targetVolume, float seconds)
    {
        if (seconds <= 0f)
        {
            src.volume = targetVolume;
            yield break;
        }

        float start = src.volume;
        float t = 0f;

        while (t < seconds)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / seconds);
            src.volume = Mathf.Lerp(start, targetVolume, k);
            yield return null;
        }

        src.volume = targetVolume;
    }
}
