using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject homingbulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("발사 설정")]
    [SerializeField] private float fireRate = 0.15f;
    private float nextFireTime;

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        Key attackKey = Key.Z;
        Key ultimateKey = Key.X;

        if (KeyManager.Instance != null && KeyManager.Instance.keys != null)
        {
            if (KeyManager.Instance.keys.TryGetValue(KeyAction.Attack, out var atk)) attackKey = atk;
            if (KeyManager.Instance.keys.TryGetValue(KeyAction.Ultimate, out var ult)) ultimateKey = ult;
        }

        if (kb[attackKey].isPressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }

        if (kb[ultimateKey].wasPressedThisFrame)
        {
            HomingShoot();
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("[PlayerShooting] Bullet Prefab이 Inspector에 할당되지 않았습니다!");
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        PullingManager.Instance.Spawn(bulletPrefab, spawnPos, Quaternion.identity);
    }

    private void HomingShoot()
    {
        GameObject prefabToSpawn = homingbulletPrefab != null ? homingbulletPrefab : bulletPrefab;
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("[PlayerShooting] 발사할 총알 Prefab이 Inspector에 할당되지 않았습니다!");
            return;
        }

        if (homingbulletPrefab == null)
        {
            Debug.LogWarning("[PlayerShooting] Homingbullet Prefab이 할당되지 않아 일반 Bullet으로 대체 발사했습니다. Inspector에서 Homingbullet Prefab을 할당해주세요.");
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        PullingManager.Instance.Spawn(prefabToSpawn, spawnPos, Quaternion.identity);
    }
}
