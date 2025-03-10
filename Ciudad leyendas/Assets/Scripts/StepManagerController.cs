using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    private string _userId;
    private Jugador _jugador;

    [Serializable]
    public class StepsData
    {
        public long total_steps;
        public long recent_steps;
        public long last_sync_time;
        public long timestamp;
    }

    public StepsData currentStepsData;
    private string jsonFilePath;

    void Start()
    {
        CheckForSession();

        jsonFilePath = Path.Combine(Application.persistentDataPath,
            "../Android/data/com.ciudad.leyendas/files/steps_data.json");
        Debug.Log($"Archivo JSON externo: {jsonFilePath}");
        StartCoroutine(CheckAndLoadStepsData());
    }

    IEnumerator CheckAndLoadStepsData()
    {
        while (true)
        {
            if (File.Exists(jsonFilePath))
            {
                Debug.Log($"Archivo JSON externo: EXISTE en {jsonFilePath}");
                string jsonData = File.ReadAllText(jsonFilePath);
                currentStepsData = JsonUtility.FromJson<StepsData>(jsonData);
                Debug.Log($"Pasos recientes: {currentStepsData.recent_steps}");
            }
            else
            {
                Debug.LogWarning("Archivo de datos de pasos no encontrado");
            }

            UpdateUI();
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
            totalStepsText.text = $"Total Steps: {currentStepsData.total_steps}";

        if (recentStepsText != null)
            recentStepsText.text = $"Recent Steps: {currentStepsData.recent_steps}";
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
                jugador.PasosTotales = (int)currentStepsData.total_steps;

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