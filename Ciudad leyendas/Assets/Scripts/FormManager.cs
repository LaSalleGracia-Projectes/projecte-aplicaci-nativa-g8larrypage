using System;
using System.Threading.Tasks;
using Supabase;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FormManager : MonoBehaviour
{
    [Header("LogIn Form")]
    public GameObject logInForm;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button logInButton;
    public TMP_Text infoText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        infoText.text = "";
        logInButton.onClick.AddListener(LogIn);
    }


    // Update is called once per frame
    void Update()
    {
        
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
                // Add additional logic for successful login
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
