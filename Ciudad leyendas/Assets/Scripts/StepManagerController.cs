using System;
using UnityEngine;
using Models;
using System.Threading.Tasks;
using Supabase;
using Supabase.Postgrest;
using TMPro;
using UnityEngine.UI;
using Client = Supabase.Client;

public class StepManagerController : MonoBehaviour
{
    public TMP_Text totalStepsText;
    public TMP_Text recentStepsText;

    private int _totalSteps;
    private int _recentSteps;
    private string _userId;

    void Start()
    {
        // Get user ID from session
        CheckForSession();
        
        // Fetch initial step data
        FetchStepData();
        
        // Set up periodic update (every 30 seconds)
        InvokeRepeating(nameof(FetchStepData), 30f, 30f);
    }

    private async void CheckForSession()
    {
        try
        {
            var supabase = await ConnectSupabase();
            var session = await supabase.Auth.RetrieveSessionAsync();

            if (session != null && session.User != null)
            {
                _userId = session.User.Id;
                Debug.Log("User ID retrieved: " + _userId);
            }
            else
            {
                Debug.LogError("No active session found");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error checking session: " + e.Message);
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
            var supabase = await ConnectSupabase();
            var response = await supabase.From<Jugador>()
                .Filter("id_usuario", Constants.Operator.Equals, _userId)
                .Get();
                
            if (response.Models.Count > 0)
            {
                var jugador = response.Models[0];
                jugador.PasosTotales = _totalSteps;
                
                await supabase.From<Jugador>().Update(jugador);
                Debug.Log("Steps data synced with backend");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error syncing steps with backend: " + e.Message);
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