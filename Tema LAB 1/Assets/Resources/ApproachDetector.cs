using UnityEngine;
using Vuforia;

public class ApproachDetector : MonoBehaviour
{
    public ObserverBehaviour personaj1;
    public ObserverBehaviour personaj2;

    [Tooltip("Cât de mult trebuie să varieze distanța ca să conteze (anti-tremur)")]
    public float prag = 0.02f;

    [Tooltip("La câte secunde compară distanța")]
    public float interval = 0.2f;

    [Tooltip("Netezire: 0 = deloc, aproape de 1 = foarte netezit")]
    [Range(0f, 0.95f)]
    public float netezire = 0.7f;

    enum Stare { Necunoscut, SeApropie, SeDeparteaza, PeLoc }
    Stare stareCurenta = Stare.Necunoscut;

    float distantaNetezita = -1f;
    float distantaAnterioara = -1f;
    float timer = 0f;

    bool EsteUrmarit(ObserverBehaviour o)
    {
        var s = o.TargetStatus.Status;
        return s == Status.TRACKED || s == Status.EXTENDED_TRACKED;
    }

    void Update()
    {
        // Dacă una dintre imagini nu e vizibilă, resetăm
        if (!EsteUrmarit(personaj1) || !EsteUrmarit(personaj2))
        {
            if (stareCurenta != Stare.Necunoscut)
                Debug.Log("Un personaj nu mai e vizibil.");
            stareCurenta = Stare.Necunoscut;
            distantaNetezita = -1f;
            distantaAnterioara = -1f;
            return;
        }

        float d = Vector3.Distance(personaj1.transform.position, personaj2.transform.position);

        // Netezire exponențială
        distantaNetezita = distantaNetezita < 0f
            ? d
            : netezire * distantaNetezita + (1f - netezire) * d;

        timer += Time.deltaTime;
        if (timer < interval) return;
        timer = 0f;

        if (distantaAnterioara < 0f)
        {
            distantaAnterioara = distantaNetezita;
            return;
        }

        float diferenta = distantaNetezita - distantaAnterioara;
        Stare stareNoua;

        if (diferenta < -prag) stareNoua = Stare.SeApropie;
        else if (diferenta > prag) stareNoua = Stare.SeDeparteaza;
        else stareNoua = Stare.PeLoc;

        if (stareNoua != stareCurenta)
        {
            switch (stareNoua)
            {
                case Stare.SeApropie:
                    Debug.Log($"Personajele SE APROPIE (distanța: {distantaNetezita:F2})");
                    break;
                case Stare.SeDeparteaza:
                    Debug.Log($"Personajele SE DEPĂRTEAZĂ (distanța: {distantaNetezita:F2})");
                    break;
                case Stare.PeLoc:
                    Debug.Log($"Personajele stau pe loc (distanța: {distantaNetezita:F2})");
                    break;
            }
            stareCurenta = stareNoua;
        }

        distantaAnterioara = distantaNetezita;
    }
}