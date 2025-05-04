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

    private bool settingsOpen = false;

    void Start()
    {
        settingsPanel.SetActive(false);
        closeButton.onClick.AddListener(CloseSettings);

        if (volumeSlider != null)
        {
            //volumeSlider.value = BackgroundMusic.Instance.GetComponent<AudioSource>().volume;
            volumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }
    }

    public async void OpenSettings()
    {
        settingsPanel.SetActive(true);
        gridManager.SetActive(false);
        ajustesButton.SetActive(false);
        settingsOpen = true;

        await DisplayPlayerName();
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        gridManager.SetActive(true);
        ajustesButton.SetActive(true);
        settingsOpen = false;
    }

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

    void SetMusicVolume(float volume)
    {
        BackgroundMusic.Instance.SetVolume(volume);
    }

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
