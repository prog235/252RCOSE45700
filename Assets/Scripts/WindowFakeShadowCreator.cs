using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WindowFakeShadowCreator : MonoBehaviour
{
    [Header("Parent for shadow-only copies (FakeWalls)")]
    public Transform fakeWallsParent;

    [Header("Optional shadow-only material")]
    public Material shadowOnlyMaterial;

    private Dictionary<Transform, GameObject> shadowMap =
        new Dictionary<Transform, GameObject>();

    // 기본 Cube Mesh 캐싱 (한 번만 로드)
    private Mesh cubeMesh;

    void Awake()
    {
        // Unity 기본 Cube mesh 불러오기
        cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
    }

    public void SpawnShadowFor(Transform source)
    {
        if (source == null || fakeWallsParent == null)
            return;

        // 기존 그림자 있으면 삭제
        if (shadowMap.TryGetValue(source, out GameObject oldShadow))
        {
            if (oldShadow != null)
                Destroy(oldShadow);

            shadowMap.Remove(source);
        }

        // 원본 복제 생성
         // 1) 부모 없이 복제본 생성
        GameObject shadow = Instantiate(source.gameObject);

        // 2) 원본과 동일한 월드 변환 적용
        shadow.transform.position   = source.position + new Vector3(0.5f, 0f, 0f);
        shadow.transform.rotation   = source.rotation;
        shadow.transform.localScale = source.lossyScale;

        const float thickness = 0.001f;  

        Vector3 s = shadow.transform.localScale;
        s.x = thickness;                   
        shadow.transform.localScale = s;

        // 3) 부모를 FakeWalls로 옮기되, 월드 좌표는 유지
        shadow.transform.SetParent(fakeWallsParent, true); // true = worldPositionStays


        // ------------------------------
        //  MeshFilter를 Cube로 교체 (핵심 추가 부분)
        // ------------------------------

        MeshFilter mf = shadow.GetComponent<MeshFilter>();
        if (mf != null && cubeMesh != null)
        {
            mf.mesh = cubeMesh;
        }

        MeshFilter[] allMF = shadow.GetComponentsInChildren<MeshFilter>(true);
        foreach (var m in allMF)
        {
            m.mesh = cubeMesh;
        }

        // ------------------------------
        // Renderer 설정 (Shadows Only)
        // ------------------------------
        Renderer[] renderers = shadow.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            r.shadowCastingMode = ShadowCastingMode.ShadowsOnly;

            if (shadowOnlyMaterial != null)
                r.material = shadowOnlyMaterial;
        }

        // Collider 비활성화
        Collider[] colliders = shadow.GetComponentsInChildren<Collider>(true);
        foreach (Collider c in colliders)
            c.enabled = false;

        // 매핑 저장
        shadowMap[source] = shadow;
    }

    public void RemoveShadowFor(Transform source)
    {
        if (source == null)
            return;

        if (shadowMap.TryGetValue(source, out GameObject shadow))
        {
            if (shadow != null)
                Destroy(shadow);

            shadowMap.Remove(source);
        }
    }
}
