using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "OverTheHorizon/Player", order = 1)]
public class PlayerData : ScriptableObject
{
    // 데이터로 사용할 변수들 선언
    public string playerName;
    public int playerID;
    public int playerHealth;
    public float playerSpeed;
   
    public Sprite playerIcon;
    public Sprite[] playerAnimation;
    [Tooltip("playerAnimation 재생 속도 (초당 프레임 수)")]
    [Min(0.1f)] public float animationFps = 8f;
    [TextArea] public string playerDescription;

    [Header("캐릭터 크기 설정")]
    [Tooltip("게임 씬에 스폰될 때 적용되는 크기 배율 (1 = 원본 크기, 0.5 = 절반)")]
    [Range(0.05f, 5f)]
    public float playerScale = 1f;
}