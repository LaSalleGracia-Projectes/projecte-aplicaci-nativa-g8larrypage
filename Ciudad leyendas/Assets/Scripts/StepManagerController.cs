using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Models;
using Services;
using Supabase.Postgrest;
using TMPro;
using UnityEngine.Android;
using Client = Supabase.Client;

public class StepManagerController : MonoBehaviour
{
    private Client _supabase;
    private LoginManager _loginManager;

    public TMP_Text totalStepsText;
    public TMP_Text recentStepsText;

    private string _userId;
    private Jugador _jugador;

    [Serializable]
    public class StepsData
    {
        public long totalSteps;
        public long recentSteps;
        public long lastSyncTime;
        public long timestamp;
    }

    public StepsData currentStepsData;
    private string _jsonFilePath;

    void Start()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) &&
            Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            // Permisos concedidos, continuar con la lógica
            Initialize();
        }
        else
        {
            // Solicitar permisos
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
    }

    void Initialize()
    {
        CheckForSession();

        _jsonFilePath = "/storage/emulated/0/Android/data/com.ciudad.leyendas/files/steps_data.json";
        Debug.Log($"Archivo JSON externo: {_jsonFilePath}");
        StartCoroutine(CheckAndLoadStepsData());
    }

    IEnumerator CheckAndLoadStepsData()
    {
        while (true)
        {
            if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                if (File.Exists(_jsonFilePath))
                {
                    Debug.Log($"Archivo JSON externo: EXISTE en {_jsonFilePath}");
                    string jsonData = File.ReadAllText(_jsonFilePath);
                    currentStepsData = JsonUtility.FromJson<StepsData>(jsonData);
                    Debug.Log($"Pasos recientes: {currentStepsData.recentSteps}");
                }
                else
                {
                    Debug.LogWarning("Archivo de datos de pasos no encontrado");
                }

                UpdateUI();
            }
            else
            {
                Debug.LogWarning("Permiso de lectura de almacenamiento externo no concedido");
            }

            yield return new WaitForSeconds(5f); // Refrescar cada 5 segundos
        }
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

    private void UpdateUI()
    {
        if (totalStepsText != null)
            totalStepsText.text = $"Total Steps: {currentStepsData.totalSteps}";

        if (recentStepsText != null)
            recentStepsText.text = $"Recent Steps: {currentStepsData.recentSteps}";
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
                jugador.PasosTotales = (int)currentStepsData.totalSteps;

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