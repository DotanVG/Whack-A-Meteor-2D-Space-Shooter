using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Centralized input manager that abstracts input queries and supports both keyboard/mouse and gamepad.
/// Works with all gamepad types including generic controllers.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Controller Settings")]
    public float gamepadDeadzone = 0.2f;
    public float hammerCursorSensitivity = 500f;
    public bool snapHammerToCenter = false;

    // Hammer cursor position tracking (for gamepad)
    private Vector2 hammerScreenPosition;
    private Vector2 hammerVelocity;
    private bool isUsingGamepad = false;
    private float lastInputTime = 0f;
    private const float INPUT_TIMEOUT = 0.1f; // Time before considering input inactive

    private Camera mainCamera;
    private Gamepad currentGamepad; // Track the active gamepad
    private float lastRightTriggerValue = 0f; // Track previous frame's trigger value for press detection

    /// <summary>
    /// Auto-creates InputManager if it doesn't exist
    /// </summary>
    public static InputManager GetOrCreateInstance()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("InputManager");
            Instance = go.AddComponent<InputManager>();
            DontDestroyOnLoad(go);
        }
        return Instance;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        hammerScreenPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        FindActiveGamepad();
    }

    void OnEnable()
    {
        // Listen for device changes
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        // When a device is added or removed, update gamepad reference
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed)
        {
            FindActiveGamepad();
        }
    }

    void Update()
    {
        // Update active gamepad reference
        FindActiveGamepad();

        // Detect active input device
        DetectActiveInputDevice();

        // Update hammer cursor position for gamepad (right stick controls hammer like mouse)
        // Always update if gamepad is connected to allow seamless switching
        if (currentGamepad != null)
        {
            UpdateHammerCursorPosition();
        }
    }

    /// <summary>
    /// Finds any connected gamepad (works with generic controllers, not just Xbox)
    /// </summary>
    private void FindActiveGamepad()
    {
        Gamepad previousGamepad = currentGamepad;
        
        // Try Gamepad.current first (Xbox controllers)
        if (Gamepad.current != null)
        {
            currentGamepad = Gamepad.current;
            if (previousGamepad != currentGamepad)
            {
                Debug.Log($"Gamepad detected: {currentGamepad.name} (Gamepad.current)");
            }
            return;
        }

        // Check all input devices for gamepads (for generic controllers like Speedlink)
        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad gamepad)
            {
                currentGamepad = gamepad;
                if (previousGamepad != currentGamepad)
                {
                    Debug.Log($"Gamepad detected: {currentGamepad.name} (device scan)");
                }
                return;
            }
        }

        if (previousGamepad != null && previousGamepad != null)
        {
            Debug.Log("Gamepad disconnected");
        }
        currentGamepad = null;
    }

    private void DetectActiveInputDevice()
    {
        // Check for keyboard activity (for movement, shooting, etc.)
        bool keyboardActive = Keyboard.current != null && (
            Keyboard.current.wKey.isPressed ||
            Keyboard.current.aKey.isPressed ||
            Keyboard.current.sKey.isPressed ||
            Keyboard.current.dKey.isPressed ||
            Keyboard.current.spaceKey.isPressed ||
            Keyboard.current.leftShiftKey.isPressed ||
            Keyboard.current.enterKey.isPressed ||
            Keyboard.current.escapeKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed
        );

        // Check for gamepad activity (excluding right stick which is handled in GetHammerPosition)
        bool gamepadActive = false;
        if (currentGamepad != null)
        {
            try
            {
                gamepadActive = (
                    currentGamepad.leftStick.ReadValue().magnitude > gamepadDeadzone ||
                    currentGamepad.buttonSouth.isPressed ||
                    currentGamepad.buttonWest.isPressed ||
                    currentGamepad.buttonEast.isPressed ||
                    currentGamepad.buttonNorth.isPressed ||
                    currentGamepad.rightShoulder.isPressed ||
                    currentGamepad.leftShoulder.isPressed ||
                    currentGamepad.rightTrigger.isPressed ||
                    currentGamepad.leftTrigger.isPressed ||
                    currentGamepad.leftStickButton.isPressed ||
                    currentGamepad.startButton.isPressed ||
                    currentGamepad.dpad.ReadValue().magnitude > 0.1f
                );
            }
            catch
            {
                // If reading fails, try to find a new gamepad
                FindActiveGamepad();
            }
        }

        // Update device status based on keyboard/gamepad activity
        // Note: Mouse activity is handled in GetHammerPosition() for unified cursor control
        bool previousGamepadStatus = isUsingGamepad;
        if (keyboardActive)
        {
            isUsingGamepad = false;
            lastInputTime = Time.time;
        }
        else if (gamepadActive)
        {
            isUsingGamepad = true;
            lastInputTime = Time.time;
        }
        
        // Debug: Log device status changes
        if (previousGamepadStatus != isUsingGamepad)
        {
            Debug.Log($"Input device changed to: {(isUsingGamepad ? "Gamepad" : "Keyboard/Mouse")}");
        }
    }

    private void UpdateHammerCursorPosition()
    {
        if (currentGamepad == null) return;

        try
        {
            Vector2 rightStick = currentGamepad.rightStick.ReadValue();
            
            // Apply deadzone
            if (rightStick.magnitude < gamepadDeadzone)
            {
                rightStick = Vector2.zero;
                if (snapHammerToCenter)
                {
                    hammerScreenPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
                }
            }
            else
            {
                // Normalize after deadzone
                float magnitude = rightStick.magnitude;
                float normalizedMagnitude = (magnitude - gamepadDeadzone) / (1f - gamepadDeadzone);
                rightStick = rightStick.normalized * normalizedMagnitude;

                // Update position based on stick input
                hammerVelocity = rightStick * hammerCursorSensitivity * Time.deltaTime;
                hammerScreenPosition += hammerVelocity;

                // Clamp to screen bounds
                hammerScreenPosition.x = Mathf.Clamp(hammerScreenPosition.x, 0f, Screen.width);
                hammerScreenPosition.y = Mathf.Clamp(hammerScreenPosition.y, 0f, Screen.height);
                
                // Debug: Log when right stick is actively moving cursor
                Debug.Log($"Right stick moving cursor to: ({hammerScreenPosition.x:F1}, {hammerScreenPosition.y:F1})");
            }
        }
        catch
        {
            // If reading fails, try to find a new gamepad
            FindActiveGamepad();
        }
    }

    // Gameplay Input Queries
    public float GetMoveForward()
    {
        float value = 0f;

        // Check keyboard first (WASD)
        if (Keyboard.current != null && Keyboard.current.wKey.isPressed)
        {
            value = 1f;
        }

        // Check gamepad: Mario Kart style - Forward (D-pad up) OR A button (buttonSouth) OR Left stick forward
        if (currentGamepad != null && (isUsingGamepad || value == 0f))
        {
            try
            {
                // Left stick forward
                float stickY = currentGamepad.leftStick.y.ReadValue();
                if (stickY > gamepadDeadzone)
                {
                    value = Mathf.Max(value, stickY);
                }

                // D-pad up (Forward)
                if (currentGamepad.dpad.up.isPressed)
                {
                    value = Mathf.Max(value, 1f);
                }

                // A button (buttonSouth) - Mario Kart style acceleration
                if (currentGamepad.buttonSouth.isPressed)
                {
                    value = Mathf.Max(value, 1f);
                }
            }
            catch { FindActiveGamepad(); }
        }

        return value;
    }

    public float GetMoveBackward()
    {
        float value = 0f;

        // Check keyboard first (WASD)
        if (Keyboard.current != null && Keyboard.current.sKey.isPressed)
        {
            value = 1f;
        }

        // Check gamepad - only left stick backward (LT is for shooting, not movement)
        if (currentGamepad != null && (isUsingGamepad || value == 0f))
        {
            try
            {
                float stickY = currentGamepad.leftStick.y.ReadValue();
                if (stickY < -gamepadDeadzone)
                {
                    value = Mathf.Max(value, -stickY);
                }
            }
            catch { FindActiveGamepad(); }
        }

        return value;
    }

    public float GetRotateLeft()
    {
        float value = 0f;

        // Check keyboard first (WASD)
        if (Keyboard.current != null && Keyboard.current.aKey.isPressed)
        {
            value = 1f;
        }

        // Check gamepad (only if keyboard not pressed, or if gamepad is actively being used)
        if (currentGamepad != null && (isUsingGamepad || value == 0f))
        {
            try
            {
                float stickX = currentGamepad.leftStick.x.ReadValue();
                if (stickX < -gamepadDeadzone)
                {
                    value = Mathf.Max(value, -stickX);
                }

                if (currentGamepad.dpad.left.isPressed)
                {
                    value = Mathf.Max(value, 1f);
                }
            }
            catch { FindActiveGamepad(); }
        }

        return value;
    }

    public float GetRotateRight()
    {
        float value = 0f;

        // Check keyboard first (WASD)
        if (Keyboard.current != null && Keyboard.current.dKey.isPressed)
        {
            value = 1f;
        }

        // Check gamepad (only if keyboard not pressed, or if gamepad is actively being used)
        if (currentGamepad != null && (isUsingGamepad || value == 0f))
        {
            try
            {
                float stickX = currentGamepad.leftStick.x.ReadValue();
                if (stickX > gamepadDeadzone)
                {
                    value = Mathf.Max(value, stickX);
                }

                if (currentGamepad.dpad.right.isPressed)
                {
                    value = Mathf.Max(value, 1f);
                }
            }
            catch { FindActiveGamepad(); }
        }

        return value;
    }

    public bool GetBoost()
    {
        // Check keyboard first (Shift)
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
            return true;
        // Check gamepad: LS click (leftStickButton) OR LB (leftShoulder)
        // Check even if not actively using gamepad, to allow seamless switching
        if (currentGamepad != null)
        {
            try
            {
                return currentGamepad.leftStickButton.isPressed || currentGamepad.leftShoulder.isPressed;
            }
            catch { FindActiveGamepad(); }
        }
        return false;
    }

    public bool GetBoostDown()
    {
        // Check keyboard first (Shift)
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame)
            return true;
        // Check gamepad: LS click (leftStickButton) OR LB (leftShoulder)
        // Check even if not actively using gamepad, to allow seamless switching
        if (currentGamepad != null)
        {
            try
            {
                return currentGamepad.leftStickButton.wasPressedThisFrame || currentGamepad.leftShoulder.wasPressedThisFrame;
            }
            catch { FindActiveGamepad(); }
        }
        return false;
    }

    public bool GetShoot()
    {
        // Check keyboard first (Space)
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
            return true;
        // Check gamepad: LT (leftTrigger) OR X button (buttonWest)
        // Check even if not actively using gamepad, to allow seamless switching
        if (currentGamepad != null)
        {
            try
            {
                float leftTrigger = currentGamepad.leftTrigger.ReadValue();
                return (leftTrigger > gamepadDeadzone) || currentGamepad.buttonWest.isPressed;
            }
            catch { FindActiveGamepad(); }
        }
        return false;
    }

    public bool GetShootDown()
    {
        // Check keyboard first (Space)
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            return true;
        // Check gamepad: LT (leftTrigger) OR X button (buttonWest)
        // Check even if not actively using gamepad, to allow seamless switching
        if (currentGamepad != null)
        {
            try
            {
                float leftTrigger = currentGamepad.leftTrigger.ReadValue();
                bool triggerPressed = leftTrigger > gamepadDeadzone && leftTrigger > 0.5f; // Require significant press
                return triggerPressed || currentGamepad.buttonWest.wasPressedThisFrame;
            }
            catch { FindActiveGamepad(); }
        }
        return false;
    }

    public bool GetHammerSwing()
    {
        // Check mouse first (left click)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;
        // Check gamepad: RT (rightTrigger) for hammer hit
        // Check even if not actively using gamepad, to allow seamless switching
        if (currentGamepad != null)
        {
            try
            {
                float rightTrigger = currentGamepad.rightTrigger.ReadValue();
                // Detect trigger press: current value is high and previous was low
                bool triggerPressed = rightTrigger > 0.5f && lastRightTriggerValue <= 0.3f;
                lastRightTriggerValue = rightTrigger;
                return triggerPressed;
            }
            catch { FindActiveGamepad(); }
        }
        return false;
    }

    public Vector2 GetHammerPosition()
    {
        // UNIFIED CURSOR SYSTEM: Mouse and right stick control the same cursor position.
        //
        // BUG FIX: the old code only updated hammerScreenPosition when mouse delta > 2px,
        // causing the hammer to freeze on slow movement then jump when speed exceeded the
        // threshold. Fix: in mouse mode always read the current mouse position directly;
        // the threshold is only used to detect a switch FROM gamepad mode.

        if (Mouse.current != null)
        {
            Vector2 currentMousePos = Mouse.current.position.ReadValue();
            Vector2 mouseDelta      = Mouse.current.delta.ReadValue();

            // Switch back to mouse mode if the user moves the mouse or clicks.
            if (isUsingGamepad)
            {
                bool mouseActive = mouseDelta.sqrMagnitude > 4f ||    // ~2px movement
                                   Mouse.current.leftButton.isPressed ||
                                   Mouse.current.leftButton.wasPressedThisFrame;
                if (mouseActive)
                {
                    isUsingGamepad = false;
                    lastInputTime  = Time.time;
                    Debug.Log($"Switched to mouse control at: ({currentMousePos.x:F1}, {currentMousePos.y:F1})");
                }
            }

            // In mouse mode: always track current mouse position — no delta threshold.
            if (!isUsingGamepad)
            {
                hammerScreenPosition = currentMousePos;
                return hammerScreenPosition;
            }
        }

        // Gamepad right-stick: detect switch to gamepad mode.
        if (currentGamepad != null)
        {
            Vector2 rightStick = currentGamepad.rightStick.ReadValue();
            if (rightStick.magnitude > gamepadDeadzone)
            {
                if (!isUsingGamepad)
                {
                    isUsingGamepad = true;
                    lastInputTime  = Time.time;
                    Debug.Log($"Switched to gamepad control from: ({hammerScreenPosition.x:F1}, {hammerScreenPosition.y:F1})");
                }
            }
        }

        // Gamepad mode (or no input device): return position updated by UpdateHammerCursorPosition().
        return hammerScreenPosition;
    }

    // Menu Input Queries
    public Vector2 GetNavigate()
    {
        Vector2 value = Vector2.zero;

        // Check keyboard
        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.isPressed) value.y += 1f;
            if (Keyboard.current.downArrowKey.isPressed) value.y -= 1f;
            if (Keyboard.current.leftArrowKey.isPressed) value.x -= 1f;
            if (Keyboard.current.rightArrowKey.isPressed) value.x += 1f;
        }

        // Check gamepad - always check, not just when isUsingGamepad
        if (currentGamepad != null)
        {
            try
            {
                // D-pad takes priority for menu navigation
                Vector2 dpad = currentGamepad.dpad.ReadValue();
                if (dpad.magnitude > 0.1f)
                {
                    value = dpad;
                }
                else
                {
                    // Fallback to left stick if D-pad not used
                    Vector2 stick = currentGamepad.leftStick.ReadValue();
                    if (stick.magnitude > gamepadDeadzone)
                    {
                        value = stick;
                    }
                }
            }
            catch { FindActiveGamepad(); }
        }

        return value;
    }

    public bool GetSubmit()
    {
        // Check keyboard
        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            return true;
        // Check gamepad A button (works with all gamepad types)
        if (currentGamepad != null)
        {
            try
            {
                return currentGamepad.buttonSouth.wasPressedThisFrame;
            }
            catch { FindActiveGamepad(); }
        }
        return false;
    }

    public bool GetCancel()
    {
        // Check keyboard
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            return true;
        // Check gamepad B button (works with all gamepad types)
        if (currentGamepad != null)
        {
            try
            {
                return currentGamepad.buttonEast.wasPressedThisFrame;
            }
            catch { FindActiveGamepad(); }
        }
        return false;
    }

    public bool GetPause()
    {
        // Check keyboard
        if (Keyboard.current != null && (Keyboard.current.pKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame))
            return true;
        // Check gamepad Start button (works with all gamepad types)
        if (currentGamepad != null)
        {
            try
            {
                return currentGamepad.startButton.wasPressedThisFrame;
            }
            catch { FindActiveGamepad(); }
        }
        return false;
    }

    public bool GetRestart()
    {
        // Check keyboard
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            return true;
        // Check gamepad Y button (works with all gamepad types)
        if (currentGamepad != null)
        {
            try
            {
                return currentGamepad.buttonNorth.wasPressedThisFrame;
            }
            catch { FindActiveGamepad(); }
        }
        return false;
    }

    // Legacy keyboard support (for backward compatibility)
    public bool GetKey(KeyCode key)
    {
        if (Keyboard.current == null) return false;
        return key switch
        {
            KeyCode.W => Keyboard.current.wKey.isPressed,
            KeyCode.A => Keyboard.current.aKey.isPressed,
            KeyCode.S => Keyboard.current.sKey.isPressed,
            KeyCode.D => Keyboard.current.dKey.isPressed,
            KeyCode.Space => Keyboard.current.spaceKey.isPressed,
            KeyCode.LeftShift => Keyboard.current.leftShiftKey.isPressed,
            KeyCode.Escape => Keyboard.current.escapeKey.isPressed,
            KeyCode.Return => Keyboard.current.enterKey.isPressed,
            KeyCode.P => Keyboard.current.pKey.isPressed,
            KeyCode.Q => Keyboard.current.qKey.isPressed,
            KeyCode.R => Keyboard.current.rKey.isPressed,
            _ => false
        };
    }

    public bool GetKeyDown(KeyCode key)
    {
        if (Keyboard.current == null) return false;
        return key switch
        {
            KeyCode.W => Keyboard.current.wKey.wasPressedThisFrame,
            KeyCode.A => Keyboard.current.aKey.wasPressedThisFrame,
            KeyCode.S => Keyboard.current.sKey.wasPressedThisFrame,
            KeyCode.D => Keyboard.current.dKey.wasPressedThisFrame,
            KeyCode.Space => Keyboard.current.spaceKey.wasPressedThisFrame,
            KeyCode.LeftShift => Keyboard.current.leftShiftKey.wasPressedThisFrame,
            KeyCode.Escape => Keyboard.current.escapeKey.wasPressedThisFrame,
            KeyCode.Return => Keyboard.current.enterKey.wasPressedThisFrame,
            KeyCode.P => Keyboard.current.pKey.wasPressedThisFrame,
            KeyCode.Q => Keyboard.current.qKey.wasPressedThisFrame,
            KeyCode.R => Keyboard.current.rKey.wasPressedThisFrame,
            _ => false
        };
    }

    public bool GetMouseButtonDown(int button)
    {
        if (Mouse.current == null) return false;
        return button switch
        {
            0 => Mouse.current.leftButton.wasPressedThisFrame,
            1 => Mouse.current.rightButton.wasPressedThisFrame,
            2 => Mouse.current.middleButton.wasPressedThisFrame,
            _ => false
        };
    }

    public Vector3 GetMousePosition()
    {
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
        return Vector3.zero;
    }

    public bool IsUsingGamepad()
    {
        return isUsingGamepad;
    }

    /// <summary>
    /// Manually force gamepad mode (for testing/debugging)
    /// </summary>
    public void ForceGamepadMode(bool enable)
    {
        isUsingGamepad = enable;
        lastInputTime = Time.time;
        Debug.Log($"Gamepad mode manually {(enable ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// Get current gamepad name for debugging
    /// </summary>
    public string GetGamepadName()
    {
        return currentGamepad != null ? currentGamepad.name : "None";
    }

    // Map control methods (for compatibility, but not needed with direct queries)
    public void EnableGameplayMap() { }
    public void EnableMenuMap() { }
    public void EnableBothMaps() { }
}
