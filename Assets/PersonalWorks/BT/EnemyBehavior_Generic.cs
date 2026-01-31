using Fusion;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBehavior_Generic : NetworkBehaviour, IEntity
{
    // Ŭ���̾�Ʈ�� �����ؾ� �ϴ� ������
    [Networked] public float CurrentHealth { get; set; }
    [Networked] public bool IsDead { get; set; }

    [Networked]
    [OnChangedRender(nameof(OnHeadingChanged))] // ���� �ٲ�� OnHeadingChanged ����
    public Vector2 NetworkedHeading { get; set; }


    [Title("Properties")]
    [SerializeField] private bool isProjectileAttack = false;
    [SerializeField] private WeaponType weaponType = WeaponType.Weapon1;
    [SerializeField] private ExpressionType expressionType = ExpressionType.Neutral;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float damage = 10;
    [SerializeField] private float defense = 0f;

    [Title("Settings")]
    [SerializeField] private float attackDuration = 0.7f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField, MinMaxSlider(0.1f,10f)] private Vector2 attackCooldown = new Vector2(1.2f, 5f);
    [SerializeField, MinMaxSlider(0.1f,10f)] private Vector2 seekInterval = new Vector2(0.5f, 3f);
    [SerializeField] private float hurtCooldown = 0.3f;
    [SerializeField] private LayerMask attackTarget;
    [SerializeField] private AnimationCurve staggerOffCurve;
    [Title("Audios")]
    [SerializeField] private AudioSource aud_Fire;
    [SerializeField] private AudioSource aud_Swing;
    [SerializeField] private AudioSource aud_Hurt;
    [SerializeField] private AudioSource aud_Death;

    [Title("ChildReferences")]
    [SerializeField, Required] private Animator spriteAnimator;
    [SerializeField, Required] private BoxCollider2D attackHitbox;
    [SerializeField] private WeaponController weaponController;

    [SerializeField, Required] private GameObject damageTextPrefab;
    [SerializeField] private GameObject projectilePrefab;

    [Title("Drop")]
    [SerializeField] private NetworkPrefabRef moneyPrefab;
    [SerializeField] private int dropMoneyMin = 5;
    [SerializeField] private int dropMoneyMax = 15;

    [Title("Debug")]
    [SerializeField] private Transform trackingTarget;

    [SerializeField] private float currentHealth = 0f;

    [SerializeField] private float lastAttackTimer = 0f;
    [SerializeField] private float lastSeekTimer = 0f;
    [SerializeField] private float nextSeekTime = 0f;
    [SerializeField] private float hurtTimer = 0f;

    private float nextAttackTime = 0f;

    private Vector2 forwarding = Vector2.right;

    private EntityStats bonusStats;

    MovementState_Base currentMovementState;
    [SerializeField] private string debug_currentMovement;

    Rigidbody2D rbody;
    public override void Spawned()
    {
        lastAttackTimer = Time.time;
        lastSeekTimer = Time.time;

        currentHealth = maxHealth;
        CurrentHealth = maxHealth;
        rbody = GetComponent<Rigidbody2D>();

        // 무기 타입 초기화 (가면 타입과 1:1 대응)
        if (weaponController != null)
        {
            weaponController.SetWeaponType((int)expressionType);
        }

        ChangeMovementState(new Movement_Roam(this));
    }

    #region Interface Implementation

    Animator IEntity.Animator => spriteAnimator;
    public float MaxHealth => maxHealth;
    //public float CurrentHealth => currentHealth;
    public ExpressionType Expression => expressionType;
    public WeaponType Weapon => weaponType;
    //public NetworkBool IsDead => isDead;

    public GameObject GameObject => this.gameObject;

    public EntityStats BonusStats => bonusStats;
    
    public void TakeDamage(float damage, Vector2 direction)
    {
        if (Object == null || !Object.IsValid) return; // ��Ʈ��ũ ��ü�� ��ȿ���� ������ �ߴ�
        if (IsDead) return;

        float effectiveDamage = damage - defense;
        if (effectiveDamage < 0f)
            effectiveDamage = 0f;

        /*GameObject dmgTextObj = Instantiate(damageTextPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        dmgTextObj.GetComponent<DamageText>().SetText(effectiveDamage.ToString("F0"));*/

        OnHurt(direction, damage);
    }

    #endregion

    #region Unity Events
    private void OnHeadingChanged()
    {
        if (spriteAnimator != null)
        {
            spriteAnimator.SetFloat("Heading_X", NetworkedHeading.x);
            spriteAnimator.SetFloat("Heading_Y", NetworkedHeading.y);
        }

        // 무기 조준 방향 업데이트
        if (weaponController != null)
        {
            weaponController.SetAimDirection(NetworkedHeading);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // ȣ��Ʈ�� �ƴ϶�� �Ʒ� ������ �������� ����
        if (!Object.HasStateAuthority || IsDead) return;
        
        if (currentMovementState != null)
            currentMovementState.UpdateState();

        rbody.linearVelocity = Vector2.Lerp(rbody.linearVelocity, Vector2.zero, 0.2f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion

    #region MovementStates

    public void ChangeMovementState(MovementState_Base newMovementInstance)
    {
        currentMovementState?.ExitState();
        currentMovementState = newMovementInstance;
        currentMovementState.EnterState();
        debug_currentMovement = currentMovementState.DebugStateName;
    }

    public class MovementState_Base
    {
        protected EnemyBehavior_Generic enemy;
        protected string debugStateName = "None";
        public string DebugStateName => debugStateName;

        public MovementState_Base(EnemyBehavior_Generic enemy)
        {
            this.enemy = enemy;
        }
        public virtual void EnterState() { }
        public virtual void UpdateState() { }
        public virtual void ExitState() { }
    }

    class Movement_Idle : MovementState_Base
    {
        float nextSeekTime = 0f;

        public Movement_Idle(EnemyBehavior_Generic enemy) : base(enemy)
        {
            debugStateName = "Idle";
        }
        public override void EnterState()
        {
            enemy.spriteAnimator.Play("Idle");
            nextSeekTime = Random.Range(enemy.seekInterval.x, enemy.seekInterval.y);
        }
        public override void UpdateState()
        {
            if (enemy.lastSeekTimer >= nextSeekTime + Time.time)
            {
                enemy.lastSeekTimer = Time.time;
                enemy.DecideForState(enemy);
                return;
            }
        }
    }

    class Movement_Roam : MovementState_Base
    {
        //Vector2 roamDirection;

        public Movement_Roam(EnemyBehavior_Generic enemy) : base(enemy)
        {
            debugStateName = "Roam";
        }

        public override void EnterState()
        {
            enemy.NetworkedHeading = Random.insideUnitCircle.normalized;
            enemy.spriteAnimator.Play("Move");
            enemy.nextSeekTime = Random.Range(enemy.seekInterval.x, enemy.seekInterval.y);
        }

        public override void UpdateState()
        {
            enemy.rbody.linearVelocity = enemy.NetworkedHeading * enemy.moveSpeed;

            if (Time.time >= enemy.nextSeekTime + enemy.lastSeekTimer)
            {
                enemy.lastSeekTimer = Time.time;
                enemy.DecideForState(enemy);
                return;
            }
        }
    }

    class Movement_Chase : MovementState_Base
    {
        public bool CanAttack()
        {
            return Time.time >= enemy.lastAttackTimer + enemy.nextAttackTime;
        }

        public Movement_Chase(EnemyBehavior_Generic enemy) : base(enemy)
        {
            debugStateName = "Chase";
        }
        public override void EnterState()
        {
            enemy.spriteAnimator.Play("Move");
        }
        public override void UpdateState()
        {
            float distanceToTarget = Vector2.Distance(enemy.transform.position, enemy.trackingTarget.position);

            enemy.HeadForTarget(enemy.trackingTarget.position);

            Vector2 direction = (enemy.trackingTarget.position - enemy.transform.position).normalized;
            enemy.rbody.linearVelocity = direction * enemy.moveSpeed;

            if (distanceToTarget <= enemy.attackRange && CanAttack())
            {
                enemy.ChangeMovementState(new Movement_Attack(enemy));
                return;
            }
            else if (distanceToTarget > enemy.detectionRange)
            {
                enemy.ChangeMovementState(new Movement_Roam(enemy));
                return;
            }
            
        }
    }

    class Movement_Attack : MovementState_Base
    {
        Coroutine attackCor;
        public bool IsAttacking => attackCor != null;

        private IEnumerator Cor_Attack()
        {
            enemy.HeadForTarget(enemy.trackingTarget.position);
            enemy.PerformAttack();

            yield return new WaitForSeconds(enemy.attackDuration);

            enemy.lastAttackTimer = Time.time;
            attackCor = null;
        }

        public Movement_Attack(EnemyBehavior_Generic enemy) : base(enemy)
        {
            debugStateName = "Attack";
        }
        public override void EnterState()
        {
            enemy.spriteAnimator.Play("Attack");
            enemy.lastAttackTimer = Time.time;
            enemy.nextAttackTime = Random.Range(enemy.attackCooldown.x, enemy.attackCooldown.y);
            attackCor = enemy.StartCoroutine(Cor_Attack());
        }
        public override void UpdateState()
        {
            if (!IsAttacking)
            {
                enemy.DecideForState(enemy);
                return;
            }
        }

        public override void ExitState()
        {
            if (attackCor != null)
            {
                enemy.StopCoroutine(attackCor);
                attackCor = null;
            }
        }
    }

    class Movement_Stagger : MovementState_Base
    {
        float staggerDuration = 0.5f;
        float timeStaggered = 0f;
        Vector2 pushForce = Vector2.zero;

        public Movement_Stagger(EnemyBehavior_Generic enemy, Vector2 force) : base(enemy)
        {
            debugStateName = "Stagger";
            pushForce = force;
            enemy.rbody.linearVelocity = Vector2.zero;
        }
        public override void EnterState()
        {
            //enemy.spriteAnimator.Play("Stagger");
        }
        public override void UpdateState()
        {
            timeStaggered += Time.fixedDeltaTime;
            enemy.rbody.linearVelocity = pushForce * enemy.staggerOffCurve.Evaluate(timeStaggered / staggerDuration);

            if (timeStaggered >= staggerDuration)
            {
                enemy.DecideForState(enemy);
                return;
            }
        }
    }

    class Movement_Dead : MovementState_Base
    {
        float despawnDelay = 2f;
        float despawnTimer = 0f;

        public Movement_Dead(EnemyBehavior_Generic enemy) : base(enemy)
        {
            debugStateName = "Dead";
        }
        public override void EnterState()
        {
            enemy.spriteAnimator.Play("Dead");
            enemy.rbody.linearVelocity = Vector2.zero;
            enemy.rbody.bodyType = RigidbodyType2D.Kinematic;
        }

        public override void UpdateState()
        {
            despawnTimer += Time.fixedDeltaTime;

            if (despawnTimer >= despawnDelay)
            {
                enemy.Runner.Despawn(enemy.GetComponent<NetworkObject>());
            }
        }
    }

    #endregion

    #region Public / Interface Methods

    public void OnHurt(Vector2 force, float damage)
    {
        aud_Hurt.Play();

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            ChangeMovementState(new Movement_Stagger(this, force));
        }
    }

    public void Die()
    {
        aud_Death.Play();
        currentHealth = 0f;
        IsDead = true;

        // 서버에서만 돈 드랍 및 Despawn 처리
        if (Object.HasStateAuthority)
        {
            SpawnMoneyDrop();
            StartCoroutine(Cor_DespawnAfterDelay(2f));
        }

        ChangeMovementState(new Movement_Dead(this));
    }

    private IEnumerator Cor_DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (Object != null && Object.IsValid)
        {
            Runner.Despawn(Object);
        }
    }

    private void SpawnMoneyDrop()
    {
        if (!moneyPrefab.IsValid) return;

        // 랜덤 위치에 돈 스폰
        Vector3 dropPos = transform.position + (Vector3)Random.insideUnitCircle * 0.5f;
        var moneyObj = Runner.Spawn(moneyPrefab, dropPos, Quaternion.identity);

        // 금액 설정
        if (moneyObj.TryGetComponent<MoneyPickup>(out var pickup))
        {
            int amount = Random.Range(dropMoneyMin, dropMoneyMax + 1);
            pickup.SetAmount(amount);
        }
    }

    #endregion

    #region Private Methods 

    private bool SeekTarget(float range)
    {
        RaycastHit2D[] targets = Physics2D.CircleCastAll(transform.position, range, Vector2.up, 0f, attackTarget); // Physics2D.OverlapCircle(transform.position, range, attackTarget);

        if (targets.Length <= 0) return false;

        RaycastHit2D nearest = targets[0];

        if (nearest)
        {
            trackingTarget = nearest.transform;
            return true;
        }
        else
        {
            trackingTarget = null;
            return false;
        }
    }

    private void HeadForTarget(Vector3 target)
    {
        NetworkedHeading = (target - transform.position).normalized;

        spriteAnimator.SetFloat("Heading_X", NetworkedHeading.x);
        spriteAnimator.SetFloat("Heading_Y", NetworkedHeading.y);
    }

    private void DecideForState(EnemyBehavior_Generic enemyInstance)
    {
        if (SeekTarget(enemyInstance.attackRange))
        {
            ChangeMovementState(new Movement_Attack(enemyInstance));
        }
        else if (SeekTarget(enemyInstance.detectionRange))
        {
            ChangeMovementState(new Movement_Chase(enemyInstance));
        }
        else
        {
            ChangeMovementState(new Movement_Roam(enemyInstance));
        }
    }

    private void PerformAttack()
    {
        if(trackingTarget == null)
            return;

        // 무기 공격 애니메이션
        if (weaponController != null)
            weaponController.PlayAttack();

        if (isProjectileAttack)
        {
            aud_Fire.Play();

            if (projectilePrefab == null)
                return;

            NetworkObject projectileObj = Runner.Spawn(projectilePrefab, attackHitbox.bounds.center, Quaternion.identity);
            projectileObj.transform.right = (trackingTarget.position - transform.position).normalized;
            return;
        }
        else
        {
            aud_Swing.Play();

            Physics2D.OverlapBoxAll(attackHitbox.bounds.center, attackHitbox.bounds.size, 0f, attackTarget)
                .ToList()
                .ForEach(target =>
                {
                    IEntity entity = target.GetComponent<IEntity>();
                    entity?.TakeDamage(damage, (target.transform.position - transform.position).normalized);
                });
        }
    }

    #endregion

}
