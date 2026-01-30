using System.Collections;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshPro textmesh;

    private void Start()
    {
        StartCoroutine(Cor_DelayedDestroy());
    }

    public void SetText(string text)
    {
        textmesh.text = text;
    }

    IEnumerator Cor_DelayedDestroy()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
