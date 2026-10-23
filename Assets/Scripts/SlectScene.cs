using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SlectScene : MonoBehaviour
{
    public const string SelectedIndexKey = "SelectedCharacterIndex";

    public static PlayerData SelectedData { get; private set; }

    [Header("캐릭터 데이터")]
    [SerializeField] private PlayerData[] CharterData;
    private int NowNumber = 0;

    [Header("UI")]
    [SerializeField] private Image Charters;
    [SerializeField] private TMP_Text NameText;
    [SerializeField] private TMP_Text HPText;
    [SerializeField] private TMP_Text SpeedText;
    [SerializeField] private TMP_Text EditText;

    [Header("버튼 (선택 사항)")]
    [SerializeField] private Button PrevButton;
    [SerializeField] private Button NextButton;
    [SerializeField] private Button SelectButton;

    [Header("씬 이동")]
    [SerializeField] private string NextSceneName = "2.GameScene";
    [SerializeField] private bool RememberLastSelection = true;

    [Header("초상화 프레임 애니메이션 (선택 사항)")]
    [SerializeField] private bool UseFrameAnimation = true;
    [SerializeField] private float FramesPerSecond = 8f;

    private Coroutine _animRoutine;
    private bool _isLoading;

    public int CurrentIndex => NowNumber;

    void Start()
    {
        if (CharterData == null || CharterData.Length == 0)
        {
            Debug.LogError("[SlectScene] CharterData 가 비어 있습니다. 인스펙터에서 PlayerData 를 넣어주세요.");
            enabled = false;
            return;
        }

        if (RememberLastSelection)
            NowNumber = PlayerPrefs.GetInt(SelectedIndexKey, 0);
        NowNumber = Mathf.Clamp(NowNumber, 0, CharterData.Length - 1);

        if (PrevButton != null) PrevButton.onClick.AddListener(Prev);
        if (NextButton != null) NextButton.onClick.AddListener(Next);
        if (SelectButton != null) SelectButton.onClick.AddListener(Select);

        Refresh();
    }

    void OnDestroy()
    {
        if (PrevButton != null) PrevButton.onClick.RemoveListener(Prev);
        if (NextButton != null) NextButton.onClick.RemoveListener(Next);
        if (SelectButton != null) SelectButton.onClick.RemoveListener(Select);

        StopAnimation();
    }

    void Update()
    {
        if (_isLoading) return;

        if (PressedLeft()) Prev();
        else if (PressedRight()) Next();

        if (PressedConfirm()) Select();
    }

    // ───────── 캐릭터 전환 ─────────
    public void Next() => Change(1);
    public void Prev() => Change(-1);

    private void Change(int direction)
    {
        if (_isLoading || CharterData == null || CharterData.Length <= 1) return;

        int count = CharterData.Length;
        NowNumber = (NowNumber + direction + count) % count;
        Refresh();
    }

    private void Refresh()
    {
        PlayerData data = CharterData[NowNumber];
        if (data == null)
        {
            Debug.LogWarning($"[SlectScene] CharterData[{NowNumber}] 가 비어 있습니다.");
            StopAnimation();
            if (Charters != null) Charters.enabled = false;
            return;
        }

        if (Charters != null)
        {
            Charters.enabled = true;
            Charters.preserveAspect = true;
            Charters.sprite = data.playerIcon;
        }

        if (NameText != null) NameText.text = data.playerName;
        if (HPText != null) HPText.text = $"HP : {data.playerHealth}";
        if (SpeedText != null) SpeedText.text = $"Speed : {data.playerSpeed:0.##}";
        if (EditText != null) EditText.text = data.playerDescription;

        StartAnimation(data.playerAnimation);
    }

    // ───────── 초상화 프레임 애니메이션 ─────────
    private void StartAnimation(Sprite[] frames)
    {
        StopAnimation();

        if (!UseFrameAnimation || frames == null || frames.Length <= 1 || Charters == null) return;

        _animRoutine = StartCoroutine(PlayFrames(frames));
    }

    private void StopAnimation()
    {
        if (_animRoutine == null) return;

        StopCoroutine(_animRoutine);
        _animRoutine = null;
    }

    private System.Collections.IEnumerator PlayFrames(Sprite[] frames)
    {
        int index = 0;
        var wait = new WaitForSecondsRealtime(1f / Mathf.Max(1f, FramesPerSecond));

        while (true)
        {
            if (Charters != null && frames[index] != null)
                Charters.sprite = frames[index];

            index = (index + 1) % frames.Length;
            yield return wait;
        }
    }

    // ───────── 선택 확정 ─────────
    public void Select()
    {
        if (_isLoading || CharterData == null) return;

        PlayerData data = CharterData[NowNumber];
        if (data == null) return;

        _isLoading = true;
        SelectedData = data;
        PlayerPrefs.SetInt(SelectedIndexKey, NowNumber);
        PlayerPrefs.Save();

        if (string.IsNullOrEmpty(NextSceneName))
        {
            Debug.LogWarning("[SlectScene] NextSceneName 이 비어 있어 씬을 이동하지 않습니다.");
            _isLoading = false;
            return;
        }

        SceneManager.LoadScene(NextSceneName);
    }

    // ───────── 입력 (구/신 Input System 모두 지원) ─────────
    private bool PressedLeft()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
#endif
    }

    private bool PressedRight()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
#endif
    }

    private bool PressedConfirm()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space);
#endif
    }
}
