using System.Collections.Generic;
using UnityEngine;

// 적 탄환. 수백 발을 빠르게 만들고 지우기 위해 오브젝트 풀을 사용
// 각도 기준: 0도 = 오른쪽, 90도 = 위, -90도(270도) = 아래
public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float Speed = 5f; // 예시 기본값 설정
    [SerializeField] private float LifeTime = 3f; // 탄환이 사라지기까지 걸리는 시간
    [SerializeField] private float Damage = 1f;   // 플레이어에게 입히는 데미지

    // 주석 처리: 자기 자신을 굳이 참조할 필요가 없다면 삭제해도 됩니다.
    // [SerializeField] private GameObject bullet; 

    public float bulletAngle;

    // 무한 루프를 없애고 매 프레임 이동하도록 Update로 변경
    private void Update()
    {
        // Space.Self 기준으로 위쪽(Vector2.up)으로 이동
        this.gameObject.transform.Translate(Vector2.up * Speed * Time.deltaTime, Space.Self);
    }

    // 화면 밖으로 나갔을 때의 처리
    private void OnBecameInvisible()
    {
        // [중요] 오브젝트 풀을 사용하므로 Destroy 대신 비활성화를 해야 합니다!
        this.gameObject.SetActive(false);

        // 만약 풀링 매니저에 반납하는 구조라면 여기에 반납 코드를 넣습니다.
        // 예: PoolManager.ReturnBullet(this.gameObject);
    }
}
