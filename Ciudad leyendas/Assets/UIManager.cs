using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject clanUI;
    public GameObject profileUI;
    public GameObject gridManager;
    public Button ajustes;

    // Función para abrir/cerrar el panel del clan
    // Función para abrir/cerrar el panel del clan
    public void ToggleClanUI()
    {
        // Si el profileUI está activo, lo desactivamos
        profileUI.SetActive(false);

        bool activando = !clanUI.activeSelf;

        // Alternamos el estado del clanUI
        clanUI.SetActive(activando);

        // Activamos o desactivamos el gridManager y el botón de ajustes según corresponda
        gridManager.SetActive(!activando);
        ajustes.gameObject.SetActive(!activando);
    }

// Función para abrir/cerrar el panel del perfil
    public void ToggleProfileUI()
    {
        // Si el clanUI está activo, lo desactivamos
        clanUI.SetActive(false);

        bool activando = !profileUI.activeSelf;

        // Alternamos el estado del profileUI
        profileUI.SetActive(activando);

        // Activamos o desactivamos el gridManager y el botón de ajustes según corresponda
        gridManager.SetActive(!activando);
        ajustes.gameObject.SetActive(!activando);
    }
}