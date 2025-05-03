using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Models;
using Services;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ClanUIManager : MonoBehaviour
{
    [Header("No clan menu stuff")] public GameObject noClanMenu;


    [FormerlySerializedAs("ClanCodeInputField")] [Header("Search Clan Menu Stuff")]
    public TMP_InputField clanCodeInputField;

    public TMP_Text searchResultText;

    [Header("Create Clan Menu Stuff")] public GameObject createClanMenu;
    public TMP_InputField clanNameCreate;
    public Button btnOpenCreateClanMenu;
    public TMP_Text textInfoClanMenu;

    [Header("Clan Details UI")] public GameObject clanDetailsMenu;
    public Button btnCloseClanDetailsMenu;
    public TMP_Text clanNameText;
    public TMP_Text clanCode;
    public Button copyClanCodeButton;
    public Button joinLeaveClanButton;
    public TMP_Text clanDetailsText;
    public GameObject clanPlayers;


    private ClanServices clanServices;
    private Coroutine searchCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        // Inicializamos el servicio de clanes
        clanServices = new ClanServices();

        // Ocultamos todos los menús inicialmente
        noClanMenu.SetActive(false);
        createClanMenu.SetActive(false);
        clanDetailsMenu.SetActive(false);

        // Configurar el listener para el input field
        clanCodeInputField.onValueChanged.AddListener(OnClanCodeInputChanged);

        // Comprobar si el jugador ya pertenece a un clan
        int clanId = PlayerPrefs.GetInt("IdClan", 0);
        Debug.Log($"ClanID al iniciar: {clanId}");

        if (clanId > 0)
        {
            // El jugador ya pertenece a un clan, obtener y mostrar información
            try
            {
                var clan = await clanServices.GetClanByInfo(clanId.ToString());
                if (clan != null)
                {
                    // Mostramos los detalles del clan, pero mantenemos oculto el menú de búsqueda
                    noClanMenu.SetActive(false);
                    clanDetailsMenu.SetActive(true);

                    // Mostrar los detalles del clan
                    ShowClanDetails(clan);
                    Debug.Log($"Mostrando información del clan: {clan.Nombre}");
                }
                else
                {
                    // No se encontró el clan, mostramos el menú de búsqueda y "No Clan"
                    noClanMenu.SetActive(true);
                    clanDetailsMenu.SetActive(false);
                    Debug.LogWarning("No se pudo encontrar el clan con ID: " + clanId);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error al cargar información del clan: " + ex.Message);
                noClanMenu.SetActive(true);
                clanDetailsMenu.SetActive(false);
            }
        }
        else
        {
            // El jugador no pertenece a ningún clan, mostramos la interfaz de búsqueda
            noClanMenu.SetActive(true);
            btnOpenCreateClanMenu.interactable = true;
            Debug.Log("El jugador no pertenece a ningún clan, mostrando menú de búsqueda");
        }
    }

    // Método para mostrar los detalles de un clan
    // Método para mostrar los detalles de un clan
    private async void ShowClanDetails(Clan clan)
    {
        clanDetailsMenu.SetActive(true);
        clanNameText.text = clan.Nombre;
        clanCode.text = clan.ClanCode;

        // Obtener el ID del jugador actual y su clan
        int jugadorId = PlayerPrefs.GetInt("jugador_id", 0);
        int jugadorClanId = PlayerPrefs.GetInt("IdClan", 0);

        // Verificar si el jugador está viendo su propio clan o uno ajeno
        bool isOwnClan = (jugadorClanId == clan.IdClan);

        // Configurar el botón de cerrar según corresponda
        if (btnCloseClanDetailsMenu != null)
        {
            btnCloseClanDetailsMenu.gameObject.SetActive(!isOwnClan);
        }

        // Configurar el botón según si es el líder o no
        if (jugadorId == clan.IdLeader)
        {
            // Es el líder del clan, puede borrarlo
            joinLeaveClanButton.GetComponentInChildren<TMP_Text>().text = "Borrar Clan";
            joinLeaveClanButton.interactable = true;
        }
        else
        {
            // Es miembro normal, puede abandonar el clan
            joinLeaveClanButton.GetComponentInChildren<TMP_Text>().text = "Abandonar Clan";
            joinLeaveClanButton.interactable = true;
        }

        // Obtener los jugadores del clan
        var players = await clanServices.GetClanPlayers(clan.IdClan);

        if (players != null)
        {
            var totalSteps = players.Sum(player => player.PasosTotales);
            int mediaSteps = players.Count > 0 ? totalSteps / players.Count : 0;

            clanDetailsText.text =
                $"Numero de miembros: {players.Count}/10\n" +
                $"Pasos totales: {totalSteps}\n" +
                $"Pasos por miembro: {mediaSteps}\n" +
                $"________________________________";

            DisplayClanMembers(players, clan.IdLeader);
        }
        else
        {
            clanDetailsText.text = "No se pudo obtener información de los miembros del clan";
            ClearMemberTexts();
        }
    }

// Implementación del método JoinLeaveClan usando ClanServices
// Implementación del método JoinLeaveClan usando ClanServices
    public async void JoinLeaveClan()
    {
        // Obtener el ID del jugador actual
        int jugadorId = PlayerPrefs.GetInt("jugador_id", 0);
        int clanIdActual = PlayerPrefs.GetInt("IdClan", 0);

        if (jugadorId == 0)
        {
            Debug.LogError("No hay un jugador identificado");
            return;
        }

        if (clanIdActual == 0)
        {
            // Unirse al clan
            if (clanCode.text != null && clanCode.text.Length > 0)
            {
                var clan = await clanServices.GetClanByInfo(clanCode.text);
                if (clan != null)
                {
                    int resultado = await clanServices.JoinClan(clan.IdClan);

                    if (resultado == 1)
                    {
                        Debug.Log("Te has unido al clan correctamente");
                        // Actualizar la interfaz
                        PlayerPrefs.SetInt("IdClan", clan.IdClan);
                        findClan(); // Refrescar la información
                    }
                    else
                    {
                        Debug.LogError("No se pudo unir al clan, código de error: " + resultado);
                    }
                }
            }
        }
        else
        {
            // Verificar si el usuario es el líder del clan
            var clan = await clanServices.GetClanByInfo(clanIdActual.ToString());

            if (clan != null && clan.IdLeader == jugadorId)
            {
                // Es líder, borrar el clan directamente sin confirmación
                bool resultado = await clanServices.DeleteClan(clan.IdClan);

                if (resultado)
                {
                    Debug.Log("Has borrado el clan correctamente");
                    PlayerPrefs.SetInt("IdClan", 0);
                    CloseClanDetailsMenu();
                    noClanMenu.SetActive(true);
                }
                else
                {
                    Debug.LogError("No se pudo borrar el clan");
                }
            }
            else
            {
                // No es líder, abandonar el clan
                int resultado = await clanServices.LeaveClan();

                if (resultado == 1)
                {
                    Debug.Log("Has abandonado el clan correctamente");
                    PlayerPrefs.SetInt("IdClan", 0);
                    CloseClanDetailsMenu();
                    noClanMenu.SetActive(true);
                }
                else
                {
                    Debug.LogError("No se pudo abandonar el clan, código de error: " + resultado);
                }
            }
        }
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
    public async void CreateClan()
    {
        if (clanNameCreate.text.Length < 5)
        {
            textInfoClanMenu.text = "Clan name must be at least 5 characters long.";
            textInfoClanMenu.color = Color.red;
        }
        else
        {
            // Call the ClanServices to create a clan
            bool result = await clanServices.CreateClan(clanNameCreate.text);
            if (result)
            {
                textInfoClanMenu.text = "Clan created successfully!";
                textInfoClanMenu.color = Color.green;

                // Actualizar la interfaz, obtenemos el clan recién creado
                var clan = await clanServices.GetClanByInfo(clanNameCreate.text);
                if (clan != null)
                {
                    // El jugador ahora pertenece a este clan
                    PlayerPrefs.SetInt("IdClan", clan.IdClan);

                    // Ocultar los menús de creación y búsqueda
                    noClanMenu.SetActive(false);
                    createClanMenu.SetActive(false);

                    // Mostrar los detalles del clan
                    ShowClanDetails(clan);
                }
            }
            else
            {
                textInfoClanMenu.text = "Something went wrong! Try again. Make sure you are not already in a clan.";
                textInfoClanMenu.color = Color.red;
            }
        }
    }

    public async void findClan()
    {
        string clanInfo = clanCodeInputField.text;

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
            // Utilizamos el servicio existente para obtener el clan
            var clan = await clanServices.GetClanByInfo(clanInfo);

            if (clan != null)
            {
                if (searchResultText != null)
                {
                    searchResultText.text = $"Clan encontrado: {clan.Nombre}";
                    searchResultText.color = Color.green;
                }

                // Configurar la interfaz con la información del clan
                clanDetailsMenu.SetActive(true);
                clanNameText.text = clan.Nombre;
                clanCode.text = clan.ClanCode;

                // Utilizamos el servicio existente para obtener los jugadores del clan
                var players = await clanServices.GetClanPlayers(clan.IdClan);

                // Obtener el ID del clan actual del jugador
                int jugadorIdClanActual = PlayerPrefs.GetInt("IdClan", 0);
                Debug.Log($"ID del clan actual del jugador: {jugadorIdClanActual}, ID del clan buscado: {clan.IdClan}");

                // Configuración del botón según si es su clan actual o no
                if (jugadorIdClanActual == clan.IdClan)
                {
                    // Es su clan actual, puede abandonarlo
                    joinLeaveClanButton.GetComponentInChildren<TMP_Text>().text = "Abandonar Clan";
                    joinLeaveClanButton.interactable = true;
                }
                else if (jugadorIdClanActual != 0)
                {
                    // Pertenece a otro clan, no puede unirse
                    joinLeaveClanButton.GetComponentInChildren<TMP_Text>().text = "Unirse al Clan";
                    joinLeaveClanButton.interactable = false;
                }
                else
                {
                    // No pertenece a ningún clan, puede unirse
                    joinLeaveClanButton.GetComponentInChildren<TMP_Text>().text = "Unirse al Clan";
                    joinLeaveClanButton.interactable = true;
                }

                if (players != null)
                {
                    var totalSteps = players.Sum(player => player.PasosTotales);
                    int mediaSteps = players.Count > 0 ? totalSteps / players.Count : 0;

                    clanDetailsText.text =
                        $"Numero de miembros: {players.Count}/10\n" +
                        $"Pasos totales: {totalSteps}\n" +
                        $"Pasos por miembro: {mediaSteps}\n" +
                        $"________________________________";

                    DisplayClanMembers(players, clan.IdLeader);
                }
                else
                {
                    clanDetailsText.text = "No se pudo obtener información de los miembros del clan";
                    ClearMemberTexts();
                }
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

    // Método separado para mostrar los miembros del clan
    private void DisplayClanMembers(List<Jugador> players, long leaderId)
    {
        // Encontrar al líder
        var lider = players.FirstOrDefault(p => p.IdJugador == leaderId);

        // Ordenar el resto de jugadores por pasos totales (de mayor a menor)
        var miembrosSinLider = players.Where(p => p.IdJugador != leaderId)
            .OrderByDescending(p => p.PasosTotales)
            .ToList();

        // Mostrar líder en InfoMember(0)
        TMP_Text infoLider = GameObject.Find("InfoMember (0)")?.GetComponent<TMP_Text>();
        if (infoLider != null && lider != null)
        {
            infoLider.text = $"1 | {lider.IdJugador} | {lider.Nombre} | {lider.PasosTotales} pasos";
            infoLider.color = Color.yellow; // Destacar al líder
        }

        // Mostrar resto de miembros en InfoMember (1-9)
        for (int i = 0; i < miembrosSinLider.Count && i < 9; i++)
        {
            TMP_Text infoMember = GameObject.Find($"InfoMember ({i + 1})")?.GetComponent<TMP_Text>();
            if (infoMember != null)
            {
                infoMember.text =
                    $"{i + 2} | {miembrosSinLider[i].IdJugador} | {miembrosSinLider[i].Nombre} | {miembrosSinLider[i].PasosTotales} pasos";
                infoMember.color = Color.white;
            }
        }

        // Mostrar "---" en textos restantes
        for (int i = players.Count; i < 10; i++)
        {
            TMP_Text infoMember = GameObject.Find($"InfoMember ({i})")?.GetComponent<TMP_Text>();
            if (infoMember != null)
            {
                infoMember.text = "---";
                infoMember.color = Color.gray;
            }
        }
    }

    // Método para limpiar todos los textos de miembros
    private void ClearMemberTexts()
    {
        for (int i = 0; i < 10; i++)
        {
            TMP_Text infoMember = GameObject.Find($"InfoMember ({i})")?.GetComponent<TMP_Text>();
            if (infoMember != null)
            {
                infoMember.text = "---";
                infoMember.color = Color.gray;
            }
        }
    }

    // Implementación del método JoinLeaveClan usando ClanServices

    public void CopyClanCode()
    {
        string clanCodeText = clanCode.text;
        GUIUtility.systemCopyBuffer = clanCodeText;
        Debug.Log("Clan code copied to clipboard: " + clanCodeText);
    }

    public void CloseClanDetailsMenu()
    {
        clanDetailsMenu.SetActive(false);
    }
}