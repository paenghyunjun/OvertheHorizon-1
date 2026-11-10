using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 인트로 EDIT 패널의 키 변경 버튼
// - Inspector에서 어떤 행동의 키를 바꿀지(action)와 키 이름을 보여줄 글자(keyText)를 연결
// - Button의 OnClick에 Rebind를 연결하면, 누른 뒤 입력한 키로 바뀜 (ESC는 취소)
public class KeyBindButton : MonoBehaviour
{
    [SerializeField] private KeyAction action;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private string waitingText = "PRESS KEY";

    private KeyManager manager;

    private void OnEnable()
    {
        manager = KeyManager.Instance;
        manager.OnKeysChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (manager == null) return;

        manager.CancelRebind();
        manager.OnKeysChanged -= Refresh;
    }

    public void Rebind()
    {
        manager.ChangeKey(action);
        keyText.text = waitingText;

        // 버튼이 선택된 채로 두면 Enter/Space 입력이 버튼을 다시 누르게 되므로 선택 해제
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void Refresh()
    {
        if (manager.keys.TryGetValue(action, out var key))
        {
            keyText.text = key.ToString();
        }
    }
}
