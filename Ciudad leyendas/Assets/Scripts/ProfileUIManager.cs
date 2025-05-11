using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Models;
using Services;
using System.Threading.Tasks;

public class ProfileUIManager : MonoBehaviour
{
    public TMP_Text infoPlayerText;
    public Image profileImage;
    public TMP_InputField playerName;

    private readonly SupabaseManager _supabaseManager = SupabaseManager.Instance;
    private Jugador _jugador;
    public GameObject profileUI;

    public GameObject ajustes;
    public GameObject gridManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ajustes.SetActive(false);
        gridManager.SetActive(false);
        
        GetPlayerData();
    }

    private async void GetPlayerData()
    {
        try
        {
            int jugadorId = PlayerPrefs.GetInt("jugador_id", 0);

            if (jugadorId == 0)
            {
                Debug.LogError("No se encontró ID de jugador en PlayerPrefs");
                if (infoPlayerText != null)
                {
                    infoPlayerText.text = "Error: No se pudo cargar la información del jugador";
                }

                return;
            }

            _jugador = await GetJugador(jugadorId);

            if (_jugador != null)
            {
                // Obtener información del clan si existe
                string clanInfo = "No está en un clan";
                int clanId = PlayerPrefs.GetInt("IdClan", 0);

                if (clanId > 0)
                {
                    var clan = await GetClan(clanId);
                    if (clan != null)
                    {
                        clanInfo = clan.Nombre;
                    }
                }

                // Actualizar la interfaz con los datos del jugador
                if (infoPlayerText != null)
                {
                    infoPlayerText.text =
                        $"XP: {_jugador.Experiencia}\n\n" +
                        $"Pasos: {_jugador.PasosTotales}\n\n" +
                        $"Clan: {clanInfo}";
                }

                if (playerName != null)
                {
                    playerName.text = _jugador.Nombre;
                }

                // Cargar avatar desde DiceBear
                if (profileImage != null)
                {
                    await LoadProfileImage(_jugador.Nombre);
                }

                Debug.Log($"Datos del jugador cargados: {_jugador.Nombre}, Clan: {clanInfo}");
            }
            else
            {
                Debug.LogError("No se pudo obtener información del jugador");
                if (infoPlayerText != null)
                {
                    infoPlayerText.text = "Error: No se pudo cargar la información del jugador";
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al cargar datos del jugador: {ex.Message}");
            if (infoPlayerText != null)
            {
                infoPlayerText.text = "Error al cargar datos del jugador";
            }
        }
    }

    private async Task LoadProfileImage(string nombreJugador)
    {
        try
        {
            string url = $"https://api.dicebear.com/9.x/bottts/jpg?seed={nombreJugador}";

            using (var www = UnityEngine.Networking.UnityWebRequest.Get(url))
            {
                www.downloadHandler = new UnityEngine.Networking.DownloadHandlerTexture(true);

                await www.SendWebRequest();

                if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error al descargar imagen de perfil: {www.error}");
                    return;
                }

                var texture = ((UnityEngine.Networking.DownloadHandlerTexture)www.downloadHandler).texture;
                var sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                profileImage.sprite = sprite;
            }

            Debug.Log($"Imagen de perfil cargada para: {nombreJugador}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al cargar imagen de perfil: {ex.Message}");
        }
    }

    private async Task<Clan> GetClan(int clanId)
    {
        try
        {
            var supabase = await _supabaseManager.GetClient();
            var response = await supabase.From<Clan>()
                .Filter("id_clan", Supabase.Postgrest.Constants.Operator.Equals, clanId)
                .Get();

            if (response.Models.Count > 0)
            {
                return response.Models[0];
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al obtener clan: {ex.Message}");
            return null;
        }
    }

    private async Task<Jugador> GetJugador(int jugadorId)
    {
        try
        {
            var supabase = await _supabaseManager.GetClient();
            var response = await supabase.From<Jugador>()
                .Filter("id_jugador", Supabase.Postgrest.Constants.Operator.Equals, jugadorId)
                .Get();

            if (response.Models.Count > 0)
            {
                return response.Models[0];
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al obtener jugador: {ex.Message}");
            return null;
        }
    }

    public async void SignOut()
    {
        try
        {
            // Obtener cliente de Supabase
            var supabase = await _supabaseManager.GetClient();

            // Cerrar sesión en Supabase
            await supabase.Auth.SignOut();

            // Borrar todos los valores importantes de PlayerPrefs
            PlayerPrefs.DeleteKey("jugador_id");
            PlayerPrefs.DeleteKey("NombreJugador");
            PlayerPrefs.DeleteKey("IdClan");
            PlayerPrefs.DeleteKey("user_id");
            PlayerPrefs.DeleteKey("user_email");
            PlayerPrefs.DeleteKey("supabase_session");
            PlayerPrefs.DeleteKey("AndroidId");

            // Asegurarse de que los cambios se guarden
            PlayerPrefs.Save();

            Debug.Log("Sesión cerrada exitosamente");

            // Pausa para mejorar la experiencia de usuario
            await Task.Delay(800);

            // Cargar la escena de login
            UnityEngine.SceneManagement.SceneManager.LoadScene("LogInRegister");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al cerrar sesión: {ex.Message}");
        }
    }

    public async void CambiarNombreJugador()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(playerName.text))
            {
                Debug.LogError("El nombre no puede estar vacío");
                return;
            }

            string nuevoNombre = playerName.text;
            int jugadorId = PlayerPrefs.GetInt("jugador_id", 0);

            if (jugadorId == 0)
            {
                Debug.LogError("No se encontró ID de jugador en PlayerPrefs");
                return;
            }

            // Obtener cliente de Supabase
            var supabase = await _supabaseManager.GetClient();

            // Primero obtenemos el jugador actual
            var jugador = await GetJugador(jugadorId);
            if (jugador == null)
            {
                Debug.LogError("No se pudo obtener el jugador actual");
                return;
            }

            // Actualizamos el nombre
            jugador.Nombre = nuevoNombre;

            // Enviamos la actualización a Supabase
            var response = await supabase.From<Jugador>().Update(jugador);

            if (response.Models.Count > 0)
            {
                Debug.Log($"Nombre cambiado exitosamente a: {nuevoNombre}");

                // Actualizar el valor en PlayerPrefs
                PlayerPrefs.SetString("NombreJugador", nuevoNombre);
                PlayerPrefs.Save();

                // Actualizar la imagen de perfil con el nuevo nombre
                await LoadProfileImage(nuevoNombre);

                // Recargar los datos del jugador para refrescar la interfaz
                GetPlayerData();
            }
            else
            {
                Debug.LogError("Error al cambiar el nombre del jugador");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al cambiar el nombre del jugador: {ex.Message}");
        }
    }

    /// <summary>
    /// Desactiva completamente la interfaz del perfil de usuario
    /// </summary>
    public void DisableProfileUI()
    {
        // Desactivar la UI del perfil
        profileUI.SetActive(false);

        // Reactivar elementos de la interfaz principal
        gridManager.SetActive(true);
        ajustes.gameObject.SetActive(true);
    }
}