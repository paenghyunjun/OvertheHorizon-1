using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SelectScene : MonoBehaviour
{
    public static PlayerData SelectedData { get; private set; }

    [Header("캐릭터 데이터")]
    [SerializeField] private PlayerData[] CharterData;
    private int NowNumber = 0;

    [Header("UI")]
    [SerializeField] private Image Charters;
    [SerializeField] private TMP_Text NameText;
    [SerializeField] private TMP_Text HPText;
    [SerializeField] private TMP_Text SpeedText;
    [SerializeField] private TMP_Text DescriptionText;

    private void Start()
    {
        if (CharterData == null || CharterData.Length == 0)
        {
            Debug.LogError("CharterData에 등록된 캐릭터 데이터가 없습니다!");
            return;
        }

        UpdateUI();
    }

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

    public void SelectCharacter()
    {
        if (CharterData == null || CharterData.Length == 0) return;

        SelectedData = CharterData[NowNumber];
        SceneManager.LoadScene("2_GameScene");
    }

    private void UpdateUI()
    {
        if (CharterData[NowNumber] == null) return;

        var data = CharterData[NowNumber];

        Charters.sprite = data.playerIcon;
        NameText.text = data.playerName;
        HPText.text = "체력 : " + data.playerHealth.ToString();
        SpeedText.text = "속도 : " + data.playerSpeed.ToString();
        DescriptionText.text = data.playerDescription;

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