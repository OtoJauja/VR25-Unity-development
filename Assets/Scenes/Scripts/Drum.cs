using System.Collections;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class Drum : MonoBehaviour
{
    [Header("Velocity -> Sound settings")]
    public float minVelocity = 0.5f;   // minimum speed to count as a hit
    public float maxVelocity = 6.0f;   // speed corresponding to full volume
    public float volumeMultiplier = 1.0f; // volume multiplier
    public float pitchRandomness = 0.05f; // small pitch variance

    [Header("Hit cooldown (ms)")]
    public int hitCooldownMs = 60;

    [Header("Visual feedback")]
    public float hitScale = 0.92f;
    public float feedbackDuration = 0.08f;

    [Header("Optional haptics")]
    [Range(0f, 1f)] public float maxHapticAmplitude = 0.8f;
    public float hapticDuration = 0.03f;

    AudioSource audioSource;
    float lastHitTime = -9999f;

    Vector3 originalScale;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        originalScale = transform.localScale;
    }

    void OnTriggerEnter(Collider other)
    {
        // Try to get the velocity component from the tip
        DrumstickVelocity tip = other.GetComponent<DrumstickVelocity>();
        if (tip == null) return;

        float now = Time.time * 1000f; // milliseconds
        if (now - lastHitTime < hitCooldownMs) return;

        float speed = tip.GetSpeed();

        if (speed < minVelocity) return;

        lastHitTime = now;

        // Map speed to volume (0-1)
        float t = Mathf.InverseLerp(minVelocity, maxVelocity, speed);
        float vol = Mathf.Clamp01(t) * volumeMultiplier;

        // Slight pitch randomization to avoid monotony
        audioSource.pitch = 1.0f + Random.Range(-pitchRandomness, pitchRandomness);

        // Play sound, PlayOneShot so multiple quick hits sound ok
        audioSource.PlayOneShot(audioSource.clip, vol);

        // Visual feedback
        StopAllCoroutines();
        StartCoroutine(HitFeedback());

        // Haptics
        StartCoroutine(SendHaptic(t, tip.GetXRNode()));
    }

    IEnumerator HitFeedback()
    {
        float elapsed = 0f;
        float dur = feedbackDuration;
        Vector3 target = originalScale * hitScale;

        // quick shrink then back
        transform.localScale = target;
        yield return new WaitForSeconds(dur);
        transform.localScale = originalScale;
    }

    IEnumerator SendHaptic(float normalizedStrength, XRNode node)
    {
        // normalizedStrength in 0..1
        if (normalizedStrength <= 0f) yield break;

        var device = InputDevices.GetDeviceAtXRNode(node);
        if (!device.isValid) yield break;

        // clamp amplitude
        float amp = Mathf.Clamp01(normalizedStrength) * maxHapticAmplitude;

        HapticCapabilities caps;
        if (device.TryGetHapticCapabilities(out caps) && caps.supportsImpulse)
        {
            uint channel = 0;
            device.SendHapticImpulse(channel, amp, hapticDuration);
        }
        yield break;
    }
}