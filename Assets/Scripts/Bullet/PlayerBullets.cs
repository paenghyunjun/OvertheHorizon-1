using System.Collections.Generic;
using UnityEngine;

// 현재 화면에 있는 플레이어 탄 목록. 적이 이 목록으로 피격 검사를 함
public static class PlayerBullets
{
    public static readonly List<Transform> Active = new List<Transform>();

    // 플레이를 다시 시작할 때 이전에 남은 목록 초기화
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Active.Clear();
    }
}
