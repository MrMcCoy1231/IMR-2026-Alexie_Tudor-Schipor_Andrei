using System.Collections;
using UnityEngine;

// Pe Target. Detecteaza lovitura, calculeaza scorul dupa distanta de centru, semnal vizual.
public class DartTarget : MonoBehaviour
{
    public ScoreBoard scoreBoard;

    [Tooltip("Punctaj pe zone, de la centru spre margine")]
    public int[] ringScores = { 50, 25, 15, 10, 5 };

    [Header("Semnal vizual")]
    public Color flashColor = Color.yellow;
    public float flashTime = 0.3f;

    Renderer rend;
    Color baseColor;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        baseColor = rend.material.color;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody == null) return;
        DartFlight dart = collision.rigidbody.GetComponent<DartFlight>();
        if (dart == null || dart.HasScored) return;

        Vector3 hitPoint = collision.GetContact(0).point;

        // distanta de la centru, masurata pe fata discului (normala = axa Y a cilindrului)
        Vector3 offset = Vector3.ProjectOnPlane(hitPoint - transform.position, transform.up);
        float distance = offset.magnitude;
        float radius = transform.lossyScale.x * 0.5f;        // cilindrul are raza 0.5 in spatiul local

        float t = Mathf.Clamp01(distance / radius);          // 0 = centru, 1 = margine
        int ring = Mathf.Min((int)(t * ringScores.Length), ringScores.Length - 1);
        int points = ringScores[ring];

        Debug.Log($"Lovitura! Distanta de centru: {distance * 100f:F1} cm, zona {ring + 1}, {points} puncte");

        dart.StickInto(hitPoint);
        if (scoreBoard != null) scoreBoard.AddHit(points, distance);
        StopAllCoroutines();
        StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        rend.material.color = flashColor;
        yield return new WaitForSeconds(flashTime);
        rend.material.color = baseColor;
    }
}
