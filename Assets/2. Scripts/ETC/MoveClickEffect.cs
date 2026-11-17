using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이동 명령 지점 표시 (이터널 리턴 참고). 텍스처 없이 메시를 만들어 그린다.
///   0.00s  빨간 원 + 하늘색 점선 링
///   ~0.15s 링이 조금 커지며 파랗게, 빨간 원은 작은 점으로 줄어듦
///   ~0.06s 링 바깥 세 방향(위, 왼쪽 아래, 오른쪽 아래)에서 파란 화살표가 가운데로 모임
///   ~0.12s 바닥에 보라색 육각형 격자가 퍼지며 나타남
///   끝     링·화살표·점이 먼저 사라지고 육각형이 마지막으로 흐려진 뒤 삭제
/// </summary>
public class MoveClickEffect : MonoBehaviour
{
    [Tooltip("정점 색을 쓰는 반투명 머티리얼 (Sprites-Default)")]
    [SerializeField] Material baseMaterial;
    [SerializeField] float duration = 0.9f;

    [Header("크기")]
    [SerializeField] float ringRadius = 1.0f;
    [SerializeField] float ringWidth = 0.09f;
    [SerializeField] float arrowSize = 0.6f;
    [SerializeField] float hexGridRadius = 1.9f;
    [SerializeField] float hexSize = 0.32f;
    [SerializeField] float hexLineWidth = 0.045f;

    [Header("색")]
    [SerializeField] Color discColor = new Color(1f, 0.3f, 0.3f, 0.9f);
    [SerializeField] Color ringStartColor = new Color(0.45f, 0.9f, 1f, 1f);
    [SerializeField] Color ringEndColor = new Color(0.35f, 0.6f, 1f, 0.9f);
    [SerializeField] Color arrowColor = new Color(0.45f, 0.72f, 1f, 1f);
    [SerializeField] Color hexColor = new Color(0.85f, 0.45f, 1f, 1f);

    // 모양이 같으니 모든 이펙트가 메시를 공유한다
    static Mesh discMesh, ringMesh, arrowMesh, hexMesh;

    Transform disc, ring, hex;
    readonly Transform[] arrows = new Transform[3];
    readonly float[] arrowAngles = { 90f, 210f, 330f }; // +Z(화면 위쪽) 기준 120도 간격
    readonly List<Material> materials = new List<Material>();
    Material discMat, ringMat, arrowMat, hexMat;
    float time;

    void Awake()
    {
        BuildSharedMeshes();

        hex = CreatePart("Hex", hexMesh, 0.010f, out hexMat);
        disc = CreatePart("Disc", discMesh, 0.015f, out discMat);
        ring = CreatePart("Ring", ringMesh, 0.020f, out ringMat);
        for (int i = 0; i < arrows.Length; i++)
            arrows[i] = CreatePart("Arrow", arrowMesh, 0.025f, out arrowMat, i > 0 ? arrowMat : null);

        Animate(0f);
    }

    void Update()
    {
        time += Time.deltaTime;
        if (time >= duration)
        {
            Destroy(gameObject);
            return;
        }
        Animate(time);
    }

    void OnDestroy()
    {
        foreach (var m in materials) if (m != null) Destroy(m);
    }

    /// <summary>특정 시점의 모습으로 바로 이동 (단계별 확인·미리보기용).</summary>
    public void Seek(float t)
    {
        time = Mathf.Clamp(t, 0f, duration - 0.001f);
        Animate(time);
    }

    void Animate(float t)
    {
        // 빨간 원 → 작은 점 → 사라짐
        float discScale = Mathf.Lerp(0.55f, 0.12f, Ease(t, 0f, 0.25f)) * ringRadius;
        disc.localScale = new Vector3(discScale, 1f, discScale);
        SetColor(discMat, discColor, 1f - Ease(t, 0.5f, 0.65f));

        // 링: 살짝 커지며 하늘색 → 파랑, 후반에 사라짐
        float r = Mathf.Lerp(0.7f, 1f, Ease(t, 0f, 0.15f)) * ringRadius;
        ring.localScale = new Vector3(r, 1f, r);
        Color ringColor = Color.Lerp(ringStartColor, ringEndColor, Ease(t, 0f, 0.2f));
        SetColor(ringMat, ringColor, 1f - Ease(t, 0.45f, 0.75f));

        // 화살표: 링 바깥에서 가운데로 모임
        float arrowRadius = Mathf.Lerp(1.35f, 0.55f, Ease(t, 0.06f, 0.65f)) * ringRadius;
        for (int i = 0; i < arrows.Length; i++)
        {
            float a = arrowAngles[i] * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            arrows[i].localPosition = dir * arrowRadius + Vector3.up * 0.025f;
            arrows[i].localRotation = Quaternion.LookRotation(-dir, Vector3.up); // 가운데를 향함
            arrows[i].localScale = Vector3.one * arrowSize;
        }
        SetColor(arrowMat, arrowColor, Ease(t, 0.06f, 0.15f) * (1f - Ease(t, 0.5f, 0.75f)));

        // 육각형 격자: 퍼지며 나타났다가 마지막에 흐려짐
        float hexScale = Mathf.Lerp(0.7f, 1f, Ease(t, 0.12f, 0.5f));
        hex.localScale = new Vector3(hexScale, 1f, hexScale);
        SetColor(hexMat, hexColor, Ease(t, 0.12f, 0.35f) * (1f - Ease(t, 0.6f, 0.9f)));
    }

    static float Ease(float t, float start, float end)
    {
        float x = Mathf.Clamp01((t - start) / (end - start));
        return 1f - (1f - x) * (1f - x); // ease-out
    }

    static void SetColor(Material mat, Color color, float alphaScale)
    {
        color.a *= Mathf.Clamp01(alphaScale);
        mat.color = color;
    }

    Transform CreatePart(string partName, Mesh mesh, float height, out Material mat, Material shared = null)
    {
        var go = new GameObject(partName);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.up * height;

        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        if (shared != null)
        {
            mat = shared;
        }
        else
        {
            mat = baseMaterial != null ? new Material(baseMaterial) : new Material(Shader.Find("Sprites/Default"));
            mat.renderQueue = 3000;
            materials.Add(mat);
        }
        mr.sharedMaterial = mat;
        return go.transform;
    }

    // ---------------- 메시 생성 (반지름 1 기준, 트랜스폼 스케일로 크기 조절) ----------------

    void BuildSharedMeshes()
    {
        if (discMesh == null) discMesh = BuildDisc(32);
        if (ringMesh == null) ringMesh = BuildDashedRing(20, 0.72f, ringWidth / ringRadius);
        if (arrowMesh == null) arrowMesh = BuildArrow();
        if (hexMesh == null) hexMesh = BuildHexGrid(hexGridRadius, hexSize, hexLineWidth);
    }

    static Mesh BuildDisc(int segments)
    {
        var verts = new List<Vector3> { Vector3.zero };
        var tris = new List<int>();
        for (int i = 0; i <= segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            verts.Add(new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)));
            if (i > 0) { tris.Add(0); tris.Add(i + 1); tris.Add(i); }
        }
        return Finish("ClickDisc", verts, tris, null);
    }

    /// <summary>점선 링: dashes개의 호, fill = 호가 차지하는 비율.</summary>
    static Mesh BuildDashedRing(int dashes, float fill, float width)
    {
        var verts = new List<Vector3>();
        var tris = new List<int>();
        const int sub = 4;
        float inner = 1f - width * 0.5f, outer = 1f + width * 0.5f;

        for (int d = 0; d < dashes; d++)
        {
            float start = d * Mathf.PI * 2f / dashes;
            float length = Mathf.PI * 2f / dashes * fill;
            int baseIndex = verts.Count;
            for (int s = 0; s <= sub; s++)
            {
                float a = start + length * s / sub;
                Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                verts.Add(dir * inner);
                verts.Add(dir * outer);
                if (s > 0)
                {
                    int i = baseIndex + s * 2;
                    tris.Add(i - 2); tris.Add(i - 1); tris.Add(i);
                    tris.Add(i - 1); tris.Add(i + 1); tris.Add(i);
                }
            }
        }
        return Finish("ClickRing", verts, tris, null);
    }

    /// <summary>+Z를 가리키는 화살표 (길이 1, 원점이 화살촉 끝).</summary>
    static Mesh BuildArrow()
    {
        var verts = new List<Vector3>
        {
            // 화살촉
            new Vector3(0f, 0f, 0f), new Vector3(-0.45f, 0f, -0.5f), new Vector3(0.45f, 0f, -0.5f),
            // 몸통
            new Vector3(-0.17f, 0f, -0.5f), new Vector3(0.17f, 0f, -0.5f),
            new Vector3(-0.17f, 0f, -1f), new Vector3(0.17f, 0f, -1f)
        };
        var tris = new List<int> { 0, 2, 1, 3, 4, 6, 3, 6, 5 };
        var colors = new List<Color>
        {
            Color.white, Color.white, Color.white,
            Color.white, Color.white, new Color(1, 1, 1, 0.2f), new Color(1, 1, 1, 0.2f) // 꼬리는 흐리게
        };
        return Finish("ClickArrow", verts, tris, colors);
    }

    /// <summary>평평한 육각형 격자 테두리. 바깥으로 갈수록 정점 알파를 낮춰 가장자리가 흐려진다.</summary>
    static Mesh BuildHexGrid(float radius, float size, float lineWidth)
    {
        var verts = new List<Vector3>();
        var tris = new List<int>();
        var colors = new List<Color>();
        var edges = new HashSet<long>();
        float sqrt3 = Mathf.Sqrt(3f);
        int range = Mathf.CeilToInt(radius / (size * 1.5f)) + 1;

        for (int q = -range; q <= range; q++)
        {
            for (int r = -range; r <= range; r++)
            {
                // 뾰족한 면이 옆을 보는(flat-top) 육각형 배치
                Vector3 center = new Vector3(size * 1.5f * q, 0f, size * sqrt3 * (r + q * 0.5f));
                if (center.magnitude > radius) continue;

                for (int k = 0; k < 6; k++)
                {
                    Vector3 a = center + HexCorner(size, k);
                    Vector3 b = center + HexCorner(size, k + 1);

                    // 이웃 칸과 겹치는 변은 한 번만 그리기
                    Vector3 mid = (a + b) * 0.5f;
                    long key = ((long)Mathf.RoundToInt(mid.x * 1000f) << 32) ^ (uint)Mathf.RoundToInt(mid.z * 1000f);
                    if (!edges.Add(key)) continue;

                    AddLine(verts, tris, colors, a, b, lineWidth, radius);
                }
            }
        }
        return Finish("ClickHexGrid", verts, tris, colors);
    }

    static Vector3 HexCorner(float size, int k)
    {
        float a = Mathf.Deg2Rad * 60f * k;
        return new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * size;
    }

    static void AddLine(List<Vector3> verts, List<int> tris, List<Color> colors, Vector3 a, Vector3 b, float width, float radius)
    {
        Vector3 dir = (b - a).normalized;
        Vector3 side = Vector3.Cross(Vector3.up, dir) * (width * 0.5f);
        int i = verts.Count;
        verts.Add(a - side); verts.Add(a + side); verts.Add(b + side); verts.Add(b - side);
        tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
        tris.Add(i); tris.Add(i + 2); tris.Add(i + 3);

        float fa = Fade(a.magnitude / radius), fb = Fade(b.magnitude / radius);
        colors.Add(new Color(1, 1, 1, fa)); colors.Add(new Color(1, 1, 1, fa));
        colors.Add(new Color(1, 1, 1, fb)); colors.Add(new Color(1, 1, 1, fb));
    }

    static float Fade(float x) => Mathf.Clamp01(1f - x * x);

    static Mesh Finish(string meshName, List<Vector3> verts, List<int> tris, List<Color> colors)
    {
        var mesh = new Mesh { name = meshName };
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        if (colors != null) mesh.SetColors(colors);
        else
        {
            var white = new List<Color>(verts.Count);
            for (int i = 0; i < verts.Count; i++) white.Add(Color.white);
            mesh.SetColors(white);
        }
        mesh.RecalculateBounds();
        // 양면으로 보이도록 (Sprites-Default는 컬링 없음)
        return mesh;
    }
}
