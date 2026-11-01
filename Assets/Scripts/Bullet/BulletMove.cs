using UnityEngine;

public class BulletMove : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float destroyTime;
    // Start is called once before the first execution
    //of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    // 적이 피격 검사를 할 수 있도록 목록에 등록
    private void OnEnable() => PlayerBullets.Active.Add(transform);
    private void OnDisable() => PlayerBullets.Active.Remove(transform);

    // Update is called once per frame
    void Update()
    {
        transform.Translate(speed * Time.deltaTime * Vector3.up);//
    }
}