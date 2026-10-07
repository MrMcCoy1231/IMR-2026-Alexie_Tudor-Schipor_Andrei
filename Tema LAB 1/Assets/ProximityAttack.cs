using UnityEngine;
using Vuforia;

public class ProximityAttack : MonoBehaviour
{
    [Header("Target-uri")]
    public ObserverBehaviour personaj1;
    public ObserverBehaviour personaj2;

    [Header("Animatoare")]
    public Animator animator1;
    public Animator animator2;

    [Header("Atac")]
    [Tooltip("Sub această distanță personajele atacă")]
    public float distantaAtac = 0.25f;
    [Tooltip("Pauza minimă între două atacuri (secunde)")]
    public float pauzaAtac = 2f;

    float ultimulAtac = -100f;
    bool eraAproape = false;

    bool EsteUrmarit(ObserverBehaviour o)
    {
        var s = o.TargetStatus.Status;
        return s == Status.TRACKED || s == Status.EXTENDED_TRACKED;
    }

    void Update()
    {
        if (!EsteUrmarit(personaj1) || !EsteUrmarit(personaj2))
        {
            eraAproape = false;
            return;
        }

        float d = Vector3.Distance(personaj1.transform.position, personaj2.transform.position);
        bool esteAproape = d < distantaAtac;

        if (esteAproape && !eraAproape)
            Debug.Log($"Personaj detectat în apropiere (distanța: {d:F2})");

        if (esteAproape && Time.time - ultimulAtac > pauzaAtac)
        {
            Debug.Log("ATAC!");
            if (animator1 != null) animator1.SetTrigger("Attack");
            if (animator2 != null) animator2.SetTrigger("Attack");
            ultimulAtac = Time.time;
        }

        eraAproape = esteAproape;
    }
}