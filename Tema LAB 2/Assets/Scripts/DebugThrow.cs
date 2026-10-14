using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Tasta F: mana dreapta APUCA sageata (Grab/Select prin XRI), o tine putin, apoi o arunca spre tinta.
public class DebugThrow : MonoBehaviour
{
    public Rigidbody dart;
    public Transform target;

    [Tooltip("Interactorul mainii drepte: XR Origin > Camera Offset > Right Controller > Near-Far Interactor")]
    public XRBaseInputInteractor hand;

    [Tooltip("Cat timp tine sageata in mana inainte de aruncare (secunde)")]
    public float holdTime = 0.5f;
    public float speed = 6f;
    [Tooltip("Cat de imprastiate sunt aruncarile (pentru scoruri diferite)")]
    public float spread = 0.2f;

    bool busy;

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || busy || !kb.fKey.wasPressedThisFrame) return;

        if (hand != null) StartCoroutine(GrabAndThrow());
        else
        {
            Transform cam = Camera.main.transform;
            PrepareDart();
            dart.position = cam.position + cam.forward * 0.3f;
            Launch();
        }
    }

    IEnumerator GrabAndThrow()
    {
        busy = true;
        var grab = dart.GetComponent<XRGrabInteractable>();
        PrepareDart();

        // 1. mana apuca sageata (exact ca un Grab real, sageata vine in mana pe AttachPoint)
        // cast la interfata: varianta cu XRBaseInteractable e marcata ca invechita in XRI 3
        hand.StartManualInteraction((IXRSelectInteractable)grab);
        float t = 0f;
        while (!grab.isSelected && t < 0.5f) { t += Time.deltaTime; yield return null; }

        // 2. o tine putin in mana
        yield return new WaitForSeconds(holdTime);

        // 3. ii da drumul
        hand.EndManualInteraction();
        t = 0f;
        while (grab.isSelected && t < 0.5f) { t += Time.deltaTime; yield return null; }

        // 4. dupa ce XRI aplica aruncarea lui, o trimitem spre tinta din pozitia mainii
        yield return new WaitForFixedUpdate();
        Launch();
        busy = false;
    }

    void PrepareDart()
    {
        var flight = dart.GetComponent<DartFlight>();
        if (flight != null) flight.ResetState();
        dart.isKinematic = false;
    }

    void Launch()
    {
        // punct tintit: centrul tintei + o abatere aleatoare pe fata discului
        Vector3 aim = target.position
                    + target.right * Random.Range(-spread, spread)
                    + target.forward * Random.Range(-spread, spread);

        // viteza necesara ca sa ajunga exact in punct, tinand cont de gravitatie
        Vector3 start = dart.position;
        float time = Mathf.Max(Vector3.Distance(start, aim) / speed, 0.05f);
        Vector3 velocity = (aim - start) / time - 0.5f * Physics.gravity * time;

        dart.rotation = Quaternion.LookRotation(velocity) * Quaternion.Euler(90f, 0f, 0f);
        dart.linearVelocity = velocity;
        dart.angularVelocity = Vector3.zero;
    }
}
