using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 무기 컨트롤러 - 플레이어/적 공용
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    public List<GameObject> weaponEffects = new List<GameObject>();

    [Header("Override Controllers (가면 타입별)")]
    [SerializeField] private AnimatorOverrideController[] weaponOverrideControllers = new AnimatorOverrideController[4];

    [Header("Settings")]
    [SerializeField] private float orbitDistance = 0.5f;
    [SerializeField] private Vector2 offset = Vector2.right;


    // 현재 가면/무기 타입
    private int currentType = 0;

    // 현재 무기 이펙트
    public GameObject weaponEffect;

    // 외부에서 방향 설정용
    private Vector2 aimDirection = Vector2.right;

    private void Start()
    {
        weaponEffect = weaponEffects[0];
    }

    private void Update()
    {
        UpdateRotation();
        UpdatePosition();
    }

    /// <summary>
    /// 무기 타입 변경 (가면 타입과 1:1 대응)
    /// </summary>
    public void SetWeaponType(int typeIndex)
    {
        if (typeIndex < 0 || typeIndex >= 4) return;
        currentType = typeIndex;
        weaponEffect = weaponEffects[typeIndex];

        if (animator != null && weaponOverrideControllers[typeIndex] != null)
        {
            animator.runtimeAnimatorController = weaponOverrideControllers[typeIndex];
        }
    }

    /// <summary>
    /// 조준 방향 설정
    /// </summary>
    public void SetAimDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.01f)
        {
            aimDirection = direction.normalized;
        }
    }

    /// <summary>
    /// 공격 애니메이션 재생
    /// </summary>
    public void PlayAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// 무기 회전 업데이트
    /// </summary>
    private void UpdateRotation()
    {
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.Euler(0, 0, angle);

        // 좌우 반전 (왼쪽을 향할 때 스프라이트 뒤집기)
        if (spriteRenderer != null)
        {
            spriteRenderer.flipY = aimDirection.x < 0;
        }
    }

    /// <summary>
    /// 무기 위치 업데이트 (오너 주위 공전)
    /// </summary>
    private void UpdatePosition()
    {
        // 방향에 따른 오프셋 위치
        transform.localPosition = aimDirection * orbitDistance + offset;
    }

    /// <summary>
    /// 현재 무기 타입
    /// </summary>
    public int CurrentType => currentType;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(aimDirection * 0.5f));
    }
}
