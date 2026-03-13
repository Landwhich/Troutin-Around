using UnityEngine;
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
    public float waterLevel = 0;
    private Transform _lureTarget;

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
        if (_lureTarget != null)
            _desiredDirection = (_lureTarget.position - transform.position).normalized;
        targetRot = Quaternion.LookRotation(_desiredDirection, Vector3.up);
        break;

        case fishState.lured:
            Destroy(this);
            break;
        default:
        break;
        }
        // 3 - set heading for forward transform
        transform.rotation  = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * dt);
        transform.position += transform.forward * speed * dt;
        // 4 - clamp to bounds - prob can remove this
        Vector3 center = sphereCenter ? sphereCenter.position : Vector3.zero;
        Vector3 offset = transform.position - center;

        // sphere 
        if (offset.magnitude > sphereRadius)
        {
            transform.position = center + offset.normalized * sphereRadius;
            _desiredDirection  = Vector3.Reflect(_desiredDirection, offset.normalized);
        }

        // flat water
        if (transform.position.y > waterLevel)
        {
            transform.position    = new Vector3(transform.position.x, waterLevel, transform.position.z);
            _desiredDirection     = Vector3.Reflect(_desiredDirection, Vector3.up);
        }
        UpdateShaderData(dt);
    }

    void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("lure")) return;
        
        float dist = Vector3.Distance(transform.position, other.transform.position);
        if (dist < 0.5f) 
            _state = fishState.lured;
        else {
            _lureTarget = other.transform;
            _state = fishState.attracted;
        }
    }

    Vector3 BoundarySteering()
    {
        Vector3 steering = Vector3.zero;
        Vector3 center   = sphereCenter ? sphereCenter.position : Vector3.zero;

        // Sphere wall
        Vector3 toCenter     = center - transform.position;
        float distFromCenter = toCenter.magnitude;
        float threshold      = sphereRadius * boundarySteerFraction;

        if (distFromCenter > threshold)
        {
            float urgency = (distFromCenter - threshold) / (sphereRadius - threshold);
            steering += toCenter.normalized * (urgency * boundaryStrength);
        }

        // Water ceiling
        float distToSurface   = waterLevel - transform.position.y;
        float surfaceThreshold = sphereRadius * (1f - boundarySteerFraction);

        if (distToSurface < surfaceThreshold && distToSurface > 0)
        {
            float urgency = 1f - (distToSurface / surfaceThreshold);
            steering += Vector3.down * (urgency * boundaryStrength);
        }

        return steering;
    }

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

    // Deprecated
    void UpdateShaderData(float dt)
    {
        if (_renderer == null) return;

        _swimPhase += speed * dt * Mathf.PI;

        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat("_SwimPhase", _swimPhase);
        _mpb.SetFloat("_Speed",     speed);
        _renderer.SetPropertyBlock(_mpb);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(sphereCenter ? sphereCenter.position : Vector3.zero, sphereRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
    }
}