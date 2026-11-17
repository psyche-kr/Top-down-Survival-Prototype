using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 유닛 머리 위 UI (닉네임, 레벨 칸, 장비 등급 바, HP바). 프리팹으로 만들어 UnitProfile이 생성·연결한다.
/// 화면 공간 Canvas에서 유닛을 따라가므로 줌과 상관없이 크기가 일정하다.
/// HP바 색: 나 초록, 아군 파랑, 적 빨강. 눈금: 100마다 가는 선, 1000마다 굵은 선.
/// </summary>
[DefaultExecutionOrder(100)] // 카메라(CameraController.LateUpdate)가 움직인 뒤에 화면 위치 계산
public class CharacterOverheadUI : MonoBehaviour
{
    [Tooltip("유닛 위치 기준 머리 위 높이")]
    [SerializeField] Vector3 worldOffset = new Vector3(0f, 3.2f, 0f);

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] Image healthFillImage;
    [SerializeField] RectTransform tickContainer;
    [SerializeField] Image tickPrefab;
    [SerializeField] GameObject locatorMarker;

    [Header("장비 등급 바 (HP바 위, 왼쪽부터 무기·옷·머리·팔·신발)")]
    [SerializeField] Image[] equipSegments = new Image[PlayerEquipment.SlotCount];

    [Header("HP 색")]
    [SerializeField] Color selfColor = new Color(0.3f, 0.85f, 0.15f);
    [SerializeField] Color allyColor = new Color(0.2f, 0.5f, 1f);
    [SerializeField] Color enemyColor = new Color(0.85f, 0.12f, 0.15f);

    [Header("눈금")]
    [SerializeField] int smallTickHealth = 100;
    [SerializeField] int bigTickHealth = 1000;
    [SerializeField] float smallTickWidth = 1f;
    [SerializeField] float bigTickWidth = 2f;
    [SerializeField] Color smallTickColor = new Color(0f, 0f, 0f, 0.55f);
    [SerializeField] Color bigTickColor = new Color(0f, 0f, 0f, 0.95f);

    readonly List<Image> ticks = new List<Image>();
    UnitProfile target;
    PlayerEquipment equipment;
    RectTransform rect;
    Camera mainCamera;

    public GameObject LocatorMarker => locatorMarker;

    void Awake()
    {
        rect = (RectTransform)transform;
        mainCamera = Camera.main;
        if (tickPrefab != null) tickPrefab.gameObject.SetActive(false);
        if (locatorMarker != null) locatorMarker.SetActive(false);
    }

    public void Bind(UnitProfile profile, PlayerEquipment equip)
    {
        Unbind();
        target = profile;
        equipment = equip;
        if (target != null) target.Changed += Refresh;
        if (equipment != null) equipment.Changed += RefreshEquipment;
        Refresh();
    }

    void Unbind()
    {
        if (target != null) target.Changed -= Refresh;
        if (equipment != null) equipment.Changed -= RefreshEquipment;
    }

    void OnDestroy() => Unbind();

    void LateUpdate()
    {
        if (target == null || mainCamera == null) return;

        Vector3 screen = mainCamera.WorldToScreenPoint(target.transform.position + worldOffset);

        // 카메라 뒤로 가면 화면 밖으로 치움
        rect.position = screen.z > 0f
            ? new Vector3(screen.x, screen.y, 0f)
            : new Vector3(-10000f, -10000f, 0f);
    }

    public void Refresh()
    {
        if (target == null) return;

        nameText.text = target.Nickname;
        levelText.text = target.Level.ToString();

        healthFillImage.color = target.Relation switch
        {
            UnitRelation.Ally => allyColor,
            UnitRelation.Enemy => enemyColor,
            _ => selfColor
        };

        RectTransform fill = healthFillImage.rectTransform;
        float ratio = (float)target.CurrentHealth / target.MaxHealth;
        fill.anchorMax = new Vector2(ratio, fill.anchorMax.y);

        BuildTicks(target.MaxHealth);
        RefreshEquipment();
    }

    /// <summary>각 장비 칸의 등급 색. 장비가 없거나 비어 있으면 일반(회색).</summary>
    public void RefreshEquipment()
    {
        for (int i = 0; i < equipSegments.Length; i++)
        {
            if (equipSegments[i] == null) continue;

            ItemRarity rarity = equipment != null ? equipment.GetRarity((EquipSlot)i) : ItemRarity.Common;
            equipSegments[i].color = ItemRarityColor.Get(rarity);
        }
    }

    void BuildTicks(int maxHealth)
    {
        if (tickPrefab == null || tickContainer == null || smallTickHealth <= 0) return;

        int count = Mathf.Max(0, Mathf.CeilToInt((float)maxHealth / smallTickHealth) - 1);

        while (ticks.Count < count)
            ticks.Add(Instantiate(tickPrefab, tickContainer));

        for (int i = 0; i < ticks.Count; i++)
        {
            bool used = i < count;
            ticks[i].gameObject.SetActive(used);
            if (!used) continue;

            int health = (i + 1) * smallTickHealth;
            bool big = bigTickHealth > 0 && health % bigTickHealth == 0;
            float x = (float)health / maxHealth;

            RectTransform t = ticks[i].rectTransform;
            t.anchorMin = new Vector2(x, 0f);
            t.anchorMax = new Vector2(x, 1f);
            t.anchoredPosition = Vector2.zero;
            t.sizeDelta = new Vector2(big ? bigTickWidth : smallTickWidth, 0f);
            ticks[i].color = big ? bigTickColor : smallTickColor;
        }
    }
}
