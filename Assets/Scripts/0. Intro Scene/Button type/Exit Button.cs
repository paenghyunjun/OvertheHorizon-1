using UnityEngine;
using UnityEngine.EventSystems;

public class ExitButton : UIStateFeedback
{

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}