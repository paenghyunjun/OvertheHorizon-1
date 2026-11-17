using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public enum State { Ready, Wave, WaveDelay, Boss, RoundClear, GameClear }

    [Serializable]
    public class EnemyGroup
    {
        public EnemyKind kind = EnemyKind.Starling;
        [Tooltip("비워두면 종류별 기본 원형 적을 사용")]
        public Enemy prefab;
        public int count = 10;
        public int hp = 20;
        [Tooltip("한 마리 처치 시 점수")]
        public int score = 100;
        [Tooltip("조준탄 발사 간격(초)")]
        public float attackInterval = 0.75f;
        [Tooltip("화면 위쪽에서 얼마나 내려와 멈출지. 작을수록 뒤(위)쪽 줄")]
        public float stopFromTop = 2.5f;
    }

    [Serializable]
    public class Round
    {
        public string name = "Stage 1";

        [Header("일반 스테이지")]
        [Tooltip("무리 하나에 함께 나오는 적 구성")]
        public EnemyGroup[] waveGroups;
        [Tooltip("무리가 몇 번 나오는지")]
        public int waveCount = 5;
        [Tooltip("앞 무리가 전멸한 뒤 다음 무리가 나오기까지(초)")]
        public float waveDelay = 2f;

        [Header("보스")]
        public bool hasBoss = true;
        public EnemyGroup boss;

        [Header("보스 소환")]
        [Tooltip("보스 체력이 이 비율 이하가 될 때마다 한 번씩 소환 (큰 값부터)")]
        public float[] summonHpRatios;
        public EnemyGroup[] summonGroups;
        [Tooltip("소환몹 체력 배율")]
        public float summonStatScale = 0.5f;
        [Tooltip("소환몹 점수 배율")]
        public float summonScoreScale = 1.5f;
        [Tooltip("소환 후 이 시간이 지나면 살아남은 소환몹은 퇴장(초)")]
        public float summonDuration = 10f;

        public static Round Stage1()
        {
            return new Round
            {
                name = "Stage 1",
                waveGroups = new[]
                {
                    new EnemyGroup { kind = EnemyKind.Starling, count = 10, hp = 20, score = 100, attackInterval = 0.75f, stopFromTop = 2.6f },
                    new EnemyGroup { kind = EnemyKind.HoneyBuzzard, count = 5, hp = 25, score = 200, attackInterval = 1.5f, stopFromTop = 1.3f },
                },
                waveCount = 5,
                waveDelay = 2f,
                hasBoss = true,
                boss = new EnemyGroup { kind = EnemyKind.GoldenEagle, count = 1, hp = 3000, score = 10000, attackInterval = 0.75f, stopFromTop = 2f },
                summonHpRatios = new[] { 0.75f, 0.5f, 0.25f },
                summonGroups = new[]
                {
                    new EnemyGroup { kind = EnemyKind.Starling, count = 10, hp = 20, score = 100, attackInterval = 0.75f, stopFromTop = 3.8f },
                    new EnemyGroup { kind = EnemyKind.HoneyBuzzard, count = 2, hp = 25, score = 200, attackInterval = 1.5f, stopFromTop = 3.2f },
                },
                summonStatScale = 0.5f,
                summonScoreScale = 1.5f,
                summonDuration = 10f,
            };
        }
    }

    public static RoundManager Instance { get; private set; }

    [Tooltip("씬 시작 후 첫 무리가 나오기까지(초)")]
    [SerializeField] private float firstDelay = 1.5f;
    [Tooltip("보스를 잡은 뒤 다음 라운드가 시작되기까지(초)")]
    [SerializeField] private float roundDelay = 3f;
    [Tooltip("라운드 목록. 마지막 라운드의 보스가 최종 보스")]
    [SerializeField] private Round[] rounds = { Round.Stage1() };

    [Header("기본 적 모습 (그룹의 Prefab 칸을 비웠을 때 사용)")]
    [SerializeField] private Sprite[] starlingFrames;
    [SerializeField] private Sprite[] honeyBuzzardFrames;
    [SerializeField] private Sprite[] goldenEagleFrames;

    public event Action<int> OnRoundStarted;
    public event Action<int, int> OnWaveStarted;
    public event Action<Enemy> OnBossSpawned;
    public event Action<int> OnRoundCleared;
    public event Action OnGameCleared;

    public State CurrentState { get; private set; } = State.Ready;
    public int CurrentRound { get; private set; }
    public int RoundCount => rounds != null ? rounds.Length : 0;
    public int CurrentWave { get; private set; }
    public int WaveCount { get; private set; }
    public Enemy Boss { get; private set; }
    public int AliveCount => alive.Count;

    private readonly List<Enemy> alive = new List<Enemy>();
    private readonly List<Enemy> summoned = new List<Enemy>();
    private readonly Dictionary<EnemyKind, Enemy> templates = new Dictionary<EnemyKind, Enemy>();
    private readonly List<Enemy> buffer = new List<Enemy>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private IEnumerator Start()
    {
        ScoreManager.Instance.ResetScore();
        yield return new WaitForSeconds(firstDelay);

        for (int r = 0; r < rounds.Length; r++)
        {
            var round = rounds[r];
            CurrentRound = r + 1;
            OnRoundStarted?.Invoke(CurrentRound);

            yield return RunWaves(round);
            if (round.hasBoss && round.boss != null) yield return RunBoss(round);

            OnRoundCleared?.Invoke(CurrentRound);

            if (r < rounds.Length - 1)
            {
                CurrentState = State.RoundClear;
                yield return new WaitForSeconds(roundDelay);
            }
        }

        CurrentState = State.GameClear;
        OnGameCleared?.Invoke();
    }

    private IEnumerator RunWaves(Round round)
    {
        WaveCount = round.waveGroups != null && round.waveGroups.Length > 0 ? round.waveCount : 0;

        for (int w = 1; w <= WaveCount; w++)
        {
            CurrentState = State.Wave;
            CurrentWave = w;
            OnWaveStarted?.Invoke(w, WaveCount);

            foreach (var group in round.waveGroups) SpawnGroup(group, 1f, 1f, false, alive);

            while (alive.Count > 0) yield return null;

            if (w < WaveCount)
            {
                CurrentState = State.WaveDelay;
                yield return new WaitForSeconds(round.waveDelay);
            }
        }
    }

    private IEnumerator RunBoss(Round round)
    {
        CurrentState = State.Boss;

        bool bossAlive = true;
        Boss = SpawnOne(round.boss, 0.5f, 1f, 1f, false);
        Boss.OnRemoved += _ => bossAlive = false;
        OnBossSpawned?.Invoke(Boss);

        int nextSummon = 0;
        while (bossAlive)
        {
            while (round.summonHpRatios != null && nextSummon < round.summonHpRatios.Length
                   && Boss.HpRatio <= round.summonHpRatios[nextSummon])
            {
                nextSummon++;
                StartCoroutine(Summon(round));
            }
            yield return null;
        }

        Boss = null;

        buffer.Clear();
        buffer.AddRange(summoned);
        foreach (var e in buffer) e.Leave();
        buffer.Clear();
    }

    private IEnumerator Summon(Round round)
    {
        if (round.summonGroups == null) yield break;

        var batch = new List<Enemy>();
        foreach (var group in round.summonGroups)
            SpawnGroup(group, round.summonStatScale, round.summonScoreScale, true, batch);

        yield return new WaitForSeconds(round.summonDuration);

        foreach (var e in batch.ToArray()) e.Leave();
    }

    private void SpawnGroup(EnemyGroup group, float hpScale, float scoreScale, bool isSummon, List<Enemy> tracker)
    {
        if (group == null) return;

        for (int i = 0; i < group.count; i++)
        {
            float t = (i + 1f) / (group.count + 1f);
            var e = SpawnOne(group, t, hpScale, scoreScale, isSummon);

            tracker.Add(e);
            e.OnRemoved += dead => tracker.Remove(dead);

            if (isSummon)
            {
                summoned.Add(e);
                e.OnRemoved += dead => summoned.Remove(dead);
            }
        }
    }

    private Enemy SpawnOne(EnemyGroup group, float t, float hpScale, float scoreScale, bool isSummon)
    {
        var cam = Camera.main;
        float h = cam != null ? cam.orthographicSize : 5f;
        float w = cam != null ? h * cam.aspect : 9f;
        Vector2 c = cam != null ? (Vector2)cam.transform.position : Vector2.zero;

        float x = c.x + Mathf.Lerp(-w, w, t) * 0.8f;
        Vector2 target = new Vector2(x, c.y + h - group.stopFromTop);
        Vector2 start = new Vector2(x, c.y + h + 1f);

        var e = PullingManager.Instance.Spawn(TemplateFor(group), start, Quaternion.identity);
        e.Configure(group.kind, Mathf.RoundToInt(group.score * scoreScale), isSummon, group.attackInterval);
        e.Setup(target, Mathf.RoundToInt(group.hp * hpScale), group.prefab != null ? null : DefaultPatterns(group.kind));
        return e;
    }

    private static Enemy.Pattern[] DefaultPatterns(EnemyKind kind)
    {
        if (kind == EnemyKind.GoldenEagle)
            return new[] { Enemy.Pattern.Ring, Enemy.Pattern.AimedFan, Enemy.Pattern.Spiral, Enemy.Pattern.Curve };
        return new[] { Enemy.Pattern.AimedShot };
    }

    private Enemy TemplateFor(EnemyGroup group)
    {
        if (group.prefab != null) return group.prefab;
        if (templates.TryGetValue(group.kind, out var template)) return template;

        Sprite[] frames;
        float spriteScale, spriteHit, circleSize;
        Color circleColor;
        switch (group.kind)
        {
            case EnemyKind.HoneyBuzzard:
                frames = honeyBuzzardFrames; spriteScale = 1f; spriteHit = 0.4f;
                circleSize = 0.7f; circleColor = new Color(0.95f, 0.75f, 0.2f);
                break;
            case EnemyKind.GoldenEagle:
                frames = goldenEagleFrames; spriteScale = 1.5f; spriteHit = 1f;
                circleSize = 2.2f; circleColor = new Color(0.55f, 0.35f, 0.15f);
                break;
            default:
                frames = starlingFrames; spriteScale = 1f; spriteHit = 0.25f;
                circleSize = 0.45f; circleColor = new Color(0.9f, 0.2f, 0.5f);
                break;
        }
        bool hasSprite = frames != null && frames.Length > 0 && frames[0] != null;

        var go = new GameObject(group.kind.ToString());
        go.SetActive(false);
        go.transform.SetParent(transform, false);
        go.transform.localScale = Vector3.one * (hasSprite ? spriteScale : circleSize);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = hasSprite ? frames[0] : EnemyBulletPool.CircleSprite;
        sr.color = hasSprite ? Color.white : circleColor;
        sr.sortingOrder = 10;

        template = go.AddComponent<Enemy>();
        template.HitRadius = hasSprite ? spriteHit : circleSize * 0.5f;
        if (hasSprite) template.SetFrames(frames, group.kind == EnemyKind.GoldenEagle ? 4f : 7f);
        templates.Add(group.kind, template);
        return template;
    }
}
