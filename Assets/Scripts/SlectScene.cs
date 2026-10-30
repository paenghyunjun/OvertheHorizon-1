using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SelectScene : MonoBehaviour
{
    public static PlayerData SelectedData { get; private set; }

    [Header("선택하는 캐릭터")]
    [SerializeField] private PlayerData[] CharterData;
    private int NowNumber = 0;

    [Header("UI")]
    [SerializeField] private Image Charters;
    [SerializeField]private TMPro.TMP_Text NameText;
    [SerializeField] private TMPro.TMP_Text HPText;
    [SerializeField] private TMPro.TMP_Text SpeedText;
    [SerializeField] private TMPro.TMP_Text EditText;
    private void Awake()
    {
        UpdateUI();
    }
    public void NextCharter()
    {
        NowNumber++;
        if (NowNumber >= CharterData.Length)
        {
            NowNumber = 0;
        }
        UpdateUI();
    }
    public void PreviousCharter()
    {
        NowNumber--;
        if (NowNumber < 0)
        {
            NowNumber = CharterData.Length - 1;
        }
        UpdateUI();
    }
    void UpdateUI()
    {
        Charters.sprite = CharterData[NowNumber].playerIcon;
        NameText.text = CharterData[NowNumber].playerName;
        HPText.text = "HP : " + CharterData[NowNumber].playerHealth.ToString();
        SpeedText.text =  CharterData[NowNumber].playerSpeed.ToString();
        EditText.text = CharterData[NowNumber].playerDescription;
    }
}