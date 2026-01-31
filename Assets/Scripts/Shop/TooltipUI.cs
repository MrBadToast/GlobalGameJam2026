using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 상점 아이템 툴팁 UI
/// </summary>
public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Settings")]
    [SerializeField] private Vector2 offset = new Vector2(20f, -20f);

    private RectTransform tooltipRectTransform;
    private Canvas parentCanvas;

    private void Awake()
    {
        Instance = this;

        if (tooltipRoot != null)
        {
            tooltipRectTransform = tooltipRoot.GetComponent<RectTransform>();
            parentCanvas = tooltipRoot.GetComponentInParent<Canvas>();
            tooltipRoot.SetActive(false);
        }
    }

    /// <summary>
    /// 툴팁 표시
    /// </summary>
    public void Show(ItemData itemData, Vector2 screenPosition)
    {
        if (itemData == null || tooltipRoot == null) return;

        // 데이터 설정
        if (iconImage != null)
        {
            iconImage.sprite = itemData.icon;
            iconImage.enabled = itemData.icon != null;
        }

        if (itemNameText != null)
        {
            itemNameText.text = itemData.itemName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = itemData.description;
        }

        // 위치 업데이트
        UpdatePosition(screenPosition);

        tooltipRoot.SetActive(true);
    }

    /// <summary>
    /// 툴팁 위치 업데이트
    /// </summary>
    public void UpdatePosition(Vector2 screenPosition)
    {
        if (tooltipRectTransform == null || parentCanvas == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            screenPosition + offset,
            parentCanvas.worldCamera,
            out localPoint
        );

        tooltipRectTransform.anchoredPosition = localPoint;
    }

    /// <summary>
    /// 툴팁 숨기기
    /// </summary>
    public void Hide()
    {
        if (tooltipRoot != null)
        {
            tooltipRoot.SetActive(false);
        }
    }

    /// <summary>
    /// 오프셋 설정
    /// </summary>
    public void SetOffset(Vector2 newOffset)
    {
        offset = newOffset;
    }
}
