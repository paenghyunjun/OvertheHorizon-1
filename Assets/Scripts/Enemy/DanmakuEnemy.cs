using System.Collections;
using DG.Tweening;
using UnityEngine;

// 박스를 임시 보스로 쓰는 탄막 적
// 체력에 따라 3페이즈로 나뉘고, 페이즈마다 다른 탄막 패턴을 반복함
[RequireComponent(typeof(SpriteRenderer))]
public class DanmakuEnemy : MonoBehaviour
{
    [Header("체력")]
    [SerializeField] private int maxHp = 300;
    [SerializeField] private int damagePerBullet = 1;

    [Header("이동")]
    [Tooltip("등장 후 자리잡을 위치")]
    [SerializeField] private Vector2 appearPosition = new Vector2(0f, 3f);
    [SerializeField] private float appearTime = 1.5f;
    [Tooltip("패턴 사이에 좌우로 움직이는 범위")]
    [SerializeField] private float moveRangeX = 2.5f;

    [Header("탄 색")]
    [SerializeField] private Color ringColor = new Color(0.3f, 0.8f, 1f);
    [SerializeField] private Color aimedColor = new Color(1f, 0.25f, 0.3f);
    [SerializeField] private Color spiralColor = new Color(0.9f, 0.3f, 1f);
    [SerializeField] private Color flowerColorA = new Color(1f, 0.9f, 0.2f);
    [SerializeField] private Color flowerColorB = new Color(0.3f, 1f, 0.5f);

    private SpriteRenderer sr;
    private Color baseColor;
    private int hp;
    private int phase = -1;
    private bool invincible = true;
    private bool dead = false;

    // 화면 위쪽 체력바
    private Transform hpBarFill;
    private Transform hpBarBack;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr.sprite == null) sr.sprite = DanmakuSprites.Square;
        baseColor = sr.color;

        // HomingBullet이 "Enemy" 태그로 적을 찾음
        gameObject.tag = "Enemy";
        hp = maxHp;
    }

    private void Start()
    {
        CreateHpBar();
        StartCoroutine(Appear());
    }

    private void Update()
    {
        if (!dead) CheckPlayerBullets();
    }

    private void LateUpdate()
    {
        UpdateHpBar();
    }

    // ───────────── 등장 / 페이즈 관리 ─────────────

    private IEnumerator Appear()
    {
        invincible = true;
        yield return transform.DOMove(appearPosition, appearTime).SetEase(Ease.OutCubic).WaitForCompletion();
        StartPhase(0);
    }

    private int CalcPhase()
    {
        float ratio = (float)hp / maxHp;
        if (ratio > 0.66f) return 0;
        if (ratio > 0.33f) return 1;
        return 2;
    }

    private void StartPhase(int newPhase)
    {
        phase = newPhase;

        // 패턴 중단 + 화면 탄 제거 (스펠 브레이크 연출)
        StopAllCoroutines();
        EnemyBullet.ClearAll();
        StartCoroutine(PhaseLoop(newPhase));
    }

    private IEnumerator PhaseLoop(int p)
    {
        // 페이즈 시작 직후 잠깐 무적 + 숨 고르기
        invincible = true;
        transform.DOKill();
        yield return transform.DOMove(appearPosition, 0.8f).SetEase(Ease.OutQuad).WaitForCompletion();
        invincible = false;
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            switch (p)
            {
                case 0:
                    yield return RingBurst(3, 20, 2.5f, 0.4f, ringColor);
                    yield return AimedFan(3, 5, 12f, 4f, aimedColor);
                    break;
                case 1:
                    yield return Spiral(4f, 3, 13f, 0.06f, 3f, spiralColor);
                    yield return AimedFan(2, 7, 10f, 5f, aimedColor);
                    break;
                default:
                    yield return Flower(5, 16, 2f, 35f, flowerColorA, flowerColorB);
                    yield return Spiral(3f, 4, -9f, 0.05f, 3.5f, ringColor);
                    break;
            }
            yield return MoveRandom();
        }
    }

    // ───────────── 탄막 패턴 ─────────────

    // 원형 탄: 360도로 같은 간격으로 퍼짐. 웨이브마다 반 칸씩 엇갈려서 빈틈이 바뀜
    private IEnumerator RingBurst(int waves, int count, float speed, float interval, Color color)
    {
        float offset = Random.Range(0f, 360f);
        float step = 360f / count;
        for (int w = 0; w < waves; w++)
        {
            for (int i = 0; i < count; i++)
            {
                EnemyBullet.Spawn(transform.position, offset + step * i, speed, color);
            }
            offset += step * 0.5f;
            yield return new WaitForSeconds(interval);
        }
    }

    // 자기 저격 부채꼴: 발사 순간의 플레이어 방향을 중심으로 여러 갈래로 쏨
    private IEnumerator AimedFan(int bursts, int ways, float spread, float speed, Color color)
    {
        for (int b = 0; b < bursts; b++)
        {
            float center = AimAngle();
            for (int i = 0; i < ways; i++)
            {
                float a = center + (i - (ways - 1) / 2f) * spread;
                EnemyBullet.Spawn(transform.position, a, speed + b * 0.4f, color, 0.3f);
            }
            yield return new WaitForSeconds(0.25f);
        }
        yield return new WaitForSeconds(0.4f);
    }

    // 회전 탄: 매 발사마다 각도를 조금씩 돌려서 소용돌이 모양
    private IEnumerator Spiral(float duration, int arms, float rotateStep, float interval, float speed, Color color)
    {
        float a = Random.Range(0f, 360f);
        float t = 0f;
        while (t < duration)
        {
            for (int k = 0; k < arms; k++)
            {
                EnemyBullet.Spawn(transform.position, a + 360f / arms * k, speed, color, 0.22f);
            }
            a += rotateStep;
            t += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    // 꽃 모양: 같은 방향으로 두 발을 쏘되 서로 반대로 휘게 해서 꽃잎이 그려짐
    private IEnumerator Flower(int waves, int count, float speed, float curve, Color colorA, Color colorB)
    {
        float step = 360f / count;
        for (int w = 0; w < waves; w++)
        {
            float offset = w * 7f;
            for (int i = 0; i < count; i++)
            {
                float a = offset + step * i;
                EnemyBullet.Spawn(transform.position, a, speed, colorA, 0.25f, 0.5f, curve);
                EnemyBullet.Spawn(transform.position, a, speed, colorB, 0.25f, 0.5f, -curve);
            }
            yield return new WaitForSeconds(0.6f);
        }
    }

    private IEnumerator MoveRandom()
    {
        Vector2 target = new Vector2(
            Random.Range(-moveRangeX, moveRangeX),
            appearPosition.y + Random.Range(-0.5f, 0.5f));
        yield return transform.DOMove(target, 1f).SetEase(Ease.InOutSine).WaitForCompletion();
    }

    // 플레이어를 향하는 각도 (플레이어가 없으면 아래쪽)
    private float AimAngle()
    {
        if (PlayerHit.Instance == null) return -90f;
        Vector2 d = PlayerHit.Instance.HitPosition - (Vector2)transform.position;
        return Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
    }

    // ───────────── 피격 / 격파 ─────────────

    // 플레이어 탄이 박스 영역 안에 들어왔는지 검사
    private void CheckPlayerBullets()
    {
        Bounds bounds = sr.bounds;
        var list = PlayerBullets.Active;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            Transform b = list[i];
            if (b == null) { list.RemoveAt(i); continue; }

            Vector3 p = b.position;
            p.z = bounds.center.z;
            if (!bounds.Contains(p)) continue;

            list.RemoveAt(i);
            Destroy(b.gameObject);
            TakeDamage(damagePerBullet);
            if (dead) return;
        }
    }

    private void TakeDamage(int damage)
    {
        if (invincible || dead) return;

        hp = Mathf.Max(hp - damage, 0);

        // 맞을 때 하얗게 번쩍임
        sr.DOKill();
        sr.color = Color.white;
        sr.DOColor(baseColor, 0.1f);

        if (hp <= 0)
        {
            Die();
            return;
        }

        int newPhase = CalcPhase();
        if (newPhase != phase) StartPhase(newPhase);
    }

    private void Die()
    {
        dead = true;
        StopAllCoroutines();
        EnemyBullet.ClearAll();
        transform.DOKill();
        transform.DOScale(0f, 0.4f).SetEase(Ease.InBack).OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if (sr != null) sr.DOKill();
        if (hpBarBack != null) Destroy(hpBarBack.gameObject);
    }

    // ───────────── 체력바 ─────────────

    private void CreateHpBar()
    {
        hpBarBack = CreateBarPart("EnemyHpBar", new Color(0f, 0f, 0f, 0.5f), 40, null);
        hpBarFill = CreateBarPart("Fill", new Color(1f, 0.3f, 0.4f), 41, hpBarBack);
    }

    private static Transform CreateBarPart(string name, Color color, int order, Transform parent)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = DanmakuSprites.Square;
        r.color = color;
        r.sortingOrder = order;
        return go.transform;
    }

    private void UpdateHpBar()
    {
        Camera cam = Camera.main;
        if (cam == null || hpBarBack == null) return;

        float width = cam.orthographicSize * cam.aspect * 2f * 0.9f;
        float height = 0.08f;
        Vector3 top = cam.transform.position + new Vector3(0f, cam.orthographicSize - 0.25f, 0f);
        top.z = 0f;

        hpBarBack.position = top;
        hpBarBack.localScale = new Vector3(width, height, 1f);

        // 왼쪽 끝을 고정한 채로 비율만큼 줄어들게
        float ratio = (float)hp / maxHp;
        hpBarFill.localScale = new Vector3(ratio, 1f, 1f);
        hpBarFill.localPosition = new Vector3((ratio - 1f) * 0.5f, 0f, 0f);
    }
}
