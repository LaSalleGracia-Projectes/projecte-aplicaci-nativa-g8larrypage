using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPanel;
    public Button openPopupButton;
    public Button closeButton;
    public GameObject gridManager;
    public Transform placedStructuresParent;
    public GameObject ajustesButton;

    void Start()
    {
        popupPanel.SetActive(false);
        openPopupButton.onClick.AddListener(ShowPopup);
        closeButton.onClick.AddListener(ClosePopup);
    }

    void ShowPopup()
    {
        popupPanel.SetActive(true);
        gridManager.SetActive(false);
        TogglePlacedStructures(false);
        ajustesButton.SetActive(false);
       
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
        gridManager.SetActive(true);
        TogglePlacedStructures(true);
        ajustesButton.SetActive(true);
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
