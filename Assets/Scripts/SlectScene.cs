using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SlectScene : MonoBehaviour
{
    [SerializeField] private PlayerData[] CharterData;
    private int NowNumber = 0;
    [SerializeField] private Image Charters;
    [SerializeField]private TMPro.TMP_Text NameText;
    [SerializeField] private TMPro.TMP_Text HPText;
    [SerializeField] private TMPro.TMP_Text SpeedText;
    [SerializeField] private TMPro.TMP_Text EditText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
