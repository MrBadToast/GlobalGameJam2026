using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    private Animator anim;
    [SerializeField] private float destroyDelay = 1.0f; // 애니메이션 길이에 맞춰 조절

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        int randomIndex = Random.Range(0, 6);

        anim.Play("Slash_" + randomIndex);

        Invoke(nameof(SelfDestroy), destroyDelay);
    }

    void SelfDestroy()
    {
        Destroy(gameObject);
    }
}
