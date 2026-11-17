using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 캐릭터가 오브젝트 뒤에 가려졌을 때, 가려진 부분만 반투명 실루엣으로 보여준다.
/// 캐릭터의 모든 MeshRenderer / SkinnedMeshRenderer에 머티리얼 두 개를 덧붙인다.
///   Mark      : 보이는 캐릭터 픽셀에 스텐실을 찍는다 (자기 몸에 가려진 부분은 실루엣이 생기지 않게)
///   Silhouette: 깊이 테스트가 Greater(= 다른 물체 뒤)이고 스텐실이 안 찍힌 곳만 색을 그린다
/// 렌더러 에셋(Renderer Feature) 설정 없이 동작한다.
/// </summary>
[DisallowMultipleComponent]
public class XRaySilhouette : MonoBehaviour
{
    const string ShaderName = "Custom/XRaySilhouette";
    const int StencilBit = 64; // 다른 기능과 겹치지 않도록 상위 비트 사용

    [SerializeField, Tooltip("Custom/XRaySilhouette 셰이더 (비워두면 이름으로 찾음)")]
    Shader shader;

    [SerializeField, Tooltip("실루엣 색상 (알파 = 진하기)")]
    Color color = new Color(0.25f, 0.75f, 1f, 0.6f);

    [SerializeField, Range(0.5f, 8f), Tooltip("클수록 가장자리만 진해짐")]
    float rimPower = 2f;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int RimPowerId = Shader.PropertyToID("_RimPower");
    static readonly int ZTestId = Shader.PropertyToID("_ZTest");
    static readonly int ColorMaskId = Shader.PropertyToID("_ColorMask");
    static readonly int StencilRefId = Shader.PropertyToID("_StencilRef");
    static readonly int StencilCompId = Shader.PropertyToID("_StencilComp");
    static readonly int StencilPassId = Shader.PropertyToID("_StencilPass");
    static readonly int OffsetFactorId = Shader.PropertyToID("_OffsetFactor");
    static readonly int OffsetUnitsId = Shader.PropertyToID("_OffsetUnits");

    Material markMaterial;
    Material silhouetteMaterial;
    readonly List<Renderer> targetRenderers = new List<Renderer>();

    void Awake()
    {
        if (shader == null) shader = Shader.Find(ShaderName);
        if (shader == null || !shader.isSupported)
        {
            Debug.LogWarning($"[XRaySilhouette] '{ShaderName}' 셰이더를 찾을 수 없어 비활성화합니다.", this);
            enabled = false;
            return;
        }

        markMaterial = CreateMarkMaterial();
        silhouetteMaterial = CreateSilhouetteMaterial();

        // 외곽선 등 Ignore Raycast 레이어의 보조 렌더러는 제외
        int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            if (r is not MeshRenderer && r is not SkinnedMeshRenderer) continue;
            if (r.gameObject.layer == ignoreLayer) continue;
            targetRenderers.Add(r);
        }
    }

    void OnEnable()
    {
        foreach (var r in targetRenderers)
        {
            if (r == null) continue;
            var mats = new List<Material>(r.sharedMaterials);
            if (!mats.Contains(markMaterial)) mats.Add(markMaterial);
            if (!mats.Contains(silhouetteMaterial)) mats.Add(silhouetteMaterial);
            r.sharedMaterials = mats.ToArray();
        }
    }

    void OnDisable()
    {
        foreach (var r in targetRenderers)
        {
            if (r == null) continue;
            var mats = new List<Material>(r.sharedMaterials);
            mats.Remove(markMaterial);
            mats.Remove(silhouetteMaterial);
            r.sharedMaterials = mats.ToArray();
        }
    }

    void OnDestroy()
    {
        if (markMaterial != null) Destroy(markMaterial);
        if (silhouetteMaterial != null) Destroy(silhouetteMaterial);
    }

    /// <summary>실루엣 색 변경 (예: 아군/적군 색 구분)</summary>
    public void SetColor(Color newColor)
    {
        color = newColor;
        if (silhouetteMaterial != null) silhouetteMaterial.SetColor(BaseColorId, color);
    }

    Material CreateMarkMaterial()
    {
        var mat = new Material(shader) { name = "XRay Mark (Runtime)", renderQueue = (int)RenderQueue.Transparent + 450 };
        mat.SetFloat(ZTestId, (float)CompareFunction.LessEqual);
        mat.SetFloat(ColorMaskId, 0f);
        mat.SetFloat(StencilRefId, StencilBit);
        mat.SetFloat(StencilCompId, (float)CompareFunction.Always);
        mat.SetFloat(StencilPassId, (float)StencilOp.Replace);
        // 원래 캐릭터 깊이와 정확히 같아야 통과하므로 살짝 앞으로 당긴다
        mat.SetFloat(OffsetFactorId, -1f);
        mat.SetFloat(OffsetUnitsId, -1f);
        return mat;
    }

    Material CreateSilhouetteMaterial()
    {
        var mat = new Material(shader) { name = "XRay Silhouette (Runtime)", renderQueue = (int)RenderQueue.Transparent + 451 };
        mat.SetColor(BaseColorId, color);
        mat.SetFloat(RimPowerId, rimPower);
        mat.SetFloat(ZTestId, (float)CompareFunction.Greater);
        mat.SetFloat(ColorMaskId, (float)ColorWriteMask.All);
        mat.SetFloat(StencilRefId, StencilBit);
        mat.SetFloat(StencilCompId, (float)CompareFunction.NotEqual);
        mat.SetFloat(StencilPassId, (float)StencilOp.Keep);
        mat.SetFloat(OffsetFactorId, 0f);
        mat.SetFloat(OffsetUnitsId, 0f);
        return mat;
    }

#if UNITY_EDITOR
    // 플레이 중 인스펙터에서 색/림 값을 바꾸면 바로 반영
    void OnValidate()
    {
        if (silhouetteMaterial == null) return;
        silhouetteMaterial.SetColor(BaseColorId, color);
        silhouetteMaterial.SetFloat(RimPowerId, rimPower);
    }
#endif
}
