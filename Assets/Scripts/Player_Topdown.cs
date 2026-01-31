using Fusion;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movementInput;
    public Vector2 attackDirection; // 공격 방향 추가
    public NetworkBool isAttackPressed; // 공격 버튼 상태 추가
}

[RequireComponent(typeof(Rigidbody2D))]
public class Player_Topdown : NetworkBehaviour, IEntity, IPlayerLeft
{
    // ============= NickName =================
    public TextMeshProUGUI playerNickName;
    public static Player_Topdown Local { get; private set; }

    [Networked]
    [OnChangedRender(nameof(OnNickNameChanged))]
    public NetworkString<_16> nickName { get; set; }

    // ============= Attack ================
    [SerializeField, Required] private GameObject damageTextPrefab;

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
    [SerializeField/*, Required()*/] private Animator animator;

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

    public override void Spawned()
    {
        OnNickNameChanged();
        if (Object.HasInputAuthority)
        {
            Local = this;

            // 2. 로컬 플레이어의 닉네임을 설정
            string savedNickName = PlayerPrefs.GetString("PlayerNickName", "Unknown");

            // 호스트라면 직접 설정, 클라이언트라면 RPC 호출
            if (Object.HasStateAuthority)
            {
                nickName = savedNickName;
            }
            else
            {
                RPC_SetNickName(savedNickName);
            }

            Debug.Log("Spawned local player!!");
        }
        else
        {
            Debug.Log("Spawned remote player!!!!");
        }

        transform.name = $"P_{Object.Id}";
    }

    public void Awake()
    {
        animator = GetComponentInChildren<Animator>();
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
        if (!Object.HasInputAuthority) return;

        // 마우스 월드 좌표 저장 (Gizmo용)
        if (mainCamera != null)
        {
            mouseWorldPosition = mainCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
        }

        if (IsDead) return;

        HandleMovementInput();
        HandleExpressionInput();
        GetNetworkInput();
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

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData data = new NetworkInputData();
        data.movementInput = inputVector;

        // 마우스 방향 계산
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
        data.attackDirection = (mouseWorldPos - (Vector2)transform.position).normalized;

        // 이번 틱에 공격 버튼을 눌렀는지 확인
        data.isAttackPressed = input.Player.Attack.WasPressedThisFrame();

        return data;
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

    public override void FixedUpdateNetwork()
    {
        if (IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 입력을 가져옴 (서버와 클라이언트 모두에서 실행되지만, 
        // StateAuthority인 서버의 계산이 최종 확정됨)
        if (GetInput(out NetworkInputData inputData))
        {
            // 공격 버튼이 눌렸을 때만 실행
            if (inputData.isAttackPressed)
            {
                // 현재 표정에 따른 공격 타입 확인
                AttackRangeType attackType = ExpressionData.GetAttackRange(expression);

                if (attackType == AttackRangeType.Melee)
                {
                    ProcessMeleeAttack(inputData.attackDirection);
                }
                else if (attackType == AttackRangeType.Ranged)
                {
                    ProcessRangedAttack(inputData.attackDirection);
                }
            }
        }
    }

    // ========== [근접 공격 로직] ==========
    private void ProcessMeleeAttack(Vector2 direction)
    {
        float cooldown = attackCooldown / ExpressionData.GetAttackSpeedMultiplier(expression, bonusStats);
        if (Time.time < lastAttackTime + cooldown) return;
        lastAttackTime = Time.time;

        // 서버에서만 실제 판정 수행
        if (Object.HasStateAuthority)
        {
            Vector2 attackPos = (Vector2)transform.position + direction * meleeAttackOffset;

            // CombatUtils.Attack 내부에 target.TakeDamage 로직이 있다고 가정합니다.
            CombatUtils.Attack(
                this,
                attackPos,
                meleeAttackRadius,
                CombatUtils.MonsterMask,
                baseDamage,
                meleeAttackSound,
                soundVolume
            );

            // 애니메이션 및 사운드 동기화를 위한 RPC (필요 시)
            RPC_PlayMeleeEffects(attackPos);
        }
    }

    // ========== [원거리 공격 로직] ==========
    private void ProcessRangedAttack(Vector2 direction)
    {
        float cooldown = attackCooldown / ExpressionData.GetAttackSpeedMultiplier(expression, bonusStats);
        if (Time.time < lastAttackTime + cooldown) return;
        lastAttackTime = Time.time;

        if (Object.HasStateAuthority)
        {
            Vector2 origin = (Vector2)transform.position + direction * 0.5f;
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, rangedAttackRange, CombatUtils.MonsterMask);

            float damage = 0;
            Vector2 hitPoint = origin + direction * rangedAttackRange;
            Vector3 targetPos = Vector3.zero;

            if (hit.collider != null)
            {
                hitPoint = hit.point;
                if (hit.collider.TryGetComponent<IEntity>(out var target))
                {
                    damage = baseDamage;
                    target.TakeDamage(damage, direction);
                    targetPos = hit.collider.transform.position;
                }
            }

            // 모든 클라이언트에게 시각 효과 재생 요청
            RPC_PlayShootEffects(damage, targetPos, hitPoint);
        }
    }

    // ========== [원거리 공격 이펙트] ==========
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayShootEffects(float damage, Vector3 targetPos, Vector2 endPoint)
    {
        // 여기서 사운드 재생 및 LineRenderer 표시만 수행 (데미지 로직 삭제)
        StartCoroutine(ShowRayLine(transform.position, endPoint));
        if (damage > 0 && targetPos != Vector3.zero)
        {
            GameObject dmgTextObj = Instantiate(damageTextPrefab, targetPos + Vector3.up * 0.5f, Quaternion.identity);
            dmgTextObj.GetComponent<DamageText>().SetText(damage.ToString("F0"));
        }
    }

    // ========== [근거리 공격 이펙트] ==========
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayMeleeEffects(Vector2 effectPos)
    {
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

    private void OnNickNameChanged()
    {
        Debug.Log($"Nick anme changed for player to {nickName} for player {gameObject.name}");

        playerNickName.text = nickName.ToString();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string nickName, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetNickName {nickName}");
        this.nickName = nickName;
    }

    public void PlayerLeft(PlayerRef player)
    {
        throw new System.NotImplementedException();
    }
}
