using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 아래를 가리키는 V자(역삼각형 테두리) UI 그래픽. 스프라이트 없이 메시로 그린다.
/// 위쪽은 투명, 아래쪽 꼭짓점으로 갈수록 진해지는 세로 그라데이션.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class UIChevron : MaskableGraphic
{
    [SerializeField] float thickness = 4f;
    [SerializeField] Color topColor = new Color(1f, 0.85f, 0.45f, 0f);
    [SerializeField] Color bottomColor = new Color(1f, 0.82f, 0.4f, 1f);

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect r = GetPixelAdjustedRect();
        Vector2 tip = new Vector2(r.center.x, r.yMin);
        Vector2 leftTop = new Vector2(r.xMin, r.yMax);
        Vector2 rightTop = new Vector2(r.xMax, r.yMax);

        AddLine(vh, leftTop, tip, r);
        AddLine(vh, rightTop, tip, r);
    }

    void AddLine(VertexHelper vh, Vector2 from, Vector2 to, Rect r)
    {
        Vector2 dir = (to - from).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

        int start = vh.currentVertCount;
        AddVert(vh, from - normal, r);
        AddVert(vh, from + normal, r);
        AddVert(vh, to + normal, r);
        AddVert(vh, to - normal, r);

        vh.AddTriangle(start, start + 1, start + 2);
        vh.AddTriangle(start, start + 2, start + 3);
    }

    void AddVert(VertexHelper vh, Vector2 pos, Rect r)
    {
        float t = Mathf.InverseLerp(r.yMin, r.yMax, pos.y); // 0 = 아래, 1 = 위
        Color c = Color.Lerp(bottomColor, topColor, t) * color;
        vh.AddVert(pos, c, Vector2.zero);
    }
}
