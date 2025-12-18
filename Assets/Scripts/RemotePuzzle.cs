using System.Collections;
using UnityEngine;
using TMPro;

public class RemotePuzzle : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text channelText;

    [Header("TV Screen (Quad)")]
    [SerializeField] private Renderer screenRenderer;

    [Header("Base Material (required)")]
    [SerializeField] private Material screenBaseMaterial;

    [Header("Channel Textures (jpg/png)")]
    [SerializeField] private Texture2D noiseTexture;
    [SerializeField] private Texture2D channel011Texture;
    [SerializeField] private Texture2D channel235Texture;
    [SerializeField] private Texture2D channel199Texture;

    [Header("Input Settings")]
    [SerializeField] private int digitsRequired = 3;

    // 3자리 입력이 완료된 뒤, 다음 입력이 오면 새 입력으로 시작하기 위한 플래그
    private bool readyForNextEntry = false;

    // "입력 버퍼"는 반드시 빈 문자열로 시작 (초기 011 때문에 첫 입력 씹히는 문제 방지)
    private string currentInput = "";

    private void Awake()
    {
        // 버튼 연결
        var buttons = GetComponentsInChildren<RemoteButton>(true);
        foreach (var btn in buttons)
        {
            btn.Init(this);
        }

        // 머터리얼 세팅
        if (screenRenderer != null && screenBaseMaterial != null)
        {
            screenRenderer.material = screenBaseMaterial;
        }

        // 초기 상태: 텍스트는 011로 보이고, 화면도 011로 시작
        SetDisplayedChannel("011");
        ResolveChannel("011");
    }

    public void OnDigitPressed(int digit)
    {
        if (digit < 0 || digit > 9) return;

        // (중요) 이전에 3자리 입력이 완료되어 텍스트를 유지 중이면,
        // 다음 입력이 들어오는 순간에만 새 입력을 시작합니다.
        if (readyForNextEntry)
        {
            readyForNextEntry = false;
            currentInput = "";
        }

        // 0~2자리까지만 누적 입력
        if (currentInput.Length < digitsRequired)
        {
            currentInput += digit.ToString();
            SetDisplayedChannel(currentInput);
        }

        // 3자리 완료 시 채널 반영 + 텍스트 유지
        if (currentInput.Length >= digitsRequired)
        {
            ResolveChannel(currentInput);
            readyForNextEntry = true;

            // 여기서 currentInput을 지우지 않습니다.
            // 다음 버튼 입력이 들어올 때만 지워서 새 입력을 시작합니다.
        }
    }

    private void ResolveChannel(string channel)
    {
        Texture2D target = noiseTexture;

        if (channel == "011") target = channel011Texture != null ? channel011Texture : noiseTexture;
        else if (channel == "235") target = channel235Texture != null ? channel235Texture : noiseTexture;
        else if (channel == "199") target = channel199Texture != null ? channel199Texture : noiseTexture;

        ApplyTexture(target);
    }

    private void ApplyTexture(Texture2D tex)
    {
        if (screenRenderer == null || tex == null) return;

        // screenRenderer.material: 인스턴스 머터리얼, 오브젝트별로 안전
        var mat = screenRenderer.material;
        mat.mainTexture = tex;
    }

    private void SetDisplayedChannel(string text)
    {
        if (channelText == null) return;
        channelText.text = text;
    }
}
