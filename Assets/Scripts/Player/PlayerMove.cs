using UnityEngine;
using UnityEngine.InputSystem;   // Input System 패키지의 네임스페이스

public class NewInputMover : MonoBehaviour
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

        // 화면 벽 제한 처리
        ClampToBounds();
    }

    private void ClampToBounds()
    {
        if (mainCamera == null) return;

        // 1) 현재 오브젝트의 월드 좌표를 뷰포트 좌표(0~1)로 변환
        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);

        // 2) X, Y 좌표를 0과 1 사이로 제한 (화면 밖으로 나가지 않음)
        viewPos.x = Mathf.Clamp01(viewPos.x);
        viewPos.y = Mathf.Clamp01(viewPos.y);

        // 3) 다시 월드 좌표로 변환하여 오브젝트 위치에 적용
        transform.position = mainCamera.ViewportToWorldPoint(viewPos);
    }

    private static Vector2 ReadMove()
    {
        var dir = Vector2.zero;

        // 1) 키보드 — "누르고 있는가" 는 isPressed, "이번 프레임 눌렸나" 는 wasPressedThisFrame
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) dir.x += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) dir.x -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) dir.y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) dir.y -= 1f;
        }

        // 2) 게임패드 — 스틱/십자키는 "값(Vector2)" 을 읽는다. 없으면 null
        var pad = Gamepad.current;
        if (pad != null)
        {
            dir += pad.leftStick.ReadValue();   // 아날로그 (-1~1, 데드존은 패키지가 처리)
            dir += pad.dpad.ReadValue();        // 디지털 (십자키)
        }

        return dir;
    }
}