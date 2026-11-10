using UnityEngine;

// 각도 기준: 0 = 오른쪽, 90 = 위, -90 = 아래
public class EnemyBullet : MonoBehaviour
{
    [Tooltip("판정 반지름. 그림보다 작게 잡아야 억울한 피격이 없음")]
    [SerializeField] private float radius = 0.08f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float maxLifeTime = 20f;

    private SpriteRenderer sr;
    private float angle;
    private float speed;
    private float accel;
    private float angularSpeed;
    private float life;

    public float Radius => radius;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Init(Vector2 pos, float angle, float speed, Color color, float accel, float angularSpeed)
    {
        transform.position = pos;
        this.angle = angle;
        this.speed = speed;
        this.accel = accel;
        this.angularSpeed = angularSpeed;
        life = 0f;
        if (sr != null) sr.color = color;
        ApplyRotation();
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        life += dt;
        speed = Mathf.Max(0f, speed + accel * dt);
        angle += angularSpeed * dt;

        float rad = angle * Mathf.Deg2Rad;
        transform.position += new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * (speed * dt);
        if (angularSpeed != 0f) ApplyRotation();

        if (HitPlayer()) return;

        if (life > maxLifeTime || !EnemyBulletPool.Bounds.Contains(transform.position))
            EnemyBulletPool.Instance.Despawn(this);
    }

    // 원 대 원 거리 판정. 플레이어 쪽 판정 크기는 PlayerHit.HitRadius
    private bool HitPlayer()
    {
        var player = PlayerHit.Instance;
        if (player == null || !player.CanBeHit) return false;

        float r = radius + player.HitRadius;
        if (((Vector2)transform.position - player.HitPosition).sqrMagnitude > r * r) return false;

        player.TakeHit(damage);
        EnemyBulletPool.Instance.Despawn(this);
        return true;
    }

    // 탄 스프라이트는 위쪽을 바라보게 그려야 진행 방향과 맞음
    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
