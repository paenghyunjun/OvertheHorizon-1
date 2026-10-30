using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// 캐릭터 선택 씬(1_SlectScene)을 담당하는 스크립트
// - 좌우 버튼으로 캐릭터를 넘겨보고, 선택 버튼을 누르면 게임 씬으로 넘어감
// - 선택된 캐릭터 데이터는 SelectedData에 저장되어 PlayerMove 등에서 읽어감
public class SelectScene : MonoBehaviour
{
    // 선택이 확정된 캐릭터 데이터 (게임 씬에서 SelectScene.SelectedData로 접근)
    public static PlayerData SelectedData { get; private set; }

    [Header("캐릭터 데이터")]
    [SerializeField] private PlayerData[] CharterData;   // Inspector에서 캐릭터 데이터(ScriptableObject)들을 순서대로 연결
    private int NowNumber = 0;                            // 현재 화면에 보여주고 있는 캐릭터 번호

    [Header("UI")]
    [SerializeField] private Image Charters;              // 캐릭터 아이콘이 표시될 이미지
    [SerializeField] private TMP_Text NameText;           // 캐릭터 이름 텍스트
    [SerializeField] private TMP_Text HPText;             // 체력 텍스트
    [SerializeField] private TMP_Text SpeedText;          // 속도 텍스트
    [SerializeField] private TMP_Text DescriptionText;    // 캐릭터 설명 텍스트

    private void Start()
    {
        // 연결된 캐릭터 데이터가 없으면 경고 후 종료 (빈 배열로 실행되는 사고 방지)
        if (CharterData == null || CharterData.Length == 0)
        {
            Debug.LogError("CharterData에 등록된 캐릭터 데이터가 없습니다!");
            return;
        }

        UpdateUI();
    }

    // 다음 캐릭터로 넘김 (마지막이면 다시 처음으로)
    public void NextCharacter()
    {
        if (CharterData == null || CharterData.Length == 0) return;

        NowNumber++;
        if (NowNumber >= CharterData.Length)
        {
            NowNumber = 0;
        }
        UpdateUI();
    }

    // 이전 캐릭터로 넘김 (처음이면 마지막으로)
    public void PrevCharacter()
    {
        if (CharterData == null || CharterData.Length == 0) return;

        NowNumber--;
        if (NowNumber < 0)
        {
            NowNumber = CharterData.Length - 1;
        }
        UpdateUI();
    }

    // 선택 버튼: 현재 캐릭터를 확정하고 게임 씬으로 이동
    public void SelectCharacter()
    {
        if (CharterData == null || CharterData.Length == 0) return;

        SelectedData = CharterData[NowNumber];
        SceneManager.LoadScene("2_GameScene");
    }

    // 현재 캐릭터 정보를 UI에 반영
    private void UpdateUI()
    {
        if (CharterData[NowNumber] == null) return;

        var data = CharterData[NowNumber];

        Charters.sprite = data.playerIcon;
        NameText.text = data.playerName;
        HPText.text = "체력 : " + data.playerHealth.ToString();
        SpeedText.text = "속도 : " + data.playerSpeed.ToString();
        DescriptionText.text = data.playerDescription;

        // 아이콘이 너무 크게 나오지 않도록, 이미지 크기를 최대 250픽셀로 맞춤
        // (가로세로 비율은 preserveAspect로 유지하고, 긴 쪽을 250으로 줄임)
        if (Charters.sprite != null)
        {
            Charters.preserveAspect = true;

            float spriteWidth = Charters.sprite.rect.width;
            float spriteHeight = Charters.sprite.rect.height;
            float maxSize = 250f;
            float width = spriteWidth;
            float height = spriteHeight;

            if (width > maxSize || height > maxSize)
            {
                if (width > height)
                {
                    height = (maxSize / width) * height;
                    width = maxSize;
                }
                else
                {
                    width = (maxSize / height) * width;
                    height = maxSize;
                }
            }

            RectTransform rectTrans = Charters.GetComponent<RectTransform>();
            if (rectTrans != null)
            {
                rectTrans.sizeDelta = new Vector2(width, height);
            }
        }
    }
}