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
    public int ciudadId = 2; // ID de la ciudad actual
    public float updateInterval = 5f;

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
            var client = await SupabaseManager.Instance.GetClient();

            // Obtener la ciudad correspondiente
            var ciudadResponse = await client
                .From<Ciudad>()
                .Select("*")
                .Filter("id_ciudad", Operator.Equals, ciudadId)
                .Get();

            if (ciudadResponse.Models.Count == 0)
            {
                Debug.LogWarning($"No se encontró la ciudad con id: {ciudadId}");
                pasosText.text = "0";
                return;
            }

            var ciudad = ciudadResponse.Models[0];
            int idJugador = ciudad.IdJugador;

            // Obtener el jugador dueño de la ciudad
            var jugadorResponse = await client
                .From<Jugador>()
                .Select("pasos_totales")
                .Filter("id_jugador", Operator.Equals, idJugador)
                .Get();

            if (jugadorResponse.Models.Count > 0)
            {
                var jugador = jugadorResponse.Models[0];
                pasosText.text = jugador.PasosTotales.ToString("N0");
                Debug.Log($"Pasos totales del jugador {idJugador}: {jugador.PasosTotales}");
            }
            else
            {
                Debug.LogWarning($"No se encontró al jugador con id: {idJugador}");
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
