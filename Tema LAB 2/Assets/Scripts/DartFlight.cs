using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Pe Dart. Zbor cu varful inainte, infigere in tinta, intoarcere automata pe masa.
[RequireComponent(typeof(Rigidbody))]
public class DartFlight : MonoBehaviour
{
    [Tooltip("Sub aceasta viteza nu mai corectam rotatia")]
    public float minSpeed = 1f;
    [Tooltip("Dupa cate secunde se intoarce sageata pe masa")]
    public float returnDelay = 2f;
    [Tooltip("Sub aceasta inaltime consideram ca a cazut pe jos")]
    public float floorY = 0.1f;

    public bool HasScored { get; private set; }

    Rigidbody rb;
    XRGrabInteractable grab;
    Vector3 startPos;
    Quaternion startRot;
    Vector3 lastVelocity;
    bool stuck;
    Coroutine returnRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();
        startPos = transform.position;
        startRot = transform.rotation;
        if (grab != null) grab.selectEntered.AddListener(OnGrab);
    }

    void OnDestroy()
    {
        if (grab != null) grab.selectEntered.RemoveListener(OnGrab);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        ResetState();
    }

    // readuce sageata in starea "libera" (folosit la grab si de DebugThrow)
    public void ResetState()
    {
        if (returnRoutine != null) { StopCoroutine(returnRoutine); returnRoutine = null; }
        stuck = false;
        HasScored = false;
        rb.constraints = RigidbodyConstraints.None;
    }

    void FixedUpdate()
    {
        if ((grab != null && grab.isSelected) || stuck) return;

        // a cazut pe jos -> o readucem pe masa
        if (transform.position.y < floorY && returnRoutine == null)
            returnRoutine = StartCoroutine(ReturnAfter(returnDelay));

        Vector3 v = rb.linearVelocity;
        if (v.sqrMagnitude < minSpeed * minSpeed) return;

        lastVelocity = v;
        // capsula are axa lunga pe Y: rotim ca +Y (varful) sa arate in directia zborului
        rb.MoveRotation(Quaternion.LookRotation(v) * Quaternion.Euler(90f, 0f, 0f));
        rb.angularVelocity = Vector3.zero;
    }

    // apelata de DartTarget cand sageata loveste tinta
    public void StickInto(Vector3 hitPoint)
    {
        stuck = true;
        HasScored = true;

        Vector3 dir = lastVelocity.sqrMagnitude > 0.01f ? lastVelocity.normalized : transform.up;
        float halfLength = transform.lossyScale.y;            // capsula are inaltimea 2 * scale.y
        transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90f, 0f, 0f);
        transform.position = hitPoint - dir * (halfLength * 0.75f); // varful intra putin in tinta

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;      // ramane infipta

        if (returnRoutine != null) StopCoroutine(returnRoutine);
        returnRoutine = StartCoroutine(ReturnAfter(returnDelay));
    }

    IEnumerator ReturnAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (grab != null && grab.isSelected) { returnRoutine = null; yield break; }

        rb.constraints = RigidbodyConstraints.None;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = startPos;
        rb.rotation = startRot;
        transform.SetPositionAndRotation(startPos, startRot);

        stuck = false;
        HasScored = false;
        returnRoutine = null;
    }
}
