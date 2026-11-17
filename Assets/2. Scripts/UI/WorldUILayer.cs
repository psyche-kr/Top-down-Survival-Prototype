using UnityEngine;

/// <summary>
/// 월드 대상을 따라다니는 화면 UI(머리 위 HP바, 바닥 아이템 이름표)를 모아 두는 층.
/// Canvas의 첫 번째 자식이라 상자 창·HUD·툴팁 같은 패널보다 항상 아래에 그려지고, 3D 오브젝트보다는 위에 보인다.
/// </summary>
public static class WorldUILayer
{
    const string LayerName = "WorldUI";
    static RectTransform layer;

    public static RectTransform Get()
    {
        if (layer != null) return layer;

        Canvas canvas = null;
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            if (c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay) { canvas = c; break; }
        if (canvas == null) return null;

        Transform existing = canvas.transform.Find(LayerName);
        if (existing != null)
        {
            layer = (RectTransform)existing;
        }
        else
        {
            var go = new GameObject(LayerName, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            layer = (RectTransform)go.transform;
            layer.SetParent(canvas.transform, false);
            layer.anchorMin = Vector2.zero;
            layer.anchorMax = Vector2.one;
            layer.offsetMin = layer.offsetMax = Vector2.zero;
        }

        layer.SetAsFirstSibling();
        return layer;
    }
}
