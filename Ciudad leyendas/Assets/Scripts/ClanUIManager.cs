using System.Collections;
using System.Linq;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClanUIManager : MonoBehaviour
{
    [Header ("Search Clan Menu Stuff")]
    public TMP_InputField ClanCodeInputField;
    public TMP_Text searchResultText;
    
    [Header("Create Clan Menu Stuff")]
    public GameObject createClanMenu;
    public TMP_InputField clanNameCreate;
    public Button btnOpenCreateClanMenu;
    public TMP_Text textInfoClanMenu;

    private ClanServices clanServices;
    private Coroutine searchCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        createClanMenu.SetActive(false);
        btnOpenCreateClanMenu.interactable = true;
        clanServices = new ClanServices();
        
        // Configurar el listener para el input field
        ClanCodeInputField.onValueChanged.AddListener(OnClanCodeInputChanged);
    }

    private void OnClanCodeInputChanged(string text)
    {
        // Cancelar cualquier búsqueda pendiente
        if (searchCoroutine != null)
        {
            StopCoroutine(searchCoroutine);
        }
        
        // Solo iniciar búsqueda si hay al menos 5 caracteres
        if (text.Length >= 5)
        {
            searchCoroutine = StartCoroutine(DelayedSearch(text));
        }
        else
        {
            if (searchResultText != null)
            {
                searchResultText.text = "Ingresa al menos 5 caracteres";
            }
        }
    }

    private IEnumerator DelayedSearch(string clanInfo)
    {
        // Esperar medio segundo
        yield return new WaitForSeconds(0.5f);
        
        // Realizar la búsqueda
        findClan();
    }

    /// <summary>
    /// Handles click of the open clan menu "CREATE"
    /// </summary>
    public void OpenCreateClanMenu()
    {
        createClanMenu.SetActive(true);
        btnOpenCreateClanMenu.interactable = false;
    }

    /// <summary>
    /// Handles click of cancel button
    /// </summary>
    public void CloseCreateClanMenu()
    {
        createClanMenu.SetActive(false);
        btnOpenCreateClanMenu.interactable = true;
    }

    /// <summary>
    /// Handles click of create clan button
    /// </summary>
    public void CreateClan()
    {
        if (clanNameCreate.text.Length < 5)
        {
            textInfoClanMenu.text = "Clan name must be at least 5 characters long.";
            textInfoClanMenu.color = Color.red;
        }
        else
        {
            // Call the ClanServices to create a clan
            clanServices.CreateClan(clanNameCreate.text).ContinueWith(task =>
            {
                if (task.Result)
                {
                    textInfoClanMenu.text = "Clan created successfully!";
                    textInfoClanMenu.color = Color.green;
                }
                else
                {
                    textInfoClanMenu.text = "Something went wrong! Try again. Make sure you are not already in a clan.";
                    textInfoClanMenu.color = Color.red;
                }
            });
        }
    }

    public async void findClan()
    {
        string clanInfo = ClanCodeInputField.text;
        
        if (clanInfo.Length < 5)
        {
            if (searchResultText != null)
            {
                searchResultText.text = "Ingresa al menos 5 caracteres";
                searchResultText.color = Color.red;
            }
            return;
        }
        
        if (searchResultText != null)
        {
            searchResultText.text = "Buscando...";
            searchResultText.color = Color.yellow;
        }
        
        try
        {
            var clan = await clanServices.GetClanByInfo(clanInfo);
            
            if (clan != null)
            {
                if (searchResultText != null)
                {
                    searchResultText.text = $"Clan encontrado: {clan.Nombre}";
                    searchResultText.color = Color.green;
                }
                // Aquí podrías mostrar más información del clan o añadir un botón para unirse
            }
            else
            {
                if (searchResultText != null)
                {
                    searchResultText.text = "No se encontró ningún clan con ese código o nombre";
                    searchResultText.color = Color.red;
                }
            }
        }
        catch (System.Exception ex)
        {
            if (searchResultText != null)
            {
                searchResultText.text = "Error al buscar el clan: " + ex.Message;
                searchResultText.color = Color.red;
            }
            Debug.LogError("Error buscando clan: " + ex.Message);
        }
    }
}