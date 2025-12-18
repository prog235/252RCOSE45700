#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SelectionSnapper
{
    [MenuItem("Tools/Snap Selected To Ground")]
    public static void SnapSelected()
    {
        foreach (var obj in Selection.transforms)
        {
            var tr = obj;
            var origin = tr.position;

            // 아래로 Raycast
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 200f, ~0, QueryTriggerInteraction.Ignore))
            {
                float offset = 0f;

                // 콜라이더 높이 보정 (Pivot이 중앙에 있을 때)
                if (tr.TryGetComponent(out Collider col))
                    offset += col.bounds.extents.y;

                // Undo 등록 (Ctrl+Z 가능)
                Undo.RecordObject(tr, "Snap Selected To Ground");

                // 위치 이동
                tr.position = hit.point + Vector3.up * offset;

                EditorUtility.SetDirty(tr);
            }
            else
            {
                Debug.LogWarning($"[{tr.name}] No ground found below.");
            }
        }
    }
}
#endif
