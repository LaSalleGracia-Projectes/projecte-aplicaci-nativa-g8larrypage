using System;
using System.Threading.Tasks;
using Models;
using Supabase.Gotrue;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Constants = Supabase.Postgrest.Constants;

namespace Services
{
    public class LoginManager
    {
        private readonly SupabaseManager _supabaseManager = SupabaseManager.Instance;
        private const string SessionKey = "supabase_session";
        private const string UserIdKey = "user_id";
        private const string EmailKey = "user_email";

        public async Task<bool> LogIn(string email, string password, TMP_Text infoText)
        {
            try
            {
                var supabase = await _supabaseManager.GetClient();
                var response = await supabase.Auth.SignIn(email, password);

                if (response is { User: not null })
                {
                    Debug.Log("LogIn successful!");
                    if (infoText != null)
                    {
                        infoText.text = "LogIn successful!";
                        infoText.color = Color.green;
                    }
                    
                    // Save session data
                    SaveSession(response);
                    
                    // Sincronizar pasos desde datos temporales
                    await SyncTemporalStepsWithPlayer(response.User.Id);
                    
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError("LogIn failed: " + e.Message);
                if (infoText != null)
                {
                    infoText.text = "LogIn failed!";
                    infoText.color = Color.red;
                }
                return false;
            }
        }

        public async Task<bool> CheckForSession()
        {
            try
            {
                // First try to get the session from Supabase
                var supabase = await _supabaseManager.GetClient();
                var session = await supabase.Auth.RetrieveSessionAsync();

                if (session != null && session.User != null)
                {
                    Debug.Log("Live session found. User already logged in: " + session.User.Email);
                    SaveSession(session); // Update stored session with the live one
                    await CheckPlayerExists(session.User.Id);
                    await SyncTemporalStepsWithPlayer(session.User.Id);
                    return true;
                }
                else
                {
                    // If no live session, check if we have a stored session
                    string storedUserId = PlayerPrefs.GetString(UserIdKey, null);
                    if (!string.IsNullOrEmpty(storedUserId))
                    {
                        Debug.Log("Using stored session for user ID: " + storedUserId);
                        await CheckPlayerExists(storedUserId);
                        await SyncTemporalStepsWithPlayer(storedUserId);
                        return true;
                    }
                    
                    Debug.Log("No active session found");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error checking session: " + e.Message);
                
                // If server check fails, try using stored session as fallback
                string storedUserId = PlayerPrefs.GetString(UserIdKey, null);
                if (!string.IsNullOrEmpty(storedUserId))
                {
                    Debug.Log("Using stored session as fallback for user ID: " + storedUserId);
                    await CheckPlayerExists(storedUserId);
                    await SyncTemporalStepsWithPlayer(storedUserId);
                    return true;
                }
                
                return false;
            }
        }

        public void SaveSession(Session session)
        {
            if (session != null && session.User != null)
            {
                PlayerPrefs.SetString(UserIdKey, session.User.Id);
                PlayerPrefs.SetString(EmailKey, session.User.Email);
                PlayerPrefs.SetString(SessionKey, session.AccessToken);
                PlayerPrefs.Save();
                Debug.Log("Session saved for: " + session.User.Email);
            }
        }
        
        public void ClearSession()
        {
            PlayerPrefs.DeleteKey(UserIdKey);
            PlayerPrefs.DeleteKey(EmailKey);
            PlayerPrefs.DeleteKey(SessionKey);
            PlayerPrefs.Save();
            Debug.Log("Session cleared");
        }
        
        public string GetCurrentUserId()
        {
            return PlayerPrefs.GetString(UserIdKey, null);
        }
        
        public string GetCurrentUserEmail()
        {
            return PlayerPrefs.GetString(EmailKey, null);
        }

        public async Task<bool> SignOut()
        {
            try
            {
                var supabase = await _supabaseManager.GetClient();
                await supabase.Auth.SignOut();
                ClearSession();
                Debug.Log("User signed out successfully");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Error signing out: " + e.Message);
                return false;
            }
        }

        private async Task CheckPlayerExists(string userId)
        {
            try
            {
                var supabase = await _supabaseManager.GetClient();
                var response = await supabase.From<Jugador>().Select("nombre, pasos_totales")
                    .Filter("id_usuario", Constants.Operator.Equals, userId).Get();

                if (response.Models.Count == 0)
                {
                    Debug.Log("Player does not exist, creating player...");

                    string defaultName = "Player" + DateTime.Now.Ticks % 100000;
                    
                    // Obtener los pasos temporales para el nuevo jugador
                    int pasosTotales = await GetTemporalStepsData();

                    var model = new Jugador
                    {
                        Nombre = defaultName,
                        PasosTotales = pasosTotales,
                        IdUsuario = Guid.Parse(userId),
                        IdClan = null,
                    };

                    var response2 = await supabase.From<Jugador>().Insert(model);

                    var ciudad = new Ciudad
                    {
                        Nombre = ("Ciudad de " + defaultName),
                        NivelCiudad = 1,
                        IdJugador = response2.Models[0].IdJugador
                    };
                    var response3 = await supabase.From<Ciudad>().Insert(ciudad);
                    
                    Debug.Log($"Player created: {response2.Models[0].Nombre} with {pasosTotales} steps");
                    Debug.Log("City created: " + response3.Models[0].IdCiudad);
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
        
        private async Task<int> GetTemporalStepsData()
        {
            try
            {
                // Obtener el Android ID encriptado
                string androidId = PlayerPrefs.GetString("AndroidId");
                if (string.IsNullOrEmpty(androidId))
                {
                    EncryptId encryptId = new EncryptId();
                    androidId = encryptId.EncryptAndroidId(encryptId.GetAndroidId());
                    PlayerPrefs.SetString("AndroidId", androidId);
                }
                
                // Buscar datos temporales para este dispositivo
                var supabase = await _supabaseManager.GetClient();
                var response = await supabase.From<TemporalData>()
                    .Select("pasos_totales")
                    .Filter("android_id", Constants.Operator.Equals, androidId)
                    .Get();

                if (response.Models.Count > 0)
                {
                    return response.Models[0].PasosTotales;
                }
                
                return 0;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error obteniendo datos temporales: {e.Message}");
                return 0;
            }
        }
        
        private async Task SyncTemporalStepsWithPlayer(string userId)
        {
            try
            {
                int pasosTotalesTemporales = await GetTemporalStepsData();
                
                if (pasosTotalesTemporales <= 0)
                {
                    Debug.Log("No hay pasos temporales para sincronizar");
                    return;
                }
                
                var supabase = await _supabaseManager.GetClient();
                var response = await supabase.From<Jugador>()
                    .Select("id_jugador, pasos_totales")
                    .Filter("id_usuario", Constants.Operator.Equals, userId)
                    .Get();

                if (response.Models.Count > 0)
                {
                    var jugador = response.Models[0];
                    
                    if (pasosTotalesTemporales > jugador.PasosTotales)
                    {
                        Debug.Log($"Actualizando pasos del jugador de {jugador.PasosTotales} a {pasosTotalesTemporales}");
                        jugador.PasosTotales = pasosTotalesTemporales;
                        await supabase.From<Jugador>().Update(jugador);
                        Debug.Log("Pasos del jugador actualizados correctamente");
                    }
                    else
                    {
                        Debug.Log($"El jugador ya tiene más pasos ({jugador.PasosTotales}) que los temporales ({pasosTotalesTemporales})");
                    }
                }
                else
                {
                    Debug.LogWarning("No se encontró el jugador al intentar sincronizar pasos");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error sincronizando pasos temporales: {e.Message}");
            }
        }

        public void GoToGame()
        {
            SceneManager.LoadScene("CollectStepsTest");
        }
    }
}