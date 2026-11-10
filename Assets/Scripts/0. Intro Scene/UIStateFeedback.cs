using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIStateFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    protected RectTransform rectTransform;

    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.2f; // 커지는 크기
    [SerializeField] private float duration = 0.2f; // 걸리는 시간

    private Vector3 defaultScale;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        defaultScale = rectTransform.localScale;
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.DOKill();
        rectTransform.DOScale(defaultScale * hoverScale, duration).SetEase(Ease.OutQuad);
    }

    public virtual void OnPointerExit(PointerEventData eventData) // protected를 사용할 경우 인터페이스 메서드 구현이 불가능하므로 public으로 변경
    {
        rectTransform.DOKill();
        rectTransform.DOScale(defaultScale, duration).SetEase(Ease.OutQuad);
    }
}