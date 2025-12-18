using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine.Splines; 

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    public event Action ZoomIn;
    public event Action ZoomOut;

    [SerializeField] List<CinemachineCamera> cams = new();
    [SerializeField] int activePriority = 20, inactivePriority = 10;
    [SerializeField] List<CrossFader> faders = new();
    [SerializeField] List<WallRenderQueue> walls = new();
    

    [Header("Rotate Settings")]
    [SerializeField] private float rotateDuration = 1.2f;
    [SerializeField, Tooltip("0.5 (normalized) == 180° along the spline")]
    private float halfTurnDelta = 0.5f;
    private int cur;
    private bool isRotating;
    private CinemachineCamera ZoomInCam;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }
        Instance = this; 

        foreach (var cam in cams) cam.Priority = inactivePriority;
        if (cams.Count > 0)
        {
            cur = 0;
            cams[0].Priority = activePriority;
        }
    }

    public void Rotate()
    {
        if (!isRotating)
        {
            StartCoroutine(RotateHalfTurn());
            faders[cur].Fade();
        }
    }

    public void LookAt(CinemachineCamera cam)
    {
        cam.Priority = activePriority;
        cams[cur].Priority = inactivePriority;
        ZoomIn?.Invoke();
        ZoomInCam = cam;
    }

    public void BackToRoom()
    {
        cams[cur].Priority = activePriority;
        ZoomInCam.Priority = inactivePriority;
        ZoomOut?.Invoke();
    }

    public void SwitchTo(int idx)
    {
        cams[cur].Priority = inactivePriority;
        cur = idx;
        cams[cur].Priority = activePriority;
    }

    private IEnumerator RotateHalfTurn()
    {
        if (cams == null || cams.Count == 0) yield break;

        var cam = cams[cur];
        if (cam == null) yield break;

        var dolly = cam.GetComponent<CinemachineSplineDolly>();
        if (dolly == null)
        {
            Debug.LogWarning("[CineSwitcher] CinemachineSplineDolly not found on active camera.");
            yield break;
        }

        // CM3: PositionUnits는 PathIndexUnit, 위치는 CameraPosition을 사용
        if (dolly.PositionUnits != PathIndexUnit.Normalized)
            dolly.PositionUnits = PathIndexUnit.Normalized;  // 값 보존하며 단위만 변경

        isRotating = true;

        float start = dolly.CameraPosition;                 // [0..1]
        float end = Mathf.Repeat(start + halfTurnDelta, 1f);
        float t = 0f;
        float prevPos = start;

        while (t < rotateDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / rotateDuration);

            float pos = start + halfTurnDelta * u;
            dolly.CameraPosition = Mathf.Repeat(pos, 1f);

            if (prevPos < 0.125 && pos > 0.125)
                walls[cur].RenderAtPos(0);

            if (prevPos < 0.375 && pos > 0.375)
                walls[cur].RenderAtPos(1);

            if (prevPos < 0.625 && pos > 0.625)
                walls[cur].RenderAtPos(2);

            if (prevPos < 0.875 && pos > 0.875)
                walls[cur].RenderAtPos(3);

            prevPos = pos;

            yield return null;
        }

        dolly.CameraPosition = end;
        isRotating = false;
    }
}
