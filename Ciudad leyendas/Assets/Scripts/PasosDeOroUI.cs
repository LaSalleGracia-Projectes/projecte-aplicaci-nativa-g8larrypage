using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Models;
using Services;
using static Supabase.Postgrest.Constants;

public class PasosDeOroUI : MonoBehaviour
{
    public TextMeshProUGUI pasosText;
    public float updateInterval = 5f;

    private const string JugadorIdKey = "jugador_id";

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

    private async void SafeUpdatePasos()
    {
        await UpdatePasosUI();
    }

    private async Task UpdatePasosUI()
    {
        try
        {
            // Obtener el id_jugador como int desde PlayerPrefs
            int storedJugadorId = PlayerPrefs.GetInt(JugadorIdKey, -1);  // -1 es el valor por defecto si no se encuentra

            if (storedJugadorId == -1)
            {
                Debug.LogWarning("No se encontró el jugador_id en PlayerPrefs.");
                pasosText.text = "0";
                return;
            }

            var client = await SupabaseManager.Instance.GetClient();

            // Obtener el jugador con ese id_jugador
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
}
