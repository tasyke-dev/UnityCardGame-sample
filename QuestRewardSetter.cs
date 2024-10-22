using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestRewardSetter : MonoBehaviour
{
    public Image rewardImage;
    public TextMeshProUGUI rewardAmount;

    public void SetQuestReward(Sprite sprite, int amount)
    {
        rewardImage.sprite = sprite;
        rewardAmount.text = "x"+amount.ToString();
    }
}
