using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ESCStop : MonoBehaviour
{
    bool IsPause = false;
    [SerializeField]private GameObject Panel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPause == false)
            {
                // 일시정지 활성화
                Time.timeScale = 0;
                IsPause = true;
                Panel.gameObject.SetActive(true);
            }
            else // if (IsPause == true) 대신 else를 사용합니다.
            {
                // 일시정지 비활성화
                Time.timeScale = 1;
                IsPause = false;
                Panel.gameObject.SetActive(false);
            }
        }
    }
}