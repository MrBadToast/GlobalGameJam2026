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
    public WeaponType Weapon => expression.ToWeapon();  // 표정에서 자동 파생
    public bool IsDead { get; private set; } = false;
    public Animator Animator => animator;

    // ========== 플레이어 전용 ==========
    [Header("Player Only")]
    [SerializeField] private int money = 0;
    public int Money => money;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("ChildReferences")]
    [SerializeField, Required()] private Animator animator;

    private Rigidbody2D rb;
    private Vector2 inputVector;

    private InputSystem_Actions input;


    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
        input = new InputSystem_Actions();
        currentHealth = maxHealth;
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
        if (IsDead) return;

        float h = input.Player.Move.ReadValue<Vector2>().x;
        float v = input.Player.Move.ReadValue<Vector2>().y;
        inputVector = new Vector2(h, v);

        if (inputVector.sqrMagnitude > 1f)
            inputVector = inputVector.normalized;

        animator.SetBool("IsMove", inputVector.sqrMagnitude > 0f);
        animator.SetFloat("MoveX", inputVector.x);
        animator.SetFloat("MoveY", inputVector.y);
    }

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

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
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
}
