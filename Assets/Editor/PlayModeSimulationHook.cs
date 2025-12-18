#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PlayModeSimulationHook
{
    static PlayModeSimulationHook()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Play 모드로 들어갔을 때
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("[PlayModeSimulationHook] /simulation/start");
            SimulationHttp.PostJsonFireAndForget("/simulation/start");
        }

        // Play 모드에서 나가기 직전(Stop 누른 순간)
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Debug.Log("[PlayModeSimulationHook] /simulation/end");
            SimulationHttp.PostJsonFireAndForget("/simulation/end");
        }
    }
}
#endif
