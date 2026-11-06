using System.Collections;
using UnityEngine;

// 플레이어 피격 판정. 그림 전체가 아니라 중심의 작은 원만 판정이 있음
public class PlayerHit : MonoBehaviour
{
    public static PlayerHit Instance { get; private set; }

    [SerializeField] private float hitRadius = 0.05f;
    [SerializeField] private float invincibleTime = 2f;
    [Tooltip("선택된 캐릭터 데이터가 없을 때 쓰는 체력")]
    [SerializeField] private int defaultHp = 100;

    public float HitRadius => hitRadius;
    public Vector2 HitPosition => transform.position;
    public bool CanBeHit => !isInvincible && hp > 0;
    public int Hp => hp;
    public int MaxHp => maxHp;

    // 체력이 바뀔 때마다 호출됨 (체력 UI 갱신용)
    public event System.Action OnHpChanged;

    private int hp;
    private int maxHp;
    private bool isInvincible = false;
    private SpriteRenderer sr;
    private PlayerMove move;
    private GameObject hitboxView;

    private void Awake()
    {
        Instance = this;
        sr = GetComponent<SpriteRenderer>();
        move = GetComponent<PlayerMove>();

        // 체력 UI가 Start에서 읽을 수 있도록 Awake에서 초기화
        maxHp = SelectScene.SelectedData != null ? SelectScene.SelectedData.playerHealth : defaultHp;
        hp = maxHp;
    }

    private void Start()
    {
        CreateHitboxView();
    }

    private void LateUpdate()
    {
        // 저속 이동(Focus) 중에만 판정점 표시
        // 플레이어 스케일이 프레임마다 바뀌므로 자식이 아닌 별도 오브젝트로 따라다님
        if (hitboxView == null) return;
        hitboxView.transform.position = transform.position;
        hitboxView.SetActive(move != null && move.IsFocused && hp > 0);
    }

    public void TakeHit(int damage)
    {
        if (!CanBeHit) return;

        hp = Mathf.Max(hp - damage, 0);
        Debug.Log($"[PlayerHit] 피격! 남은 체력 : {hp}");
        OnHpChanged?.Invoke();


        if (hp <= 0)
        {
            Debug.Log("[PlayerHit] 게임 오버");
            gameObject.SetActive(false);
            return;
        }

        StartCoroutine(Invincible());
    }

    // 무적 시간 동안 깜빡임
    private IEnumerator Invincible()
    {
        isInvincible = true;
        float t = 0f;
        while (t < invincibleTime)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.08f);
            t += 0.08f;
        }
        if (sr != null) sr.enabled = true;
        isInvincible = false;
    }

    private void CreateHitboxView()
    {
        // 빨간 테두리 + 흰 심지
        hitboxView = new GameObject("PlayerHitbox");
        hitboxView.transform.localScale = Vector3.one * hitRadius * 2f * 1.8f;
        var rim = hitboxView.AddComponent<SpriteRenderer>();
        
        rim.color = new Color(1f, 0.2f, 0.2f);
        rim.sortingOrder = 30;

        var core = new GameObject("Core");
        core.transform.SetParent(hitboxView.transform, false);
        core.transform.localScale = Vector3.one * 0.6f;
        var coreSr = core.AddComponent<SpriteRenderer>();
        coreSr.sortingOrder = 31;

        hitboxView.SetActive(false);
    }

    private void OnDisable()
    {
        if (hitboxView != null) hitboxView.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (hitboxView != null) Destroy(hitboxView);
    }
}
