
using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float destroyTime = 5f;
    [SerializeField] private Transform target;

    void Start()
    {
        Destroy(gameObject, destroyTime);
        FindNearestEnemy();
    }

    void Update()
    {
        // 타겟이 없거나 파괴되었으면 다시 탐색
        if (target == null)
        {
            FindNearestEnemy();
        }

        // 타겟이 있으면 유도 이동, 없으면 위쪽으로 직진
        if (target != null)
        {
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(direction * (speed * Time.deltaTime));
        }
        else
        {
            transform.Translate(speed * Time.deltaTime * Vector3.up);
        }
    }

    private void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return;

        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }

        target = nearestEnemy;
    }
}
