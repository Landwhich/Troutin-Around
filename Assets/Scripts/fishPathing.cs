using UnityEngine;

/// <summary>
/// Controls a single fish moving in straight lines inside a bounding sphere.
/// Fish steer away from each other and reflect off the sphere boundary.
/// 
/// VERTEX SHADER NOTE:
/// This script drives transform.position and transform.rotation only.
/// Your vertex shader can read per-instance data via MaterialPropertyBlock:
///   - "_SwimPhase"  (float) — advances each frame, drives fin/body oscillation
///   - "_Speed"      (float) — current speed, can scale deformation amplitude
/// Set these each frame via mpb.SetFloat() after position updates.
/// Each fish should have its own MaterialPropertyBlock to avoid batching conflicts.
/// </summary>
public class FishController : MonoBehaviour
{
    public Transform sphereCenter;
    public float sphereRadius = 10f;

    public float speed = 5f;
    public float turnSpeed = 90f;

    public float avoidanceRadius = 3f;
    public float avoidanceStrength = 20f;

    public float boundarySteerFraction = 0.65f;
    public float boundaryStrength = 30f;

    // Shader data 
    private MaterialPropertyBlock _mpb;
    private Renderer _renderer;
    private float _swimPhase;

    // state
    private Vector3 _desiredDirection;
    private static FishController[] _allFish;

    enum fishState{ swimming, attracted, lured }
    private fishState _state = fishState.swimming;

    void Awake()
    {
        _mpb      = new MaterialPropertyBlock();
        _renderer = GetComponentInChildren<Renderer>();

        // Give every fish a random start direction and phase offset
        _desiredDirection = Random.onUnitSphere;
        _swimPhase        = Random.Range(0f, Mathf.PI * 2f);
    }

    void OnEnable()
    {
        _allFish = FindObjectsByType<FishController>(FindObjectsSortMode.None);
    }

    // ─────────────────────────────────────────────────────────────────
    void Update()
    {
        // 1 - steering, heading and time
        float dt = Time.deltaTime;
        Vector3 steering = Vector3.zero;
        Quaternion targetRot = Quaternion.identity;
        switch (_state)
        {

        case fishState.swimming:
        steering += BoundarySteering();
        steering += AvoidanceSteering();
        // 2 - Apply steering to desired direction accounting for natural movement
        if (steering.sqrMagnitude > 0.001f)
            _desiredDirection = (_desiredDirection + steering * dt).normalized;
        targetRot = Quaternion.LookRotation(_desiredDirection, Vector3.up);
        break;

        case fishState.attracted:
        // 2 - Oncollision triggers a desired direction change
        targetRot = Quaternion.LookRotation(_desiredDirection, Vector3.up);
        break;

        case fishState.lured:
        //pass for now
        default:
        break;
        }
        // 3 - set heading for forward transform
        transform.rotation   = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * dt);
        transform.position += transform.forward * speed * dt;
        // 4 - clamp to bounds
        // Vector3 center = sphereCenter ? sphereCenter.position : Vector3.zero;
        // Vector3 offset = transform.position - center;
        // if (offset.magnitude > sphereRadius)
        // {
        //     transform.position    = center + offset.normalized * sphereRadius;
        //     _desiredDirection     = Vector3.Reflect(_desiredDirection, offset.normalized);
        // }
        // 5 - update shader
        UpdateShaderData(dt);
    }

    void OnCollisionEnter(Collision collision){
        Debug.Log("lure");
        if (collision.gameObject.CompareTag("lure")) {
            Debug.Log("lure");
            _desiredDirection = collision.gameObject.transform.position;
            _state = fishState.attracted;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    /// <summary>
    /// Steer back toward centre when approaching the sphere wall.
    /// Returns an inward force vector (not normalised – magnitude encodes urgency).
    /// </summary>
    Vector3 BoundarySteering()
    {
        Vector3 center      = sphereCenter ? sphereCenter.position : Vector3.zero;
        Vector3 toCenter    = center - transform.position;
        float   distFromCenter = toCenter.magnitude;
        float   threshold   = sphereRadius * boundarySteerFraction;

        if (distFromCenter < threshold) return Vector3.zero;

        // Linearly ramp up force as fish approaches wall
        float urgency = (distFromCenter - threshold) / (sphereRadius - threshold);
        return toCenter.normalized * (urgency * boundaryStrength);
    }

    // ─────────────────────────────────────────────────────────────────
    /// <summary>
    /// Steer away from nearby fish.
    /// Uses a simple repulsion sum — no alignment, no cohesion (not boids).
    /// </summary>
    Vector3 AvoidanceSteering()
    {
        if (_allFish == null) return Vector3.zero;

        Vector3 repulsion    = Vector3.zero;
        float   radiusSq     = avoidanceRadius * avoidanceRadius;

        foreach (FishController other in _allFish)
        {
            if (other == this || other == null) continue;

            Vector3 diff   = transform.position - other.transform.position;
            float   distSq = diff.sqrMagnitude;

            if (distSq < radiusSq && distSq > 0.0001f)
            {
                // Stronger repulsion the closer they are
                float weight = 1f - (distSq / radiusSq);
                repulsion   += diff.normalized * weight;
            }
        }

        return repulsion * avoidanceStrength;
    }

    // ─────────────────────────────────────────────────────────────────
    /// <summary>
    /// Push per-instance floats to the GPU via MaterialPropertyBlock.
    /// Your vertex shader can read _SwimPhase and _Speed to drive
    /// body/tail deformation without touching the C# side.
    /// </summary>
    void UpdateShaderData(float dt)
    {
        if (_renderer == null) return;

        _swimPhase += speed * dt * Mathf.PI; // phase advances proportional to speed

        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat("_SwimPhase", _swimPhase);
        _mpb.SetFloat("_Speed",     speed);
        _renderer.SetPropertyBlock(_mpb);
    }

    // ─────────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(sphereCenter ? sphereCenter.position : Vector3.zero, sphereRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
    }
}