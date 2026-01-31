using UnityEngine;

/// <summary>
/// 상점 상호작용 - 범위 내에서 E키로 상점 열기
/// Collider2D (IsTrigger) 필요
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ShopInteractable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShopUI shopUI;

    [Header("UI")]
    [SerializeField] private GameObject interactPrompt;  // "E키를 눌러 상점 열기" UI (선택)

    private bool isPlayerInRange = false;

    private void Start()
    {
        // Collider가 Trigger인지 확인
        var collider = GetComponent<Collider2D>();
        if (!collider.isTrigger)
        {
            collider.isTrigger = true;
        }

        // 프롬프트 숨기기
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            OpenShop();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }

    private void OpenShop()
    {
        if (shopUI != null)
        {
            shopUI.OpenShop();
        }
        else
        {
            Debug.LogWarning("ShopUI가 설정되지 않았습니다.");
        }
    }
}
