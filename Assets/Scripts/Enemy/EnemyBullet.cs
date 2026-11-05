using System.Collections.Generic;
using UnityEngine;

// 적 탄환. 수백 발을 빠르게 만들고 지우기 위해 오브젝트 풀을 사용
// 각도 기준: 0도 = 오른쪽, 90도 = 위, -90도(270도) = 아래
public class EnemyBullet : MonoBehaviour
{
    private static readonly Stack<EnemyBullet> pool = new Stack<EnemyBullet>();
    private static readonly List<EnemyBullet> active = new List<EnemyBullet>();
    private static Transform poolRoot;
    private static Camera cam;

    private const float MaxSpeed = 15f;
    private const float MaxLifeTime = 20f;
    private const float ScreenMargin = 1f;

    public static int ActiveCount => active.Count;

    private SpriteRenderer body;
    private float angle;           // 진행 방향 (도)
    private float speed;           // 현재 속도
    private float accel;           // 초당 속도 변화량 (음수면 감속)
    private float angularVelocity; // 초당 방향 회전량 (휘는 탄)
    private float hitRadius;
    private int damage;
    private float life;

    /// <summary>
    /// 탄 한 발 발사
    /// </summary>
    /// <param name="size">보이는 지름. 실제 판정은 이보다 작음 (보이는 것보다 판정이 작은 게 탄막 게임의 기본)</param>
    public static EnemyBullet Spawn(Vector2 position, float angle, float speed, Color color,
        float size = 0.25f, float accel = 0f, float angularVelocity = 0f, int damage = 10)
    {
        EnsureRoot();

        // 풀에 파괴된 탄이 섞여 있으면 건너뜀
        EnemyBullet b = null;
        while (b == null && pool.Count > 0) b = pool.Pop();
        if (b == null) b = Create();

        b.transform.position = position;
        b.transform.localScale = Vector3.one * size;
        b.body.color = color;
        b.angle = angle;
        b.speed = speed;
        b.accel = accel;
        b.angularVelocity = angularVelocity;
        b.hitRadius = size * 0.35f;
        b.damage = damage;
        b.life = 0f;
        b.gameObject.SetActive(true);
        active.Add(b);
        return b;
    }

    /// <summary>
    /// 화면의 모든 적 탄 제거 (피격, 페이즈 전환, 보스 격파 시)
    /// </summary>
    public static void ClearAll()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            // 씬 전환 등으로 이미 파괴된 탄은 목록에서만 제거
            if (active[i] == null)
            {
                active.RemoveAt(i);
                continue;
            }
            active[i].Despawn();
        }
    }

    // 플레이 시작 / 씬 로드 때 static 목록 초기화
    // (static 값은 씬이 바뀌어도, Domain Reload를 끈 경우 플레이를 다시 눌러도 남아 있음)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        pool.Clear();
        active.Clear();
        poolRoot = null;
        cam = null;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 이전 씬의 탄들은 이미 파괴되었으므로 목록을 비움
        if (mode == UnityEngine.SceneManagement.LoadSceneMode.Single)
        {
            pool.Clear();
            active.Clear();
            cam = null;
        }
    }

    private static void EnsureRoot()
    {
        // 씬이 바뀌면 이전 씬의 풀 오브젝트는 같이 파괴되므로 목록을 새로 시작
        if (poolRoot == null)
        {
            pool.Clear();
            active.Clear();
            poolRoot = new GameObject("EnemyBullets").transform;
        }
    }

    private static EnemyBullet Create()
    {
        var go = new GameObject("EnemyBullet");
        go.SetActive(false);
        go.transform.SetParent(poolRoot, false);

        // 바깥: 색이 들어간 원
        var body = go.AddComponent<SpriteRenderer>();
        body.sprite = DanmakuSprites.Circle;
        body.sortingOrder = 20;

        // 안쪽: 흰 심지 (탄이 잘 보이게)
        var coreGo = new GameObject("Core");
        coreGo.transform.SetParent(go.transform, false);
        coreGo.transform.localScale = Vector3.one * 0.55f;
        var core = coreGo.AddComponent<SpriteRenderer>();
        core.sprite = DanmakuSprites.Circle;
        core.sortingOrder = 21;

        var bullet = go.AddComponent<EnemyBullet>();
        bullet.body = body;
        return bullet;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        life += dt;

        // 1. 가속/감속과 방향 회전 적용
        speed = Mathf.Clamp(speed + accel * dt, 0f, MaxSpeed);
        angle += angularVelocity * dt;

        // 2. 이동
        float rad = angle * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        transform.position += dir * (speed * dt);

        // 3. 플레이어 판정 (원 vs 원 거리 비교)
        PlayerHit player = PlayerHit.Instance;
        if (player != null && player.CanBeHit)
        {
            float r = hitRadius + player.HitRadius;
            if (((Vector2)transform.position - player.HitPosition).sqrMagnitude < r * r)
            {
                Despawn();
                player.TakeHit(damage);
                return;
            }
        }

        // 4. 화면 밖으로 나갔거나 너무 오래 살아있으면 회수
        if (life > MaxLifeTime || IsOutOfScreen())
        {
            Despawn();
        }
    }

    private bool IsOutOfScreen()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;

        // 가속탄이 화면 밖에서 돌아올 수 있도록 약간의 여유를 둠
        float h = cam.orthographicSize + ScreenMargin;
        float w = cam.orthographicSize * cam.aspect + ScreenMargin;
        Vector3 p = transform.position - cam.transform.position;
        return p.x < -w || p.x > w || p.y < -h || p.y > h;
    }

    private void Despawn()
    {
        if (!gameObject.activeSelf) return;

        gameObject.SetActive(false);
        active.Remove(this);
        pool.Push(this);
    }
}
