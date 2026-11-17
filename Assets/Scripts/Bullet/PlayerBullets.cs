using System.Collections.Generic;
using UnityEngine;

public static class PlayerBullets
{
    public static readonly List<Transform> Active = new List<Transform>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Active.Clear();
    }
}
