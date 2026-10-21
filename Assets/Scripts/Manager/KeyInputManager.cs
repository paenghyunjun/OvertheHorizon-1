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
    public static KeyManager Instance { get; private set; }

    // KeyCode 대신 UnityEngine.InputSystem.Key 사용
    public Dictionary<KeyAction, Key> keys = new Dictionary<KeyAction, Key>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitDefaultKeys();
        }
        else
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
        StartCoroutine(WaitForKeyPress(actionToRebind, onKeyRebound));
    }

    private IEnumerator WaitForKeyPress(KeyAction actionToRebind, Action<Key> onKeyRebound)
    {
        yield return null; // 프레임 지연 (이전 입력 누수 방지)

        bool keyAssigned = false;

        while (!keyAssigned)
        {
            var kb = Keyboard.current;
            if (kb != null && kb.anyKey.wasPressedThisFrame)
            {
                // Key.None과 Key.IMESelected 등 특수 키 제외 감지
                foreach (Key k in Enum.GetValues(typeof(Key)))
                {
                    if (k == Key.None) continue;

                    if (kb[k].wasPressedThisFrame)
                    {
                        keys[actionToRebind] = k;
                        keyAssigned = true;
                        onKeyRebound?.Invoke(k);
                        break;
                    }
                }
            }
            yield return null;
        }
    }
}