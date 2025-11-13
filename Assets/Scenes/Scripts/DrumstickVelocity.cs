using UnityEngine;
using UnityEngine.XR;

public class DrumstickVelocity : MonoBehaviour
{
    [Tooltip("Set which hand this tip belongs to (used for haptics).")]
    public XRNode xrNode = XRNode.RightHand; // set in inspector

    Vector3 lastPos;
    public Vector3 Velocity { get; private set; }

    void Start()
    {
        lastPos = transform.position;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        if (dt > 0f)
        {
            Vector3 newPos = transform.position;
            Velocity = (newPos - lastPos) / dt;
            lastPos = newPos;
        }
    }

    // optional helper to get magnitude quickly
    public float GetSpeed()
    {
        return Velocity.magnitude;
    }

    public XRNode GetXRNode()
    {
        return xrNode;
    }
}