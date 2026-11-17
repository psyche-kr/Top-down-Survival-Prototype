using UnityEngine;

/// <summary>나와의 관계. HP바 색과 호버 색을 정한다.</summary>
public enum UnitRelation
{
    Self,  // 내 캐릭터 (초록 HP바)
    Ally,  // 아군 (파란 HP바, 호버 파란색)
    Enemy  // 적 (빨간 HP바, 호버 빨간색)
}

/// <summary>
/// 유닛(플레이어, 더미, 나중에 적·동물)의 표시 정보: 닉네임, 레벨, 체력, 관계.
/// 시작할 때 머리 위 UI 프리팹을 Canvas에 만들어 연결한다. 레벨·체력 시스템이 생기면 이 값을 갱신한다.
/// </summary>
public class UnitProfile : MonoBehaviour
{
    [SerializeField] string nickname = "Player";
    [SerializeField] int level = 1;
    [SerializeField] int maxHealth = 930;
    [SerializeField] int currentHealth = 930;
    [SerializeField] UnitRelation relation = UnitRelation.Self;

    [Header("Overhead UI")]
    [SerializeField] CharacterOverheadUI overheadPrefab;
    [Tooltip("장착 등급 바에 표시할 장비 (없으면 전부 일반)")]
    [SerializeField] PlayerEquipment equipment;

    public string Nickname => nickname;
    public int Level => level;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public UnitRelation Relation => relation;
    public CharacterOverheadUI OverheadUI { get; private set; }

    /// <summary>닉네임·레벨·체력이 바뀌면 호출 (HUD, 머리 위 UI 갱신용).</summary>
    public event System.Action Changed;

    void Start()
    {
        if (overheadPrefab == null) return;

        // 상자 창·HUD 같은 패널보다 아래 층에 만들기
        RectTransform layer = WorldUILayer.Get();
        if (layer == null) return;

        OverheadUI = Instantiate(overheadPrefab, layer);
        OverheadUI.name = $"OverheadUI ({nickname})";
        OverheadUI.Bind(this, equipment);
    }

    void OnDestroy()
    {
        if (OverheadUI != null) Destroy(OverheadUI.gameObject);
    }

    public void SetHealth(int current, int max)
    {
        maxHealth = Mathf.Max(1, max);
        currentHealth = Mathf.Clamp(current, 0, maxHealth);
        Changed?.Invoke();
    }

    public void SetLevel(int value)
    {
        level = Mathf.Max(1, value);
        Changed?.Invoke();
    }

    void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Changed?.Invoke();
    }
}
