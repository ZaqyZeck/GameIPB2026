using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-95)]
public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D pressedCursor;
    [SerializeField] private Vector2 hotspot = new Vector2(2f, 2f);

    private bool isPressed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("CursorManager");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<CursorManager>();
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
        DontDestroyOnLoad(gameObject);

        LoadCursorTextures();
        SetDefaultCursor();
    }

    private void LoadCursorTextures()
    {
        if (defaultCursor == null)
            defaultCursor = Resources.Load<Texture2D>("Cursor/cursor");

        if (pressedCursor == null)
            pressedCursor = Resources.Load<Texture2D>("Cursor/cursor_pressed");
    }

    private void Update()
    {
        bool pointerDown = false;
        bool pointerUp = false;

        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame) pointerDown = true;
            if (Mouse.current.leftButton.wasReleasedThisFrame) pointerUp = true;
        }
        else if (Pointer.current != null)
        {
            if (Pointer.current.press.wasPressedThisFrame) pointerDown = true;
            if (Pointer.current.press.wasReleasedThisFrame) pointerUp = true;
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) pointerDown = true;
            if (Input.GetMouseButtonUp(0)) pointerUp = true;
        }

        if (pointerDown && !isPressed)
        {
            SetPressedCursor();
        }
        else if (pointerUp && isPressed)
        {
            SetDefaultCursor();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            SetDefaultCursor();
        }
    }

    public void SetDefaultCursor()
    {
        isPressed = false;
        if (defaultCursor != null)
        {
            Cursor.SetCursor(defaultCursor, hotspot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    public void SetPressedCursor()
    {
        isPressed = true;
        if (pressedCursor != null)
        {
            Cursor.SetCursor(pressedCursor, hotspot, CursorMode.Auto);
        }
    }
}
