using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClanUIManager : MonoBehaviour
{
    public TMP_InputField clanNameCreate;

    [Header("Clan Menu Stuff")] public GameObject createClanMenu;
    public Button btnOpenCreateClanMenu;
    public TMP_Text textInfoClanMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        createClanMenu.SetActive(false);
        btnOpenCreateClanMenu.interactable = true;
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
        if (clanNameCreate.text.Length < 3)
        {
            textInfoClanMenu.text = "Clan name must be at least 3 characters long.";
            textInfoClanMenu.color = Color.red;
        }
        else
        {
            // Call the ClanServices to create a clan
            ClanServices clanServices = new ClanServices();
            clanServices.CreateClan(clanNameCreate.text).ContinueWith(task =>
            {
                if (task.Result)
                {
                    textInfoClanMenu.text = "Clan created successfully!";
                    textInfoClanMenu.color = Color.green;
                }
                else
                {
                    textInfoClanMenu.text = "Something went wrong! Try again.";
                    textInfoClanMenu.color = Color.red;
                }
            });
            Debug.Log("Clan created with name: " + clanNameCreate.text);
        }
    }
}