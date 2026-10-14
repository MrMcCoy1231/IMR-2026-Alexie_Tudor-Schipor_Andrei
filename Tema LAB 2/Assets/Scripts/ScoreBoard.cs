using TMPro;
using UnityEngine;

// Pe Canvas (World Space). Afiseaza scorul total si ultima lovitura.
public class ScoreBoard : MonoBehaviour
{
    public TMP_Text totalText;
    public TMP_Text lastHitText;

    int total;
    int hits;

    void Start()
    {
        Refresh("Arunca o sageata!");
    }

    public void AddHit(int points, float distance)
    {
        total += points;
        hits++;
        Refresh($"+{points} puncte\n({distance * 100f:F0} cm de centru)");
    }

    void Refresh(string lastHit)
    {
        if (totalText != null) totalText.text = $"Scor: {total}   Lovituri: {hits}";
        if (lastHitText != null) lastHitText.text = lastHit;
    }
}
