using UnityEngine;

/// <summary>
/// 조합식: 재료 A + 재료 B → 결과 아이템 (이터널 리턴 방식).
/// RecipeDatabase에 등록하면 재료 아이템의 툴팁 '상위 아이템'에 결과가 자동으로 표시된다.
/// </summary>
[CreateAssetMenu(menuName = "Item/Recipe")]
public class ItemRecipe : ScriptableObject
{
    public Item ingredientA;
    public Item ingredientB;
    public Item result;

    public bool Uses(Item item) => item != null && (ingredientA == item || ingredientB == item);
}
