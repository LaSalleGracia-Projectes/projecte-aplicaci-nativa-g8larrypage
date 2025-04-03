using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPanel;  // Panel del pop-up (tienda)
    public Button openPopupButton; // Botón para abrir la tienda
    public Button closeButton;     // Botón de cerrar la tienda
    public GameObject gridManager; // Referencia al GridManager
    public Transform placedStructuresParent; // Padre de todas las estructuras colocadas

    void Start()
    {
        // Asegurar que la tienda está oculta al inicio
        popupPanel.SetActive(false);

        // Asignar funciones a los botones
        openPopupButton.onClick.AddListener(ShowPopup);
        closeButton.onClick.AddListener(ClosePopup);
    }

    // Mostrar la tienda y ocultar el GridManager y las estructuras colocadas
    void ShowPopup()
    {
        popupPanel.SetActive(true);
        gridManager.SetActive(false); // Oculta el GridManager
        TogglePlacedStructures(false); // Oculta las construcciones colocadas
    }

    // Cerrar la tienda y volver a mostrar el GridManager y las estructuras
    public void ClosePopup()
    {
        popupPanel.SetActive(false);
        gridManager.SetActive(true); // Muestra el GridManager nuevamente
        TogglePlacedStructures(true); // Muestra las construcciones de nuevo
    }

    // Método para mostrar u ocultar las estructuras
    void TogglePlacedStructures(bool state)
    {
        if (placedStructuresParent != null)
        {
            foreach (Transform structure in placedStructuresParent)
            {
                structure.gameObject.SetActive(state);
            }
        }
    }
}
