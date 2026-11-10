using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 웨이브마다 적을 화면 위에서 내려보냄. 웨이브의 적이 전부 죽으면 다음 웨이브
public class EnemySpawner : MonoBehaviour
{
    [Tooltip("비워두면 기본 원형 적을 사용")]
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private float firstDelay = 1.5f;
    [SerializeField] private float waveDelay = 2f;
    [SerializeField] private int maxPerWave = 3;
    [SerializeField] private int baseHp = 40;
    [SerializeField] private int hpPerWave = 15;
    [Tooltip("화면 위쪽에서 얼마나 내려와 멈출지")]
    [SerializeField] private float stopFromTop = 1.5f;

    private readonly List<Enemy> alive = new List<Enemy>();
    private int wave;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(firstDelay);
        while (true)
        {
            wave++;
            SpawnWave();
            while (alive.Count > 0) yield return null;
            yield return new WaitForSeconds(waveDelay);
        }
    }

    private void SpawnWave()
    {
        var cam = Camera.main;
        float h = cam != null ? cam.orthographicSize : 5f;
        float w = cam != null ? h * cam.aspect : 9f;
        Vector2 c = cam != null ? (Vector2)cam.transform.position : Vector2.zero;

        int count = Mathf.Min(wave, maxPerWave);
        int hp = baseHp + hpPerWave * (wave - 1);

        for (int i = 0; i < count; i++)
        {
            float t = (i + 1f) / (count + 1f);
            float x = c.x + Mathf.Lerp(-w, w, t) * 0.7f;
            Vector2 target = new Vector2(x, c.y + h - stopFromTop);
            Vector2 start = new Vector2(x, c.y + h + 1f);

            var e = CreateEnemy(start);
            // 프리팹을 넣었으면 프리팹에 설정한 패턴 순서를 그대로 씀
            e.Setup(target, hp, enemyPrefab == null ? PatternsFor(i) : null);
            e.OnDead += dead => alive.Remove(dead);
            alive.Add(e);
        }
    }

    // 여러 마리일 때 같은 패턴이 겹치지 않도록 시작 패턴을 밀어줌
    private Enemy.Pattern[] PatternsFor(int index)
    {
        var all = (Enemy.Pattern[])System.Enum.GetValues(typeof(Enemy.Pattern));
        var result = new Enemy.Pattern[all.Length];
        for (int i = 0; i < all.Length; i++) result[i] = all[(i + index + wave) % all.Length];
        return result;
    }

    private Enemy CreateEnemy(Vector2 pos)
    {
        if (enemyPrefab != null) return Instantiate(enemyPrefab, pos, Quaternion.identity);

        var go = new GameObject("Enemy");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.8f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = EnemyBulletPool.CircleSprite;
        sr.color = new Color(0.9f, 0.2f, 0.5f);
        sr.sortingOrder = 10;
        return go.AddComponent<Enemy>();
    }
}
