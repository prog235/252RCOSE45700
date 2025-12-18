using UnityEngine;
public class MatProbe : MonoBehaviour
{
    void OnEnable()
    {
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            foreach (var m in r.sharedMaterials)
            {
                if (!m) continue;
                Debug.Log($"[MatProbe] {r.name} -> {m.name} / shader:{m.shader.name} " +
                          $"has _BaseColor:{m.HasProperty("_BaseColor")} _Color:{m.HasProperty("_Color")} " +
                          $"_Surface:{(m.HasProperty("_Surface")? m.GetFloat("_Surface").ToString():"-")} " +
                          $"AlphaClip:{(m.HasProperty("_AlphaClip")? m.GetFloat("_AlphaClip").ToString():"-")}");
            }
        }
    }
}
