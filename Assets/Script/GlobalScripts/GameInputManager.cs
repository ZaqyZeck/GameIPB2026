using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance { get; private set; }

    // Semantic events for gameplay scripts
    public event Action<Vector2> OnPrimaryTap;
    public event Action OnDrop;
    public event Action OnDialogueNext;
    public event Action OnDialoguePrev;
    public event Action OnFastForward;

    private InputActionMap actionMap;
    private InputAction pointAction;
    private InputAction primaryTapAction;
    private InputAction dropAction;
    private InputAction dialogueNextAction;
    private InputAction dialoguePrevAction;
    private InputAction fastForwardAction;

    public Vector2 CurrentPointerPosition
    {
        get
        {
            if (pointAction != null) return pointAction.ReadValue<Vector2>();
            if (Pointer.current != null) return Pointer.current.position.ReadValue();
            return Vector2.zero;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("GameInputManager");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<GameInputManager>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializeActions();
    }

    private void InitializeActions()
    {
        if (actionMap != null) return;

        actionMap = new InputActionMap("Gameplay");

        // 1. Pointer Position (Mouse cursor or Touch position)
        pointAction = actionMap.AddAction("Point", InputActionType.Value);
        pointAction.expectedControlType = "Vector2";
        pointAction.AddBinding("<Pointer>/position");

        // 2. Primary Tap (Mouse Left Button, Touch Press, Gamepad South Button)
        primaryTapAction = actionMap.AddAction("PrimaryTap", InputActionType.Button);
        primaryTapAction.AddBinding("<Pointer>/press");
        primaryTapAction.AddBinding("<Gamepad>/buttonSouth");

        // 3. Drop Item (Mouse Right Click, Keyboard Space, Keyboard F, Gamepad East Button)
        dropAction = actionMap.AddAction("Drop", InputActionType.Button);
        dropAction.AddBinding("<Mouse>/rightButton");
        dropAction.AddBinding("<Keyboard>/space");
        dropAction.AddBinding("<Keyboard>/f");
        dropAction.AddBinding("<Gamepad>/buttonEast");

        // 4. Dialogue Next (Keyboard Space, Enter, Gamepad South)
        dialogueNextAction = actionMap.AddAction("DialogueNext", InputActionType.Button);
        dialogueNextAction.AddBinding("<Keyboard>/space");
        dialogueNextAction.AddBinding("<Keyboard>/enter");
        dialogueNextAction.AddBinding("<Gamepad>/buttonSouth");

        // 5. Dialogue Prev (Keyboard Q, Left Arrow, Gamepad Dpad Left)
        dialoguePrevAction = actionMap.AddAction("DialoguePrev", InputActionType.Button);
        dialoguePrevAction.AddBinding("<Keyboard>/q");
        dialoguePrevAction.AddBinding("<Keyboard>/leftArrow");
        dialoguePrevAction.AddBinding("<Gamepad>/dpad/left");

        // 6. Fast Forward (Any Key, Pointer Press, Gamepad South)
        fastForwardAction = actionMap.AddAction("FastForward", InputActionType.Button);
        fastForwardAction.AddBinding("<Keyboard>/anyKey");
        fastForwardAction.AddBinding("<Pointer>/press");
        fastForwardAction.AddBinding("<Gamepad>/buttonSouth");

        // Wire event triggers
        primaryTapAction.performed += ctx =>
        {
            if (!IsGameplayInputAllowed) return;
            Vector2 pos = CurrentPointerPosition;
            OnPrimaryTap?.Invoke(pos);
        };

        dropAction.performed += ctx => TriggerDrop();
        dialogueNextAction.performed += ctx => TriggerDialogueNext();
        dialoguePrevAction.performed += ctx => TriggerDialoguePrev();
        fastForwardAction.performed += ctx => TriggerFastForward();

        actionMap.Enable();
    }

    private bool isPaused;

    public bool IsGameplayInputAllowed
    {
        get
        {
            if (isPaused) return false;
            if (LevelManager.Instance != null && !LevelManager.Instance.IsPlaying) return false;
            return true;
        }
    }

    private void OnEnable()
    {
        actionMap?.Enable();
        GameEventBus.OnPause += HandlePause;
        GameEventBus.OnResume += HandleResume;
    }

    private void OnDisable()
    {
        actionMap?.Disable();
        GameEventBus.OnPause -= HandlePause;
        GameEventBus.OnResume -= HandleResume;
    }

    private void HandlePause() => isPaused = true;
    private void HandleResume() => isPaused = false;

    private void OnDestroy()
    {
        GameEventBus.OnPause -= HandlePause;
        GameEventBus.OnResume -= HandleResume;

        if (actionMap != null)
        {
            actionMap.Disable();
            actionMap.Dispose();
            actionMap = null;
        }
    }

    /// <summary>
    /// Helper to detect if a tap/click landed on a UI element (so gameplay ignores it).
    /// Works for Mouse, Touch, and Mobile WebGL.
    /// </summary>
    public bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        #if UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            var touch = Touchscreen.current.touches[0];
            if (touch.press.isPressed)
            {
                return EventSystem.current.IsPointerOverGameObject(touch.touchId.ReadValue());
            }
        }
        #endif

        return EventSystem.current.IsPointerOverGameObject();
    }

    // Public triggers for UI buttons (e.g. Mobile On-Screen Drop Button)
    public void TriggerDrop()
    {
        if (!IsGameplayInputAllowed) return;
        OnDrop?.Invoke();
    }

    public void TriggerDialogueNext()
    {
        if (!IsGameplayInputAllowed) return;
        OnDialogueNext?.Invoke();
    }

    public void TriggerDialoguePrev()
    {
        if (!IsGameplayInputAllowed) return;
        OnDialoguePrev?.Invoke();
    }

    public void TriggerFastForward()
    {
        if (!IsGameplayInputAllowed) return;
        OnFastForward?.Invoke();
    }
}
