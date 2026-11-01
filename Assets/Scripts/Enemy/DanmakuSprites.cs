using UnityEngine;

// 이미지 에셋 없이 탄막/박스용 스프라이트를 코드로 만들어 주는 도우미
public static class DanmakuSprites
{
    private static Sprite circle;
    private static Sprite square;

    // 지름 1 유닛짜리 원
    public static Sprite Circle
    {
        get
        {
            if (circle == null) circle = CreateCircle(64);
            return circle;
        }
    }

    // 한 변 1 유닛짜리 사각형
    public static Sprite Square
    {
        get
        {
            if (square == null) square = CreateSquare(4);
            return square;
        }
    }

    private static Sprite CreateCircle(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        float r = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                // 가장자리 1픽셀을 부드럽게 (안티에일리어싱)
                float a = Mathf.Clamp01(r - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite CreateSquare(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
