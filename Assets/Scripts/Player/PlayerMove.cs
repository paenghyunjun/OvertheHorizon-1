using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove: MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Vector2 dir = ReadMove();

        if (dir.sqrMagnitude > 1f) dir.Normalize();
        transform.Translate(dir * (speed * Time.deltaTime));

        ClampToBounds();
    }

    private void ClampToBounds()
    {
        if (mainCamera == null) return;

        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);

        viewPos.x = Mathf.Clamp01(viewPos.x);
        viewPos.y = Mathf.Clamp01(viewPos.y);

        transform.position = mainCamera.ViewportToWorldPoint(viewPos);
    }

    private static Vector2 ReadMove()
    {
        var dir = Vector2.zero;

        // 1) 키보드 입력 처리
        var kb = Keyboard.current;
        if (kb != null && KeyManager.Instance != null)
        {
            if (kb[KeyManager.Instance.keys[KeyAction.Right]].isPressed) dir.x += 1f;
            if (kb[KeyManager.Instance.keys[KeyAction.Left]].isPressed) dir.x -= 1f;
            if (kb[KeyManager.Instance.keys[KeyAction.Forward]].isPressed) dir.y += 1f;
            if (kb[KeyManager.Instance.keys[KeyAction.Backward]].isPressed) dir.y -= 1f;
        }

        // 2) 게임패드 입력 처리
        var pad = Gamepad.current;
        if (pad != null)
        {
            dir += pad.leftStick.ReadValue();
            dir += pad.dpad.ReadValue();
        }

        return dir;
    }
}