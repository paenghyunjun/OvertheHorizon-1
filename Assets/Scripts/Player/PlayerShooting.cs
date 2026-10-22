using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject homingbulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("발사 설정")]
    [SerializeField] private float fireRate = 0.15f; // Z키 연사 간격 (초 단위)
    private float nextFireTime;

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // KeyManager에서 키를 가져오되, 없을 경우 기본 키(Z, X)로 안전하게 폴백
        Key attackKey = Key.Z;
        Key ultimateKey = Key.X;

        if (KeyManager.Instance != null && KeyManager.Instance.keys != null)
        {
            if (KeyManager.Instance.keys.TryGetValue(KeyAction.Attack, out var atk)) attackKey = atk;
            if (KeyManager.Instance.keys.TryGetValue(KeyAction.Ultimate, out var ult)) ultimateKey = ult;
        }

        // 기본 공격 (Z 키): 누르고 있으면 연사
        if (kb[attackKey].isPressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }

        // 유도탄 공격 (X 키): 누를 때마다 단발 발사
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
        Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
    }

    private void HomingShoot()
    {
        // homingbulletPrefab이 없으면 bulletPrefab으로 임시 발사하여 동작 여부 확인
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
        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }
}

