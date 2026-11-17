using TMPro;
using UnityEngine;

/// <summary>
/// 화면 하단 HUD (스테이터스, 장비 칸, 초상화, 스킬, 체력바, 크레딧, 인벤토리).
/// 아직 체력·레벨·크레딧 시스템이 없어서 표시용 값만 받는다. 시스템이 생기면 여기에 연결한다.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance { get; private set; }

    [Tooltip("체력 비율만큼 가로 앵커를 줄여서 채움을 표현")]
    [SerializeField] RectTransform healthFill;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI creditText;

    [Header("표시용 기본값")]
    [SerializeField] int maxHealth = 930;
    [SerializeField] int level = 1;
    [SerializeField] int credit = 0;

    [Tooltip("연결하면 내 캐릭터의 레벨·체력을 표시 (없으면 위 기본값)")]
    [SerializeField] UnitProfile profile;

    void Awake() => Instance = this;

    void OnEnable()
    {
        if (profile != null) profile.Changed += RefreshFromCharacter;
    }

    void OnDisable()
    {
        if (profile != null) profile.Changed -= RefreshFromCharacter;
    }

    void Start()
    {
        SetHealth(maxHealth, maxHealth);
        SetLevel(level);
        SetCredit(credit);
        RefreshFromCharacter();
    }

    void RefreshFromCharacter()
    {
        if (profile == null) return;
        SetHealth(profile.CurrentHealth, profile.MaxHealth);
        SetLevel(profile.Level);
    }

    public void SetHealth(int current, int max)
    {
        if (healthFill != null)
        {
            float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            healthFill.anchorMax = new Vector2(ratio, healthFill.anchorMax.y);
        }
        if (healthText != null) healthText.text = $"{current} / {max}";
    }

    public void SetLevel(int value)
    {
        if (levelText != null) levelText.text = value.ToString();
    }

    public void SetCredit(int value)
    {
        if (creditText != null) creditText.text = value.ToString();
    }
}
