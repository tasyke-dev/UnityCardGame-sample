using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LevelSetter : MonoBehaviour
{
    public LevelInfo levelInfo;
    public string chapterId;
    public string levelId;
    public TextMeshProUGUI levelId_TMP;
    public TextMeshProUGUI levelRank_TMP;
    public List<CardMenu> levelCards;
    public bool bossLevel;
    public Button levelButton;



    public void SetLevel(string chapterdId, LevelInfo level)
    {
        levelInfo = level;
        chapterId = chapterdId;
        levelId = level.levelId;
        levelId_TMP.text = levelId;
        levelRank_TMP.text = level.levelRank;
        levelCards = level.levelCards;
        bossLevel = level.bossLevel;
        levelButton.onClick.AddListener(OnLevelButtonClick);

        LevelManager.Instance.CheckLevelStatus(chapterdId, levelId, isCompleted =>
        {
            levelButton.interactable = isCompleted;
        });

        // Добавляем слушатель события для кнопки
        levelButton.onClick.AddListener(OnLevelButtonClick);

        // Вызываем метод для открытия окна уровня только если кнопка разблокирована
        if (!levelButton.interactable)
        {
            levelButton.onClick.RemoveListener(OnLevelButtonClick);
        }
    }

    void OnLevelButtonClick()
    {
        GameLoader.Instance.SetLevelWindow(chapterId, levelInfo);
    }


}
