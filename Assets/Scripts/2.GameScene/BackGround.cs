using UnityEngine;

public class BackGround : MonoBehaviour
{
    private Vector3 original_position; // Transform 대신 Vector3 좌표값으로 저장
    [SerializeField] private float speed = 0.1f;
    [SerializeField] private GameObject BackGrounds;
    [SerializeField] private float minusY = -10f;
    [SerializeField] private float resetY = 10f; // 다시 올라갈 Y 좌표 (필요에 따라 조절)

    private void Awake()
    {
        // 초기 위치의 '값'만 복사해서 저장
        original_position = transform.position;
    }

    void Update()
    {
        // 이동
        transform.position += Vector3.down * speed * Time.deltaTime;

        if (BackGrounds != null)
        {
            BackGrounds.transform.position += Vector3.down * speed * Time.deltaTime;
        }

        // Y축이 마이너스 한계에 도달하면 위치 재설정
        if (transform.position.y <= minusY)
        {
            // 단순히 원래 자리로 보내거나, 위쪽으로 순간 이동시킴
            transform.position = new Vector3(original_position.x, resetY, original_position.z);
        }

        if (BackGrounds != null && BackGrounds.transform.position.y <= minusY)
        {
            BackGrounds.transform.position = new Vector3(BackGrounds.transform.position.x, resetY, BackGrounds.transform.position.z);
        }
    }
}