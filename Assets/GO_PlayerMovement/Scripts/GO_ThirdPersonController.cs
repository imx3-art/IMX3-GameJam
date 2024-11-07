using UnityEngine;
using Cinemachine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class GO_ThirdPersonController : MonoBehaviour
    {
        [Header("Player Movementa")]
        [Tooltip("Velocidad de sigilo del personaje en m/s")]
        public float StealthSpeed = 1.0f;
        
        [Tooltip("Move speed of the character in m/s")]
        public new float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public new float SprintSpeed = 5.335f;

        [Tooltip("Acceleration and deceleration")]
        public new float SpeedChangeRate = 10.0f;
        
        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;
        
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;
        
        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;
        
        [Header("Camera Movement")]
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        private CinemachineFramingTransposer _framingTransposer;
    
        [Header("Camera Settings")]
        [Tooltip("Desplazamiento máximo desde el centro en el eje X")]
        [SerializeField] private float maxOffsetX = 0.2f;

        [Tooltip("Desplazamiento máximo desde el centro en el eje Y")]
        [SerializeField] private float maxOffsetY = 0.2f;

        [Tooltip("Tiempo de suavizado para el movimiento de la cámara")]
        [SerializeField] private float smoothTime = 0.2f;

        private CharacterController _controller;
        private GO_InputsPlayer _input;
        private GameObject _mainCamera;
        
        private float _screenXVelocity = 0f;
        private float _screenYVelocity = 0f;
        
        private Animator _animator;
        private bool _hasAnimator;
        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        
        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDMotionSpeed;

        private void Awake()
        {
            _mainCamera = Camera.main.gameObject;
            _framingTransposer = _virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }

        private void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<GO_InputsPlayer>();

            AssignAnimationIDs();
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            GroundedCheck();
            Move();
            AdjustCameraFraming();
        }
        
        private void LateUpdate()
        {
            
        }

        private void Move()
        {
            float targetSpeed = _input.stealth ? StealthSpeed : _input.sprint ? SprintSpeed : MoveSpeed;

            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime));

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void AdjustCameraFraming()
        {
            float moveDirectionX = _input.move.x;
            float moveDirectionY = _input.move.y;

            float targetScreenX = 0.5f - moveDirectionX * maxOffsetX;
            float targetScreenY = 0.5f + moveDirectionY * maxOffsetY;

            targetScreenX = Mathf.Clamp(targetScreenX, 0.3f, 0.7f);
            targetScreenY = Mathf.Clamp(targetScreenY, 0.3f, 0.7f);

            _framingTransposer.m_ScreenX = Mathf.SmoothDamp(_framingTransposer.m_ScreenX, targetScreenX, ref _screenXVelocity, smoothTime);
            _framingTransposer.m_ScreenY = Mathf.SmoothDamp(_framingTransposer.m_ScreenY, targetScreenY, ref _screenYVelocity, smoothTime);
        }
        
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }
        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }
        
        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}
