using System;
using Models;
using Services;
using TMPro;
using UnityEngine;
using Utils;

public class CollectTestData : MonoBehaviour
{
    public TMP_Text text;
    public TMP_Text stepsText;
    private String _androidId;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Check if the Android ID is already stored in PlayerPrefs
        if (PlayerPrefs.HasKey("AndroidId") || 
            PlayerPrefs.GetString("AndroidId") != null)
        {
            _androidId = PlayerPrefs.GetString("AndroidId");
            Debug.Log("Stored Android ID: " + _androidId);
            text.text = _androidId;
            return;
        }

        EncryptId encryptId = new EncryptId();
        _androidId = encryptId.EncryptAndroidId(encryptId.GetAndroidId());
        Debug.Log("Encrypted Android ID: " + _androidId);
        text.text = _androidId;
        PlayerPrefs.SetString("AndroidId", _androidId);

        GetTempData();
    }

    private async void GetTempData()
    {
        try
        {
            var supabase = await SupabaseManager.Instance.GetClient();
            var response = await supabase.From<TemporalData>()
                .Select("id, android_id, pasos_totales, nuevos_pasos_sync")
                .Filter("android_id", Supabase.Postgrest.Constants.Operator.Equals, _androidId)
                .Get();

            if (response.Models.Count > 0)
            {
                var tempData = response.Models[0];
                Debug.Log($"Datos encontrados - Pasos totales: {tempData.PasosTotales}, Nuevos pasos: {tempData.NuevosPasosSync}");
                stepsText.text = $"Pasos totales: {tempData.PasosTotales}, Pasos totales: {tempData.PasosTotales}, Nuevos pasos: {tempData.NuevosPasosSync}";
            }
            else
            {
                Debug.Log("No se encontraron datos para este dispositivo");
                var newTempData = new TemporalData
                {
                    AndroidId = _androidId,
                    PasosTotales = 0,
                    NuevosPasosSync = 0
                };

                await supabase.From<TemporalData>().Insert(newTempData);
                Debug.Log("Nuevo registro creado en temporal_data");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al obtener datos temporales: {e.Message}");
        }
    }
}