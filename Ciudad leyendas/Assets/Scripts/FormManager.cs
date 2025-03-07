using System;
using System.Threading.Tasks;
using Models;
using Supabase;
using Supabase.Postgrest;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Client = Supabase.Client;

public class FormManager : MonoBehaviour
{
    [Header("LogIn Form")] public GameObject logInForm;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button logInButton;
    public TMP_Text infoText;
    public Button logInGoogleButton;
    public GoogleSignInManager googleSignInManager;
    public Button goToSignUpForm;
    [Header("SignUp Form")] public GameObject signUpForm;
    public TMP_InputField usernameInput;
    public TMP_InputField signUpEmailInput;
    public TMP_InputField signUpEmailConfirmInput;
    public TMP_InputField signUpPasswordInput;
    public TMP_InputField signUpPasswordConfirmInput;
    public TMP_Text infoTextSignUp;
    public Button signUpButton;
    public Button goToLogInForm;

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

    /// <summary>
    /// Checks if there is a session already open when the game starts
    /// </summary>
    private async void CheckForSession()
    {
        try
        {
            var supabase = await ConnectSupabase();
            var session = await supabase.Auth.RetrieveSessionAsync();
        
            if (session != null && session.User != null)
            {
                Debug.Log("Session found. User already logged in: " + session.User.Email);
                
                await CheckPlayerExists(session.User.Id);
                
                GoToGame();
            }
            else
            {
                Debug.Log("No active session found");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error checking session: " + e.Message);
        }
    }

    /// <summary>
    /// Method to check if the player exists in the database
    /// </summary>
    /// <param name="userId">Id from the user session</param>
    private static async Task CheckPlayerExists(string userId)
    {
        var supabase = await ConnectSupabase();

        try
        {
            var response = await supabase.From<Jugador>().Select("nombre")
                .Filter("id_usuario", Constants.Operator.Equals, userId).Get();

            if (response.Models.Count == 0)
            {
                Debug.Log("Player does not exist, creating player...");
                
                string defaultName = "Player" + DateTime.Now.Ticks % 10000;
            
                var model = new Jugador
                {
                    Nombre = defaultName,
                    PasosTotales = 0,
                    IdUsuario = Guid.Parse(userId),
                    IdClan = 0
                };

                var response2 = await supabase.From<Jugador>().Insert(model);
                Debug.Log("Player created: " + response2.Models[0].Nombre);
            }
            else
            {
                Debug.Log("Player exists: " + response.Models[0].Nombre);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error checking player: " + e.Message);
        }
    }

    /// <summary>
    /// Method to sign up a new user
    /// </summary>
    private async void SignUp()
    {
        String username = usernameInput.text;
        String signUpEmail = signUpEmailConfirmInput.text;
        String signUpEmailConfirm = signUpEmailConfirmInput.text;
        String signUpPassword = signUpPasswordConfirmInput.text;
        String signUpPasswordConfirm = signUpPasswordConfirmInput.text;

        await VerifyInputFields(signUpEmail, signUpEmailConfirm, signUpPassword, signUpPasswordConfirm, username);
    }

    /// <summary>
    /// Method to verify the input fields of the sign-up form
    /// </summary>
    /// <param name="signUpEmail">Email from user</param>
    /// <param name="signUpEmailConfirm">Confirmation for user email</param>
    /// <param name="signUpPassword">User password</param>
    /// <param name="signUpPasswordConfirm">Confirmation for user password</param>
    /// <param name="username">Username set by user</param>
    private async Task VerifyInputFields(string signUpEmail, string signUpEmailConfirm, string signUpPassword,
        string signUpPasswordConfirm, string username)
    {
        Debug.Log("SignUp Email: " + signUpEmail);
        // Check if email and password are the same
        if (signUpEmail != signUpEmailConfirm)
        {
            infoTextSignUp.text = "Emails do not match!";
            infoTextSignUp.color = Color.red;
            return;
        }

        // Check if password and password confirm are the same
        Debug.Log("SignUp Password: " + signUpPassword);
        if (signUpPassword != signUpPasswordConfirm)
        {
            infoTextSignUp.text = "Passwords do not match!";
            infoTextSignUp.color = Color.red;
            return;
        }

        // Check if email is valid
        Debug.Log("SignUp Password: " + signUpPassword);
        if (!signUpEmail.Contains("@"))
        {
            infoTextSignUp.text = "Invalid email!";
            infoTextSignUp.color = Color.red;
            return;
        }

        // Check if password is strong enough
        Debug.Log("SignUp Password: " + signUpPassword);
        if (signUpPassword.Length < 10 || !HasUpperCase(signUpPassword) || !HasLowerCase(signUpPassword) ||
            !HasSymbol(signUpPassword))
        {
            infoTextSignUp.text =
                "Password must be at least 10 characters long, contain an uppercase letter, a lowercase letter, and a symbol!";
            infoTextSignUp.color = Color.red;
            return;
        }

        // Check if username is valid
        Debug.Log("SignUp Username: " + username);
        if (username.Length < 3)
        {
            infoTextSignUp.text = "Username must be at least 3 characters long!";
            infoTextSignUp.color = Color.red;
            return;
        }

        // Check if username is unique
        Debug.Log("SignUp Password: " + signUpPassword);
        try
        {
            Debug.Log("Checking if username is unique...");
            var supabase = await ConnectSupabase();
            var response = await supabase.From<Jugador>().Select("nombre")
                .Filter("nombre", Constants.Operator.Equals, username).Get();
            if (response.Model != null)
            {
                infoTextSignUp.text = "Username already exists!";
                infoTextSignUp.color = Color.red;
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("SignUp failed: " + e.Message);
            infoTextSignUp.text = "SignUp failed!";
            infoTextSignUp.color = Color.red;
        }

        // If all checks pass, sign up
        Debug.Log("All checks passed!");
        try
        {
            var supabase = await ConnectSupabase();
            var session = await supabase.Auth.SignUp(signUpEmail, signUpPassword);
            if (session is { User: not null })
            {
                Debug.Log("SignUp successful!");
                infoTextSignUp.text = "SignUp successful!";
                infoTextSignUp.color = Color.green;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("SignUp failed: " + e.Message);
            infoTextSignUp.text = "SignUp failed!";
            infoTextSignUp.color = Color.red;
        }
    }

    /// <summary>
    /// Checks if input string has at least one uppercase letter
    /// </summary>
    /// <param name="input">User password</param>
    /// <returns></returns>
    private bool HasUpperCase(string input)
    {
        foreach (char c in input)
        {
            if (char.IsUpper(c))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if input string has at least one lowercase letter
    /// </summary>
    /// <param name="input">User password</param>
    /// <returns></returns>
    private bool HasLowerCase(string input)
    {
        foreach (char c in input)
        {
            if (char.IsLower(c))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if input string has at least one symbol
    /// </summary>
    /// <param name="input">User password</param>
    /// <returns></returns>
    private bool HasSymbol(string input)
    {
        foreach (char c in input)
        {
            if (!char.IsLetterOrDigit(c))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Method to log in a user
    /// </summary>
    private async void LogIn()
    {
        try
        {
            var email = emailInput.text;
            var password = passwordInput.text;

            var supabase = await ConnectSupabase();

            var response = await supabase.Auth.SignIn(email, password);

            if (response is { User: not null })
            {
                Debug.Log("LogIn successful!");
                infoText.text = "LogIn successful!";
                infoText.color = Color.green;
                GoToGame();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("LogIn failed: " + e.Message);
            infoText.text = "LogIn failed!";
            infoText.color = Color.red;
        }
    }

    /// <summary>
    /// After user logs, go to the game scene
    /// </summary>
    public static void GoToGame()
    {
        SceneManager.LoadScene("GridTest");
    }

    /// <summary>
    /// Allows to connect to Supabase
    /// </summary>
    /// <returns></returns>
    private static async Task<Client> ConnectSupabase()
    {
        var url = SupabaseKeys.supabaseURL;
        var key = SupabaseKeys.supabaseKey;

        var options = new SupabaseOptions()
        {
            AutoConnectRealtime = true
        };

        var supabase = new Client(url, key, options);

        await supabase.InitializeAsync();

        return supabase;
    }
}