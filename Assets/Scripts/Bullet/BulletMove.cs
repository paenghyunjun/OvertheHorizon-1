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

    // Update is called once per frame
    void Update()
    {
        transform.Translate(speed * Time.deltaTime * Vector3.up);//
    }
}