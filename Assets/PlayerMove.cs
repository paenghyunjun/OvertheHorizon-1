using Unity.VisualScripting;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private SpriteRenderer spriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer> ();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");//오른쪽 왼쪽을 입력 받을 수 있다.
        float vertical = Input.GetAxisRaw("Vertical");//위 아래를 입력 받을 수 있다.

        Vector2 movement = new Vector2(horizontal, vertical).normalized; //Vector2는 2차원이다.그러니까 우리가 필요한건 x,y를 집어 넣어야하는데,위에서 만들었으니까 가져오면 된다.

        this.transform.position += (Vector3)(movement * speed * Time.deltaTime);

    }
}
