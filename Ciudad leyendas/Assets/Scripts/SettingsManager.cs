using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using Models;
using Services;

public class SettingsManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject gridManager;
    public GameObject ajustesButton;
    public Button closeButton;
    public Slider volumeSlider;
    public TextMeshProUGUI playerNameText;
    public TMP_InputField playerNameInputField;
    public Button confirmNameButton;

    private bool settingsOpen = false;

    void Start()
    {
        // Asegurarse de que el panel de configuración esté inicialmente oculto
        settingsPanel.SetActive(false);

        // Botón de cerrar
        closeButton.onClick.AddListener(CloseSettings);

        // Slider de volumen
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(SetMusicVolume);

        // Ocultar input de nombre y botón de confirmación al inicio
        if (playerNameInputField != null)
            playerNameInputField.gameObject.SetActive(false);

        if (confirmNameButton != null)
        {
            confirmNameButton.gameObject.SetActive(false);
            confirmNameButton.onClick.AddListener(OnConfirmNameClicked);
        }

        // Añadir listener para editar nombre al hacer clic en el texto del nombre
        if (playerNameText != null)
        {
            Button textButton = playerNameText.GetComponent<Button>();
            if (textButton != null)
            {
                textButton.onClick.AddListener(EnableNameEditing);
                Debug.Log("Botón de nombre añadido correctamente.");
            }
            else
            {
                Debug.LogError("playerNameText no tiene un componente Button.");
            }
        }
    }

    // Método para activar la edición del nombre
    private void EnableNameEditing()
    {
        // Mostrar el InputField y el botón de confirmación
        playerNameInputField.gameObject.SetActive(true);
        confirmNameButton.gameObject.SetActive(true);

        // Copiar el nombre actual al InputField
        string currentName = playerNameText.text.Replace("Jugador: ", "");
        playerNameInputField.text = currentName;

        Debug.Log("Habilitando edición de nombre...");
    }

    // Método cuando el jugador confirma el nuevo nombre
    public async void OnConfirmNameClicked()
    {
        int storedJugadorId = PlayerPrefs.GetInt("jugador_id", -1);
        if (storedJugadorId == -1)
        {
            Debug.LogError("ID del jugador no encontrado en PlayerPrefs (clave: 'jugador_id').");
            return;
        }

        string newName = playerNameInputField.text.Trim();
        if (string.IsNullOrEmpty(newName))
        {
            Debug.LogWarning("El nombre no puede estar vacío.");
            return;
        }

        try
        {
            var client = await SupabaseManager.Instance.GetClient();
            var response = await client.From<Jugador>().Where(j => j.IdJugador == storedJugadorId).Get();

            if (response.Models.Count > 0)
            {
                var jugador = response.Models[0];
                jugador.Nombre = newName;
                await client.From<Jugador>().Upsert(jugador);

                Debug.Log("Nombre actualizado correctamente.");

                // Actualizar UI
                playerNameText.text = $"Jugador: {newName}";
                playerNameInputField.gameObject.SetActive(false);
                confirmNameButton.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("No se encontró jugador con ese ID para actualizar.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al actualizar el nombre del jugador: {ex.Message}");
        }
    }

    // Método para abrir el panel de configuración
    public async void OpenSettings()
    {
        settingsPanel.SetActive(true);
        gridManager.SetActive(false);
        ajustesButton.SetActive(false);
        settingsOpen = true;

        await DisplayPlayerName();
    }

    // Método para cerrar el panel de configuración
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        gridManager.SetActive(true);
        ajustesButton.SetActive(true);
        settingsOpen = false;
    }

    // Método de actualización continua (si se necesita para ocultar el panel al hacer clic fuera)
    void Update()
    {
        if (settingsOpen && Input.GetMouseButtonDown(0))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                settingsPanel.GetComponent<RectTransform>(),
                Input.mousePosition,
                Camera.main))
            {
                CloseSettings();
            }
        }
    }

    // Método para cambiar el volumen de la música
    void SetMusicVolume(float volume)
    {
        BackgroundMusic.Instance.SetVolume(volume);
    }

    // Método para mostrar el nombre del jugador desde Supabase
    private async Task DisplayPlayerName()
    {
        int storedJugadorId = PlayerPrefs.GetInt("jugador_id", -1);
        if (storedJugadorId == -1)
        {
            Debug.LogError("ID del jugador no encontrado en PlayerPrefs (clave: 'jugador_id').");
            return;
        }

        try
        {
            var client = await SupabaseManager.Instance.GetClient();
            var response = await client.From<Jugador>().Where(j => j.IdJugador == storedJugadorId).Get();

            if (response.Models.Count > 0)
            {
                string playerName = response.Models[0].Nombre;
                if (playerNameText != null)
                {
                    playerNameText.text = $"Jugador: {playerName}";
                }
            }
            else
            {
                Debug.LogWarning("No se encontró jugador con ese ID.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al obtener el nombre del jugador: {ex.Message}");
        }
    }
}
