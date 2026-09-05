using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

/// <summary>
/// Handles all Unity Gaming Services (UGS) authentication:
/// - Initializing Unity Services
/// - Registering a new account (username/password)
/// - Logging in (username/password)
/// - Auto-login using a cached session token
/// - Signing out
///
/// Attach this to a persistent GameObject (e.g. an empty "Managers" object)
/// that exists in your first/bootstrap scene.
/// </summary>
public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = true;

    // ---- Public state ----
    public bool IsSignedIn => AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn;
    public string PlayerId => AuthenticationService.Instance?.PlayerId;
    public string PlayerName { get; private set; }

    // ---- Events your UI can subscribe to ----
    public event Action OnInitialized;
    public event Action OnSignedIn;
    public event Action OnSignedOut;
    public event Action<string> OnAuthError;       // generic error message
    public event Action<string> OnRegisterSuccess; // returns playerId
    public event Action<string> OnRegisterError;
    public event Action<string> OnLoginError;

    public bool IsInitialized { get; private set; }

    private async void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InitializeUnityServices();
    }

    private void OnEnable()
    {
        // These fire once UGS is initialized, wired in InitializeUnityServices()
    }

    // ---------------------------------------------------------------
    //  INITIALIZATION
    // ---------------------------------------------------------------

    private async Task InitializeUnityServices()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }

            // Hook up SDK-level events (fired for any sign-in method, incl. auto-login)
            AuthenticationService.Instance.SignedIn -= HandleSignedIn;
            AuthenticationService.Instance.SignedIn += HandleSignedIn;

            AuthenticationService.Instance.SignedOut -= HandleSignedOut;
            AuthenticationService.Instance.SignedOut += HandleSignedOut;

            AuthenticationService.Instance.SignInFailed -= HandleSignInFailed;
            AuthenticationService.Instance.SignInFailed += HandleSignInFailed;

            AuthenticationService.Instance.Expired -= HandleSessionExpired;
            AuthenticationService.Instance.Expired += HandleSessionExpired;

            IsInitialized = true;
            Log("Unity Services initialized.");
            OnInitialized?.Invoke();

            // Try to silently sign back in with a cached session token, if one exists.
            await TryAutoLoginAsync();
        }
        catch (Exception e)
        {
            LogError($"Failed to initialize Unity Services: {e}");
            OnAuthError?.Invoke("Failed to initialize services. Check your internet connection.");
        }
    }

    /// <summary>
    /// If a valid session token is cached on this device (from a previous login),
    /// this signs the player back in automatically without needing credentials.
    /// Call this at app start (already called from Awake) or after a manual "Continue" tap.
    /// </summary>
    public async Task<bool> TryAutoLoginAsync()
    {
        if (!AuthenticationService.Instance.SessionTokenExists)
        {
            Log("No cached session token found. User must log in manually.");
            return false;
        }

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            // Note: SignInAnonymouslyAsync will actually resume the cached session
            // (username/password or anonymous) if a valid token exists, rather than
            // creating a new anonymous identity, as long as the token is still valid.
            Log("Auto-login with cached session succeeded.");
            return true;
        }
        catch (AuthenticationException ex)
        {
            LogError($"Auto-login failed (Authentication): {ex.Message}");
            OnAuthError?.Invoke("Your session expired. Please log in again.");
        }
        catch (RequestFailedException ex)
        {
            LogError($"Auto-login failed (Request): {ex.Message}");
            OnAuthError?.Invoke("Could not reach the server. Please check your connection.");
        }

        return false;
    }

    // ---------------------------------------------------------------
    //  REGISTER
    // ---------------------------------------------------------------

    /// <summary>
    /// Creates a brand-new account tied to a username/password.
    /// Unity's requirements: username 3-20 chars (letters, numbers, ., @, _, -),
    /// password 8-30 chars, at least 1 upper, 1 lower, 1 number, 1 symbol.
    /// </summary>
    public async Task RegisterAsync(string username, string password)
    {
        if (!ValidateCredentialsFormat(username, password, out string formatError))
        {
            OnRegisterError?.Invoke(formatError);
            return;
        }

        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Log($"Registration successful. PlayerId: {AuthenticationService.Instance.PlayerId}");
            PlayerName = username;
            OnRegisterSuccess?.Invoke(AuthenticationService.Instance.PlayerId);
            // SignedIn event will also fire automatically after a successful sign-up.
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            OnRegisterError?.Invoke("That username is already taken.");
        }
        catch (AuthenticationException ex)
        {
            LogError($"Registration failed (Authentication): {ex.Message}");
            OnRegisterError?.Invoke(FriendlyAuthError(ex));
        }
        catch (RequestFailedException ex)
        {
            LogError($"Registration failed (Request): {ex.Message}");
            OnRegisterError?.Invoke("Could not reach the server. Please try again.");
        }
    }

    // ---------------------------------------------------------------
    //  LOGIN
    // ---------------------------------------------------------------

    public async Task LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            OnLoginError?.Invoke("Please enter both username and password.");
            return;
        }

        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);

            Log($"Login successful. PlayerId: {AuthenticationService.Instance.PlayerId}");
            PlayerName = username;
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.InvalidParameters)
        {
            OnLoginError?.Invoke("Invalid username or password format.");
        }
        catch (AuthenticationException ex)
        {
            LogError($"Login failed (Authentication): {ex.ErrorCode} - {ex.Message}");
            OnLoginError?.Invoke("Incorrect username or password.");
        }
        catch (RequestFailedException ex)
        {
            LogError($"Login failed (Request): {ex.ErrorCode} - {ex.Message}");
            OnLoginError?.Invoke("Could not reach the server. Please try again.");
        }
    }

    // ---------------------------------------------------------------
    //  SIGN OUT
    // ---------------------------------------------------------------

    /// <param name="clearCachedSession">
    /// If true, also deletes the cached session token so TryAutoLoginAsync()
    /// won't silently sign the player back in next launch.
    /// </param>
    public void SignOut(bool clearCachedSession = false)
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut(clearCachedSession);
            PlayerName = null;
            Log("Signed out.");
        }
    }

    // ---------------------------------------------------------------
    //  SDK EVENT HANDLERS
    // ---------------------------------------------------------------

    private void HandleSignedIn()
    {
        Log($"[Event] SignedIn. PlayerId: {AuthenticationService.Instance.PlayerId}");
        OnSignedIn?.Invoke();
    }

    private void HandleSignedOut()
    {
        Log("[Event] SignedOut.");
        OnSignedOut?.Invoke();
    }

    private void HandleSignInFailed(RequestFailedException ex)
    {
        LogError($"[Event] SignInFailed: {ex.Message}");
        OnAuthError?.Invoke("Sign-in failed. Please try again.");
    }

    private void HandleSessionExpired()
    {
        Log("[Event] Session expired.");
        OnAuthError?.Invoke("Your session has expired. Please log in again.");
    }

    // ---------------------------------------------------------------
    //  HELPERS
    // ---------------------------------------------------------------

    private bool ValidateCredentialsFormat(string username, string password, out string error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 20)
        {
            error = "Username must be 3-20 characters.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || password.Length > 30)
        {
            error = "Password must be 8-30 characters.";
            return false;
        }

        bool hasUpper = false, hasLower = false, hasDigit = false, hasSymbol = false;
        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsLower(c)) hasLower = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else hasSymbol = true;
        }

        if (!(hasUpper && hasLower && hasDigit && hasSymbol))
        {
            error = "Password needs an uppercase letter, a lowercase letter, a number, and a symbol.";
            return false;
        }

        return true;
    }

    private string FriendlyAuthError(AuthenticationException ex)
    {
        //return ex.ErrorCode switch
        //{
        //    AuthenticationErrorCodes.AccountAlreadyLinked => "That account is already linked.",
        //    AuthenticationErrorCodes.InvalidCredentials => "Incorrect username or password.",
        //    AuthenticationErrorCodes.InvalidParameters => "Invalid input. Check your username/password format.",
        //    AuthenticationErrorCodes.InvalidSessionToken => "Session expired. Please log in again.",
        //    _ => "Something went wrong. Please try again."
        //};
        return "ada kesalahan";
    }

    private void Log(string message)
    {
        if (logDebugMessages) Debug.Log($"[Auth] {message}");
    }

    private void LogError(string message)
    {
        if (logDebugMessages) Debug.LogError($"[Auth] {message}");
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        if (AuthenticationService.Instance != null)
        {
            AuthenticationService.Instance.SignedIn -= HandleSignedIn;
            AuthenticationService.Instance.SignedOut -= HandleSignedOut;
            AuthenticationService.Instance.SignInFailed -= HandleSignInFailed;
            AuthenticationService.Instance.Expired -= HandleSessionExpired;
        }
    }
}