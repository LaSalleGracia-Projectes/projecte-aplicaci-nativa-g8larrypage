using System;
using TMPro;
using UnityEngine;
using Services;
using static Supabase.Postgrest.Constants;
using Models;
using System.Threading.Tasks;

public class PasosDeOroUI : MonoBehaviour
{
    public static PasosDeOroUI Instance { get; private set; }

    public TextMeshProUGUI pasosText;
    public float updateInterval = 5f;

    private const string JugadorIdKey = "jugador_id";

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Mantener entre escenas
    }

    private async void Start()
    {
        if (pasosText == null)
        {
            Debug.LogError("No se ha asignado el TextMeshProUGUI para mostrar los pasos.");
            return;
        }

        await UpdatePasosUI();
        InvokeRepeating(nameof(SafeUpdatePasos), updateInterval, updateInterval);
    }

    public async void SafeUpdatePasos()
    {
        await UpdatePasosUI();
    }

    private async Task UpdatePasosUI()
    {
        try
        {
            int storedJugadorId = PlayerPrefs.GetInt(JugadorIdKey, -1);
            if (storedJugadorId == -1)
            {
                Debug.LogWarning("No se encontró el jugador_id en PlayerPrefs.");
                pasosText.text = "0";
                return;
            }

            var client = await SupabaseManager.Instance.GetClient();

            var jugadorResponse = await client
                .From<Jugador>()
                .Select("pasos_totales")
                .Filter("id_jugador", Operator.Equals, storedJugadorId)
                .Get();

            if (jugadorResponse.Models.Count > 0)
            {
                var jugador = jugadorResponse.Models[0];
                pasosText.text = jugador.PasosTotales.ToString("N0");
                Debug.Log($"Pasos totales del jugador {storedJugadorId}: {jugador.PasosTotales}");
            }
            else
            {
                Debug.LogWarning($"No se encontró al jugador con id_jugador: {storedJugadorId}");
                pasosText.text = "0";
            }
        }
        catch (Exception ex)
        {
            pasosText.text = "0";
            Debug.LogError($"Error actualizando los pasos de oro: {ex.Message}");
        }
    }

    public async Task<int> ObtenerPasosTotales()
    {
        int storedJugadorId = PlayerPrefs.GetInt(JugadorIdKey, -1);
        if (storedJugadorId == -1)
            return 0;

        var client = await SupabaseManager.Instance.GetClient();
        var jugadorResponse = await client
            .From<Jugador>()
            .Select("pasos_totales")
            .Filter("id_jugador", Operator.Equals, storedJugadorId)
            .Get();

        if (jugadorResponse.Models.Count > 0)
        {
            var jugador = jugadorResponse.Models[0];
            return jugador.PasosTotales;
        }

        return 0;
    }

    public async Task DescontarPasos(int cantidad)
    {
        int storedJugadorId = PlayerPrefs.GetInt(JugadorIdKey, -1);
        if (storedJugadorId == -1) return;

        var client = await SupabaseManager.Instance.GetClient();
        var jugadorResponse = await client
            .From<Jugador>()
            .Select("*")
            .Filter("id_jugador", Operator.Equals, storedJugadorId)
            .Get();

        if (jugadorResponse.Models.Count > 0)
        {
            var jugador = jugadorResponse.Models[0];
            jugador.PasosTotales -= cantidad;
            await client.From<Jugador>().Update(jugador);
            await UpdatePasosUI(); // Refresca visualmente el texto
        }
    }
}
