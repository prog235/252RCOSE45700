using UnityEngine;
using UnityEngine.Splines;

/// 빈 오브젝트에 붙이면 반지름(radius)의 '원형' 스플라인을 1개 생성/갱신
public class BuildCircleSpline : MonoBehaviour
{
    [SerializeField] float radius = 5f;
    [SerializeField] bool replaceExisting = true;

    void Reset()      => Create();
    void OnValidate() { if (!Application.isPlaying) Create(); }

    void Create()
    {
        var container = GetComponent<SplineContainer>();
        if (!container) container = gameObject.AddComponent<SplineContainer>();

        // Get or create the first spline
        Spline spline;
        if (replaceExisting || container.Splines.Count == 0)
        {
            if (container.Splines.Count == 0)
                spline = container.AddSpline();
            else
                spline = container.Splines[0];

            spline.Clear();
        }
        else
        {
            spline = container.Splines[0];
        }

        spline.Closed = true;

        // circle cubic-bezier coefficient
        const float k = 0.5522847498307936f;
        float R = Mathf.Max(0.0001f, radius);

        // Place 4 knots at room-corner directions: 45°, 135°, 225°, 315°
        // Angle on XZ-plane: pos = (R*cosθ, 0, R*sinθ)
        // TangentOut along the circle direction: t = k*R * (-sinθ, 0, cosθ)
        float startRad = Mathf.PI * 0.25f; // 45 degrees
        for (int i = 0; i < 4; i++)
        {
            float theta = startRad + i * Mathf.PI * 0.5f; // +90° each step
            float c = Mathf.Cos(theta);
            float s = Mathf.Sin(theta);

            Vector3 p = new Vector3(R * c, 0f, R * s);
            Vector3 tOut = new Vector3(-s * k * R, 0f, c * k * R);

            var knot = new BezierKnot(p)
            {
                // symmetric handles for a smooth circular arc
                TangentOut = tOut,
                TangentIn  = -tOut
            };
            spline.Add(knot);
        }
    }
}
