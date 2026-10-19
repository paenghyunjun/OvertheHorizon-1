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
    [TextArea] public string playerDescription;
}