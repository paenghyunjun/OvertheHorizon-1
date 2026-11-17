using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyKind { Starling, HoneyBuzzard, GoldenEagle }

public class Enemy : MonoBehaviour, IPoolable
{
    public enum Pattern { Ring, AimedFan, Spiral, Curve, AimedShot }

    public static readonly List<Enemy> Active = new List<Enemy>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Active.Clear();
    }

    [Header("종류 / 점수")]
    [SerializeField] private EnemyKind kind = EnemyKind.Starling;
    [Tooltip("처치 시 플레이어가 얻는 점수. RoundManager 가 스폰할 때 덮어씀")]
    [SerializeField] private int scoreValue = 100;

    [Header("체력 / 피격")]
    [SerializeField] private int maxHp = 60;
    [Tooltip("플레이어 탄에 맞는 범위")]
    [SerializeField] private float hitRadius = 0.4f;
    [Tooltip("플레이어 탄 한 발에 깎이는 체력. 캐릭터별 공격력(3~5)이 PlayerData 에 생기면 그 값으로 교체")]
    [SerializeField] private int damagePerHit = 4;

    [Header("이동")]
    [SerializeField] private float enterSpeed = 3f;
    [SerializeField] private float swayWidth = 1f;
    [SerializeField] private float swaySpeed = 0.6f;

    [Header("패턴")]
    [SerializeField] private Pattern[] patterns = { Pattern.Ring, Pattern.AimedFan, Pattern.Spiral, Pattern.Curve };
    [SerializeField] private float patternDuration = 4f;
    [SerializeField] private float patternRest = 0.8f;
    [SerializeField] private float bulletSpeed = 2.5f;
    [Tooltip("AimedShot 패턴의 발사 간격(초). 기획서: 찌르레기 0.75, 벌매 1.5")]
    [SerializeField] private float aimedShotInterval = 0.75f;

    [Header("모습")]
    [Tooltip("날갯짓 프레임. 2장 이상이면 순서대로 반복 재생")]
    [SerializeField] private Sprite[] frames;
    [Tooltip("초당 프레임 수")]
    [SerializeField] private float frameRate = 6f;

    public event System.Action<Enemy> OnDead;
    public event System.Action<Enemy> OnRemoved;

    public EnemyKind Kind => kind;
    public int ScoreValue => scoreValue;
    public bool IsSummoned { get; private set; }
    public int Hp => hp;
    public int MaxHp => maxHp;
    public float HpRatio => maxHp > 0 ? (float)hp / maxHp : 0f;
    public float HitRadius { get => hitRadius; set => hitRadius = value; }

    public void SetFrames(Sprite[] frames, float frameRate)
    {
        this.frames = frames;
        this.frameRate = frameRate;
    }

    private int hp;
    private Vector2 anchor;
    private bool arrived;
    private bool begun;
    private bool leaving;
    private bool removed;
    private SpriteRenderer sr;
    private Color baseColor;
    private float flashTimer;
    private float frameOffset;

    private void Awake()
    {
        hp = maxHp;
        anchor = transform.position;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
    }

    private void OnEnable() => Active.Add(this);
    private void OnDisable() => Active.Remove(this);

    public void OnSpawned()
    {
        arrived = false;
        begun = false;
        leaving = false;
        removed = false;
        flashTimer = 0f;
        IsSummoned = false;
        frameOffset = Random.value;
        if (sr != null) sr.color = baseColor;
    }

    public void OnDespawned()
    {
        StopAllCoroutines();
        OnDead = null;
        OnRemoved = null;
    }

    public void Configure(EnemyKind kind, int scoreValue, bool summoned, float aimedShotInterval)
    {
        this.kind = kind;
        this.scoreValue = scoreValue;
        IsSummoned = summoned;
        if (aimedShotInterval > 0f) this.aimedShotInterval = aimedShotInterval;
    }

    public void Setup(Vector2 target, int hp, Pattern[] patterns = null)
    {
        anchor = target;
        maxHp = this.hp = Mathf.Max(1, hp);
        if (patterns != null && patterns.Length > 0) this.patterns = patterns;
        Begin();
    }

    private void Start()
    {
        if (!begun) Begin();
    }

    private void Begin()
    {
        begun = true;
        StopAllCoroutines();
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (((Vector2)transform.position - anchor).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector2.MoveTowards(transform.position, anchor, enterSpeed * Time.deltaTime);
            yield return null;
        }
        arrived = true;

        if (patterns == null || patterns.Length == 0) yield break;

        yield return new WaitForSeconds(Random.value * aimedShotInterval);

        int i = 0;
        while (true)
        {
            yield return RunPattern(patterns[i % patterns.Length]);
            yield return new WaitForSeconds(patternRest);
            i++;
        }
    }

    private void Update()
    {
        if (arrived && !leaving)
        {
            float x = Mathf.Sin(Time.time * swaySpeed * Mathf.PI) * swayWidth;
            transform.position = anchor + new Vector2(x, 0f);
        }

        CheckPlayerBullets();

        if (sr != null && frames != null && frames.Length > 1)
        {
            int index = (int)((Time.time + frameOffset) * frameRate) % frames.Length;
            if (frames[index] != null) sr.sprite = frames[index];
        }

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && sr != null) sr.color = baseColor;
        }
    }

    private void CheckPlayerBullets()
    {
        var list = PlayerBullets.Active;
        Vector2 pos = transform.position;
        float r2 = hitRadius * hitRadius;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            var b = list[i];
            if (b == null || ((Vector2)b.position - pos).sqrMagnitude > r2) continue;

            PullingManager.Release(b.gameObject);
            TakeDamage(damagePerHit);
            if (removed) return;
        }
    }

    public void TakeDamage(int damage)
    {
        if (removed || hp <= 0) return;
        hp -= damage;

        if (sr != null)
        {
            sr.color = baseColor == Color.white ? new Color(1f, 0.45f, 0.45f) : Color.white;
            flashTimer = 0.05f;
        }

        if (hp <= 0) Remove(true);
    }

    public void Leave()
    {
        if (removed || leaving) return;
        leaving = true;
        StopAllCoroutines();
        StartCoroutine(LeaveRoutine());
    }

    private IEnumerator LeaveRoutine()
    {
        var cam = Camera.main;
        float top = cam != null ? cam.transform.position.y + cam.orthographicSize + 1f : transform.position.y + 12f;

        while (transform.position.y < top)
        {
            transform.position += Vector3.up * (enterSpeed * 2f * Time.deltaTime);
            yield return null;
        }
        Remove(false);
    }

    private void Remove(bool killed)
    {
        if (removed) return;
        removed = true;

        if (killed)
        {
            ScoreManager.Instance.AddKillScore(scoreValue);
            OnDead?.Invoke(this);
        }
        OnRemoved?.Invoke(this);

        PullingManager.Release(gameObject);
    }

    private IEnumerator RunPattern(Pattern p)
    {
        switch (p)
        {
            case Pattern.Ring: return Ring();
            case Pattern.AimedFan: return AimedFan();
            case Pattern.Spiral: return Spiral();
            case Pattern.AimedShot: return AimedShot();
            default: return Curve();
        }
    }

    private IEnumerator Ring()
    {
        const int count = 24;
        float step = 360f / count;
        float offset = 0f;
        float end = Time.time + patternDuration;
        while (Time.time < end)
        {
            for (int i = 0; i < count; i++)
                Fire(offset + step * i, bulletSpeed, new Color(1f, 0.35f, 0.35f), BulletShape.Orb);
            offset += step * 0.5f;
            yield return new WaitForSeconds(0.45f);
        }
    }

    private IEnumerator AimedFan()
    {
        const int ways = 5;
        const float spread = 14f;
        float end = Time.time + patternDuration;
        while (Time.time < end)
        {
            float aim = AimAngle();
            for (int burst = 0; burst < 3; burst++)
            {
                for (int i = 0; i < ways; i++)
                    Fire(aim + (i - (ways - 1) * 0.5f) * spread, bulletSpeed * 1.6f, new Color(0.4f, 0.7f, 1f), BulletShape.Feather);
                yield return new WaitForSeconds(0.08f);
            }
            yield return new WaitForSeconds(0.6f);
        }
    }

    private IEnumerator Spiral()
    {
        const int arms = 3;
        float angle = 0f;
        float end = Time.time + patternDuration;
        while (Time.time < end)
        {
            for (int i = 0; i < arms; i++)
                Fire(angle + 360f / arms * i, bulletSpeed, new Color(1f, 0.85f, 0.3f), BulletShape.Rice);
            angle += 11f;
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator Curve()
    {
        const int count = 12;
        float step = 360f / count;
        bool flip = false;
        float end = Time.time + patternDuration;
        while (Time.time < end)
        {
            float turn = flip ? -35f : 35f;
            for (int i = 0; i < count; i++)
                Fire(step * i, bulletSpeed * 0.6f, new Color(0.75f, 0.45f, 1f), BulletShape.Star, 0.5f, turn);
            flip = !flip;
            yield return new WaitForSeconds(0.35f);
        }
    }

    private IEnumerator AimedShot()
    {
        float end = Time.time + patternDuration;
        while (Time.time < end)
        {
            Fire(AimAngle(), bulletSpeed, new Color(1f, 0.6f, 0.3f), BulletShape.Rice);
            yield return new WaitForSeconds(aimedShotInterval);
        }
    }

    private void Fire(float angle, float speed, Color color, BulletShape shape, float accel = 0f, float angularSpeed = 0f)
    {
        if (IsSummoned) color = new Color(0.3f, 0.55f, 1f);
        EnemyBulletPool.Instance.Spawn(transform.position, angle, speed, color, accel, angularSpeed, shape);
    }

    private float AimAngle()
    {
        var player = PlayerHit.Instance;
        if (player == null) return -90f;
        Vector2 d = player.HitPosition - (Vector2)transform.position;
        return Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
