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
        private const string JugadorIdKey = "jugador_id";

        public async Task<bool> LogIn(string email, string password, TMP_Text infoText)
        {
            try
            {
                var supabase = await _supabaseManager.GetClient();
                var response = await supabase.Auth.SignIn(email, password);

                if (response is { User: not null })
                {
                    Debug.Log("Inicio de sesión exitoso!");
                    if (infoText != null)
                    {
                        infoText.text = "Inicio de sesión exitoso!";
                        infoText.color = Color.green;
                    }

                    SaveSession(response);
                    await SyncTemporalStepsWithPlayer(response.User.Id);
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Debug.LogError("Inicio de sesión fallido: " + e.Message);
                if (infoText != null)
                {
                    infoText.text = "¡Inicio de sesión fallido!";
                    infoText.color = Color.red;
                }

                return false;
            }
        }

        public async Task<bool> CheckForSession()
        {
            try
            {
                var supabase = await _supabaseManager.GetClient();
                var session = await supabase.Auth.RetrieveSessionAsync();

                if (session != null && session.User != null)
                {
                    Debug.Log("Sesión activa encontrada. Usuario ya conectado: " + session.User.Email);
                    SaveSession(session);
                    await CheckPlayerExists(session.User.Id);
                    await SyncTemporalStepsWithPlayer(session.User.Id);
                    return true;
                }
                else
                {
                    string storedUserId = PlayerPrefs.GetString(UserIdKey, null);
                    if (!string.IsNullOrEmpty(storedUserId))
                    {
                        Debug.Log("Usando sesión guardada para el ID de usuario: " + storedUserId);
                        await CheckPlayerExists(storedUserId);
                        await SyncTemporalStepsWithPlayer(storedUserId);
                        return true;
                    }

                    Debug.Log("No se encontró ninguna sesión activa");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error al verificar la sesión: " + e.Message);
                string storedUserId = PlayerPrefs.GetString(UserIdKey, null);
                if (!string.IsNullOrEmpty(storedUserId))
                {
                    Debug.Log("Usando la sesión guardada como respaldo para el ID de usuario: " + storedUserId);
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
                Debug.Log("Sesión guardada para: " + session.User.Email);
            }
        }

        public void ClearSession()
        {
            PlayerPrefs.DeleteKey(UserIdKey);
            PlayerPrefs.DeleteKey(EmailKey);
            PlayerPrefs.DeleteKey(SessionKey);
            PlayerPrefs.DeleteKey(JugadorIdKey);
            PlayerPrefs.Save();
            Debug.Log("Sesión borrada");
        }

        public string GetCurrentUserId() => PlayerPrefs.GetString(UserIdKey, null);

        public string GetCurrentUserEmail() => PlayerPrefs.GetString(EmailKey, null);

        public async Task<bool> SignOut()
        {
            try
            {
                var supabase = await _supabaseManager.GetClient();
                await supabase.Auth.SignOut();
                ClearSession();
                Debug.Log("Usuario desconectado exitosamente");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Error al desconectar: " + e.Message);
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
                    Debug.Log("El jugador no existe, creando jugador y usuario...");
                    string defaultName = "Player" + DateTime.Now.Ticks % 100000;
                    int pasosTotales = await GetTemporalStepsData();

                    Guid userGuid = Guid.Parse(userId);

                    var model = new Jugador
                    {
                        Nombre = defaultName,
                        PasosTotales = pasosTotales,
                        IdUsuario = userGuid,
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

                    Debug.Log($"Jugador creado: {response2.Models[0].Nombre} con {pasosTotales} pasos");
                    PlayerPrefs.SetInt(JugadorIdKey, response2.Models[0].IdJugador);
                    Debug.Log("Ciudad creada: " + response3.Models[0].IdCiudad);
                }
                else
                {
                    Debug.Log("El jugador existe: " + response.Models[0].Nombre);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error al verificar el jugador: " + e.Message);
            }
        }

        private async Task<int> GetTemporalStepsData()
        {
            try
            {
                string androidId = PlayerPrefs.GetString("AndroidId");
                if (string.IsNullOrEmpty(androidId))
                {
                    EncryptId encryptId = new EncryptId();
                    androidId = encryptId.EncryptAndroidId(encryptId.GetAndroidId());
                    PlayerPrefs.SetString("AndroidId", androidId);
                }

                var supabase = await _supabaseManager.GetClient();
                var response = await supabase.From<TemporalData>().Select("pasos_totales")
                    .Filter("android_id", Constants.Operator.Equals, androidId).Get();

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
                var response = await supabase.From<Jugador>().Select("id_jugador, pasos_totales")
                    .Filter("id_usuario", Constants.Operator.Equals, userId).Get();

                if (response.Models.Count > 0)
                {
                    var jugador = response.Models[0];

                    if (pasosTotalesTemporales > jugador.PasosTotales)
                    {
                        Debug.Log(
                            $"Actualizando pasos del jugador de {jugador.PasosTotales} a {pasosTotalesTemporales}");
                        jugador.PasosTotales = pasosTotalesTemporales;
                        await supabase.From<Jugador>().Update(jugador);
                        Debug.Log("Pasos del jugador actualizados correctamente");
                    }
                    else
                    {
                        Debug.Log(
                            $"El jugador ya tiene más pasos ({jugador.PasosTotales}) que los temporales ({pasosTotalesTemporales})");
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
            SceneManager.LoadScene("ClanPrototipo");
        }
    }
}