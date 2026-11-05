using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 게임 씬의 플레이어 상태 표시 (이름, 현재 체력/시작 체력, 체력바)
public class PlayerStatusUI : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text nameText;

    private PlayerHit player;

    private void Start()
    {
        // 게임 씬을 바로 실행해 선택된 캐릭터가 없으면 씬에 적힌 글자를 그대로 둠
        if (SelectScene.SelectedData != null)
        {
            nameText.text = SelectScene.SelectedData.playerName;
        }

        player = PlayerHit.Instance;
        if (player == null) return;

        player.OnHpChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (player != null) player.OnHpChanged -= Refresh;
    }

    private void Refresh()
    {
        hpSlider.value = (float)player.Hp / Mathf.Max(player.MaxHp, 1);
        hpText.text = $"{player.Hp}/{player.MaxHp}";
    }
}
