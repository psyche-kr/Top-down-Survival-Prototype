using System.Collections.Generic;
using UnityEngine;

/// <summary>모든 조합식 목록. 어떤 아이템이 어떤 상위 아이템의 재료인지 찾을 때 사용한다.</summary>
[CreateAssetMenu(menuName = "Item/Recipe Database")]
public class RecipeDatabase : ScriptableObject
{
    public List<ItemRecipe> recipes = new List<ItemRecipe>();

    /// <summary>이 아이템을 재료로 쓰는 상위 아이템 목록 (중복 제거, 등록 순서 유지).</summary>
    public List<Item> GetUpperItems(Item item)
    {
        var result = new List<Item>();
        if (item == null) return result;

        foreach (var recipe in recipes)
        {
            if (recipe == null || recipe.result == null || !recipe.Uses(item)) continue;
            if (!result.Contains(recipe.result)) result.Add(recipe.result);
        }

        return result;
    }
}
