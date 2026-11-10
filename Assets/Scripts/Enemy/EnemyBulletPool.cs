using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletPool : MonoBehaviour
{
    private static EnemyBulletPool instance;

    // 씬에 없으면 처음 쓸 때 자동으로 만들어짐
    public static EnemyBulletPool Instance
    {
        get
        {
            if (instance == null) new GameObject("EnemyBulletPool").AddComponent<EnemyBulletPool>();
            return instance;
        }
    }

    // 이 범위를 벗어난 탄은 회수
    public static Rect Bounds { get; private set; }

    [Tooltip("비워두면 기본 원형 탄을 사용")]
    [SerializeField] private EnemyBullet bulletPrefab;
    [SerializeField] private int preload = 300;
    [SerializeField] private float boundsMargin = 1f;
    [SerializeField] private float defaultBulletSize = 0.25f;

    private readonly Stack<EnemyBullet> pool = new Stack<EnemyBullet>();
    private static Sprite circleSprite;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (bulletPrefab == null) bulletPrefab = CreateDefaultBullet();
        UpdateBounds();

        for (int i = 0; i < preload; i++)
        {
            var b = Instantiate(bulletPrefab, transform);
            b.gameObject.SetActive(false);
            pool.Push(b);
        }
    }

    private void Update()
    {
        UpdateBounds();
    }

    public EnemyBullet Spawn(Vector2 pos, float angle, float speed, Color color, float accel = 0f, float angularSpeed = 0f)
    {
        var b = pool.Count > 0 ? pool.Pop() : Instantiate(bulletPrefab, transform);
        b.gameObject.SetActive(true);
        b.Init(pos, angle, speed, color, accel, angularSpeed);
        return b;
    }

    public void Despawn(EnemyBullet b)
    {
        if (!b.gameObject.activeSelf) return;
        b.gameObject.SetActive(false);
        pool.Push(b);
    }

    // 화면의 모든 적 탄 제거 (봄, 보스 페이즈 전환 등)
    public void ClearAll()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf && child.TryGetComponent(out EnemyBullet b)) Despawn(b);
        }
    }

    private void UpdateBounds()
    {
        var cam = Camera.main;
        if (cam == null) return;
        float h = cam.orthographicSize;
        float w = h * cam.aspect;
        Vector2 c = cam.transform.position;
        Bounds = new Rect(c.x - w - boundsMargin, c.y - h - boundsMargin, (w + boundsMargin) * 2f, (h + boundsMargin) * 2f);
    }

    private EnemyBullet CreateDefaultBullet()
    {
        var go = new GameObject("DefaultEnemyBullet");
        go.SetActive(false);
        go.transform.SetParent(transform, false);
        go.transform.localScale = Vector3.one * defaultBulletSize;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CircleSprite;
        sr.sortingOrder = 20;

        return go.AddComponent<EnemyBullet>();
    }

    // 지름 1 유닛짜리 원 스프라이트
    public static Sprite CircleSprite
    {
        get
        {
            if (circleSprite != null) return circleSprite;

            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r;
                    float a = Mathf.Clamp01((1f - d) * 12f);
                    float core = d < 0.55f ? 1f : 0.75f;
                    tex.SetPixel(x, y, new Color(core, core, core, a));
                }
            }
            tex.Apply();
            circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return circleSprite;
        }
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
