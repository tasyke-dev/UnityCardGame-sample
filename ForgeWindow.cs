using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase;
using System;

public class ForgeWindow : MonoBehaviour
{
    public List<ForgeInfo> forges;
    private int currentForgeIndex = 0;
    public int forgeCardId;
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI attack;
    public TextMeshProUGUI health;
    public TextMeshProUGUI manaCost;
    public TextMeshProUGUI stars;
    public UnityEngine.UI.Image abilityIcon;
    public UnityEngine.UI.Image element;
    public UnityEngine.UI.Image imageComponent;

    public List<ResourceInfo> resources;
    public List<ResourceInfo> playerResources;

    public Transform resourcesGrid;

    public GameObject resPref;

    DatabaseReference databaseReference;
    private string userId;

    void Awake()
    {
        userId = CardLoader.Instance.GetUserId();
        databaseReference = CardLoader.Instance.GetDatabaseReference();
        GetPlayerResources();
    }

    public void NextForge()
    {
        currentForgeIndex = (currentForgeIndex + 1) % forges.Count;
        LoadForgeInfo();
    }

    public void PreviousForge()
    {
        currentForgeIndex = (currentForgeIndex - 1 + forges.Count) % forges.Count;
        LoadForgeInfo();
    }

    private void LoadForgeInfo()
    {
        ForgeInfo currentForge = forges[currentForgeIndex];

        forgeCardId = currentForge.forgeCardId;
        cardName.text = currentForge.card.name;
        attack.text = currentForge.card.attack.ToString();
        health.text = currentForge.card.health.ToString();
        manaCost.text = currentForge.card.manacost.ToString();
        stars.text = currentForge.card.stars;
        abilityIcon.sprite = CardLoader.Instance.GetAbilityIcon(currentForge.card.abilities[0]);
        imageComponent.sprite = Resources.Load<Sprite>(currentForge.card.picturePath);
        element.sprite = GetElementSprite(currentForge.card.element);
        
        resources = currentForge.resources;

        foreach (Transform child in resourcesGrid)
        {
            Destroy(child.gameObject);
        }

        foreach (ResourceInfo resource in resources)
        {
            ResInfo resInfo = Instantiate(resPref, resourcesGrid).GetComponent<ResInfo>();
            foreach (ResourceInfo playerResource in playerResources)
            {
                UnityEngine.Debug.Log(playerResource.name);
                UnityEngine.Debug.Log(resource.name);
                if (playerResource.name == resource.name)
                {
                    resInfo.SetResource(resource.name, resource.value.ToString(), playerResource.value.ToString());
                    break;
                }
                else
                {
                    resInfo.SetResource(resource.name, resource.value.ToString(), "0");
                }
            }

        }
       
    }

    public void GetPlayerResources()
    {
        playerResources.Clear();
        DatabaseReference userResReference = databaseReference.Child("users").Child(userId).Child("resources");

        userResReference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot userResSnapshot = task.Result;

                foreach (DataSnapshot resSnapshot in userResSnapshot.Children)
                {
                    string resourceName = resSnapshot.Key;
                    int resourceValue = Convert.ToInt32(resSnapshot.Value); 

                    playerResources.Add(new ResourceInfo
                    {
                        name = resourceName,
                        value = resourceValue
                    });
                }
                forges = GameData.Instance.GetAllForgesData();
                LoadForgeInfo();
            }
        });
    }


    public Sprite GetElementSprite(string elementType)
    {
        switch (elementType)
        {
            case "Water":
                return Resources.Load<Sprite>("Sprites/Elements/water");

            case "Fire":
                return Resources.Load<Sprite>("Sprites/Elements/fire");

            case "Earth":
                return Resources.Load<Sprite>("Sprites/Elements/earth");

            case "Air":
                return Resources.Load<Sprite>("Sprites/Elements/air");

            default:
                return Resources.Load<Sprite>("Sprites/Elements/empty");
        }
    }
}
