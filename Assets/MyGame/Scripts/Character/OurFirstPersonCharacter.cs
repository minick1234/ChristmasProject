using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace EasyCharacterMovement
{
    [RequireComponent(typeof(CharacterLook))]
    public class OurFirstPersonCharacter : Character
    {
        #region EDITOR EXPOSED FIELDS

        [Space(15f)]
        [Tooltip("The first person rig root pivot. This handles the Yaw rotation.")]
        public Transform rootPivot;

        [Tooltip("The first person rig eye pivot. This handles the Pitch rotation.")]
        public Transform eyePivot;

        [Space(15f)]
        [Tooltip("The default eye height (eg: walking).")]
        [SerializeField]
        private float _eyeHeight;

        [Tooltip("The eye height while Character is crouched.")]
        [SerializeField]
        private float _eyeHeightCrouched;

        [Space(15f)]
        [Tooltip("The speed multiplier while Character is walking forward.")]
        [SerializeField]
        private float _forwardSpeedMultiplier;

        [Tooltip("The speed multiplier while Character is walking backward.")]
        [SerializeField]
        private float _backwardSpeedMultiplier;

        [Tooltip("The speed multiplier while Character is walking sideways.")]
        [SerializeField]
        private float _strafeSpeedMultiplier;
        
        [Space(15f)]
        [Header("Interaction Settings")]
        [Tooltip("The length that the character can interact with an object")] 
        [SerializeField] private float _interactionDistance = 3f;
        
        [Tooltip("The layer in which we interact with.")]
        [SerializeField] private LayerMask _interactionLayer;

        [Tooltip("Are interactions currently enabled")]
        [SerializeField] private bool CanInteract = true;
        
        [Space(15f)]
        [Header("Shopping Cart Settings")]
        [Tooltip("The point at which the shopping cart becomes attached")]
        [SerializeField] private GameObject _shoppingCartAttachPoint;

        [Tooltip("Is the shopping cart currently attached to the player?!")]
        [SerializeField] private bool _shoppingCartAttached = false;

        [Tooltip("The current shopping cart in the world thats attached")]
        [SerializeField] private GameObject _ShoppingCartAttached;
        
        
        #endregion

        #region FIELDS

        private CharacterLook _characterLook;
        
        #endregion

        #region PROPERTIES

        /// <summary>
        /// Cached CharacterLook component.
        /// </summary>

        protected CharacterLook characterLook
        {
            get
            {
                if (_characterLook == null)
                    _characterLook = GetComponent<CharacterLook>();

                return _characterLook;
            }
        }

        /// <summary>
        /// Default eye height (in meters).
        /// </summary>

        public float eyeHeight
        {
            get => _eyeHeight;
            set => _eyeHeight = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Default crouched eye height (in meters).
        /// </summary>

        public float eyeHeightCrouched
        {
            get => _eyeHeightCrouched;
            set => _eyeHeightCrouched = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The speed multiplier while Character is walking forward.
        /// </summary>

        public float forwardSpeedMultiplier
        {
            get => _forwardSpeedMultiplier;
            set => _forwardSpeedMultiplier = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The speed multiplier while Character is walking backwards.
        /// </summary>

        public float backwardSpeedMultiplier
        {
            get => _backwardSpeedMultiplier;
            set => _backwardSpeedMultiplier = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The speed multiplier while Character is strafing.
        /// </summary>

        public float strafeSpeedMultiplier
        {
            get => _strafeSpeedMultiplier;
            set => _strafeSpeedMultiplier = Mathf.Max(0.0f, value);
        }

        #endregion

        #region INPUT ACTIONS

        /// <summary>
        /// Mouse Look InputAction.
        /// </summary>

        protected InputAction mouseLookInputAction { get; set; }

        /// <summary>
        /// Controller Look InputAction.
        /// </summary>

        protected InputAction controllerLookInputAction { get; set; }

        /// <summary>
        /// Cursor lock InputAction.
        /// </summary>

        protected InputAction cursorLockInputAction { get; set; }

        /// <summary>
        /// Cursor unlock InputAction.
        /// </summary>

        protected InputAction cursorUnlockInputAction { get; set; }

        protected InputAction interactInputAction { get; set; }
        
        #endregion

        #region INPUT ACTION HANDLERS

        /// <summary>
        /// Gets the mouse look value.
        /// Return its current value or zero if no valid InputAction found.
        /// </summary>
        
        protected virtual Vector2 GetMouseLookInput()
        {
            return mouseLookInputAction?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        /// <summary>
        /// Gets the controller look input value.
        /// Return its current value or zero if no valid InputAction found.
        /// </summary>
        
        protected virtual Vector2 GetControllerLookInput()
        {
            return controllerLookInputAction?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        /// <summary>
        /// Cursor lock input action handler.
        /// </summary>

        protected virtual void OnCursorLock(InputAction.CallbackContext context)
        {
            // Do not allow to lock cursor if using UI

            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
                return;

            if (context.started)
                characterLook.LockCursor();
        }

        /// <summary>
        /// Cursor unlock input action handler.
        /// </summary>

        protected virtual void OnCursorUnlock(InputAction.CallbackContext context)
        {
            if (context.started)
                characterLook.UnlockCursor();
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            if (context.started || context.performed)
            {
                Interact();
            }else if (context.canceled)
            {
                StopInteracting();
            }
        }
        
        #endregion

        #region METHODS        

        /// <summary>
        /// Return the CharacterLook component.
        /// This is guaranteed to be not null.
        /// </summary>

        public virtual CharacterLook GetCharacterLook()
        {
            return characterLook;
        }
        
        /// <summary>
        /// The Eye right vector.
        /// </summary>

        public virtual Vector3 GetEyeRightVector()
        {
            return eyePivot.right;
        }

        /// <summary>
        /// The Eye Up vector.
        /// </summary>

        public virtual Vector3 GetEyeUpVector()
        {
            return eyePivot.up;
        }

        /// <summary>
        /// The Eye forward vector.
        /// </summary>

        public virtual Vector3 GetEyeForwardVector()
        {
            return eyePivot.forward;
        }
        
        /// <summary>
        /// Look up / down. Adds Pitch rotation to eyePivot.
        /// </summary>

        public void AddEyePitchInput(float value)
        {
            if (value == 0.0f)
                return;

            eyePivot.localRotation *= Quaternion.Euler(characterLook.invertLook ? -value : value, 0.0f, 0.0f);

            if (characterLook.clampPitchRotation)
                eyePivot.localRotation = eyePivot.localRotation.clampPitch(characterLook.minPitchAngle, characterLook.maxPitchAngle);
        }

        /// <summary>
        /// The current speed multiplier based on movement direction,
        /// eg: walking forward, walking backwards or strafing side to side.
        /// </summary>

        protected virtual float GetSpeedModifier()
        {
            // Compute planar move direction

            Vector3 characterUp = GetUpVector();
            Vector3 planarMoveDirection = Vector3.ProjectOnPlane(GetMovementDirection(), characterUp);

            // Compute actual walk speed factoring movement direction

            Vector3 characterForward = Vector3.ProjectOnPlane(GetForwardVector(), characterUp).normalized;

            float forwardMovement = Vector3.Dot(planarMoveDirection, characterForward);

            float speedMultiplier = forwardMovement >= 0.0f
                ? Mathf.Lerp(strafeSpeedMultiplier, forwardSpeedMultiplier, forwardMovement)
                : Mathf.Lerp(strafeSpeedMultiplier, backwardSpeedMultiplier, -forwardMovement);

            return speedMultiplier;
        }

        /// <summary>
        /// The maximum speed for current movement mode, factoring walking movement direction.
        /// </summary>

        public override float GetMaxSpeed()
        {
            float actualMaxSpeed = base.GetMaxSpeed();

            return actualMaxSpeed * GetSpeedModifier();
        }
        
        /// <summary>
        /// Initialize player InputActions (if any).
        /// E.g. Subscribe to input action events and enable input actions here.
        /// </summary>

        protected override void InitPlayerInput()
        {
            // Call base method implementation

            base.InitPlayerInput();

            // Attempts to cache this InputActions (if any)

            if (inputActions == null)
                return;

            // Setup input action handlers

            mouseLookInputAction = inputActions.FindAction("Mouse Look");
            mouseLookInputAction?.Enable();

            controllerLookInputAction = inputActions.FindAction("Controller Look");
            controllerLookInputAction?.Enable();

            cursorLockInputAction = inputActions.FindAction("Cursor Lock");
            if (cursorLockInputAction != null)
            {
                cursorLockInputAction.started += OnCursorLock;
                cursorLockInputAction.Enable();
            }

            cursorUnlockInputAction = inputActions.FindAction("Cursor Unlock");
            if (cursorUnlockInputAction != null)
            {
                cursorUnlockInputAction.started += OnCursorUnlock;
                cursorUnlockInputAction.Enable();
            }

            interactInputAction = inputActions.FindAction("Interact");
            if (interactInputAction != null)
            { 
                interactInputAction.performed += OnInteract;
                interactInputAction.canceled += OnInteract;

                interactInputAction?.Enable();
            }

        }

        /// <summary>
        /// Unsubscribe from input action events and disable input actions.
        /// </summary>

        protected override void DeinitPlayerInput()
        {
            // Call base method implementation

            base.DeinitPlayerInput();

            if (mouseLookInputAction != null)
            {
                mouseLookInputAction.Disable();
                mouseLookInputAction = null;
            }

            if (controllerLookInputAction != null)
            {
                controllerLookInputAction.Disable();
                controllerLookInputAction = null;
            }

            if (cursorLockInputAction != null)
            {
                cursorLockInputAction.started -= OnCursorLock;
                
                cursorLockInputAction.Disable();
                cursorLockInputAction = null;
            }
            
            if (cursorUnlockInputAction != null)
            {
                cursorUnlockInputAction.started -= OnCursorUnlock;
                
                cursorUnlockInputAction.Disable();
                cursorUnlockInputAction = null;

            }

            if (interactInputAction != null)
            { 
                interactInputAction.performed += OnInteract;
                interactInputAction.canceled += OnInteract;

                interactInputAction?.Disable();
                interactInputAction = null;
            }
            
        }

        /// <summary>
        /// Extends HandleInput method to add camera look input.
        /// </summary>

        protected override void HandleInput()
        {
            // Handle default movement input

            base.HandleInput();
            
            // Is Character is disabled, or externally controlled (eg: by a controller), halts camera look

            if (inputActions == null)
                return;

            // If disabled, or cursor is unlocked, halts camera input

            if (IsDisabled() || !characterLook.IsCursorLocked())
                return;

            // Mouse Look Input

            Vector2 mouseLookInput = GetMouseLookInput();

            if (mouseLookInput.sqrMagnitude > 0.0f)
            {
                // Perform 'Look' rotation with Mouse

                if (mouseLookInput.x != 0.0f)
                    AddYawInput(mouseLookInput.x * characterLook.mouseHorizontalSensitivity);

                if (mouseLookInput.y != 0.0f)
                    AddEyePitchInput(mouseLookInput.y * characterLook.mouseVerticalSensitivity);
            }
            else
            {
                // Perform 'Look' rotation with Controller, this will perform rotation at (rotationRate) rate

                Vector2 controllerLookInput = GetControllerLookInput();

                if (controllerLookInput.x != 0.0f)
                    AddYawInput(controllerLookInput.x * characterLook.controllerHorizontalSensitivity * rotationRate * Time.deltaTime);

                if (controllerLookInput.y != 0.0f)
                    AddEyePitchInput(controllerLookInput.y * characterLook.controllerVerticalSensitivity * rotationRate * Time.deltaTime);
            }
        }

        /// <summary>
        /// Helper method used to perform camera animation.
        /// Default implementation do basic programatically crouch animation.
        /// </summary>

        protected virtual void AnimateEye()
        {
            // Modify camera's height to simulate crouching state

            float actualEyeHeight = IsCrouching() ? eyeHeightCrouched : eyeHeight;

            eyePivot.localPosition =
                Vector3.MoveTowards(eyePivot.localPosition, new Vector3(0.0f, actualEyeHeight, 0.0f), 6.0f * Time.deltaTime);
        }

        /// <summary>
        /// Set this default values.
        /// If overridden, must call base method in order to fully initialize the class.
        /// </summary>

        protected override void OnReset()
        {
            // Character defaults

            base.OnReset();

            // This defaults

            eyeHeight = 1.6f;
            eyeHeightCrouched = 1.0f;

            forwardSpeedMultiplier = 1.0f;
            backwardSpeedMultiplier = 0.5f;
            strafeSpeedMultiplier = 0.75f;

            SetRotationMode(RotationMode.None);
        }

        /// <summary>
        /// Validate editor exposed fields. 
        /// If overridden, must call base method in order to fully initialize the class.
        /// </summary>

        protected override void OnOnValidate()
        {
            // Validates Character fields

            base.OnOnValidate();

            // Validate this

            eyeHeight = _eyeHeight;
            eyeHeightCrouched = _eyeHeightCrouched;

            forwardSpeedMultiplier = _forwardSpeedMultiplier;
            backwardSpeedMultiplier = _backwardSpeedMultiplier;
            strafeSpeedMultiplier = _strafeSpeedMultiplier;
        }

        /// <summary>
        /// Initialize this.
        /// </summary>

        protected override void OnAwake()
        {
            // Call base method

            base.OnAwake();

            // Disable Character rotation

            SetRotationMode(RotationMode.None);
        }

        /// <summary>
        /// Extends OnLateUpdate to perform programatically camera animation (eg: crouch anim).
        /// </summary>

        protected override void OnLateUpdate()
        {
            // Is Character is disabled, return

            if (IsDisabled())
                return;

            AnimateEye();
        }

        private void Interact()
        {
            RaycastHit interactableHit;
           if (Physics.Raycast(eyePivot.position, eyePivot.forward, out interactableHit, _interactionDistance, _interactionLayer))
            {
                if (interactableHit.transform.gameObject != null)
                {
                    GameObject go = interactableHit.transform.gameObject;
                    go.GetComponent<Interactable>().Interact();
                }
            }
            else if(Physics.Raycast(eyePivot.position, eyePivot.forward, out interactableHit, _interactionDistance, ~_interactionLayer))
            {
                Debug.Log("We have interacted with something not of type interactable");
            }
        }
        
        public void OnShoppingCartInteracted(object sender ,ShoppingCartInteractEventArgs e)
        {
            ShoppingCart shoppingCart = null;
            if (sender is ShoppingCart)
            {
                shoppingCart = (ShoppingCart)sender;
            }

            if (shoppingCart != null)
            {
                shoppingCart.SetNameOfInteractable("Some Random Bullshit");
                Debug.Log("Changed the name of the interactable");
            }
            
            AttachShoppingCart(e.ShoppingCartObject);   
        }
        
        /*
        public void OnShoppingCartInteracted(GameObject random)
        {
            AttachShoppingCart(random);   
        }
        */
        
        private void StopInteracting()
        {
            
        }

        public void AttachShoppingCart(GameObject shoppingCartObject)
        {
            Debug.Log("I am now attached as the cart");
            Debug.Log("The name of the shopping cart object from the event is : " + shoppingCartObject.name);
            shoppingCartObject.gameObject.transform.position = _shoppingCartAttachPoint.transform.position;
            shoppingCartObject.GetComponent<ShoppingCart>().AttachPlayerToShoppingCartJoint();
        }

        public void DetachShoppingCart()
        {
            
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            Debug.DrawRay(eyePivot.position, eyePivot.forward * _interactionDistance, Color.red);
        }
        
        
        #endregion
    }
}
