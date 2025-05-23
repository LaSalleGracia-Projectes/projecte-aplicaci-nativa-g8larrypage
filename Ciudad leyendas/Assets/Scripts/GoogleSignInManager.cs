﻿using System;
using System.Net;
using System.Threading.Tasks;
using Services;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using TMPro;
using UnityEngine;
using Client = Supabase.Client;

public class GoogleSignInManager : MonoBehaviour
{
    public TMP_Text infoText;
    private bool _doSignIn;
    private bool _doExchangeCode;
    private HttpListener httpListener;
    private string _pkce;

    private string _token;

    // Cambia de público a privado para seguir buenas prácticas
    private LoginManager _loginManager;

// Agrega un método para recibir la referencia
    public void SetLoginManager(LoginManager loginManager)
    {
        _loginManager = loginManager;
    }

// O alternativamente, inicialízalo en Awake
    private void Awake()
    {
        if (_loginManager == null)
        {
            _loginManager = new LoginManager();
        }
    }

    public void SignInWithGoogle()
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
            infoText.text = $"Success! Signed Up as {session!.User?.Email}";
            infoText.color = Color.green;

            // Guarda la sesión
            _loginManager.SaveSession(session);

            // Realiza las mismas comprobaciones que en LogIn
            if (session.User != null)
            {
                // Comprueba si el jugador existe y sincroniza los pasos temporales
                await _loginManager.CheckPlayerExists(session.User.Id);
                await _loginManager.SyncTemporalStepsWithPlayer(session.User.Id);
            }

            // Navega al juego después de completar todo
            _loginManager.GoToGame();
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
}