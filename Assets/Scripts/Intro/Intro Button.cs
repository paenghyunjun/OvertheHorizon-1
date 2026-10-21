using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class IntroButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject EditScene;
    private RectTransform rectTransform;
    private float originalX;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalX = rectTransform.anchoredPosition.x;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 기존 애니메이션을 강제로 끝내지 않고, 현재 진행 중인 값에서 자연스럽게 새 목표로 연결
        rectTransform.DOComplete();
        rectTransform.DOScale(Vector3.one * 1.2f, 0.2f);
        rectTransform.DOAnchorPosX(originalX + 10f, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마찬가지로 갑자기 멈추지 않고 자연스럽게 원래 자리로 돌아가기 시작
        rectTransform.DOComplete();
        rectTransform.DOScale(Vector3.one * 1.0f, 0.2f);
        rectTransform.DOAnchorPosX(originalX, 0.2f);
    }
    public void EditSceneOn()
    {
        EditScene.SetActive(true);
    }
    public void EditSceneOff()
    {
        EditScene.SetActive(false);
    }
    public void StartScene()
    {
        LoadingSceneManager.LoadScene("1_GameScene");
    }
}