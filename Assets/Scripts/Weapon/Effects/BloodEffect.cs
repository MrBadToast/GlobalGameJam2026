using Photon.Pun;
using UnityEngine;

public class BloodEffect : MonoBehaviour
{
    private Animator anim;
    [SerializeField] private float destroyDelay = 1.0f; // 애니메이션 길이에 맞춰 조절

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        int randomIndex = Random.Range(0, 4);

        anim.Play("Blood_" + randomIndex);

        Invoke(nameof(SelfDestroy), destroyDelay);
    }

    void SelfDestroy()
    {
        Destroy(gameObject);
    }
}
