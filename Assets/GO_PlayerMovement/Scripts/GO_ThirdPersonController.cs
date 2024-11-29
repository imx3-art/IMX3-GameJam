using System;
using UnityEngine;
using Cinemachine;
using Random = UnityEngine.Random;
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
        
        // Variables para manejar la reducción de velocidad
        public bool IsSpeedReduced { get; private set; } = false;
        private float originalMoveSpeed;
        
        public float Stamina = 100f;

        public float MaxStamina = 100f;
        public float MinStamina = 0f;
        public float StaminaDecreaseRate = 20f; 
        public float StaminaIncreaseRate = 10f; 
        
        private float staminaTimer = 0f;

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
        private bool isGrabbing = false;
        private Rigidbody grabbedObjectRb;  // Rigidbody del objeto agarrado
        private FixedJoint joint;

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
        
        // Referencia al NetworkManager
        //private GO_PlayerNetworkManager networkManager;
        
        private float _weight = 0;
        
        public event Action<float> OnStaminaChanged;
        
        [Header("Camera Distance Settings")]
        [SerializeField] private float cameraVisionDistance = 60f; // Distancia cuando se oprime espacio
        [SerializeField] private float normalCameraDistance = 40f;
        public float persecutionCameraDistance = 50f;// Distancia normal de la cámara
        [SerializeField] private float cameraTransitionSpeed = 5f;   // Velocidad de transición de la cámara

        [Header("Camera Rotation Settings")]
        [SerializeField] private float rotationSpeed = 100f; // Velocidad de rotación de la cámara
        private float yaw = 0f; // Rotación acumulada en Y
        private float initialYOffset;

        private void Awake()
        {
            //_mainCamera = Camera.main.gameObject; ALBERT
            _mainCamera = GO_MainCamera.MainCamera.gameObject;
            _framingTransposer = _virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            
            originalMoveSpeed = SprintSpeed;
        }

        private void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<GO_InputsPlayer>();
            //networkManager = GetComponentInParent<GO_PlayerNetworkManager>();

            AssignAnimationIDs();
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            FalloutGravity();
            GroundedCheck();
            Move();
            AdjustCameraFraming();
            HandleStaminaAndSpeed();

            if (_input.grabDropItem)
            {
                _animator.SetBool("Grab", true);
            }

            if (_input.Grab)
            {
                HandleGrab();
            }
            else
            {
                releaseObject();
            }
            
            if (GO_PlayerNetworkManager.localPlayer.CurrentPlayerState != PlayerState.Persecution)
            {
                if (_input.cameraVision)
                {
                    GO_InputsPlayer.IsPause = true;
                    HandleCameraDistance(cameraVisionDistance);
                    HandleCameraRotation();
                }
                else
                {
                    GO_InputsPlayer.IsPause = false;
                    HandleCameraDistance(normalCameraDistance);
                }
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
            _weight = Mathf.Lerp(_weight, _input.stealth ? 1 : 0, Time.deltaTime * 5);
            _animator.SetLayerWeight(1, _weight);
            _animator.SetLayerWeight(0, 1 - _weight);
        
             
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
                    //AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                    GO_AudioManager.Instance.PlayGameSoundDynamic(FootstepAudioClips[index], transform.position);
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

        private void HandleGrab()
        {
            if (_input.Grab && !isGrabbing)
            {
                Vector3 rayOrigin = transform.position + Vector3.up * 0.3f; // Eleva la posición en Y
                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, transform.forward, out hit, grabDistance))
                {
                    Debug.Log("Objeto detectado: " + hit.collider.name); // Mensaje de debug

                    if (hit.collider.CompareTag("GrabbableObject") && hit.collider.attachedRigidbody != null)
                    {
                        hit.collider.GetComponent<GO_NetworkObject>().ChangeAuthority();
                        grabbedObjectRb = hit.collider.attachedRigidbody;
                        //grabbedObjectRb.isKinematic = false;
                        grabbedObjectRb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;                            

                        isGrabbing = true;

                        joint = grabbedObjectRb.gameObject.AddComponent<FixedJoint>();
                        joint.connectedBody = GetComponent<Rigidbody>();

                        // Configurar los anclajes
                        Vector3 objectMidPoint = GetMidPoint(grabbedObjectRb.transform);
                        Vector3 playerMidPoint = GetMidPoint(this.transform);

                        // Configurar los anclajes
                        joint.anchor = grabbedObjectRb.transform.InverseTransformPoint(objectMidPoint);
                        joint.connectedAnchor = transform.InverseTransformPoint(playerMidPoint);

                        Debug.Log("Agarrando objeto: " + grabbedObjectRb.name);

                        // Ajustar parámetros adicionales del joint
                        joint.breakForce = 100f; // Ajusta según necesidad
                        joint.breakTorque = 100f; // Ajusta según necesidad
                        joint.enableCollision = true; // Permitir colisiones entre los cuerpos conectados
                                                      // Obtener los límites del objeto si existen

                        joint.massScale = 10; //Ajusta la tension
                        _grabLimits = hit.collider.GetComponent<GO_GrabLimits>();

                    }
                }
            }
            else if (isGrabbing && grabbedObjectRb != null)
            {
                // Validar la posición del objeto agarrado
                if (_grabLimits != null)
                {
                    Vector3 clampedPosition = _grabLimits.ClampToLimits(grabbedObjectRb.position);

                    // Aplicar el movimiento con física para mantener el objeto dentro de los límites
                    grabbedObjectRb.MovePosition(clampedPosition);
                }

                // Soltar el objeto si se suelta el botón de agarrar
                if (!_input.Grab)
                {
                    releaseObject();
                }
            }

        }

        private void releaseObject()
        {
            if (isGrabbing)
            {
                isGrabbing = false;
                if (grabbedObjectRb != null)
                {
                    //grabbedObjectRb.isKinematic = true;
                    grabbedObjectRb.constraints = RigidbodyConstraints.FreezeAll;
                    if (joint != null)
                    {
                        Destroy(joint);
                    }
                    _grabbedObject = null;
                    Debug.Log("Soltando objeto");
                }
            }
        }

        Vector3 GetMidPoint(Transform obj)
        {
            // Retorna el punto medio del objeto usando su posición y tamaño
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds.center;
            }

            // Si no hay Renderer, retorna directamente la posición del objeto
            return obj.position;
        }
        
        private void HandleStaminaAndSpeed()
        {
            //Debug.Log("handle stamina");
            if (GO_PlayerNetworkManager.localPlayer == null)
                return;

            switch (GO_PlayerNetworkManager.localPlayer.CurrentPlayerState)
            {
                case PlayerState.Normal:
                    RegenerateStamina();
                    break;

                case PlayerState.Persecution:
                    ConsumeStamina();
                    break;

                case PlayerState.Duel:
                    RestoreSpeed();
                    break;
            }
        }
        
        private void ConsumeStamina()
        {
            if (_input.sprint && Stamina > MinStamina)
            {
                staminaTimer += Time.deltaTime;

                if (staminaTimer >= 1f)
                {
                    staminaTimer -= 1f; 
                    Stamina -= StaminaDecreaseRate;
                    Stamina = Mathf.Max(Stamina, MinStamina); 
                    OnStaminaChanged?.Invoke(Stamina);
                    Debug.Log("Consumiendo stamina: " + Stamina);

                 
                    if (Stamina <= MinStamina)
                    {
                        ReduceSpeed(0.2f); 
                    }
                }
            }
        }

        public void RegenerateAllStamina()
        {
            Stamina = MaxStamina;
            RestoreSpeed();
            OnStaminaChanged?.Invoke(Stamina);
        }
        private void RegenerateStamina()
        {
            if (Stamina < MaxStamina)
            {
                staminaTimer += Time.deltaTime;
                if (staminaTimer >= 1f)
                {
                    staminaTimer = 0f;
                    Stamina += StaminaIncreaseRate;
                    Stamina = Mathf.Min(Stamina, MaxStamina);
                    OnStaminaChanged?.Invoke(Stamina);
                    Debug.Log("Regenerando stamina"+Stamina);
                    if (IsSpeedReduced && Stamina > MinStamina)
                    {
                        RestoreSpeed(); // Restaurar la velocidad al 100%
                    }
                }
            }
            else
            {
                // Si está al máximo, resetear el temporizador
                staminaTimer = 0f;
            }
        }

        // Métodos para manejar la reducción y restauración de velocidad
        public void ReduceSpeed(float multiplier)
        {
            if (!IsSpeedReduced)
            {
                Debug.Log("movespeed"+SprintSpeed);
                SprintSpeed *= multiplier;
                IsSpeedReduced = true;
            }
        }

        private void StopGrabItem()
        {
            Debug.Log("LLAMANDO LA FUNCION");
            _animator.SetBool("Grab", false);        
        }
        public void RestoreSpeed()
        {
            if (IsSpeedReduced)
            {
                SprintSpeed = originalMoveSpeed;
                IsSpeedReduced = false;
            }
        }
        
        public void HandleCameraDistance(float targetDistance)
        {
            _framingTransposer.m_CameraDistance = targetDistance;
        }

        private void HandleCameraRotation()
        {
            float mouseX = _input.look.x;

            yaw += mouseX * rotationSpeed * Time.deltaTime;

            if (_virtualCamera != null)
            {
                Vector3 currentRotation = _virtualCamera.transform.eulerAngles;
                _virtualCamera.transform.eulerAngles = new Vector3(currentRotation.x, yaw, currentRotation.z);
            }
        }

        
    }
}