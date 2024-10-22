using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


[System.Serializable]
public enum QuestType
{
    Daily,
    MainPath,
    Order,
    BattlePass
}
[System.Serializable]
public enum QuestObjective
{
    KillPlayer,
    KillCreature,
    CompleteZone,
    ObtainDrop,
    ObtainItem,
    ObtainCard,
    UpgradeCard,
    UpgradeItem
}
[System.Serializable]
public class QuestTarget
{
    public int creatureId;
    public int zoneId;
    public string drop;
    public int cardType;
    public string cardRank;
    public string cardElement;
    public string itemType;
    public string itemRank;
}
[System.Serializable]
public class QuestReward
{
    public int coins;
    public int gems;
    public int experience;
    public string drop;
    public int dropAmount;
    public int unlockQuestId;
    public string unlockLocation;
    public int cardType;
    public string item;
    public GameObject rewardPrefab;
}

[System.Serializable]
public class QuestInfo
{
    public int questId;
    public QuestType questType;
    public QuestObjective objective;
    public QuestTarget target;
    public string description;

    public int targetAmount;
    public int completedAmount;
    public QuestReward reward;
    public bool complete;
    public GameObject questPrefab;

    public QuestInfo(int qID, QuestType type, QuestObjective obj, QuestTarget trgt, string descr, int targetAm, QuestReward rew, bool done)
    {
        questId = qID;
        questType = type;
        objective = obj;
        target = trgt;
        description = descr;
        targetAmount = targetAm;
        completedAmount = 0;
        reward = rew;
        complete = done;
    }
}