
using UnityEngine;
using UnityEngine.EventSystems;

public class StartButton : UIStateFeedback
{
    [SerializeField] private string nextSceneName = "1_SlectScene"; // 다음 씬 이름을 지정합니다.
    public void OnClickStartScene()
    {
        LoadingSceneManager.LoadScene(nextSceneName);
    }
}