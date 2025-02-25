using System;
using System.Threading.Tasks;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using Client = Supabase.Client;

public class FormManager : MonoBehaviour
{
    [Header("LogIn Form")]
    public GameObject logInForm;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button logInButton;
    public TMP_Text infoText;
    public Button logInGoogleButton;

    private bool _doSignIn;
    private bool _doExchangeCode;
    private HttpListener httpListener;
    private string _pkce;
    private string _token;

    void Start()
    {
        infoText.text = "";
        logInButton.onClick.AddListener(LogIn);
        logInGoogleButton.onClick.AddListener(SignInWithGoogle);
    }

    private async void SignInWithGoogle()
    {
        _doSignIn = true;
    }

    private async void Update()
    {
        if (_doSignIn)
        {
            _doSignIn = false;
            await PerformSignIn();
        }

        if (_doExchangeCode)
        {
            _doExchangeCode = false;
            await PerformExchangeCode();
        }
    }

    private void StartLocalWebserver()
    {
        if (httpListener == null)
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://*:3000/");
            httpListener.Start();
            httpListener.BeginGetContext(new AsyncCallback(IncomingHttpRequest), httpListener);
        }
    }

    private void IncomingHttpRequest(IAsyncResult result)
    {
        HttpListener httpListener = (HttpListener)result.AsyncState;
        HttpListenerContext httpContext = httpListener.EndGetContext(result);
        HttpListenerRequest httpRequest = httpContext.Request;

        _token = httpRequest.QueryString.Get("code");

        HttpListenerResponse httpResponse = httpContext.Response;
        string responseString = "<html><body><b>DONE!</b><br>(You can close this tab/window now)</body></html>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        httpResponse.ContentLength64 = buffer.Length;
        System.IO.Stream output = httpResponse.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();

        httpListener.Stop();
        httpListener = null;
        _doExchangeCode = true;
    }

    private async Task PerformSignIn()
    {
        try
        {
            var supabase = await ConnectSupabase();
            var providerAuth = (await supabase.Auth.SignIn(Constants.Provider.Google, new SignInOptions
            {
                FlowType = Constants.OAuthFlowType.PKCE,
            }))!;
            _pkce = providerAuth.PKCEVerifier;

            StartLocalWebserver();
            Application.OpenURL(providerAuth.Uri.ToString());
        }
        catch (GotrueException goTrueException)
        {
            infoText.text = $"{goTrueException.Reason} {goTrueException.Message}";
            infoText.color = Color.red;
            Debug.Log(goTrueException.Message, gameObject);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message, gameObject);
        }
    }

    private async Task PerformExchangeCode()
    {
        try
        {
            var supabase = await ConnectSupabase();
            Session session = (await supabase.Auth.ExchangeCodeForSession(_pkce, _token)!);
            infoText.text = $"Success! Signed Up as {session.User?.Email}";
            infoText.color = Color.green;
        }
        catch (GotrueException goTrueException)
        {
            infoText.text = $"{goTrueException.Reason} {goTrueException.Message}";
            infoText.color = Color.red;
            Debug.Log(goTrueException.Message, gameObject);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message, gameObject);
        }
    }

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
            }
        }
        catch (Exception e)
        {
            Debug.LogError("LogIn failed: " + e.Message);
            infoText.text = "LogIn failed!";
            infoText.color = Color.red;
        }
    }
}