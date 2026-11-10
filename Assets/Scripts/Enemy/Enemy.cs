using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum Pattern { Ring, AimedFan, Spiral, Curve }

    [Header("체력 / 피격")]
    [SerializeField] private int maxHp = 60;
    [Tooltip("플레이어 탄에 맞는 범위")]
    [SerializeField] private float hitRadius = 0.4f;

    [Header("이동")]
    [SerializeField] private float enterSpeed = 3f;
    [SerializeField] private float swayWidth = 1f;
    [SerializeField] private float swaySpeed = 0.6f;

    [Header("패턴")]
    [SerializeField] private Pattern[] patterns = { Pattern.Ring, Pattern.AimedFan, Pattern.Spiral, Pattern.Curve };
    [SerializeField] private float patternDuration = 4f;
    [SerializeField] private float patternRest = 0.8f;
    [SerializeField] private float bulletSpeed = 2.5f;

    public event System.Action<Enemy> OnDead;

    private int hp;
    private Vector2 anchor;
    private bool arrived;
    private SpriteRenderer sr;
    private Color baseColor;
    private float flashTimer;

    private void Awake()
    {
        hp = maxHp;
        anchor = transform.position;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
    }

    // 스포너가 생성 직후 호출. 화면 밖에서 target까지 내려온 뒤 패턴 시작
    public void Setup(Vector2 target, int hp, Pattern[] patterns = null)
    {
        anchor = target;
        maxHp = this.hp = hp;
        if (patterns != null && patterns.Length > 0) this.patterns = patterns;
    }

    private IEnumerator Start()
    {
        while (((Vector2)transform.position - anchor).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector2.MoveTowards(transform.position, anchor, enterSpeed * Time.deltaTime);
            yield return null;
        }
        arrived = true;

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
        if (arrived)
        {
            float x = Mathf.Sin(Time.time * swaySpeed * Mathf.PI) * swayWidth;
            transform.position = anchor + new Vector2(x, 0f);
        }

        CheckPlayerBullets();

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

            // 같은 프레임에 다른 적이 또 맞지 않도록 즉시 비활성화
            b.gameObject.SetActive(false);
            Destroy(b.gameObject);
            TakeDamage(1);
            if (hp <= 0) return;
        }
    }

    public void TakeDamage(int damage)
    {
        if (hp <= 0) return;
        hp -= damage;

        if (sr != null)
        {
            sr.color = Color.white;
            flashTimer = 0.05f;
        }

        if (hp <= 0)
        {
            OnDead?.Invoke(this);
            Destroy(gameObject);
        }
    }

    private IEnumerator RunPattern(Pattern p)
    {
        switch (p)
        {
            case Pattern.Ring: return Ring();
            case Pattern.AimedFan: return AimedFan();
            case Pattern.Spiral: return Spiral();
            default: return Curve();
        }
    }

    // 전방위 원형탄. 매 발사마다 반 칸씩 엇갈려 틈이 생김
    private IEnumerator Ring()
    {
        const int count = 24;
        float step = 360f / count;
        float offset = 0f;
        float end = Time.time + patternDuration;
        while (Time.time < end)
        {
            for (int i = 0; i < count; i++)
                Fire(offset + step * i, bulletSpeed, new Color(1f, 0.35f, 0.35f));
            offset += step * 0.5f;
            yield return new WaitForSeconds(0.45f);
        }
    }

    // 플레이어 조준 부채꼴 3연사
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
                    Fire(aim + (i - (ways - 1) * 0.5f) * spread, bulletSpeed * 1.6f, new Color(0.4f, 0.7f, 1f));
                yield return new WaitForSeconds(0.08f);
            }
            yield return new WaitForSeconds(0.6f);
        }
    }

    // 3갈래 회전 나선
    private IEnumerator Spiral()
    {
        const int arms = 3;
        float angle = 0f;
        float end = Time.time + patternDuration;
        while (Time.time < end)
        {
            for (int i = 0; i < arms; i++)
                Fire(angle + 360f / arms * i, bulletSpeed, new Color(1f, 0.85f, 0.3f));
            angle += 11f;
            yield return new WaitForSeconds(0.05f);
        }
    }

    // 좌우로 휘어지는 탄. 감속 후 가속해서 타이밍이 어긋나게 함
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
                Fire(step * i, bulletSpeed * 0.6f, new Color(0.75f, 0.45f, 1f), 0.5f, turn);
            flip = !flip;
            yield return new WaitForSeconds(0.35f);
        }
    }

    private void Fire(float angle, float speed, Color color, float accel = 0f, float angularSpeed = 0f)
    {
        EnemyBulletPool.Instance.Spawn(transform.position, angle, speed, color, accel, angularSpeed);
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
