using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPanel;  // Panel del pop-up (tienda)
    public Button openPopupButton; // Botón para abrir la tienda
    public Button closeButton;     // Botón de cerrar la tienda
    public GameObject gridManager; // Referencia al GridManager

    void Start()
    {
        // Asegurar que la tienda está oculta al inicio
        popupPanel.SetActive(false);

        // Asignar funciones a los botones
        openPopupButton.onClick.AddListener(ShowPopup);
        closeButton.onClick.AddListener(ClosePopup);
    }

    // Mostrar la tienda y ocultar el GridManager
    void ShowPopup()
    {
        popupPanel.SetActive(true);
        gridManager.SetActive(false); // Oculta el GridManager
    }

    // Cerrar la tienda y mostrar el GridManager
    void ClosePopup()
    {
        popupPanel.SetActive(false);
        gridManager.SetActive(true); // Muestra el GridManager nuevamente
    }
}
