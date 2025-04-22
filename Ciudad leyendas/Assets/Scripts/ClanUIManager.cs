using System;
using Services;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ClanUIManager : MonoBehaviour
{
    public TMP_InputField clanNameCreate;
    public GameObject createClanMenu;
    public Button btnOpenCreateClanMenu;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        createClanMenu.SetActive(false);
        btnOpenCreateClanMenu.SetEnabled(true);
    }
    
    /// <summary>
    /// Handles click of the open clan menu "CREATE"
    /// </summary>
    public void OpenCreateClanMenu() 
    {
        createClanMenu.SetActive(true);
        btnOpenCreateClanMenu.SetEnabled(false);
    }
    
    /// <summary>
    /// Handles click of cancel button
    /// </summary>
    public void CloseCreateClanMenu()
    {
        createClanMenu.SetActive(false);
        btnOpenCreateClanMenu.SetEnabled(true);
    }
   
    /// <summary>
    /// Handles click of create clan button
    /// </summary>
    public void CreateClan()
    {
        if (clanNameCreate.text.Length < 3)
        {
            Debug.Log("Clan name must be at least 3 characters long.");
        }
        else
        {
            // Call the ClanServices to create a clan
            ClanServices clanServices = new ClanServices();
            clanServices.CreateClan(clanNameCreate.text).ContinueWith(task =>
            {
                if (task.Result)
                {
                    Debug.Log("Clan created successfully!");
                }
                else
                {
                    Debug.Log("Failed to create clan.");
                }
            });
            Debug.Log("Clan created with name: " + clanNameCreate.text);
        }
        
    }
}
