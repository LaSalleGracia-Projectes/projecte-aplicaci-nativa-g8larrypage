using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPanel;
    public Button openPopupButton;
    public Button closeButton;
    public GameObject gridManager;
    public Transform placedStructuresParent;

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
