using UnityEngine;

public class WorldAnchorTracker : MonoBehaviour
{
    [Header("추적 대상 (버튼 혹은 투명 타일 오브젝트)")]
    public Transform TargetObject; 
    
    [Header("미세 조정")]
    public Vector2 UIOffset;

    private RectTransform _rectTransform;
    private Canvas _parentCanvas;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _parentCanvas = GetComponentInParent<Canvas>();
    }
    
    void OnEnable()
    {
        UpdatePosition();
    }

    void LateUpdate()
    {
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        if (TargetObject == null) return;

        Vector2 screenPoint;
        RectTransform targetRect = TargetObject as RectTransform;

        if (targetRect != null)
        {
            Canvas targetCanvas = targetRect.GetComponentInParent<Canvas>();
            Camera targetCam = (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : targetCanvas.worldCamera;
        
            screenPoint = RectTransformUtility.WorldToScreenPoint(targetCam, targetRect.position);
        }
        else
        {
            screenPoint = Camera.main.WorldToScreenPoint(TargetObject.position);
        }
        
        Camera myCanvasCam = (_parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : _parentCanvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform.parent as RectTransform,
                screenPoint,
                myCanvasCam,
                out Vector2 localPoint))
        {
            _rectTransform.anchoredPosition = localPoint + UIOffset;
        }
    }
}