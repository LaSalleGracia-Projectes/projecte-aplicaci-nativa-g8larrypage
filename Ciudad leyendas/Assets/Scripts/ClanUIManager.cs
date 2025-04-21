using System;
using Services;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ClanUIManager : MonoBehaviour
{
    public TMP_InputField clanNameCreate;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OpenCreateClanMenu() 
    {
        // Open the create clan menu
        Debug.Log("Open Create Clan Menu");
    }
    
    public void CreateClan()
    {
        if (clanNameCreate.text.Length < 3)
        {
            Debug.Log("Clan name must be at least 3 characters long.");
        }
        else
        {
            // Call the ClanServices to create a clan
            Guid leaderId = Guid.Parse(PlayerPrefs.GetString("user_id"));
            
            ClanServices clanServices = new ClanServices();
            clanServices.CreateClan(clanNameCreate.text, leaderId).ContinueWith(task =>
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
