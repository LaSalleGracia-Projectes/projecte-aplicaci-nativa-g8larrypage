using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormManager : MonoBehaviour
{
    private LoginManager _loginManager;
    private SignUpManager _signUpManager;

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

    void Awake()
    {
        _loginManager = new LoginManager();
        _signUpManager = new SignUpManager();

        // Asigna la referencia al LoginManager
        if (googleSignInManager != null)
        {
            googleSignInManager.SetLoginManager(_loginManager);
        }
    }

    void Start()
    {
        logInForm.SetActive(true);
        signUpForm.SetActive(false);
        infoText.text = "";

        logInButton.onClick.AddListener(LogIn);
        logInGoogleButton.onClick.AddListener(googleSignInManager.SignInWithGoogle);

        goToSignUpForm.onClick.AddListener(() =>
        {
            logInForm.SetActive(false);
            signUpForm.SetActive(true);
        });

        goToLogInForm.onClick.AddListener(() =>
        {
            logInForm.SetActive(true);
            signUpForm.SetActive(false);
        });

        signUpButton.onClick.AddListener(SignUp);

        CheckForSession();
    }

    private async void CheckForSession()
    {
        bool hasSession = await _loginManager.CheckForSession();
        if (hasSession)
        {
            _loginManager.GoToGame();
        }
    }

    private async void SignUp()
    {
        string email = signUpEmailInput.text;
        string emailConfirm = signUpEmailConfirmInput.text;
        string password = signUpPasswordInput.text;
        string passwordConfirm = signUpPasswordConfirmInput.text;

        await _signUpManager.SignUp(email, emailConfirm,
            password, passwordConfirm, infoTextSignUp);
    }

    private async void LogIn()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        bool success = await _loginManager.LogIn(email, password, infoText);

        if (success)
        {
            _loginManager.GoToGame();
        }
    }
}