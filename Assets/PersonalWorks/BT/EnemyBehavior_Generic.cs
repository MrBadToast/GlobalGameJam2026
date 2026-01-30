using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBehavior_Generic : MonoBehaviour, IEntity
{
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

    [SerializeField, Required] private GameObject damageTextPrefab;
    [SerializeField] private GameObject projectilePrefab;

    [Title("Debug")]
    [SerializeField,ReadOnly] private Transform trackingTarget;

    [SerializeField, ReadOnly] private float currentHealth = 0f;

    [SerializeField, ReadOnly] private float lastAttackTimer = 0f;
    [SerializeField, ReadOnly] private float lastSeekTimer = 0f;
    [SerializeField, ReadOnly] private float nextSeekTime = 0f;
    [SerializeField, ReadOnly] private float hurtTimer = 0f;

    private float nextAttackTime = 0f;

    private bool isDead = false;

    private Vector2 forwarding = Vector2.right;

    private EntityStats bonusStats;

    MovementState_Base currentMovementState;
    [SerializeField, ReadOnly] private string debug_currentMovement;

    Rigidbody2D rbody;
    private void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
    }

    #region Interface Implementation

    Animator IEntity.Animator => spriteAnimator;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public ExpressionType Expression => expressionType;
    public WeaponType Weapon => weaponType;
    public bool IsDead => isDead;

    public GameObject GameObject => this.GameObject;

    public EntityStats BonusStats => bonusStats;

    public void TakeDamage(float damage, Vector2 direction)
    {
        if(IsDead)
            return;

        float effectiveDamage = damage - defense;
        if (effectiveDamage < 0f)
            effectiveDamage = 0f;

        GameObject dmgTextObj = Instantiate(damageTextPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        dmgTextObj.GetComponent<DamageText>().SetText(effectiveDamage.ToString("F0"));

        OnHurt(direction, damage);
    }

    #endregion

    #region Unity Events
    private void Start()
    {
        lastAttackTimer = Time.time;
        lastSeekTimer = Time.time;

        currentHealth = maxHealth;

        ChangeMovementState(new Movement_Roam(this));
    }

    private void Update()
    {
        currentMovementState.UpdateState();
    }

    private void FixedUpdate()
    {
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
            if (enemy.lastSeekTimer >= nextSeekTime + Time.deltaTime)
            {
                enemy.lastSeekTimer = Time.time;
                enemy.DecideForState(enemy);
                return;
            }
        }
    }

    class Movement_Roam : MovementState_Base
    {
        Vector2 roamDirection;

        public Movement_Roam(EnemyBehavior_Generic enemy) : base(enemy)
        {
            debugStateName = "Roam";
        }

        public override void EnterState()
        {
            roamDirection = Random.insideUnitCircle.normalized;
            enemy.spriteAnimator.Play("Move");
            enemy.nextSeekTime = Random.Range(enemy.seekInterval.x, enemy.seekInterval.y);
        }

        public override void UpdateState()
        {
            enemy.rbody.linearVelocity = roamDirection * enemy.moveSpeed;

            enemy.spriteAnimator.SetFloat("Heading_X", roamDirection.x);
            enemy.spriteAnimator.SetFloat("Heading_Y", roamDirection.y);

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
            enemy.spriteAnimator.Play("Stagger");
        }
        public override void UpdateState()
        {
            timeStaggered += Time.deltaTime;
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
            enemy.enabled = false;
        }

        public override void UpdateState()
        {
            despawnTimer += Time.deltaTime;

            if (despawnTimer >= despawnDelay)
            {
                GameObject.Destroy(enemy.gameObject);
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
        ChangeMovementState(new Movement_Dead(this));
        isDead = true;
    }

    #endregion

    #region Private Methods 

    private bool SeekTarget(float range)
    {
        RaycastHit2D target = Physics2D.CircleCast(transform.position, range, Vector2.up, 0f, attackTarget); // Physics2D.OverlapCircle(transform.position, range, attackTarget);

        if(target)
        {
            trackingTarget = target.transform;
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
        Vector2 direction = (target - transform.position).normalized;

        spriteAnimator.SetFloat("Heading_X", direction.x);
        spriteAnimator.SetFloat("Heading_Y", direction.y);
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

        if (isProjectileAttack)
        {
            aud_Fire.Play();

            if (projectilePrefab == null)
                return;

            GameObject projectileObj = Instantiate(projectilePrefab, attackHitbox.bounds.center, Quaternion.identity);
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
