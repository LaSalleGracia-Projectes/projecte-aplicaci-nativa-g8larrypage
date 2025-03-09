using System;
using UnityEngine;
using Models;
using System.Threading.Tasks;
using Services;
using Supabase;
using Supabase.Postgrest;
using TMPro;
using UnityEngine.UI;
using Client = Supabase.Client;

public class StepManagerController : MonoBehaviour
{
    private Client _supabase;
    private LoginManager _loginManager;
    
    public TMP_Text totalStepsText;
    public TMP_Text recentStepsText;

    private int _totalSteps;
    private int _recentSteps;
    private string _userId;
    private Jugador _jugador;

    void Start()
    {
        CheckForSession();
        
        // Fetch initial step data
        FetchStepData();
        
        // Set up periodic update (every 30 seconds)
        InvokeRepeating(nameof(FetchStepData), 30f, 30f);
    }

    private async void CheckForSession()
    {
        _loginManager = new LoginManager();
        bool hasSession = await _loginManager.CheckForSession();
    
        if (hasSession)
        {
            _userId = _loginManager.GetCurrentUserId();
            Debug.Log($"Retrieved user session for ID: {_userId}");
        
            // Initialize Supabase client for future API calls
            var supabaseManager = SupabaseManager.Instance;
            _supabase = await supabaseManager.GetClient();
        }
        else
        {
            Debug.LogWarning("No active session found. Step syncing will be disabled.");
        }
    }

    private void FetchStepData()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver"))
                {
                    AndroidJavaObject uri = new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>(
                        "parse", "content://com.ciudad.leyendas.provider/syncdata");

                    AndroidJavaObject cursor = contentResolver.Call<AndroidJavaObject>("query", 
                        uri, null, null, null, null);

                    if (cursor.Call<bool>("moveToFirst"))
                    {
                        int totalStepsColumnIndex = cursor.Call<int>("getColumnIndex", "total_steps");
                        int recentStepsColumnIndex = cursor.Call<int>("getColumnIndex", "recent_steps");

                        if (totalStepsColumnIndex >= 0 && recentStepsColumnIndex >= 0)
                        {
                            _totalSteps = cursor.Call<int>("getInt", totalStepsColumnIndex);
                            _recentSteps = cursor.Call<int>("getInt", recentStepsColumnIndex);
                            
                            Debug.Log($"Steps data retrieved - Total: {_totalSteps}, Recent: {_recentSteps}");
                            
                            // Update UI
                            UpdateUI();
                            
                            // Sync with backend
                            SyncStepsWithBackend();
                        }
                    }
                    
                    cursor.Call("close");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error fetching steps data: " + e.Message);
            }
        }
        else
        {
            Debug.Log("Step counting only available on Android");
            // For testing on non-Android platforms
            _totalSteps = 1000;
            _recentSteps = 100;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (totalStepsText != null)
            totalStepsText.text = $"Total Steps: {_totalSteps}";
            
        if (recentStepsText != null)
            recentStepsText.text = $"Recent Steps: {_recentSteps}";
    }

    private async void SyncStepsWithBackend()
    {
        if (string.IsNullOrEmpty(_userId))
            return;
            
        try
        {
            var response = await _supabase.From<Jugador>()
                .Filter("id_usuario", Constants.Operator.Equals, _userId)
                .Get();
                
            if (response.Models.Count > 0)
            {
                var jugador = response.Models[0];
                jugador.PasosTotales = _totalSteps;
                
                await _supabase.From<Jugador>().Update(jugador);
                Debug.Log("Steps data synced with backend");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error syncing steps with backend: " + e.Message);
        }
    }
}