using TMPro;
using UnityEngine;

public class TmiReader : MonoBehaviour
{
    [SerializeField]private TmiText[] tmiText;
    [SerializeField] private TextMeshProUGUI Tmitext;

    private void Start()
    {
        Tmitext.text = $"TMI : {tmiText[Random.Range(0, tmiText.Length)].Tmi}";
    }
}
