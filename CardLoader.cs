using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using UnityEngine.UI;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using System;
using TMPro;
using static UnityEngine.UIElements.UxmlAttributeDescription;

[System.Serializable]
public class CardMenu
{
    public string cardId;
    public List<string> abilities;
    public int attack;
    public int cardType;
    public string element;
    public int exp;
    public int health;
    public List<CardItem> items;
    public int level;
    public int manacost;
    public string name;
    public string picturePath;
    public string stars;
    public string city;
    public bool onDeck;
    public int ATperLevel;
    public int HPperLevel;
    public GameObject cardPrefab;

}


public class PlayerInfo
{
    public string playerId;
    public string username;
    public int rating;
    public GameObject arenaPlayerPrefab;
}

public class CardItem
{
    public string gemId;
    public string type;
    public string attribute;
    public string element;
    public int level;
    public string rarity;
    public int stat;
    public bool used;
    public GameObject gemPrefab;
}
public class CardLoader : MonoBehaviour
{
    public static CardLoader Instance;

    DatabaseReference databaseReference;
    public List<CardMenu> allCards;

    public List<CardMenu> commonCards;
    public List<CardMenu> rareCards;
    public List<CardMenu> epicCards;

    public GameObject cardPrefab;
    public GameObject cardSummonPrefab;

    public Transform summonCommonPlace;
    public Transform summonRarePlace;
    public Transform summonEpicPlace;

    public Transform itemsGrid;
    public List<CardItem> playerItems;
    public GameObject itemPrefab;

    private string userId;
    public GameObject cardWindow;
    public GameObject itemWindow;
    public GameObject evolveWindow;
    public GameObject evolveCardData;



    public GameObject currentGemInfo;
    public GameObject chosenGemInfo;

    public string cardItemId;
    public string slotItemId;

    MenuCardInfo currentMenuCardInfo;

    void Awake()
    {
        if (Instance == null)
        { 
            Instance = this;
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    FirebaseApp app = FirebaseApp.DefaultInstance;
                    databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

                    FirebaseAuth auth = FirebaseAuth.DefaultInstance;
                    if (auth.CurrentUser != null)
                    {
                        userId = auth.CurrentUser.UserId;
                        LoadCardsFromDatabase();
                    }
                }
                else
                {
                    Debug.LogError("Firebase dependencies check failed");
                }
            });
        }
    }

    /*
    // Загрузка всех карт на экран Summon, используются для получения в даленейшем карт - массив allCards
    void LoadCardsFromDatabase()
    {
        databaseReference.Child("cards").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                allCards = new List<CardMenu>();

                foreach (DataSnapshot cardSnapshot in snapshot.Children)
                {
                    CardMenu card = JsonUtility.FromJson<CardMenu>(cardSnapshot.GetRawJsonValue());
                    card.cardId = cardSnapshot.Key;
                    card.cardPrefab = Instantiate(cardSummonPrefab, summonPlace, false); 

                    MenuCardInfo cardComponent = card.cardPrefab.GetComponent<MenuCardInfo>();
                    if (cardComponent != null)
                    {
                        cardComponent.SetCardData(card);
                    }


                    allCards.Add(card);
                }
            }
        });
    }*/


    public void LoadCardsFromDatabase()
    {
        ClearAllCards();
        List<CardMenu> allCardsData = GameData.Instance.GetAllCardsData();

        foreach (CardMenu card in allCardsData)
        {
            if (card.stars == "F" || card.stars == "E" || card.stars == "D")
            {
                card.cardPrefab = Instantiate(cardSummonPrefab, summonCommonPlace, false);

                MenuCardInfo cardComponent = card.cardPrefab.GetComponent<MenuCardInfo>();
                if (cardComponent != null)
                {
                    cardComponent.SetCardData(card);
                }
                commonCards.Add(card);
            }

            if (card.stars == "D" || card.stars == "C" || card.stars == "B")
            {
                card.cardPrefab = Instantiate(cardSummonPrefab, summonRarePlace, false);

                MenuCardInfo cardComponent = card.cardPrefab.GetComponent<MenuCardInfo>();
                if (cardComponent != null)
                {
                    cardComponent.SetCardData(card);
                }
                rareCards.Add(card);
            }

            if (card.stars == "B" || card.stars == "A" || card.stars == "S")
            {
                card.cardPrefab = Instantiate(cardSummonPrefab, summonEpicPlace, false);

                MenuCardInfo cardComponent = card.cardPrefab.GetComponent<MenuCardInfo>();
                if (cardComponent != null)
                {
                    cardComponent.SetCardData(card);
                }
                epicCards.Add(card);
            }


            allCards.Add(card);
        }
    }

    public List<CardMenu> GetCardsList(string type)
    {
        if (type == "Common")
            return commonCards;
        else if (type == "Rare")
            return rareCards;
        else
            return epicCards;
    }

    void ClearAllCards()
    {
        // Очистка существующих карт
        foreach (CardMenu card in allCards)
        {
            Destroy(card.cardPrefab);
        }

        // Очистка списка карт
        allCards.Clear();
    }



    public void SetEvolveWindowData(MenuCardInfo card)
    {
        evolveWindow.SetActive(true);
        EvolveWindow window = FindObjectOfType<EvolveWindow>();

        if (window != null)
        {
            window.SetEvolveWindowData(card);
        }
        else
        {
            Debug.LogError("EvolveWindow component not found.");
        }
    }
    public void SetChosenEvolveCard(MenuCardInfo card)
    {
        currentMenuCardInfo = card;
        evolveCardData.SetActive(true);
        EvolveWindow window = FindObjectOfType<EvolveWindow>();

        if (window != null)
        {
            window.SetChosenCardData(card);
        }
        else
        {
            Debug.LogError("EvolveWindow component not found.");
        }
    }

    // Открытие окна карты и вызов функции передачи данных карты
    public void SetCardWindowData(MenuCardInfo card)
    {
        currentMenuCardInfo = card;
        cardWindow.SetActive(true);
        CardWindow window = FindObjectOfType<CardWindow>();

        if (window != null)
        {
            window.SetWindowCardData(card);
        }
        else
        {
            Debug.LogError("CardWindow component not found.");
        }
    }



    // Активация ВЫБРАННОГО предмета вызов функции передачи данных
    public void SetItemWindowData(CardItemInfo item)
    {
        chosenGemInfo.SetActive(true);
        ItemWindow window = FindObjectOfType<ItemWindow>();

        if (window != null)
        {
            window.SetChosenItemData(item);
        }
        else
        {
            Debug.LogError("ItemWindow component not found.");
        }
    }

    // Активация НАДЕТОГО предмета вызов функции передачи данных
    public void ShowItemWindowData(string cardId, string itemType, CardItemInfo item = null)
    {
        cardItemId = cardId;

        LoadItemsToGrid(itemType);


        itemWindow.SetActive(true);
        if (item != null)
        {
            currentGemInfo.SetActive(true);
            ItemWindow window = FindObjectOfType<ItemWindow>();
            if (window != null)
                window.SetCurrentItemData(item);
        }
        else
        {
            currentGemInfo.SetActive(false);
        }
    }

    public void LoadItemsToGrid(string itemType)
    {
        playerItems = FindObjectOfType<InventoryLoader>().GetPlayerItems();

        foreach (Transform child in itemsGrid)
        {
            Destroy(child.gameObject);
        }
        if (playerItems.Count != 0)
        {
            foreach (CardItem cardItem in playerItems)
            {
                if ((cardItem.used == false) && (cardItem.type == itemType))
                {
                    cardItem.gemPrefab = Instantiate(itemPrefab, itemsGrid, false);
                    CardItemInfo gemComponent = cardItem.gemPrefab.GetComponent<CardItemInfo>();
                    if (gemComponent != null)
                    {
                        gemComponent.SetGemData(cardItem);
                    }
                    cardItem.gemPrefab.transform.SetParent(itemsGrid.transform, false);
                }
            }
        }
    }

    public Sprite GetAbilityIcon(string ability)
    {
        switch (ability)
        {
            case "PROVOCATION":
                return Resources.Load<Sprite>("Sprites/Icons/provocation");
            case "DOUBLE_ATTACK":
                return Resources.Load<Sprite>("Sprites/Icons/double_attack");
            case "SHIELD":
                return Resources.Load<Sprite>("Sprites/Icons/shield");
            case "REGENERATION_EACH_TURN":
                return Resources.Load<Sprite>("Sprites/Icons/regeneration_each_turn");
            case "TRANSFORMATION":
                return Resources.Load<Sprite>("Sprites/Icons/transformation");
            case "VAMPIRISM":
                return Resources.Load<Sprite>("Sprites/Icons/vampirism");
            case "RETURN_DAMAGE":
                return Resources.Load<Sprite>("Sprites/Icons/return_damage");
            case "SNAKE_SKIN":
                return Resources.Load<Sprite>("Sprites/Icons/snake_skin");
            case "CRIT":
                return Resources.Load<Sprite>("Sprites/Icons/crit");
            case "SPLASH":
                return Resources.Load<Sprite>("Sprites/Icons/splash");
            case "ATTACK_BUFF":
                return Resources.Load<Sprite>("Sprites/Icons/attack_buff");
            case "AOE_HEALING":
                return Resources.Load<Sprite>("Sprites/Icons/aoe_healing");
            case "MANA_BUFF":
                return Resources.Load<Sprite>("Sprites/Icons/mana_buff");
            case "SUMMON":
                return Resources.Load<Sprite>("Sprites/Icons/summon");
            case "BERSERK":
                return Resources.Load<Sprite>("Sprites/Icons/berserk");
            case "KAMIKADZE":
                return Resources.Load<Sprite>("Sprites/Icons/kamikadze");
            case "AOE_CASTER":
                return Resources.Load<Sprite>("Sprites/Icons/aoe_caster");
            default:
                return Resources.Load<Sprite>("Sprites/Icons/none");
        }
    }


    public string GetUserId()
    {
        return userId;
    }

    public DatabaseReference GetDatabaseReference()
    {
        return databaseReference;
    }


    public string GetCardIdForItem()
    {
        return cardItemId;
    }


    public MenuCardInfo GetMenuCardInfo()
    {
        return currentMenuCardInfo;
    }
}