using Firebase.Auth;
using Firebase.Database;
using Firebase;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Firebase.Extensions;

public class ArenaLoader : MonoBehaviour
{
    ArenaLoader Instance;
    public event Action<List<CardMenu>> OnLevelDeckCardsUpdated;
    public event Action<List<CardMenu>> OnPlayerDeckCardsUpdated;
    public event Action<string> OnArenaPlayerUpdated;

    public List<CardMenu> PlayerDeckCards;
    public List<CardMenu> LevelDeckCards;
    DatabaseReference databaseReference;
    private string currentUserId;
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
                        currentUserId = auth.CurrentUser.UserId;
                    }
                }
                else
                {
                    Debug.LogError("Firebase dependencies check failed");
                }
            });
        }
    }

    public void LoadCardsToArena(string userId)
    {
        UnityEngine.Debug.Log("2asda");
        LoadUserDeckToDeck(currentUserId);
        LevelDeckCards.Clear();
        OnLevelDeckCardsUpdated?.Invoke(LevelDeckCards);
        OnArenaPlayerUpdated?.Invoke(userId);

        DatabaseReference arenaCardsReference = databaseReference.Child("users").Child(userId).Child("userCards");

        arenaCardsReference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot userCardsSnapshot = task.Result;
                foreach (DataSnapshot cardSnapshot in userCardsSnapshot.Children)
                {
                    string cardId = cardSnapshot.Key;
                    LoadArenaPlayerToLevel(cardId, userId);

                }
            }
        });
    }
    public void LoadArenaPlayerToLevel(string cardId, string userId)
    {
        DatabaseReference userCardReference = databaseReference.Child("users").Child(userId).Child("userCards").Child(cardId);
        userCardReference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot userCardSnapshot = task.Result;
                if (userCardSnapshot.Exists)
                {
                    Dictionary<string, object> cardData = userCardSnapshot.Value as Dictionary<string, object>;

                    string ability = Convert.ToString(cardData["Ability"]);
                    int attack = Convert.ToInt32(cardData["Attack"]);
                    int cardType = Convert.ToInt32(cardData["CardType"]);
                    string element = Convert.ToString(cardData["Element"]);
                    int exp = Convert.ToInt32(cardData["Exp"]);
                    int health = Convert.ToInt32(cardData["Health"]);
                    List<CardItem> itemKeys = new List<CardItem>();

                    if (cardData["Items"] is Dictionary<string, object> itemsData)
                    {
                        foreach (var itemKey in itemsData.Keys)
                        {
                            Dictionary<string, object> itemInfo = itemsData[itemKey] as Dictionary<string, object>;
                            ArenaLoadItemToCard(userId, itemKey, itemKeys);
                        }
                    }
                    else
                    {
                        itemKeys = new List<CardItem> { null };
                    }
                    int level = Convert.ToInt32(cardData["Level"]);
                    int manacost = Convert.ToInt32(cardData["Manacost"]);
                    string name = Convert.ToString(cardData["Name"]);
                    string picture = Convert.ToString(cardData["Picture"]);
                    string stars = Convert.ToString(cardData["Stars"]);
                    bool onDeck = Convert.ToBoolean(Convert.ToString(cardData["onDeck"]));

                    CardMenu card = new CardMenu
                    {
                        cardId = cardId,
                        abilities = new List<string> { ability },
                        attack = attack,
                        cardType = cardType,
                        element = element,
                        exp = exp,
                        health = health,
                        items = itemKeys,
                        level = level,
                        manacost = manacost,
                        name = name,
                        picturePath = picture,
                        stars = stars,
                        onDeck = onDeck,
                    };
                    if (onDeck)
                    {
                        LevelDeckCards.Add(card);
                        OnLevelDeckCardsUpdated?.Invoke(LevelDeckCards);
                    }

                }
            }
        });
    }



    public void LoadUserDeckToDeck(string currentUserId)
    {
        PlayerDeckCards.Clear();
        OnPlayerDeckCardsUpdated?.Invoke(PlayerDeckCards);

        DatabaseReference userCardsReference = databaseReference.Child("users").Child(currentUserId).Child("userCards");

        userCardsReference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot userCardsSnapshot = task.Result;
                foreach (DataSnapshot cardSnapshot in userCardsSnapshot.Children)
                {
                    string cardId = cardSnapshot.Key;
                    LoadPlayerDeckToLevel(cardId);

                }
            }
        });
    }
    public void LoadPlayerDeckToLevel(string cardId)
    {
        DatabaseReference userCardReference = databaseReference.Child("users").Child(currentUserId).Child("userCards").Child(cardId);
        userCardReference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot userCardSnapshot = task.Result;
                if (userCardSnapshot.Exists)
                {
                    Dictionary<string, object> cardData = userCardSnapshot.Value as Dictionary<string, object>;

                    string ability = Convert.ToString(cardData["Ability"]);
                    int attack = Convert.ToInt32(cardData["Attack"]);
                    int cardType = Convert.ToInt32(cardData["CardType"]);
                    string element = Convert.ToString(cardData["Element"]);
                    int exp = Convert.ToInt32(cardData["Exp"]);
                    int health = Convert.ToInt32(cardData["Health"]);
                    List<CardItem> itemKeys = new List<CardItem>();

                    if (cardData["Items"] is Dictionary<string, object> itemsData)
                    {
                        foreach (var itemKey in itemsData.Keys)
                        {
                            Dictionary<string, object> itemInfo = itemsData[itemKey] as Dictionary<string, object>;
                            ArenaLoadItemToCard(currentUserId, itemKey, itemKeys);
                        }
                    }
                    else
                    {
                        itemKeys = new List<CardItem> { null };
                    }
                    int level = Convert.ToInt32(cardData["Level"]);
                    int manacost = Convert.ToInt32(cardData["Manacost"]);
                    string name = Convert.ToString(cardData["Name"]);
                    string picture = Convert.ToString(cardData["Picture"]);
                    string stars = Convert.ToString(cardData["Stars"]);
                    bool onDeck = Convert.ToBoolean(Convert.ToString(cardData["onDeck"]));

                    CardMenu card = new CardMenu
                    {
                        cardId = cardId,
                        abilities = new List<string> { ability },
                        attack = attack,
                        cardType = cardType,
                        element = element,
                        exp = exp,
                        health = health,
                        items = itemKeys,
                        level = level,
                        manacost = manacost,
                        name = name,
                        picturePath = picture,
                        stars = stars,
                        onDeck = onDeck,
                    };
                    if (onDeck)
                    {
                        PlayerDeckCards.Add(card);
                        OnPlayerDeckCardsUpdated?.Invoke(PlayerDeckCards);
                    }

                }
            }
        });
    }

    public void ArenaLoadItemToCard(string userId, string itemId, List<CardItem> itemKeys)
    {
        DatabaseReference userCardReference = databaseReference.Child("users").Child(userId).Child("inventoryGems").Child(itemId);
        userCardReference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot userItemSnapshot = task.Result;
                if (userItemSnapshot.Exists)
                {
                    Dictionary<string, object> itemData = userItemSnapshot.Value as Dictionary<string, object>;

                    string attribute = Convert.ToString(itemData["attribute"]);
                    string element = Convert.ToString(itemData["element"]);
                    int level = Convert.ToInt32(itemData["level"]);
                    string rarity = Convert.ToString(itemData["rarity"]);
                    int stat = Convert.ToInt32(itemData["stat"]);
                    bool used = Convert.ToBoolean(Convert.ToString(itemData["used"]));

                    CardItem item = new CardItem
                    {
                        gemId = itemId,
                        attribute = attribute,
                        element = element,
                        level = level,
                        rarity = rarity,
                        stat = stat,
                        used = used
                    };
                    itemKeys.Add(item);
                }

            }
        });
    }

}
