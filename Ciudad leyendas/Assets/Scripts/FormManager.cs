using Services;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Version = Models.Version;

public class FormManager : MonoBehaviour
{
    private LoginManager _loginManager;
    private SignUpManager _signUpManager;

    public Image splashImage;
    public TMP_Text splashText;

    [Header("LogIn Form")] public GameObject logInForm;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button logInButton;
    public TMP_Text infoText;
    public Button logInGoogleButton;
    public GoogleSignInManager googleSignInManager;
    public Button goToSignUpForm;

    [Header("SignUp Form")] public GameObject signUpForm;
    public TMP_InputField signUpEmailInput;
    public TMP_InputField signUpEmailConfirmInput;
    public TMP_InputField signUpPasswordInput;
    public TMP_InputField signUpPasswordConfirmInput;
    public TMP_Text infoTextSignUp;
    public Button signUpButton;
    public Button goToLogInForm;

    [Header("Version")] public string currentGameVersion = "1.0.0";
    private bool _sameVersion = false;

    void Awake()
    {
        _loginManager = new LoginManager();
        _signUpManager = new SignUpManager();

        if (googleSignInManager != null)
            googleSignInManager.SetLoginManager(_loginManager);

        HideAllUIElements();

        if (splashImage != null)
            splashImage.gameObject.SetActive(true);

        if (splashText != null)
        {
            splashText.gameObject.SetActive(true);
            splashText.text = "Verificando versión...";
        }
    }

    void Start()
    {
        StartCoroutine(CheckGameVersionAndContinue());
        SetupButtonListeners();
    }

    private IEnumerator CheckGameVersionAndContinue()
    {
        yield return StartCoroutine(CheckGameVersion());

        if (_sameVersion)
        {
            yield return new WaitForSeconds(1f);
            ShowUIElementsAfterDelay();
            CheckForSession();
        }
        else
        {
            if (splashText != null)
            {
                splashText.text = "Por favor, actualiza la aplicación para continuar.";
                splashText.color = Color.yellow;
            }
        }
    }

    private IEnumerator CheckGameVersion()
    {
        bool checking = true;

        CheckLatestVersion(version =>
        {
            if (string.IsNullOrEmpty(version))
            {
                if (splashText != null)
                {
                    splashText.text = "Error al verificar la versión. Por favor, comprueba tu conexión a internet.";
                    splashText.color = Color.red;
                }

                _sameVersion = false;
            }
            else if (version != currentGameVersion)
            {
                if (splashText != null)
                {
                    splashText.text = $"Se encontró una nueva versión: {version}\nPor favor, actualiza tu juego.";
                    splashText.color = Color.yellow;
                }

                _sameVersion = false;
            }
            else
            {
                if (splashText != null)
                {
                    splashText.text = "Versión verificada correctamente.";
                    splashText.color = Color.green;
                }

                _sameVersion = true;
            }

            checking = false;
        });

        while (checking)
            yield return null;

        yield return new WaitForSeconds(2f);
    }

    private async void CheckLatestVersion(Action<string> callback)
    {
        try
        {
            var supabase = await SupabaseManager.Instance.GetClient();
            var response = await supabase.From<Version>()
                .Select("*")
                .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(1)
                .Get();

            if (response.Models.Count > 0)
            {
                string latestVersion = response.Models[0].VersionNumber;
                Debug.Log($"Versión más reciente: {latestVersion}");
                callback(latestVersion);
            }
            else
            {
                Debug.LogWarning("No se encontró información de versión");
                callback(null);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al verificar la versión: {e.Message}");
            if (splashText != null)
            {
                splashText.text = $"Error al verificar la versión. Por favor, comprueba tu conexión a internet.";
                splashText.color = Color.red;
            }

            callback(null);
        }
    }

    private void HideAllUIElements()
    {
        if (logInForm != null) logInForm.SetActive(false);
        if (signUpForm != null) signUpForm.SetActive(false);
        if (logInGoogleButton != null) logInGoogleButton.gameObject.SetActive(false);
        if (infoText != null) infoText.gameObject.SetActive(false);
        if (infoTextSignUp != null) infoTextSignUp.gameObject.SetActive(false);
    }

    private void ShowUIElementsAfterDelay()
    {
        if (splashImage != null) splashImage.gameObject.SetActive(false);
        if (splashText != null) splashText.gameObject.SetActive(false);

        if (logInForm != null) logInForm.SetActive(true);
        if (signUpForm != null) signUpForm.SetActive(false);

        if (logInGoogleButton != null) logInGoogleButton.gameObject.SetActive(true);

        if (infoText != null)
        {
            infoText.text = "";
            infoText.gameObject.SetActive(true);
        }
    }

    private void SetupButtonListeners()
    {
        if (logInButton != null)
            logInButton.onClick.AddListener(LogIn);

        if (logInGoogleButton != null && googleSignInManager != null)
            logInGoogleButton.onClick.AddListener(googleSignInManager.SignInWithGoogle);

        if (goToSignUpForm != null)
            goToSignUpForm.onClick.AddListener(() =>
            {
                logInForm.SetActive(false);
                signUpForm.SetActive(true);
                if (infoTextSignUp != null)
                    infoTextSignUp.gameObject.SetActive(false);
            });

        if (goToLogInForm != null)
            goToLogInForm.onClick.AddListener(() =>
            {
                logInForm.SetActive(true);
                signUpForm.SetActive(false);
            });

        if (signUpButton != null)
            signUpButton.onClick.AddListener(SignUp);
    }

    private async void CheckForSession()
    {
        bool hasSession = await _loginManager.CheckForSession();
        if (hasSession)
            _loginManager.GoToGame();
    }

    private async void SignUp()
    {
        string email = signUpEmailInput.text;
        string emailConfirm = signUpEmailConfirmInput.text;
        string password = signUpPasswordInput.text;
        string passwordConfirm = signUpPasswordConfirmInput.text;

        await _signUpManager.SignUp(email, emailConfirm, password, passwordConfirm, infoTextSignUp);
    }

    private async void LogIn()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        bool success = await _loginManager.LogIn(email, password, infoText);

        if (success)
            _loginManager.GoToGame();
    }
}