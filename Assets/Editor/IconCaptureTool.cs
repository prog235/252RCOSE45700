// Assets/Editor/IconCaptureTool.cs
// Unity Editor-only tool to capture a 3D object's "front" icon as a transparent PNG and import it as a Sprite.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class IconCaptureTool : EditorWindow
{
    // -------- UI fields --------
    [SerializeField] private GameObject target;      // Target root object to capture
    [SerializeField] private bool orthographic = true;
    [SerializeField] private int width = 512;
    [SerializeField] private int height = 512;
    [SerializeField] private float paddingPercent = 10f; // extra padding around bounds (% of max XY)
    [SerializeField] private float perspectiveFov = 30f; // used when orthographic == false
    [SerializeField] private Color background = new Color(0, 0, 0, 0); // fully transparent
    [SerializeField] private bool addTempLight = true; // simple neutral light if scene is dark
    [SerializeField] private string saveFolder = "Assets/Icons";
    [SerializeField] private string fileName = "icon.png";
    [SerializeField] private float yawOffset = 0f;   // rotate around Y (degrees)
    [SerializeField] private float pitchOffset = 0f; // rotate around X (degrees)

    // Temporary layer index used for isolation (31 is often free; mask works even if layer name is empty)
    private const int TempLayer = 31;

    [MenuItem("Tools/Icon Capture Tool")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<IconCaptureTool>("Icon Capture Tool");
        wnd.minSize = new Vector2(360, 380);
        wnd.target = Selection.activeGameObject;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Target & Output", EditorStyles.boldLabel);
        target = (GameObject)EditorGUILayout.ObjectField("Target", target, typeof(GameObject), true);

        orthographic = EditorGUILayout.Toggle("Orthographic", orthographic);
        using (new EditorGUILayout.HorizontalScope())
        {
            width = EditorGUILayout.IntField("Width", Mathf.Max(8, width));
            height = EditorGUILayout.IntField("Height", Mathf.Max(8, height));
        }
        paddingPercent = EditorGUILayout.Slider("Padding (%)", paddingPercent, 0f, 50f);
        if (!orthographic)
            perspectiveFov = EditorGUILayout.Slider("Perspective FOV", perspectiveFov, 10f, 70f);

        yawOffset = EditorGUILayout.Slider("Yaw Offset (Y°)", yawOffset, -180f, 180f);
        pitchOffset = EditorGUILayout.Slider("Pitch Offset (X°)", pitchOffset, -80f, 80f);

        background = EditorGUILayout.ColorField("Background (A=0)", background);
        addTempLight = EditorGUILayout.Toggle("Add Temp Light", addTempLight);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Save As", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PrefixLabel("Folder");
            if (GUILayout.Button(saveFolder, EditorStyles.objectField))
            {
                var picked = EditorUtility.OpenFolderPanel("Select Save Folder (inside Assets)", Application.dataPath, "");
                if (!string.IsNullOrEmpty(picked))
                {
                    // Convert absolute path under project to relative Assets/ path
                    var projPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                    var rel = "Assets" + picked.Replace(projPath, "").Replace("\\", "/");
                    if (rel.StartsWith("Assets"))
                        saveFolder = rel;
                    else
                        EditorUtility.DisplayDialog("Invalid Folder", "Please select a folder under the project Assets.", "OK");
                }
            }
        }
        fileName = EditorGUILayout.TextField("File Name (.png)", fileName);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(target == null))
        {
            if (GUILayout.Button("Capture & Save", GUILayout.Height(36)))
            {
                try
                {
                    CaptureAndSave();
                }
                catch (Exception e)
                {
                    Debug.LogError($"IconCaptureTool: Capture failed.\n{e}");
                }
            }
        }

        EditorGUILayout.HelpBox(
            "Front = +Z of the target. Adjust yaw/pitch offsets if your model's front differs.\n" +
            "The tool isolates the target by moving it to a temporary layer during capture (restored after).",
            MessageType.Info);
    }

    // -------- Core capture pipeline --------
    private void CaptureAndSave()
    {
        if (target == null)
        {
            Debug.LogError("IconCaptureTool: Target is null.");
            return;
        }

        EnsureFolder(saveFolder);
        string savePath = $"{saveFolder}/{SanitizeFileName(fileName)}";
        if (!savePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            savePath += ".png";

        // 1) Compute renderable bounds (all Renderers)
        Bounds b;
        if (!TryCalcBounds(target, out b))
        {
            Debug.LogError("IconCaptureTool: Target has no Renderer. Nothing to capture.");
            return;
        }

        // 2) Create hidden temp camera (and optional light)
        var camGO = new GameObject("~IconCapture_Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.enabled = false;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = background; // A = 0 for transparency
        cam.allowHDR = true;
        cam.allowMSAA = true;
        cam.depthTextureMode = DepthTextureMode.None;
        camGO.hideFlags = HideFlags.HideAndDontSave;

        GameObject lightGO = null;
        if (addTempLight)
        {
            lightGO = new GameObject("~IconCapture_Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.None;
            lightGO.hideFlags = HideFlags.HideAndDontSave;
        }

        // 3) Isolate target by moving it (and children) to a temp layer, store original layers
        var layerRestore = new Dictionary<Transform, int>(64);
        try
        {
            SetLayerRecursive(target.transform, TempLayer, layerRestore);

            cam.cullingMask = 1 << TempLayer;

            // 4) Size/position camera based on bounds
            Vector3 center = b.center;
            Vector3 size = b.size;

            // Front = target's +Z, with user yaw/pitch offsets
            Quaternion rot = Quaternion.Euler(pitchOffset, yawOffset, 0f) * target.transform.rotation;
            Vector3 forward = rot * Vector3.forward;
            Vector3 up = rot * Vector3.up;

            if (orthographic)
            {
                cam.orthographic = true;

                float maxXY = Mathf.Max(size.x, size.y);
                float pad = maxXY * (paddingPercent * 0.01f);
                cam.orthographicSize = (maxXY * 0.5f) + pad;

                // Distance: far enough to include depth; use bounds depth plus small margin
                float depth = Mathf.Max(size.z, 0.01f);
                float dist = depth * 1.5f + 2f;

                cam.transform.position = center - forward * dist;
                cam.transform.rotation = Quaternion.LookRotation(forward, up);
            }
            else
            {
                cam.orthographic = false;
                cam.fieldOfView = perspectiveFov;

                float maxXY = Mathf.Max(size.x, size.y);
                float pad = maxXY * (paddingPercent * 0.01f);
                float half = (maxXY * 0.5f) + pad;
                float dist = half / Mathf.Tan(0.5f * cam.fieldOfView * Mathf.Deg2Rad);

                // Also push a bit more based on depth
                dist += size.z * 0.5f + 0.25f;

                cam.transform.position = center - forward * dist;
                cam.transform.rotation = Quaternion.LookRotation(forward, up);
            }

            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 1000f;

            if (lightGO != null)
            {
                lightGO.transform.rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(45f, 30f, 0f);
            }

            // 5) Render to transparent RT, read back to Texture2D
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            rt.antiAliasing = 4;
            rt.hideFlags = HideFlags.HideAndDontSave;

            var prevActive = RenderTexture.active;
            try
            {
                cam.targetTexture = rt;
                RenderTexture.active = rt;
                cam.Render();

                var tex = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                tex.Apply(false, false);

                // 6) Save as PNG into Assets and import as Sprite
                var png = tex.EncodeToPNG();
                File.WriteAllBytes(savePath, png);
                UnityEngine.Object.DestroyImmediate(tex);

                AssetDatabase.ImportAsset(savePath, ImportAssetOptions.ForceUpdate);
                var ti = (TextureImporter)AssetImporter.GetAtPath(savePath);
                if (ti != null)
                {
                    ti.textureType = TextureImporterType.Sprite;
                    ti.spriteImportMode = SpriteImportMode.Single;
                    ti.alphaIsTransparency = true;
                    ti.mipmapEnabled = false;
                    ti.sRGBTexture = true;
                    ti.filterMode = FilterMode.Bilinear;
                    ti.wrapMode = TextureWrapMode.Clamp;
                    ti.SaveAndReimport();
                }

                Debug.Log($"IconCaptureTool: Saved sprite → {savePath}");
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(savePath));
            }
            finally
            {
                cam.targetTexture = null;
                RenderTexture.active = prevActive;
                if (rt != null) rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }
        finally
        {
            // 7) Cleanup & restore layers
            RestoreLayers(layerRestore);

            if (cam != null) UnityEngine.Object.DestroyImmediate(cam.gameObject);
            if (lightGO != null) UnityEngine.Object.DestroyImmediate(lightGO);
        }
    }

    // -------- Helpers --------

    // Calculate combined bounds of all renderers under target
    private static bool TryCalcBounds(GameObject go, out Bounds b)
    {
        var rends = go.GetComponentsInChildren<Renderer>(true);
        if (rends == null || rends.Length == 0)
        {
            b = new Bounds(go.transform.position, Vector3.zero);
            return false;
        }

        b = new Bounds(rends[0].bounds.center, rends[0].bounds.size);
        for (int i = 1; i < rends.Length; i++)
            b.Encapsulate(rends[i].bounds);

        return true;
    }

    // Move target and children to a temp layer, storing original layers
    private static void SetLayerRecursive(Transform root, int layer, Dictionary<Transform, int> store)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            store[t] = t.gameObject.layer;
            t.gameObject.layer = layer;
        }
    }

    private static void RestoreLayers(Dictionary<Transform, int> store)
    {
        foreach (var kv in store)
        {
            if (kv.Key != null)
                kv.Key.gameObject.layer = kv.Value;
        }
        store.Clear();
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;

        // Create nested folders under Assets if needed
        var parts = folder.Replace("\\", "/").Split('/');
        if (parts.Length == 0 || parts[0] != "Assets")
            throw new Exception("Save folder must be under Assets/");

        string cur = "Assets";
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{cur}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "icon.png";
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c.ToString(), "_");
        return name;
    }
}
#endif
