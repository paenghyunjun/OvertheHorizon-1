using UnityEngine;

public enum BulletShape { Orb, Rice, Feather, Star }

public class EnemyBulletPool : MonoBehaviour
{
    private static EnemyBulletPool instance;

    public static EnemyBulletPool Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<EnemyBulletPool>();
            if (instance == null) instance = new GameObject("EnemyBulletPool").AddComponent<EnemyBulletPool>();
            return instance;
        }
    }

    public static Rect Bounds { get; private set; }

    [Tooltip("비워두면 기본 원형 탄을 사용")]
    [SerializeField] private EnemyBullet bulletPrefab;
    [Tooltip("시작할 때 미리 만들어 둘 탄 개수")]
    [SerializeField] private int preload = 300;
    [SerializeField] private float boundsMargin = 1f;
    [SerializeField] private float defaultBulletSize = 0.25f;

    [Header("탄 모양 (Assets/Sprites/Bullets)")]
    [SerializeField] private Sprite orbSprite;
    [SerializeField] private Sprite riceSprite;
    [SerializeField] private Sprite featherSprite;
    [SerializeField] private Sprite starSprite;
    [Tooltip("탄 스프라이트를 쓸 때의 크기 배율")]
    [SerializeField] private float spriteBulletSize = 0.8f;

    private static Sprite circleSprite;
    private bool ready;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        Prepare();
    }

    private void Prepare()
    {
        if (ready) return;
        ready = true;

        if (bulletPrefab == null) bulletPrefab = CreateDefaultBullet();
        UpdateBounds();
        PullingManager.Instance.Prewarm(bulletPrefab.gameObject, preload);
    }

    private void Update()
    {
        UpdateBounds();
    }

    public EnemyBullet Spawn(Vector2 pos, float angle, float speed, Color color, float accel = 0f, float angularSpeed = 0f, BulletShape shape = BulletShape.Orb)
    {
        Prepare();
        var b = PullingManager.Instance.Spawn(bulletPrefab, pos, Quaternion.identity);
        b.SetSprite(SpriteFor(shape));
        b.Init(pos, angle, speed, color, accel, angularSpeed);
        return b;
    }

    private Sprite SpriteFor(BulletShape shape)
    {
        Sprite s = orbSprite;
        if (shape == BulletShape.Rice && riceSprite != null) s = riceSprite;
        else if (shape == BulletShape.Feather && featherSprite != null) s = featherSprite;
        else if (shape == BulletShape.Star && starSprite != null) s = starSprite;
        return s;
    }

    public void Despawn(EnemyBullet b)
    {
        if (b != null) PullingManager.Release(b.gameObject);
    }

    public void ClearAll()
    {
        if (bulletPrefab != null) PullingManager.Instance.DespawnAll(bulletPrefab.gameObject);
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
        go.transform.localScale = Vector3.one * (orbSprite != null ? spriteBulletSize : defaultBulletSize);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = orbSprite != null ? orbSprite : CircleSprite;
        sr.sortingOrder = 20;

        return go.AddComponent<EnemyBullet>();
    }

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
