using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    private float speed = 0f;

    private Camera mainCamera;

    // 스프라이트 애니메이션 자동 재생용
    private SpriteRenderer spriteRenderer;
    private Sprite[] animFrames;
    private float animFps = 8f;
    private float animTimer = 0f;
    private int animIndex = 0;

    // 크기 고정용: 첫 스프라이트 크기를 기준으로, 프레임마다 크기가 달라도 같은 크기로 보이게 함
    private float baseScale = 1f;
    private float referenceSize = 0f;

    private const float MinScale = 0.05f;
    private const float MaxScale = 5f;

    [Header("저속 이동 (Focus 키)")]
    [SerializeField] private float focusSpeedRate = 0.45f;
    public bool IsFocused { get; private set; }

    private void Awake()
    {
        // 피격 판정 컴포넌트가 없으면 자동으로 추가
        if (GetComponent<PlayerHit>() == null) gameObject.AddComponent<PlayerHit>();
    }

    private void Start()
    {
        // SelectScene에서 선택한 데이터가 있는지 확인 후 적용
        if (SelectScene.SelectedData != null)
        {
            // 1. 속도 적용
            speed = SelectScene.SelectedData.playerSpeed * 5;


            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && SelectScene.SelectedData.playerIcon != null)
            {
                spriteRenderer.sprite = SelectScene.SelectedData.playerIcon;
            }

            // 애니메이션 프레임이 있으면 자동 재생 준비
            animFrames = SelectScene.SelectedData.playerAnimation;
            animFps = SelectScene.SelectedData.animationFps;
            if (spriteRenderer != null && animFrames != null && animFrames.Length > 0 && animFrames[0] != null)
            {
                spriteRenderer.sprite = animFrames[0];
            }

            // 2. 크기 적용 (PlayerData의 playerScale 배율, 0 이하면 원본 크기로 실행)
            float savedScale = SelectScene.SelectedData.playerScale;
            baseScale = (savedScale <= 0f) ? 1f : Mathf.Clamp(savedScale, MinScale, MaxScale);

            // 처음 보이는 스프라이트 크기를 기준 크기로 저장
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                referenceSize = GetSpriteSize(spriteRenderer.sprite);
            }
            ApplyScale(spriteRenderer != null ? spriteRenderer.sprite : null);
        }
        else
        {
            // 게임 씬을 바로 실행한 경우에도 움직일 수 있도록 기본 속도 적용
            speed = 5f;
            Debug.LogWarning("선택된 캐릭터 데이터가 없습니다. 기본값으로 실행합니다.");
        }
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Vector2 dir = ReadMove();

        if (dir.sqrMagnitude > 1f) dir.Normalize();

        // Focus 키를 누르고 있으면 느리게 움직여 탄 사이를 정밀하게 피할 수 있음
        IsFocused = ReadFocus();
        float moveSpeed = IsFocused ? speed * focusSpeedRate : speed;
        transform.Translate(dir * (moveSpeed * Time.deltaTime));

        ClampToBounds();
        PlayAnimation();
    }

    private void PlayAnimation()
    {
        // 프레임이 2장 이상일 때만 순서대로 반복 재생
        if (spriteRenderer == null || animFrames == null || animFrames.Length < 2) return;

        animTimer += Time.deltaTime;
        float frameTime = 1f / Mathf.Max(animFps, 0.1f);
        while (animTimer >= frameTime)
        {
            animTimer -= frameTime;
            animIndex = (animIndex + 1) % animFrames.Length;
            if (animFrames[animIndex] != null)
            {
                spriteRenderer.sprite = animFrames[animIndex];
                ApplyScale(spriteRenderer.sprite);
            }
        }
    }

    // 스프라이트의 큰 쪽 길이 (월드 단위, Pixels Per Unit 반영)
    private static float GetSpriteSize(Sprite sprite)
    {
        Vector3 size = sprite.bounds.size;
        return Mathf.Max(size.x, size.y);
    }

    // 프레임의 원본 크기가 달라도 기준 크기 × playerScale 비율로 보이도록 스케일 보정
    private void ApplyScale(Sprite sprite)
    {
        float scale = baseScale;
        if (sprite != null && referenceSize > 0f)
        {
            float size = GetSpriteSize(sprite);
            if (size > 0f) scale *= referenceSize / size;
        }
        transform.localScale = Vector3.one * scale;
    }

    private void ClampToBounds()
    {
        if (mainCamera == null) return;

        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);

        viewPos.x = Mathf.Clamp01(viewPos.x);
        viewPos.y = Mathf.Clamp01(viewPos.y);

        transform.position = mainCamera.ViewportToWorldPoint(viewPos);
    }

    private static bool ReadFocus()
    {
        var kb = Keyboard.current;
        if (kb == null) return false;

        Key focusKey = Key.LeftShift;
        if (KeyManager.Instance != null && KeyManager.Instance.keys != null)
        {
            if (KeyManager.Instance.keys.TryGetValue(KeyAction.Focus, out var k)) focusKey = k;
        }
        return kb[focusKey].isPressed;
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
