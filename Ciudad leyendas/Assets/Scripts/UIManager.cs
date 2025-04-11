using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject structureInfoPanel;
    public Text structureNameText;
    public Text statsText;

    void Awake()
    {
        Instance = this;
        structureInfoPanel.SetActive(false);
    }

    public void ShowStructureInfo(Structure structure)
    {
        structureInfoPanel.SetActive(true);
        structureNameText.text = structure.structureName;
        statsText.text = $"Vida: {structure.health}\nDaño: {structure.damage}\nPrecio: {structure.price}";
    }

    public void HideStructureInfo()
    {
        structureInfoPanel.SetActive(false);
    }
}
