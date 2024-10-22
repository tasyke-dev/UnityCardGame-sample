using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChapterWindow : MonoBehaviour
{
    public string chapterdId;
    public List<ResourceInfo> chapterDrops;
    public TextMeshProUGUI chapterNumber;
    public TextMeshProUGUI cardName;
    public UnityEngine.UI.Image cardImage;

    public Transform dropGrid;
    public Transform levelGrid;

    public GameObject dropImagePrefab;

    public GameObject levelPrefab;
    public List<LevelSetter> levelInfos;

    public GameObject mapWindow;
    public GameObject mapBtn;
    public GameObject chapterWindow;

    public void SetChapterWindow(ChapterInfo chapter)
    {
        chapterdId = chapter.chapterId;
        chapterNumber.text = chapterdId;
        cardName.text = chapter.chapterName;
        chapterDrops = chapter.chapterDrops;
        cardImage.sprite = Resources.Load<Sprite>(chapter.chapterImage);

        foreach (Transform child in levelGrid)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in dropGrid)
        {
            Destroy(child.gameObject);
        }


        foreach (LevelInfo level in chapter.chapterLevels)
        {
            LoadLevelsToWindow(level);
        }

        foreach (ResourceInfo drop in chapterDrops)
        {
            Sprite dropSprite = Resources.Load<Sprite>("Sprites/"+drop.name);
            GameObject dropImageObject = Instantiate(dropImagePrefab, dropGrid);
            UnityEngine.UI.Image dropImage = dropImageObject.GetComponent<UnityEngine.UI.Image>();
            dropImage.sprite = dropSprite;
        }
        GameLoader.Instance.UpdateChapterDrops(chapterDrops);
    }


    public void LoadLevelsToWindow(LevelInfo level)
    {
        if (level != null)
        {
            level.levelPrefab = Instantiate(levelPrefab, levelGrid, false);

            LevelSetter component = level.levelPrefab.GetComponent<LevelSetter>();
            levelInfos.Add(component);
            if (component != null)
            {
                component.SetLevel(chapterdId, level);
            }

            level.levelPrefab.transform.SetParent(levelGrid.transform, false);
        }

    }

    public void CloseWindow()
    {
        mapWindow.SetActive(true);
        mapBtn.SetActive(true);
        chapterWindow.SetActive(false);
    }
}
