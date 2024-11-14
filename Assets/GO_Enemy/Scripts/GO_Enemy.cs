using UnityEngine;
using UnityEngine.AI;

public abstract class GO_Enemy : MonoBehaviour
{
    [Header("Movement Settings")] public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;

    [Header("Field of View Settings")] public float visionRange = 10.0f;
    [Range(0, 360)] public float visionAngle = 90.0f;
    public Vector3 offset = new Vector3(0f, .5f, 0f);

    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public GO_Enemy_State_Machine stateMachine;
    [HideInInspector] public GO_Controller_Vision visionController;
    [HideInInspector] public GO_Controller_NavMesh navMeshController;

    [Header("Animator Settings")] private Animator _animator;
    private bool _hasAnimator;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDMotionSpeed;
    private int _animIDFreeFall;
    
    // Variables para interpolación suave de animaciones
    private float _animationBlend = 0f;    // Blend actual de animación
    private float _motionSpeed = 0f;       // MotionSpeed actual
    private float _speedChangeRate = 10.0f; // Tasa de cambio para interpolación

    // Velocidad objetivo basada en el estado actual
    private float _currentStateSpeed = 0f;

    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    
    [HideInInspector] public bool hasArm = false;

    protected virtual void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        visionController = GetComponent<GO_Controller_Vision>();
        navMeshController = GetComponent<GO_Controller_NavMesh>();
        stateMachine = GetComponent<GO_Enemy_State_Machine>();

        navMeshAgent.speed = walkSpeed;
    }

    protected virtual void Start()
    {
        _hasAnimator = TryGetComponent(out _animator);
        AssignAnimationIDs();
    }

    protected virtual void Update()
    {
        _animationBlend = Mathf.Lerp(_animationBlend, _currentStateSpeed, Time.deltaTime * _speedChangeRate);
        
        _motionSpeed = _animationBlend > 0.1f ? 1f : 0f;

        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, _motionSpeed);
        }
    }
    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
    }
    
    public virtual void UpdateAnimation(string state)
    {
        switch (state)
        {
            case "Idle":
                _currentStateSpeed = 0f;
                break;
            case "Walk":
                _currentStateSpeed = walkSpeed;
                break;
            case "Run":
                _currentStateSpeed = runSpeed;
                break;
            default:
                _currentStateSpeed = 0f;
                break;
        }
    }
    
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.position, FootstepAudioVolume);
            }
        }
    }

}