using UnityEngine;

public static class Utils
{
    public static float FloatSign(float value, float epsilon = 1e-6f)
    {
        if (Mathf.Abs(value) < epsilon)
        {
            return 0; // 视为 0
        }
        return Mathf.Sign(value);
    }
    public static Vector2 RotateVector(this Vector2 v, float angleDegrees) {
        Quaternion rotation = Quaternion.Euler(0, 0, angleDegrees); // 绕Z轴旋转
        return rotation * v;
    }

    public static float GetRendererOfCircleSpriteRadius(SpriteRenderer spriteRenderer)
    {
        if (!spriteRenderer || !spriteRenderer.sprite) return 0;
        // 获取贴图像素尺寸
        float width = spriteRenderer.sprite.rect.width;
        float height = spriteRenderer.sprite.rect.height;
    
        // 假设是完美圆形，半径为宽度或高度的一半（取较小值）
        float radiusInPixels = Mathf.Min(width, height) / 2f;
    
        // 转换为世界单位
        float pixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit;
        float radiusInWorldUnits = radiusInPixels / pixelsPerUnit;

        return radiusInWorldUnits;
    }
}
