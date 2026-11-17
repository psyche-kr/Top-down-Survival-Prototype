using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>강조 종류 (이터널 리턴 기준).</summary>
public enum HighlightType
{
    Object, // 상호작용 오브젝트: 평소 얇은 하얀 테두리, 호버 시 조금 더 두꺼운 하늘색 테두리 + 살짝 칠함
    Enemy,  // 적 유닛: 평소 없음, 호버 시 빨간색
    Ally    // 아군 유닛: 평소 없음, 호버 시 파란색
}

/// <summary>
/// 마우스 호버 강조. 렌더러에 Custom/HighlightOutline 머티리얼 세 개(Mask, Fill, Outline)를 덧붙인다.
/// 오브젝트는 다른 물체에 가려져도 테두리가 비쳐 보인다. 콜라이더와 같은 오브젝트에 붙여야 호버가 잡힌다.
/// </summary>
[DisallowMultipleComponent]
public class ObjectOutline : MonoBehaviour, IHoverable
{
    const string ShaderName = "Custom/HighlightOutline";
    const int StencilBit = 32; // XRaySilhouette(64)와 겹치지 않는 비트

    [SerializeField] HighlightType type = HighlightType.Object;
    [SerializeField, Tooltip("Custom/HighlightOutline (비워두면 이름으로 찾음)")] Shader shader;

    [Header("Object")]
    [SerializeField] Color objectIdleColor = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField] Color objectHoverColor = new Color(0.35f, 0.9f, 1f, 1f);
    [SerializeField] float idleWidth = 1.5f;
    [SerializeField] float hoverWidth = 3f;
    [SerializeField, Tooltip("다른 물체에 가려져도 테두리를 보여줄지 (오브젝트)")] bool showThroughWalls = true;

    [Header("Unit")]
    [SerializeField] Color enemyColor = new Color(1f, 0.22f, 0.2f, 1f);
    [SerializeField] Color allyColor = new Color(0.25f, 0.55f, 1f, 1f);

    [Header("Fill (호버 시 살짝 칠하기)")]
    [SerializeField, Range(0f, 1f)] float hoverFillAlpha = 0.18f;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int WidthId = Shader.PropertyToID("_OutlineWidth");
    static readonly int ZTestId = Shader.PropertyToID("_ZTest");
    static readonly int CullId = Shader.PropertyToID("_Cull");
    static readonly int ColorMaskId = Shader.PropertyToID("_ColorMask");
    static readonly int StencilRefId = Shader.PropertyToID("_StencilRef");
    static readonly int StencilCompId = Shader.PropertyToID("_StencilComp");
    static readonly int StencilPassId = Shader.PropertyToID("_StencilPass");

    Material maskMaterial, fillMaterial, outlineMaterial;
    readonly List<Renderer> renderers = new List<Renderer>();
    bool hovered;

    public HighlightType Type => type;

    void Awake()
    {
        if (shader == null) shader = Shader.Find(ShaderName);
        if (shader == null || !shader.isSupported)
        {
            Debug.LogWarning($"[ObjectOutline] '{ShaderName}' 셰이더를 찾을 수 없어 비활성화합니다.", this);
            enabled = false;
            return;
        }

        int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            if (r is not MeshRenderer && r is not SkinnedMeshRenderer) continue;
            if (r.gameObject.layer == ignoreLayer) continue;
            renderers.Add(r);
        }

        maskMaterial = CreateMaterial("Outline Mask", 460);
        maskMaterial.SetFloat(ColorMaskId, 0f);
        maskMaterial.SetFloat(StencilCompId, (float)CompareFunction.Always);
        maskMaterial.SetFloat(StencilPassId, (float)StencilOp.Replace);

        fillMaterial = CreateMaterial("Outline Fill", 461);

        outlineMaterial = CreateMaterial("Outline Edge", 462);
        outlineMaterial.SetFloat(CullId, (float)CullMode.Off);
        outlineMaterial.SetFloat(StencilCompId, (float)CompareFunction.NotEqual);

        Apply();
    }

    Material CreateMaterial(string matName, int queueOffset)
    {
        var mat = new Material(shader) { name = matName + " (Runtime)", renderQueue = (int)RenderQueue.Transparent + queueOffset };
        mat.SetFloat(StencilRefId, StencilBit);
        mat.SetFloat(StencilPassId, (float)StencilOp.Keep);
        mat.SetFloat(CullId, (float)CullMode.Back);
        mat.SetFloat(ColorMaskId, (float)ColorWriteMask.All);
        mat.SetFloat(WidthId, 0f);
        return mat;
    }

    void OnEnable()
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            var mats = new List<Material>(r.sharedMaterials);
            foreach (var m in new[] { maskMaterial, fillMaterial, outlineMaterial })
                if (m != null && !mats.Contains(m)) mats.Add(m);
            r.sharedMaterials = mats.ToArray();
        }
    }

    void OnDisable()
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            var mats = new List<Material>(r.sharedMaterials);
            mats.Remove(maskMaterial);
            mats.Remove(fillMaterial);
            mats.Remove(outlineMaterial);
            r.sharedMaterials = mats.ToArray();
        }
    }

    void OnDestroy()
    {
        if (maskMaterial != null) Destroy(maskMaterial);
        if (fillMaterial != null) Destroy(fillMaterial);
        if (outlineMaterial != null) Destroy(outlineMaterial);
    }

    public void SetHovered(bool value)
    {
        hovered = value;
        Apply();
    }

    void Apply()
    {
        if (outlineMaterial == null) return;

        Color edgeColor;
        float width;
        bool throughWalls;

        if (type == HighlightType.Object)
        {
            edgeColor = hovered ? objectHoverColor : objectIdleColor;
            width = hovered ? hoverWidth : idleWidth;
            throughWalls = showThroughWalls;
        }
        else
        {
            // 유닛은 평소 테두리 없음
            edgeColor = type == HighlightType.Enemy ? enemyColor : allyColor;
            width = hovered ? hoverWidth : 0f;
            if (!hovered) edgeColor.a = 0f;
            throughWalls = false;
        }

        float zTest = (float)(throughWalls ? CompareFunction.Always : CompareFunction.LessEqual);

        maskMaterial.SetFloat(ZTestId, zTest);

        outlineMaterial.SetColor(BaseColorId, edgeColor);
        outlineMaterial.SetFloat(WidthId, width);
        outlineMaterial.SetFloat(ZTestId, zTest);

        Color fill = edgeColor;
        fill.a = hovered ? hoverFillAlpha : 0f;
        fillMaterial.SetColor(BaseColorId, fill);
        fillMaterial.SetFloat(ZTestId, zTest);
    }

#if UNITY_EDITOR
    void OnValidate() => Apply();
#endif
}
