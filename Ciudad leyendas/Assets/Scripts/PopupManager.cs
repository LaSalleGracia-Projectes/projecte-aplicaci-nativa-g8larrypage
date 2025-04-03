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
        popupPanel.SetActive(false);

        openPopupButton.onClick.AddListener(ShowPopup);
        closeButton.onClick.AddListener(ClosePopup);
    }

    public void ShowPopup()
    {
        popupPanel.SetActive(true);
        gridManager.SetActive(false);
        TogglePlacedStructures(false);
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
        gridManager.SetActive(true);
        TogglePlacedStructures(true);
    }

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
