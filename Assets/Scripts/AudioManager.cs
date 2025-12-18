using System;
using System.Collections.Generic;
using UnityEngine;

public enum SfxId
{
    Lever,
    LightOn,
    Button,
    ToolboxOpen,
    WoodFall,
    PuzzleFail,
    ItemPickup,
    Door,
    Curtain,
    DoorUnlock,
    Box,
    Drawer,
    Book,
    DiaryOpen,
    DiaryClose,
    Velcro,
    RemoteButton,
    ClockButton,
    PC,
    Eraser,
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX Settings")]
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private bool sfxMuted = false;

    [Header("SFX Clips (assign in Inspector)")]
    [SerializeField] private List<SfxEntry> sfxClips = new();

    [Header("AudioSource")]
    [SerializeField] private AudioSource sfxSource;

    private Dictionary<SfxId, AudioClip> sfxDict;

    [Serializable]
    private class SfxEntry
    {
        public SfxId id;
        public AudioClip clip;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();
        }

        // SFX best practice: 2D, no spatial blend, play one-shots
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;

        BuildSfxDictionary();
    }

    private void BuildSfxDictionary()
    {
        sfxDict = new Dictionary<SfxId, AudioClip>();

        for (int i = 0; i < sfxClips.Count; i++)
        {
            var entry = sfxClips[i];
            if (entry == null) continue;

            if (sfxDict.ContainsKey(entry.id))
                continue;

            sfxDict.Add(entry.id, entry.clip);
        }
    }

    // -------------------------
    // Public API
    // -------------------------

    public void SetSfxVolume(float volume01)
    {
        sfxVolume = Mathf.Clamp01(volume01);
    }

    public void SetSfxMuted(bool muted)
    {
        sfxMuted = muted;
    }

    public void PlaySfx(SfxId id, float volumeScale = 1f, float pitch = 1f)
    {
        if (sfxMuted) return;
        if (sfxSource == null) return;

        if (sfxDict == null) BuildSfxDictionary();

        if (!sfxDict.TryGetValue(id, out var clip) || clip == null)
            return;

        float finalVolume = Mathf.Clamp01(sfxVolume * Mathf.Clamp01(volumeScale));

        // Temporarily set pitch for this one-shot
        float prevPitch = sfxSource.pitch;
        sfxSource.pitch = Mathf.Clamp(pitch, -3f, 3f);

        sfxSource.PlayOneShot(clip, finalVolume);

        sfxSource.pitch = prevPitch;
    }

    // Convenience methods (optional)
    public void PlayButton() => PlaySfx(SfxId.Button);
    public void PlayLever() => PlaySfx(SfxId.Lever);
    public void PlayLightOn() => PlaySfx(SfxId.LightOn);
    public void PlayToolboxOpen() => PlaySfx(SfxId.ToolboxOpen);
    public void PlayItemPickup() => PlaySfx(SfxId.ItemPickup);
    public void PlayWoodFall() => PlaySfx(SfxId.WoodFall);
    public void PlayDoor() => PlaySfx(SfxId.Door);
    public void PlayDoorUnlock() => PlaySfx(SfxId.DoorUnlock);
    public void PlayCurtain() => PlaySfx(SfxId.Curtain);
    public void PlayBox() => PlaySfx(SfxId.Box);
    public void PlayDrawer() => PlaySfx(SfxId.Drawer);
    public void PlayBook() => PlaySfx(SfxId.Book);
    public void PlayDiaryOpen() => PlaySfx(SfxId.DiaryOpen);
    public void PlayDiaryClose() => PlaySfx(SfxId.DiaryClose);
    public void PlayVelcro() => PlaySfx(SfxId.Velcro);
    public void PlayRemoteButton() => PlaySfx(SfxId.RemoteButton);
    public void PlayClockButton() => PlaySfx(SfxId.ClockButton);
    public void PlayPC() => PlaySfx(SfxId.PC);
    public void PlayEraser() => PlaySfx(SfxId.Eraser);
}

