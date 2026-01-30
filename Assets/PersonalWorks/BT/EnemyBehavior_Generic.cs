using Sirenix.OdinInspector;
using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBehavior_Generic : EnemyBehavior_Base
{

    [Title("¼³Á¤")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField, MinMaxSlider(0.1f,10f)] private Vector2 attackCooldown = new Vector2(1.2f, 5f);
    [SerializeField, MinMaxSlider(0.1f,10f)] private Vector2 seekInterval = new Vector2(0.5f, 3f);
    [SerializeField] private float attackDuration = 0.6f;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask attackTarget;
    [SerializeField] private AnimationCurve staggerOffCurve;

    [Title("ChildReferences")]
    [SerializeField, Required] private Animator spriteAnimator;

    [Title("Debug")]
    [SerializeField,ReadOnly] private Transform trackingTarget;

    private float lastAttackTime = 0;
    private float lastSeekTime = 0;
    private float seekRangeOutTime = 0;

    private float nextAttackTime = 0f;

    MovementState_Base currentMovementState;
    [SerializeField, ReadOnly] private string debug_currentMovement;

    Rigidbody2D rbody;
    private void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
    }

    #region Unity Events
    protected override void Start()
    {
        lastAttackTime = Time.time;
        lastSeekTime = Time.time;

        ChangeMovementState(new Movement_Roam(this));
    }

    private void Update()
    {
        currentMovementState.UpdateState();
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
            if (enemy.lastSeekTime >= nextSeekTime + Time.deltaTime)
            {
                enemy.lastSeekTime = Time.time;
                enemy.DecideForState(enemy);
                return;
            }
        }
    }

    class Movement_Roam : MovementState_Base
    {
        float nextSeekTime = 0f;
        Vector2 roamDirection;

        public Movement_Roam(EnemyBehavior_Generic enemy) : base(enemy)
        {
            roamDirection = Random.insideUnitCircle.normalized;
            debugStateName = "Roam";
        }

        public override void EnterState()
        {
            enemy.spriteAnimator.Play("Move");
            nextSeekTime = Random.Range(enemy.seekInterval.x, enemy.seekInterval.y);
        }

        public override void UpdateState()
        {
           enemy.rbody.linearVelocity = roamDirection * enemy.moveSpeed;

            if (enemy.lastSeekTime >= nextSeekTime + Time.deltaTime)
            {
                enemy.lastSeekTime = Time.time;
                enemy.DecideForState(enemy);
                return;
            }
        }
    }

    class Movement_Chase : MovementState_Base
    {
        public bool CanAttack()
        {
            return Time.time >= enemy.lastAttackTime + enemy.nextAttackTime;
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
            enemy.FaceToTarget();
            enemy.PerformAttack();

            yield return new WaitForSeconds(enemy.attackDuration);

            attackCor = null;
        }

        public Movement_Attack(EnemyBehavior_Generic enemy) : base(enemy)
        {
            debugStateName = "Attack";
        }
        public override void EnterState()
        {
            enemy.spriteAnimator.Play("Attack");
            enemy.lastAttackTime = Time.time;
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

    #region Public / Override Methods

    public override void OnHurt(Vector2 force, float damage)
    {
        base.OnHurt(force, damage);

        ChangeMovementState(new Movement_Stagger(this, force));
    }

    public override void Die()
    {
        base.Die();

        ChangeMovementState(new Movement_Dead(this));
    }

    #endregion

    #region Private Methods 

    private bool SeekTarget(float range)
    {
        Collider2D target = Physics2D.OverlapCircle(transform.position, detectionRange, attackTarget);

        if(target != null)
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

    private void DecideForState(EnemyBehavior_Generic enemyInstance)
    {
        if (SeekTarget(attackRange))
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

    private void FaceToTarget()
    {
        if (trackingTarget == null)
            return;

        float direction = trackingTarget.position.x - transform.position.x;

        if (direction > 0f) transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        else if (direction < 0f) transform.localRotation = Quaternion.Euler(0f, 180f, 0f);  

    }

    private void PerformAttack()
    {
        
    }

    #endregion


    

}
