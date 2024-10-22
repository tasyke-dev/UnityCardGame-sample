using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase;
using DG.Tweening;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using TMPro;

public class BattleManager : MonoBehaviour
{
    DatabaseReference databaseReference;
    private string userId;
    public GameObject menuBtn;
    public GameObject LoadingScreen;
    public Image LoadingFill;
    public GameObject resultObject;
    public TextMeshProUGUI ResultTxt;

    public GameObject arenaLoot;
    public TextMeshProUGUI arenaPoints;
    public GameObject classicLoot;
    public TextMeshProUGUI goldPoints;
    public TextMeshProUGUI expPoints;
    public GameObject gemLoot;
    public TextMeshProUGUI gemRarity;
    public Image gemImage;

    public string chapterId;
    public string levelId;
    public List<ResourceInfo> levelDrops;

    public int gold = 0;
    public string itemRarity = null;
    public string itemPicture = null;

    public List<ResourceInfo> resources;
    public GameObject resPref;
    public Transform resourcesGrid;

    void Awake()
    {
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
                    levelDrops = DataManager.Instance.levelDrops;
                    chapterId = DataManager.Instance.chapterId;
                    levelId = DataManager.Instance.levelId;

                }
            }
            else
            {
                Debug.LogError("Firebase dependencies check failed");
            }
        });

    }

    public static string GenerateUniqueId()
    {
        long timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        string uniqueId = $"{timestamp}_{Guid.NewGuid().ToString()}";

        return uniqueId;
    }

    public void CheckStateOfGame(bool state, string EnemyID)
    {
        if (EnemyID == null)
            StartCoroutine(ExecuteTasks(state, EnemyID));
        else
            StartCoroutine(ExecuteArenaTasks(state, EnemyID));
    }

    private IEnumerator ExecuteArenaTasks(bool state, string EnemyID)
    {
        DatabaseReference usersReference = FirebaseDatabase.DefaultInstance.RootReference.Child("users");

        List<IEnumerator> tasks = new List<IEnumerator>();

        tasks.Add(ChangeRatingData(usersReference.Child(userId), state));
        tasks.Add(ChangeRatingData(usersReference.Child(EnemyID), !state));
        tasks.Add(ResultScreenAnimation(state, true));

        foreach (var task in tasks)
        {
            yield return StartCoroutine(task);
        }
    }


    private IEnumerator ExecuteTasks(bool state, string EnemyID)
    {
        DatabaseReference usersReference = FirebaseDatabase.DefaultInstance.RootReference.Child("users");

        List<IEnumerator> tasks = new List<IEnumerator>();

        if (state)
        {
            tasks.Add(ChangeGoldData(usersReference.Child(userId)));
            tasks.Add(ChangeCardsExp(usersReference.Child(userId)));
            tasks.Add(ChangeResourcesData(usersReference.Child(userId)));
            tasks.Add(UpdateLevels(usersReference.Child(userId)));
            if (UnityEngine.Random.Range(0, 100) <= 30)
            {
                tasks.Add(CreateItemData(usersReference.Child(userId)));
            }
        }
        tasks.Add(ResultScreenAnimation(state, false));

        foreach (var task in tasks)
        {
            yield return StartCoroutine(task);
        }
        //StartCoroutine(ResultScreenAnimation(state));
    }

    private IEnumerator CreateItemData(DatabaseReference userReference)
    {
        var gemId = GenerateUniqueId();
        var gemData = GenerateRandomItemData();
        

        yield return userReference.Child("inventoryGems").Child(gemId).SetValueAsync(gemData).ContinueWithOnMainThread(t => { });
    }

    private Dictionary<string, object> GenerateRandomItemData()
    {
        string[] rarities = { "C", "UC", "R", "VR", "L" };
        string rarity = GetRandomRarity();
        string elem = GetRandomElement();
        string attr = GetRandomAttribute();
        itemRarity = rarity;
        itemPicture = elem + attr;
        resources.Add(new ResourceInfo { name = itemPicture, value = 1 });
        Dictionary<string, object> itemData = new Dictionary<string, object>
    {
        { "type",  "Gem"},
        { "element",  elem},
        { "rarity", rarity },
        { "level", 1 },
        { "attribute", attr },
        { "stat", GetRandomStat(rarity) },
        { "used", false }
    };

        return itemData;
    }

    private string GetRandomElement()
    {
        // Логика выбора случайного элемента
        string[] elements = { "water", "air", "fire", "earth" };
        return elements[UnityEngine.Random.Range(0, elements.Length)];
    }

    private string GetRandomAttribute()
    {
        // Логика выбора случайного атрибута
        string[] attributes = { "HP", "AT" };
        return attributes[UnityEngine.Random.Range(0, attributes.Length)];
    }

    private string GetRandomRarity()
    {
        // Логика выбора случайной редкости с учетом шансов
        float rarityChance = UnityEngine.Random.Range(0f, 1f);
        if (rarityChance < 0.5f) return "C";
        else if (rarityChance < 0.7f) return "UC";
        else if (rarityChance < 0.85f) return "R";
        else if (rarityChance < 0.95f) return "VR";
        else return "L";
    }

    private int GetRandomStat(string rarity)
    {
        // Логика выбора статистики в зависимости от редкости
        int baseStat = 1;
        if (rarity == "C") return baseStat;
        else if (rarity == "UC") return baseStat + 2;
        else if (rarity == "R") return baseStat + 3;
        else if (rarity == "VR") return baseStat + 4;
        else return baseStat + 5;
    }

    private IEnumerator ChangeGoldData(DatabaseReference userReference)
    {
        var task = userReference.Child("gold").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);
        gold = UnityEngine.Random.Range(50, 251);

        if (task.Exception == null)
        {
            DataSnapshot goldSnapshot = task.Result;

            if (goldSnapshot.Exists)
            {
                int goldValue = Convert.ToInt32(goldSnapshot.Value);
                goldValue += gold;
                resources.Add(new ResourceInfo { name = "gold", value = gold });
                yield return userReference.Child("gold").SetValueAsync(goldValue).ContinueWithOnMainThread(t => { });
            }
        }
    }

    private IEnumerator ChangeCardsExp(DatabaseReference userReference)
    {
        List<string> cardsIds = CardManager.playerCardsIds;
        foreach (var cardId in cardsIds) 
        {
            var expTask = userReference.Child("userCards").Child(cardId).Child("Exp").GetValueAsync();
            var lvlTask = userReference.Child("userCards").Child(cardId).Child("Level").GetValueAsync();

            yield return new WaitUntil(() => expTask.IsCompleted && lvlTask.IsCompleted);

            DataSnapshot expSnapshot = expTask.Result;
            DataSnapshot lvlSnapshot = lvlTask.Result;
            if (expSnapshot.Exists && lvlSnapshot.Exists)
            {
                int currentExp = Convert.ToInt32(expSnapshot.Value);
                int currentLevel = Convert.ToInt32(lvlSnapshot.Value);
                currentExp += 45;
                CheckLevelUp(userReference, currentExp, currentLevel, cardId);
            }
        }
        resources.Add(new ResourceInfo { name = "exp", value = 45 });
    }
    private IEnumerator LevelUpData(DatabaseReference userReference, string cardId)
    {
        var attackTask = userReference.Child("userCards").Child(cardId).Child("Attack").GetValueAsync();
        var healthTask = userReference.Child("userCards").Child(cardId).Child("Health").GetValueAsync();
        var atLevelTask = userReference.Child("userCards").Child(cardId).Child("ATperLevel").GetValueAsync();
        var hpLevelTask = userReference.Child("userCards").Child(cardId).Child("HPperLevel").GetValueAsync();
        yield return new WaitUntil(() => attackTask.IsCompleted && healthTask.IsCompleted && atLevelTask.IsCompleted && hpLevelTask.IsCompleted);
        DataSnapshot attackSnapshot = attackTask.Result;
        DataSnapshot healthSnapshot = healthTask.Result;
        DataSnapshot ATSnapshot = atLevelTask.Result;
        DataSnapshot HPSnapshot = hpLevelTask.Result;
        if (attackSnapshot.Exists && healthSnapshot.Exists && ATSnapshot.Exists && HPSnapshot.Exists)
        {
            int currentAttack = Convert.ToInt32(attackSnapshot.Value);
            int currentHealth = Convert.ToInt32(healthSnapshot.Value);
            int ATperLevel = Convert.ToInt32(ATSnapshot.Value);
            int HPperLevel = Convert.ToInt32(HPSnapshot.Value);
            currentAttack += ATperLevel;
            currentHealth += HPperLevel;

            userReference.Child("userCards").Child(cardId).Child("Attack").SetValueAsync(currentAttack).ContinueWithOnMainThread(t => { });
            userReference.Child("userCards").Child(cardId).Child("Health").SetValueAsync(currentHealth).ContinueWithOnMainThread(t => { });
        }
    }
    private void CheckLevelUp(DatabaseReference userReference, int experience, int currentLevel, string cardId)
    {
        switch (currentLevel)
        {
            case 1:
                if (experience >= 100)
                {
                    currentLevel = 2;
                    experience -= 100;
                    StartCoroutine(LevelUpData(userReference, cardId));
                }
                break;
            case 2:
                if (experience >= 150)
                {
                    currentLevel = 3;
                    experience -= 150;
                    StartCoroutine(LevelUpData(userReference, cardId));
                }
                break;
            case 3:
                if (experience >= 200)
                {
                    currentLevel = 4;
                    experience -= 200;
                    StartCoroutine(LevelUpData(userReference, cardId));
                }
                break;
            case 4:
                if (experience >= 250)
                {
                    currentLevel = 5;
                    experience -= 250;
                    StartCoroutine(LevelUpData(userReference, cardId));
                }
                break;
            case 5:
                if (experience >= 300)
                {
                    currentLevel = 6;
                    experience -= 300;
                    StartCoroutine(LevelUpData(userReference, cardId));
                }
                break;
            case 6:
                if (experience >= 350)
                {
                    currentLevel = 7;
                    experience -= 350;
                    StartCoroutine(LevelUpData(userReference, cardId));
                }
                break;
            case 7:
                if (experience >= 400)
                {
                    currentLevel = 8;
                    experience -= 400;
                    StartCoroutine(LevelUpData(userReference, cardId));
                }
                break;
            case 8:
                if (experience >= 450)
                {
                    currentLevel = 9;
                    experience -= 450;
                    StartCoroutine(LevelUpData(userReference, cardId));
                }
                break;
            case 9:
                if (experience >= 500)
                {
                    currentLevel = 10;
                    experience -= 500;
                    StartCoroutine(LevelUpData(userReference, cardId));
                }
                break;

            default:
                break;
        }
        userReference.Child("userCards").Child(cardId).Child("Exp").SetValueAsync(experience).ContinueWithOnMainThread(t => { });
        userReference.Child("userCards").Child(cardId).Child("Level").SetValueAsync(currentLevel).ContinueWithOnMainThread(t => { });

    }

    private IEnumerator ChangeRatingData(DatabaseReference userReference, bool state)
    {
        var task = userReference.Child("rating").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception == null)
        {
            DataSnapshot rateSnapshot = task.Result;

            if (rateSnapshot.Exists)
            {
                int rateValue = Convert.ToInt32(rateSnapshot.Value);
                if (state)
                    rateValue += 3;
                else
                    rateValue -= 3;

                yield return userReference.Child("rating").SetValueAsync(rateValue).ContinueWithOnMainThread(t => { });
            }
            else
            {
                Debug.LogError("Rating value not found in user data.");
            }
        }
    }
    private IEnumerator ChangeResourcesData(DatabaseReference userReference)
    {
        foreach (ResourceInfo resInfo in levelDrops)
        {
            int chance = UnityEngine.Random.Range(1, 101);
            if (chance <= resInfo.chance)
            {
                var task = userReference.Child("resources").Child(resInfo.name).GetValueAsync();
                yield return new WaitUntil(() => task.IsCompleted);

                if (task.Exception == null)
                {
                    DataSnapshot resSnapshot = task.Result;

                    int resourceValue = UnityEngine.Random.Range(1, resInfo.value);

                    if (resSnapshot.Exists)
                    {
                        int resValue = Convert.ToInt32(resSnapshot.Value);
                        
                        resValue += resourceValue;

                        resources.Add(new ResourceInfo { name = resInfo.name, value = resourceValue });

                        yield return userReference.Child("resources").Child(resInfo.name).SetValueAsync(resValue).ContinueWithOnMainThread(t => { });
                    }
                    else
                    {
                        resources.Add(new ResourceInfo { name = resInfo.name, value = resourceValue });
                        yield return userReference.Child("resources").Child(resInfo.name).SetValueAsync(resourceValue).ContinueWithOnMainThread(t => { });
                    }
                }
            }
        }
    }

    private Sprite GetSpriteGem(string gem)
    {
        switch (gem)
        {
            case "waterHP":
                return Resources.Load<Sprite>("Sprites/Gems/WaterGemHP");
            case "waterAT":
                return Resources.Load<Sprite>("Sprites/Gems/WaterGemAT");
            case "airHP":
                return Resources.Load<Sprite>("Sprites/Gems/AirGemHP");
            case "airAT":
                return Resources.Load<Sprite>("Sprites/Gems/AirGemAT");
            case "fireHP":
                return Resources.Load<Sprite>("Sprites/Gems/FireGemHP");
            case "fireAT":
                return Resources.Load<Sprite>("Sprites/Gems/FireGemAT");
            case "earthHP":
                return Resources.Load<Sprite>("Sprites/Gems/EarthGemHP");
            case "earthAT":
                return Resources.Load<Sprite>("Sprites/Gems/EarthGemAT");
            default:
                return null;
        }
    }

    private IEnumerator UpdateLevels(DatabaseReference userReference)
    {
        int chapterNumber;
        int levelNumber;

        if (int.TryParse(chapterId.Replace("Chapter ", ""), out chapterNumber) &&
            int.TryParse(levelId.Replace("Level ", ""), out levelNumber))
        {
            // Ваша логика для изменения chapterId и levelId в соответствии с условиями
            if (levelNumber == 5)
            {
                chapterId = $"Chapter {chapterNumber + 1}";
                levelId = "Level 1";
            }
            else
            {
                levelId = $"Level {levelNumber + 1}";
            }
        }
        yield return userReference.Child("chapters").Child(chapterId).Child(levelId).SetValueAsync(true).ContinueWithOnMainThread(t => { });
    }
    private IEnumerator ResultScreenAnimation(bool state, bool arena)
    {
        if (state)
        {
            ResultTxt.text = "WIN";
            arenaPoints.text = "+3";
        }
        else
        {
            ResultTxt.text = "LOOSE";
            arenaPoints.text = "-3";
        }

        if (arena)
        {
            arenaLoot.SetActive(true);
        }
        else if (!arena && state)
        {
            foreach (ResourceInfo resource in resources)
            {
                ResInfo resInfo = Instantiate(resPref, resourcesGrid).GetComponent<ResInfo>();
                resInfo.SetBattleResource(resource.name, resource.value.ToString());

            }
            classicLoot.SetActive(true);

            /*
            if (itemRarity != null)
            {  
                gemRarity.text = itemRarity;
                gemImage.sprite = GetSpriteGem(itemPicture);
                gemLoot.SetActive(true);
            }*/


        }

        resultObject.SetActive(true);
        menuBtn.SetActive(true);


        yield return new WaitForSeconds(0.5f);

        
    }

}
