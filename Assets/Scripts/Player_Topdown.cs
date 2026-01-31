using System;
using System.Collections;
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
    [Networked] public float NetworkedHealth { get; set; }

    [Header("Expression")]
    [SerializeField] private ExpressionType expression;

    // IEntity 속성
    public float MaxHealth => maxHealth;
    public float CurrentHealth => NetworkedHealth;
    public ExpressionType Expression => expression;
    [Networked] public NetworkBool NetworkedIsDead { get; set; }
    public bool IsDead => NetworkedIsDead;
    public Animator Animator => animator;
    public GameObject GameObject => gameObject;

    // ========== 플레이어 전용 ==========
    [Header("Player Only")]
    [Networked] public int Money { get; set; }

    [Header("Potion")]
    [SerializeField] private ItemData[] potionTypes = new ItemData[3];  // 물약 종류 3개 (Inspector에서 할당)
    [Networked, Capacity(3)] public NetworkArray<int> PotionCounts => default;
    private int selectedPotionIndex = 0;

    public int SelectedPotionIndex => selectedPotionIndex;
    public ItemData SelectedPotionData => potionTypes[selectedPotionIndex];
    public int SelectedPotionCount => PotionCounts[selectedPotionIndex];
    public ItemData[] PotionTypes => potionTypes;

    // 물약 변경 이벤트 (UI 갱신용)
    public event Action OnPotionChanged;

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
    private bool isInputEnabled = true;

    private InputSystem_Actions input;

    public bool IsInputEnabled => isInputEnabled;

    public override void Spawned()
    {
        OnNickNameChanged();

        // 호스트에서 체력 초기화
        if (Object.HasStateAuthority)
        {
            NetworkedHealth = maxHealth;
            NetworkedIsDead = false;
        }

        if (Object.HasInputAuthority)
        {
            Local = this;

            // 로컬 플레이어의 닉네임을 설정
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

        if (IsDead || !isInputEnabled)
        {
            // 입력 비활성화 시 이동 멈춤
            if (!isInputEnabled)
            {
                inputVector = Vector2.zero;
                animator.SetBool("IsMove", false);
            }
            return;
        }

        HandleMovementInput();
        HandleExpressionInput();
        HandlePotionSelectInput();
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

        // 서버에서만 체력 감소 처리
        if (Object.HasStateAuthority)
        {
            NetworkedHealth -= damage;

            if (NetworkedHealth <= 0)
            {
                NetworkedHealth = 0;
                Die();
            }
        }

        // 피격 사운드
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, soundVolume);
        }

        // 피격 애니메이션
        animator?.SetTrigger("Hit");
    }

    public void Die()
    {
        if (IsDead) return;

        NetworkedIsDead = true;

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
    public void AddMoney(int amount) => Money += amount;

    // ========== 상점 RPC ==========

    /// <summary>
    /// 물약 구매 (클라이언트 → 서버)
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_BuyItem(int price, int potionIndex, RpcInfo info = default)
    {
        // 서버에서 검증
        if (Money < price) return;
        if (potionIndex < 0 || potionIndex >= 3) return;

        // 돈 차감
        Money -= price;

        // 물약 추가
        PotionCounts.Set(potionIndex, PotionCounts[potionIndex] + 1);
        OnPotionChanged?.Invoke();
    }

    /// <summary>
    /// 버프 아이템 구매 (클라이언트 → 서버)
    /// </summary>
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_BuyBuff(int price, float atk, float dmgTaken, float moveSpd, float atkSpd, RpcInfo info = default)
    {
        // 서버에서 검증
        if (Money < price) return;

        // 돈 차감
        Money -= price;

        // 버프 적용
        bonusStats.AddAttack(atk);
        bonusStats.AddDamageTaken(dmgTaken);
        bonusStats.AddMoveSpeed(moveSpd);
        bonusStats.AddAttackSpeed(atkSpd);
    }

    /// <summary>
    /// 입력 활성화/비활성화 (UI 열림 등)
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        isInputEnabled = enabled;

        if (!enabled)
        {
            // 입력 비활성화 시 즉시 멈춤
            inputVector = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ========== 물약/아이템 ==========

    private void HandlePotionSelectInput()
    {
        float scroll = UnityEngine.Input.mouseScrollDelta.y;

        if (scroll > 0)
        {
            // 스크롤 업: 이전 물약
            selectedPotionIndex--;
            if (selectedPotionIndex < 0) selectedPotionIndex = 2;
            OnPotionChanged?.Invoke();
        }
        else if (scroll < 0)
        {
            // 스크롤 다운: 다음 물약
            selectedPotionIndex++;
            if (selectedPotionIndex > 2) selectedPotionIndex = 0;
            OnPotionChanged?.Invoke();
        }

        // Q키로 물약 사용
        if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
        {
            UseSelectedPotion();
        }
    }

    /// <summary>
    /// 아이템 추가 (물약은 개수 증가, 버프는 즉시 적용)
    /// </summary>
    public bool AddItem(ItemData item)
    {
        if (item == null) return false;

        if (item.category == ItemCategory.Buff)
        {
            // 버프: 즉시 적용
            item.ApplyBuff(bonusStats);
            return true;
        }
        else if (item.category == ItemCategory.Potion)
        {
            // 물약: 해당 종류의 개수 증가
            int potionIndex = GetPotionIndex(item.itemType);
            if (potionIndex >= 0)
            {
                PotionCounts.Set(potionIndex, PotionCounts[potionIndex] + 1);
                OnPotionChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 특정 물약 종류의 개수 추가
    /// </summary>
    public void AddPotion(int potionIndex, int amount = 1)
    {
        if (potionIndex < 0 || potionIndex >= 3) return;
        PotionCounts.Set(potionIndex, PotionCounts[potionIndex] + amount);
        OnPotionChanged?.Invoke();
    }

    /// <summary>
    /// 현재 선택된 물약 사용
    /// </summary>
    public bool UseSelectedPotion()
    {
        if (PotionCounts[selectedPotionIndex] <= 0) return false;
        if (potionTypes[selectedPotionIndex] == null) return false;

        Debug.Log($"{selectedPotionIndex}");

        // 서버에 물약 사용 요청
        RPC_UsePotion(selectedPotionIndex);
        return true;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UsePotion(int potionIndex, RpcInfo info = default)
    {
        if (potionIndex < 0 || potionIndex >= 3) return;
        if (PotionCounts[potionIndex] <= 0) return;
        if (potionTypes[potionIndex] == null) return;

        ItemData potion = potionTypes[potionIndex];

        // 체력 회복
        NetworkedHealth += potion.healAmount;
        if (NetworkedHealth > maxHealth) NetworkedHealth = maxHealth;

        // 개수 감소
        PotionCounts.Set(potionIndex, PotionCounts[potionIndex] - 1);
        OnPotionChanged?.Invoke();
    }

    /// <summary>
    /// 물약 종류 선택
    /// </summary>
    public void SelectPotionSlot(int index)
    {
        if (index < 0 || index >= 3) return;
        selectedPotionIndex = index;
        OnPotionChanged?.Invoke();
    }

    /// <summary>
    /// ItemType으로 물약 인덱스 찾기
    /// </summary>
    private int GetPotionIndex(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Potion1: return 0;
            case ItemType.Potion2: return 1;
            case ItemType.Potion3: return 2;
            default: return -1;
        }
    }

    /// <summary>
    /// 특정 물약의 개수 가져오기
    /// </summary>
    public int GetPotionCount(int index)
    {
        if (index < 0 || index >= 3) return 0;
        return PotionCounts[index];
    }

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
