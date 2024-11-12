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
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

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

        private Transform _grabbedObject;
        private GO_GrabLimits _grabLimits;

        public float grabDistance = 2.0f; // Distancia máxima para agarrar objetos
        public LayerMask grabbableLayer;  // Capa para objetos que pueden ser agarrados

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
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDMotionSpeed;
        private int _animIDFreeFall;

        // timeout deltatime
        private float _fallTimeoutDelta;

        private void Awake()
        {
            //_mainCamera = Camera.main.gameObject; ALBERT
            _mainCamera = GO_MainCamera.MainCamera.gameObject;
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

            FalloutGravity();
            GroundedCheck();
            Move();
            AdjustCameraFraming();
            // Verifica si el jugador está presionando una tecla para agarrar un objeto
            if (_input.Grab) // Suponiendo que has configurado la acción de 'grabar' en el sistema de entrada
            {
                TryGrabObject();
            }else
            {
                ReleaseObject();
            }
            // Si el jugador está agarrando un objeto, aplica las restricciones
            if (_grabbedObject != null)
            {
                MoveGrabbedObjectWithLimits();
            }
        }

        private void LateUpdate()
        {

        }

        public void CenterCameraOnPlayer()
        {
            if (_virtualCamera != null)
            {
                var framingTransposer = _virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                if (framingTransposer != null)
                {
                    framingTransposer.m_ScreenX = 0.5f;
                    framingTransposer.m_ScreenY = 0.5f;
                }

                // Informar a Cinemachine del teletransporte
                Vector3 positionDelta = transform.position - _virtualCamera.transform.position;
                _virtualCamera.OnTargetObjectWarped(transform, positionDelta);
            }
        }


        private void Move()
        {
            float targetSpeed = _input.stealth ? StealthSpeed : _input.sprint ? (GO_PlayerNetworkManager.localPlayer.isDrag == 0 ? SprintSpeed : MoveSpeed) : MoveSpeed;
                        

                // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

                // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
                // if there is no input, set the target speed to 0
                if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  //_mainCamera.transform.eulerAngles.y;ALBERT
                                  _virtualCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                if (GO_PlayerNetworkManager.localPlayer.isDrag == 0)
                {
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void FalloutGravity()
        {
            if (Grounded)
            {
                // Reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // Update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // Stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f; // Slight downward force to keep grounded
                }
            }
            else
            {
                // Fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // Update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }
            }

            // Apply gravity over time if under terminal velocity
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
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
            _animIDFreeFall = Animator.StringToHash("FreeFall");
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

        private void TryGrabObject()
        {
            Transform objectTransform = transform;

            Vector3 rayOrigin = new Vector3(objectTransform.position.x, objectTransform.position.y +1f, objectTransform.position.z);

            Ray ray = new Ray(rayOrigin, transform.forward); // Usamos la posición de la cámara y su dirección hacia adelante
            RaycastHit hit;

            // Hacer visible el raycast en el editor de Unity
            Debug.DrawRay(ray.origin, ray.direction * grabDistance, Color.red, 0, true);

            // Verifica si el Raycast detecta algún objeto dentro de la distancia de agarre
            if (Physics.Raycast(ray, out hit, grabDistance, grabbableLayer))
            {
                Transform objectToGrab = hit.transform;
                GO_GrabLimits grabProperties = objectToGrab.GetComponent<GO_GrabLimits>();

                // Verifica si el objeto tiene la propiedad grabable configurada
                if (grabProperties != null)
                {
                    Debug.Log($"Objeto detectado para agarrar: {objectToGrab.name}");
                    GrabObject(objectToGrab);
                }
                else
                {
                    Debug.LogWarning("El objeto no tiene límites configurados en GrabProperties.");
                }
            }
            else
            {
                Debug.Log("No se detectó ningún objeto en la distancia de agarre.");
            }
        }

        private void MoveGrabbedObjectWithLimits()
        {
            if (_grabbedObject == null || _grabLimits == null)
            {
                Debug.LogWarning("No hay objeto agarrado o no tiene límites configurados.");
                return;
            }

            Vector3 currentPosition = _grabbedObject.position;

            float moveX = Input.GetAxis("Horizontal") * Time.deltaTime;
            float moveY = Input.GetAxis("Vertical") * Time.deltaTime;

            Vector3 targetPosition = currentPosition;
            // Actualiza la posición en función del movimiento del jugador
            targetPosition.x += _input.move.x;
            targetPosition.z += _input.move.y;

            // Aplica los límites en X y Z
            targetPosition.x = Mathf.Clamp(targetPosition.x, _grabLimits.minX, _grabLimits.maxX);
            targetPosition.z = Mathf.Clamp(targetPosition.z, _grabLimits.MinZ, _grabLimits.MaxZ);

            // Actualiza la posición del objeto
            _grabbedObject.position = Vector3.Lerp(currentPosition, targetPosition, 0.045f);

            Debug.Log($"Objeto {_grabbedObject.name} movido a la posición {_grabbedObject.position}");
        }

        private void GrabObject(Transform objectToGrab)
        {
            _grabbedObject = objectToGrab;
            _grabLimits = objectToGrab.GetComponent<GO_GrabLimits>();

            if (_grabLimits == null)
            {
                Debug.LogWarning("El objeto no tiene límites configurados en GrabProperties.");
            }
            else
            {
                Debug.Log($"Objeto {objectToGrab.name} agarrado con límites configurados.");
            }
        }

        private void ReleaseObject()
        {
            if (_grabbedObject != null)
            {
                Debug.Log($"Objeto {_grabbedObject.name} soltado.");
            }
            _grabbedObject = null;
            _grabLimits = null;
        }
    }
}
