using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player_Topdown : StaticSerializedMonoBehaviour<Player_Topdown>
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("ChildReferences")]
    [SerializeField,Required()] private Animator animator;

    private Rigidbody2D rb;
    private Vector2 inputVector;

    private InputSystem_Actions input;


    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
        input = new InputSystem_Actions();
    }

    private void Update()
    {
        float h = input.Player.Move.ReadValue<Vector2>().x;
        float v = input.Player.Move.ReadValue<Vector2>().y;
        inputVector = new Vector2(h, v);

        if (inputVector.sqrMagnitude > 1f)
            inputVector = inputVector.normalized;

        animator.SetBool("IsMoving", inputVector.sqrMagnitude > 0f);
        animator.SetFloat("MoveX", inputVector.x);
        animator.SetFloat("MoveY", inputVector.y);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = inputVector * moveSpeed;
        float rotationY = 0f;

        if (inputVector.x > 0)
            rotationY = 0f;
        else if (inputVector.x < 0)
            rotationY = 180f;

        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

    }
}
