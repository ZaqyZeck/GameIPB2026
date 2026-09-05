using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Example UI wiring for login/register screens. Adjust field names/hierarchy
/// to match your actual UI, or use this as a reference for your own controller.
///
/// Expected hierarchy (feel free to change):
/// - LoginPanel
///     - UsernameInput (TMP_InputField)
///     - PasswordInput (TMP_InputField)
///     - LoginButton (Button)
///     - GoToRegisterButton (Button)
/// - RegisterPanel
///     - UsernameInput (TMP_InputField)
///     - PasswordInput (TMP_InputField)
///     - ConfirmPasswordInput (TMP_InputField)
///     - RegisterButton (Button)
///     - GoToLoginButton (Button)
/// - StatusText (TMP_Text) - shows errors / status messages
/// - LoadingSpinner (GameObject) - optional, shown while a request is in flight
/// </summary>
public class AuthenUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private GameObject leaderboardPanel;

    [Header("Login Fields")]
    [SerializeField] private TMP_InputField loginUsernameInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button goToRegisterButton;

    [Header("Register Fields")]
    [SerializeField] private TMP_InputField registerUsernameInput;
    [SerializeField] private TMP_InputField registerPasswordInput;
    [SerializeField] private TMP_InputField registerConfirmPasswordInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button goToLoginButton;

    [Header("Common")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject loadingSpinner;
    [SerializeField] private Button signOutButton;
    [SerializeField] private TMP_Text welcomeText;

    private void OnEnable()
    {
        var auth = AuthenticationManager.Instance;
        if (auth == null) return;

        auth.OnSignedIn += HandleSignedIn;
        auth.OnSignedOut += HandleSignedOut;
        auth.OnAuthError += ShowError;
        auth.OnRegisterError += ShowError;
        auth.OnLoginError += ShowError;
        auth.OnRegisterSuccess += _ => ShowStatus("Account created! You're now signed in.");
    }

    private void OnDisable()
    {
        var auth = AuthenticationManager.Instance;
        if (auth == null) return;

        auth.OnSignedIn -= HandleSignedIn;
        auth.OnSignedOut -= HandleSignedOut;
        auth.OnAuthError -= ShowError;
        auth.OnRegisterError -= ShowError;
        auth.OnLoginError -= ShowError;
    }

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
        goToRegisterButton.onClick.AddListener(() => SwitchPanel(register: true));
        goToLoginButton.onClick.AddListener(() => SwitchPanel(register: false));
        signOutButton.onClick.AddListener(OnSignOutClicked);

        // If already signed in (e.g. auto-login already resolved before this UI loaded)
        if (AuthenticationManager.Instance != null && AuthenticationManager.Instance.IsSignedIn)
        {
            HandleSignedIn();
        }
        else
        {
            SwitchPanel(register: false);
        }
    }

    private async void OnLoginClicked()
    {
        SetLoading(true);
        ClearStatus();
        await AuthenticationManager.Instance.LoginAsync(
            loginUsernameInput.text.Trim(),
            loginPasswordInput.text);
        SetLoading(false);
    }

    private async void OnRegisterClicked()
    {
        if (registerPasswordInput.text != registerConfirmPasswordInput.text)
        {
            ShowError("Passwords do not match.");
            return;
        }

        SetLoading(true);
        ClearStatus();
        await AuthenticationManager.Instance.RegisterAsync(
            registerUsernameInput.text.Trim(),
            registerPasswordInput.text);
        SetLoading(false);
    }

    private void OnSignOutClicked()
    {
        // Pass true instead if you also want to clear the cached session token
        // (forces manual login next launch).
        AuthenticationManager.Instance.SignOut(clearCachedSession: false);
    }

    private void HandleSignedIn()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        leaderboardPanel.SetActive(true);

        if (welcomeText != null)
        {
            string name = string.IsNullOrEmpty(AuthenticationManager.Instance.PlayerName)
                ? AuthenticationManager.Instance.PlayerId
                : AuthenticationManager.Instance.PlayerName;
            welcomeText.text = $"Welcome, {name}!";
        }

        ClearStatus();
    }

    private void HandleSignedOut()
    {
        leaderboardPanel.SetActive(false);
        SwitchPanel(register: false);
    }

    private void SwitchPanel(bool register)
    {
        leaderboardPanel.SetActive(false);
        loginPanel.SetActive(!register);
        registerPanel.SetActive(register);
        ClearStatus();
    }

    private void SetLoading(bool isLoading)
    {
        if (loadingSpinner != null) loadingSpinner.SetActive(isLoading);
        loginButton.interactable = !isLoading;
        registerButton.interactable = !isLoading;
    }

    private void ShowError(string message) => ShowStatus(message);

    private void ShowStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }

    private void ClearStatus()
    {
        if (statusText != null) statusText.text = string.Empty;
    }
}
