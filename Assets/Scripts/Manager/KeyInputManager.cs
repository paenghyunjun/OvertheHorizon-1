using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum KeyAction
{
    Forward,    // 상 이동
    Backward,   // 하 이동
    Left,       // 좌 이동
    Right,      // 우 이동
    Focus,      // 저속 이동 및 히트박스 노출
    Attack,     // 기본 공격
    Ultimate    // 필살기 / 스펠카드
}

public class KeyManager : MonoBehaviour
{
    private static KeyManager instance;
    public static KeyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<KeyManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("KeyManager");
                    instance = go.AddComponent<KeyManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    // KeyCode 대신 UnityEngine.InputSystem.Key 사용
    public Dictionary<KeyAction, Key> keys = new Dictionary<KeyAction, Key>();

    // 키 설정이 바뀌거나 변경이 취소되면 호출됨 (버튼 글자 갱신용)
    public event Action OnKeysChanged;

    private const string PrefsPrefix = "Key_";
    private Coroutine rebindRoutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            if (keys.Count == 0)
            {
                InitDefaultKeys();
                LoadKeys();
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitDefaultKeys()
    {
        keys[KeyAction.Forward] = Key.W;
        keys[KeyAction.Left] = Key.A;
        keys[KeyAction.Backward] = Key.S;
        keys[KeyAction.Right] = Key.D;

        keys[KeyAction.Focus] = Key.LeftShift;
        keys[KeyAction.Attack] = Key.Z;
        keys[KeyAction.Ultimate] = Key.X;
    }

    // 키 바인딩 변경 메서드
    public void ChangeKey(KeyAction actionToRebind, Action<Key> onKeyRebound = null)
    {
        CancelRebind(); // 다른 버튼이 키 입력을 기다리는 중이면 먼저 취소
        rebindRoutine = StartCoroutine(WaitForKeyPress(actionToRebind, onKeyRebound));
    }

    // 키 입력 대기를 취소하고 원래 키로 되돌림
    public void CancelRebind()
    {
        if (rebindRoutine == null) return;

        StopCoroutine(rebindRoutine);
        rebindRoutine = null;
        OnKeysChanged?.Invoke();
    }

    private IEnumerator WaitForKeyPress(KeyAction actionToRebind, Action<Key> onKeyRebound)
    {
        yield return null; // 프레임 지연 (이전 입력 누수 방지)

        while (true)
        {
            var kb = Keyboard.current;
            if (kb != null && kb.anyKey.wasPressedThisFrame)
            {
                // ESC는 변경 취소
                if (kb.escapeKey.wasPressedThisFrame)
                {
                    CancelRebind();
                    yield break;
                }

                foreach (var control in kb.allKeys)
                {
                    if (control == null || !control.wasPressedThisFrame) continue;

                    Key pressed = control.keyCode;
                    rebindRoutine = null;
                    AssignKey(actionToRebind, pressed);
                    onKeyRebound?.Invoke(pressed);
                    yield break;
                }
            }
            yield return null;
        }
    }

    // 이미 다른 행동이 쓰는 키를 고르면 서로 맞바꿔서 한 키가 두 행동에 겹치지 않게 함
    private void AssignKey(KeyAction action, Key key)
    {
        if (keys.TryGetValue(action, out Key oldKey))
        {
            foreach (KeyAction other in Enum.GetValues(typeof(KeyAction)))
            {
                if (other != action && keys.TryGetValue(other, out Key otherKey) && otherKey == key)
                {
                    keys[other] = oldKey;
                    break;
                }
            }
        }

        keys[action] = key;
        SaveKeys();
        OnKeysChanged?.Invoke();
    }

    private void SaveKeys()
    {
        foreach (var pair in keys)
        {
            PlayerPrefs.SetString(PrefsPrefix + pair.Key, pair.Value.ToString());
        }
        PlayerPrefs.Save();
    }

    private void LoadKeys()
    {
        foreach (KeyAction action in Enum.GetValues(typeof(KeyAction)))
        {
            string saved = PlayerPrefs.GetString(PrefsPrefix + action, "");
            if (Enum.TryParse(saved, out Key key) && key != Key.None)
            {
                keys[action] = key;
            }
        }
    }
}