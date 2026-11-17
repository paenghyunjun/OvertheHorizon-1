using UnityEngine;

public class BulletMove : MonoBehaviour, IPoolable
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float destroyTime;

    private float life;

    public void OnSpawned() => life = 0f;

    public void OnDespawned() { }

    private void OnEnable() => PlayerBullets.Active.Add(transform);
    private void OnDisable() => PlayerBullets.Active.Remove(transform);

    private void Update()
    {
        transform.Translate(speed * Time.deltaTime * Vector3.up);

        life += Time.deltaTime;
        if (life >= destroyTime) PullingManager.Release(gameObject);
    }
}
