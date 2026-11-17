using UnityEngine;

public class HomingBullet : MonoBehaviour, IPoolable
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float destroyTime = 5f;
    [SerializeField] private Transform target;

    private float life;

    public void OnSpawned()
    {
        life = 0f;
        target = null;
    }

    public void OnDespawned() => target = null;

    private void OnEnable() => PlayerBullets.Active.Add(transform);
    private void OnDisable() => PlayerBullets.Active.Remove(transform);

    private void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy) FindNearestEnemy();

        if (target != null)
        {
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(direction * (speed * Time.deltaTime));
        }
        else
        {
            transform.Translate(speed * Time.deltaTime * Vector3.up);
        }

        life += Time.deltaTime;
        if (life >= destroyTime) PullingManager.Release(gameObject);
    }

    private void FindNearestEnemy()
    {
        target = null;
        float shortest = Mathf.Infinity;
        Vector2 pos = transform.position;

        var enemies = Enemy.Active;
        for (int i = 0; i < enemies.Count; i++)
        {
            float d = ((Vector2)enemies[i].transform.position - pos).sqrMagnitude;
            if (d >= shortest) continue;
            shortest = d;
            target = enemies[i].transform;
        }
    }
}
