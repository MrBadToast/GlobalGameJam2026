using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player_Topdown : StaticSerializedMonoBehaviour<Player_Topdown>, IEntity
{
    // ========== IEntity 구현 ==========
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Expression")]
    [SerializeField] private ExpressionType expression;

    // IEntity 속성
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public ExpressionType Expression => expression;
    public bool IsDead { get; private set; } = false;
    public Animator Animator => animator;
    public GameObject GameObject => gameObject;

    // ========== 플레이어 전용 ==========
    [Header("Player Only")]
    [SerializeField] private int money = 0;
    public int Money => money;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Attack")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float meleeAttackRadius = 1.5f;
    [SerializeField] private float meleeAttackOffset = 1f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Ranged Attack")]
    [SerializeField] private float rangedAttackRange = 15f;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float lineDisplayDuration = 0.1f;

    // [Header("Bullet (Legacy)")]
    // [SerializeField] private GameObject bulletPrefab;
    // [SerializeField] private float bulletSpeed = 10f;
    // [SerializeField] private Transform firePoint;

    [Header("Attack Sound")]
    [SerializeField] private AudioClip meleeAttackSound;
    [SerializeField] private AudioClip rangedAttackSound;

    [Header("ChildReferences")]
    [SerializeField, Required()] private Animator animator;

    [Header("Sound")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;

    // 추가 스탯 (아이템, 버프 등)
    private EntityStats bonusStats = new EntityStats();
    public EntityStats BonusStats => bonusStats;

    private Rigidbody2D rb;
    private Vector2 inputVector;
    private float lastAttackTime;
    public Camera mainCamera;
    private Vector2 mouseWorldPosition;

    private InputSystem_Actions input;


    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
        input = new InputSystem_Actions();
        currentHealth = maxHealth;
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Update()
    {
        // 마우스 월드 좌표 저장 (Gizmo용)
        if (mainCamera != null)
        {
            mouseWorldPosition = mainCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
        }

        if (IsDead) return;

        HandleMovementInput();
        HandleExpressionInput();
        HandleAttackInput();
    }

    private void HandleMovementInput()
    {
        float h = input.Player.Move.ReadValue<Vector2>().x;
        float v = input.Player.Move.ReadValue<Vector2>().y;
        inputVector = new Vector2(h, v);

        if (inputVector.sqrMagnitude > 1f)
            inputVector = inputVector.normalized;

        animator.SetBool("IsMove", inputVector.sqrMagnitude > 0f);
        animator.SetFloat("MoveX", inputVector.x);
        animator.SetFloat("MoveY", inputVector.y);
    }

    private void HandleExpressionInput()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            SetExpression(0);  // Neutral
        else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            SetExpression(1);  // Happy
        else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
            SetExpression(2);  // Sad
        else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4))
            SetExpression(3);  // Angry
    }

    private void HandleAttackInput()
    {
        if (input.Player.Attack.WasPressedThisFrame())
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        // 쿨다운 체크
        float cooldown = attackCooldown / ExpressionData.GetAttackSpeedMultiplier(expression, bonusStats);
        if (Time.time < lastAttackTime + cooldown) return;

        lastAttackTime = Time.time;

        // 마우스 방향 계산
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
        Vector2 attackDirection = (mouseWorldPos - (Vector2)transform.position).normalized;

        // 공격 타입에 따라 분기
        AttackRangeType attackType = ExpressionData.GetAttackRange(expression);

        if (attackType == AttackRangeType.Melee)
        {
            PerformMeleeAttack(attackDirection);
        }
        else
        {
            PerformRangedAttack(attackDirection);
        }

        // 공격 애니메이션
        animator?.SetTrigger("Attack");
    }

    private void PerformMeleeAttack(Vector2 direction)
    {
        Vector2 attackPos = (Vector2)transform.position + direction * meleeAttackOffset;

        CombatUtils.Attack(
            this,
            attackPos,
            meleeAttackRadius,
            CombatUtils.MonsterMask,
            baseDamage,
            meleeAttackSound,
            soundVolume
        );
    }

    private void PerformRangedAttack(Vector2 direction)
    {
        // 사운드 재생
        if (rangedAttackSound != null)
        {
            AudioSource.PlayClipAtPoint(rangedAttackSound, transform.position, soundVolume);
        }

        Vector2 origin = transform.position;
        Vector2 endPoint = origin + direction * rangedAttackRange;

        // 레이캐스트로 타겟 탐색
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, rangedAttackRange, CombatUtils.MonsterMask);

        if (hit.collider != null)
        {
            endPoint = hit.point;

            if (hit.collider.TryGetComponent<IEntity>(out var target) && !target.IsDead)
            {
                // 데미지 계산
                float finalDamage = ExpressionData.CalculateDamage(
                    baseDamage,
                    expression,
                    target.Expression,
                    bonusStats,
                    target.BonusStats
                );

                Vector2 hitDirection = ((Vector2)target.GameObject.transform.position - origin).normalized;
                target.TakeDamage(finalDamage, hitDirection);
            }
        }

        // 궤적 표시
        if (lineRenderer != null)
        {
            StartCoroutine(ShowRayLine(origin, endPoint));
        }
    }

    private IEnumerator ShowRayLine(Vector2 start, Vector2 end)
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        yield return new WaitForSeconds(lineDisplayDuration);

        lineRenderer.enabled = false;
    }

    // ========== Bullet 방식 (Legacy) ==========
    /*
    private void PerformRangedAttack_Bullet(Vector2 direction)
    {
        if (bulletPrefab == null) return;

        // 사운드 재생
        if (rangedAttackSound != null)
        {
            AudioSource.PlayClipAtPoint(rangedAttackSound, transform.position, soundVolume);
        }

        // 총알 생성
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // 총알 초기화
        if (bulletObj.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.Initialize(this, direction, bulletSpeed, baseDamage, CombatUtils.MonsterMask);
        }
    }
    */

    private void FixedUpdate()
    {
        if (IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = inputVector * moveSpeed;
        float rotationY = 0f;

        if (inputVector.x > 0)
            rotationY = 0f;
        else if (inputVector.x < 0)
            rotationY = 180f;

        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
    }

    // ========== IEntity 메서드 ==========

    public void TakeDamage(float damage, Vector2 direction)
    {
        if (IsDead) return;

        currentHealth -= damage;

        // 피격 사운드
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, soundVolume);
        }

        // 피격 애니메이션
        animator?.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Die()
    {
        if (IsDead) return;

        IsDead = true;

        // 죽음 사운드
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position, soundVolume);
        }

        // 죽음 애니메이션
        animator?.SetTrigger("Death");

        // TODO: 게임오버 처리
    }

    // ========== 플레이어 전용 메서드 ==========

    /// <summary>
    /// 표정 설정 (1,2,3,4 키 입력 -> 0,1,2,3 인덱스)
    /// </summary>
    public void SetExpression(int index)
    {
        if (index < 0 || index >= System.Enum.GetValues(typeof(ExpressionType)).Length)
            return;

        expression = (ExpressionType)index;
    }

    /// <summary>
    /// 골드 획득
    /// </summary>
    public void AddMoney(int amount) => money += amount;

    // ========== Gizmos ==========

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // 마우스 위치 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(mouseWorldPosition, 0.2f);

        // 플레이어 → 마우스 방향 선
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, mouseWorldPosition);

        // 공격 타입에 따른 범위 표시
        Vector2 direction = (mouseWorldPosition - (Vector2)transform.position).normalized;
        AttackRangeType attackType = ExpressionData.GetAttackRange(expression);

        if (attackType == AttackRangeType.Melee)
        {
            // 근거리: 공격 범위 원
            Gizmos.color = Color.red;
            Vector2 attackPos = (Vector2)transform.position + direction * meleeAttackOffset;
            Gizmos.DrawWireSphere(attackPos, meleeAttackRadius);
        }
        else
        {
            // 원거리: 레이캐스트 범위
            Gizmos.color = Color.green;
            Vector2 endPoint = (Vector2)transform.position + direction * rangedAttackRange;
            Gizmos.DrawLine(transform.position, endPoint);
        }
    }
}
