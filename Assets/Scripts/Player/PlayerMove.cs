using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove: MonoBehaviour
{
    [SerializeField] private float speed = 0f;

    private Camera mainCamera;

    private void Start()
    {
        // SelectScene에서 선택한 데이터가 있는지 확인 후 적용
        if (SelectScene.SelectedData != null)
        {
            // 1. 속도 적용
            speed = SelectScene.SelectedData.playerSpeed*5;


            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && SelectScene.SelectedData.playerIcon != null)
            {
                spriteRenderer.sprite = SelectScene.SelectedData.playerIcon;
            }
        }
        else
        {
            Debug.LogWarning("선택된 캐릭터 데이터가 없습니다. 기본값으로 실행합니다.");
        }
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
        if (kb != null)
        {
            Key rightKey = Key.D, leftKey = Key.A, forwardKey = Key.W, backwardKey = Key.S;

            if (KeyManager.Instance != null && KeyManager.Instance.keys != null)
            {
                KeyManager.Instance.keys.TryGetValue(KeyAction.Right, out rightKey);
                KeyManager.Instance.keys.TryGetValue(KeyAction.Left, out leftKey);
                KeyManager.Instance.keys.TryGetValue(KeyAction.Forward, out forwardKey);
                KeyManager.Instance.keys.TryGetValue(KeyAction.Backward, out backwardKey);
            }

            if (kb[rightKey].isPressed) dir.x += 1f;
            if (kb[leftKey].isPressed) dir.x -= 1f;
            if (kb[forwardKey].isPressed) dir.y += 1f;
            if (kb[backwardKey].isPressed) dir.y -= 1f;
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