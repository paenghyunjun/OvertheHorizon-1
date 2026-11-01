using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class SelectScene : MonoBehaviour
{
    public static PlayerData SelectedData { get; private set; }

    [Header("선택 가능한 캐릭터")]
    [SerializeField] private PlayerData[] CharterData;
    private int NowNumber = 0;

    [Header("UI")]
    [SerializeField] private Image Charters;
    [SerializeField] private TMP_Text NameText;
    [SerializeField] private TMP_Text HPText;
    [SerializeField] private TMP_Text SpeedText;
    [SerializeField] private TMP_Text DescriptionText;

    [Header("전환 연출")]
    [SerializeField] private string gameSceneName = "2_GameScene";
    [SerializeField] private float maxIconSize = 250f;
    [SerializeField] private float fadeTime = 0.4f;
    [Tooltip("비워두면 캐릭터 이미지가 들어있는 Canvas에 자동으로 추가")]
    [SerializeField] private CanvasGroup canvasGroup;

    private bool isLoading = false;

    private void Start()
    {
        if (CharterData == null || CharterData.Length == 0)
        {
            Debug.LogError("CharterData에 등록된 캐릭터 데이터가 없습니다!");
            return;
        }

        if (canvasGroup == null && Charters != null)
        {
            Canvas canvas = Charters.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvasGroup = canvas.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            }
        }

        // 씬 진입 시 페이드 인
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeTime).SetLink(gameObject);
        }

        UpdateUI(false);
    }

    private void Update()
    {
        if (isLoading) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) NextCharacter();
        if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) PrevCharacter();
        if (kb.enterKey.wasPressedThisFrame || kb.zKey.wasPressedThisFrame) SelectCharacter();
    }

    public void NextCharacter()
    {
        if (isLoading || CharterData == null || CharterData.Length == 0) return;

        NowNumber++;
        if (NowNumber >= CharterData.Length)
        {
            NowNumber = 0;
        }
        UpdateUI(true);
    }

    public void PrevCharacter()
    {
        if (isLoading || CharterData == null || CharterData.Length == 0) return;

        NowNumber--;
        if (NowNumber < 0)
        {
            NowNumber = CharterData.Length - 1;
        }
        UpdateUI(true);
    }

    public void SelectCharacter()
    {
        if (isLoading || CharterData == null || CharterData.Length == 0) return;
        if (CharterData[NowNumber] == null) return;

        isLoading = true;
        SelectedData = CharterData[NowNumber];

        // 중복 클릭 방지 후 페이드 아웃 → 로딩 씬을 거쳐 게임 씬으로 이동
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, fadeTime)
                .SetLink(gameObject)
                .OnComplete(() => LoadingSceneManager.LoadScene(gameSceneName));
        }
        else
        {
            LoadingSceneManager.LoadScene(gameSceneName);
        }
    }

    private void UpdateUI(bool animate)
    {
        var data = CharterData[NowNumber];
        if (data == null) return;

        if (NameText != null) NameText.text = data.playerName;
        if (HPText != null) HPText.text = "체력 : " + data.playerHealth.ToString();
        if (SpeedText != null) SpeedText.text = "속도 : " + data.playerSpeed.ToString();
        if (DescriptionText != null) DescriptionText.text = data.playerDescription;

        if (Charters == null) return;

        Charters.sprite = data.playerIcon;
        if (Charters.sprite != null)
        {
            Charters.preserveAspect = true;

            // 원본 비율을 유지하면서 maxIconSize 안에 들어오도록 크기 조정
            float width = Charters.sprite.rect.width;
            float height = Charters.sprite.rect.height;
            if (width > maxIconSize || height > maxIconSize)
            {
                float ratio = maxIconSize / Mathf.Max(width, height);
                width *= ratio;
                height *= ratio;
            }
            Charters.rectTransform.sizeDelta = new Vector2(width, height);
        }

        if (animate)
        {
            RectTransform rect = Charters.rectTransform;
            rect.DOKill(true);
            rect.localScale = Vector3.one;
            rect.DOPunchScale(Vector3.one * 0.1f, 0.2f, 6, 0.5f).SetLink(gameObject);
        }
    }
}
