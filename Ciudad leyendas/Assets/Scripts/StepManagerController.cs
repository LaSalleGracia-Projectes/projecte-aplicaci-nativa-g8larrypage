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
        // Iniciar el proceso de verificación y solicitud de permisos
        StartCoroutine(CheckAndRequestPermissions());
    }

    IEnumerator CheckAndRequestPermissions()
    {
        bool hasReadPermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
        bool hasWritePermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);

        // Si no tenemos los permisos, los solicitamos
        if (!hasReadPermission)
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
            // Esperar un poco para dar tiempo a que se muestre el diálogo
            yield return new WaitForSeconds(0.5f);
        }

        if (!hasWritePermission)
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            yield return new WaitForSeconds(0.5f);
        }

        // Dar tiempo al usuario para que responda a las solicitudes de permiso
        yield return new WaitForSeconds(1.0f);

        // Verificar si los permisos fueron concedidos
        if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) &&
            Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Debug.Log("Permisos concedidos, inicializando...");
            Initialize();
        }
        else
        {
            Debug.LogWarning("Permisos no concedidos. La aplicación no funcionará correctamente.");
            // Mostrar mensaje al usuario explicando que se necesitan los permisos
            // Puedes añadir un UI para esto
            
            // Seguir intentando hasta conseguir los permisos
            StartCoroutine(RetryPermissionCheck());
        }
    }

    IEnumerator RetryPermissionCheck()
    {
        // Esperar un tiempo y volver a intentar
        yield return new WaitForSeconds(3.0f);
        
        if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) &&
            Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Debug.Log("Permisos finalmente concedidos, inicializando...");
            Initialize();
        }
        else
        {
            // Puedes mostrar un mensaje al usuario o solicitar permisos nuevamente
            Debug.LogWarning("Permisos aún no concedidos. Intentando nuevamente...");
            StartCoroutine(CheckAndRequestPermissions());
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
                try 
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
                        // Inicializar con datos vacíos para evitar errores
                        currentStepsData = new StepsData();
                    }

                    UpdateUI();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error al leer el archivo: {e.Message}");
                }
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