using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseScreenController : MonoBehaviour
{
    public GameObject pauseScreenBoxPrefab;
    GameObject pauseScreenBox;
    GameObject saveGameScreen;

    public Sprite emptySaveSprite;
    public Sprite existingSaveSprite;
    public void ShowPauseScreen(PlayerController playerController)
    {
        if (pauseScreenBox != null)
        {
            pauseScreenBox.SetActive(true);
        }
        else
        {
            pauseScreenBox = Instantiate(pauseScreenBoxPrefab);
            saveGameScreen = pauseScreenBox.transform.Find("SaveGameScreen").gameObject;
            pauseScreenBox.GetComponent<Canvas>().worldCamera = Camera.main;

            pauseScreenBox.transform.Find("resume").GetComponent<Button>().onClick.AddListener(() =>
            {
                pauseScreenBox.SetActive(false);
                playerController.CanMove(true);
            });

            pauseScreenBox.transform.Find("saveGame").GetComponent<Button>().onClick.AddListener(() =>
            {
                saveGameScreen.SetActive(true);
                List<int> activeSlots = PersistanceController.GetInstance().GetSaves();
                GameObject panel = saveGameScreen.transform.Find("Panel").gameObject;
                for (int slot = 0; slot < panel.transform.childCount; ++slot)
                {
                    GameObject saveSlot = panel.transform.GetChild(slot).gameObject;
                    saveSlot.transform.Find("Image").GetComponent<Image>().sprite = activeSlots.Contains(saveSlot.GetComponent<SimpleValueStorage>().number) ? existingSaveSprite : emptySaveSprite;
                    saveSlot.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        PersistanceController.GetInstance().Save(saveSlot.GetComponent<SimpleValueStorage>().number);
                        playerController.CanMove(true);
                        Destroy(pauseScreenBox);
                    });
                }
            });

            pauseScreenBox.transform.Find("mainMenu").GetComponent<Button>().onClick.AddListener(() =>
            {
                PersistanceController.GetInstance().currentSave = new SaveFilePacket();
                SceneManager.LoadScene("MainMenuScene");
                Destroy(pauseScreenBox);
            });

            pauseScreenBox.transform.Find("exitGame").GetComponent<Button>().onClick.AddListener(() =>
            {
                Application.Quit();
            });

            List<int> activeSlots = PersistanceController.GetInstance().GetSaves();
            GameObject panel = saveGameScreen.transform.Find("Panel").gameObject;
            for (int slot = 0; slot < panel.transform.childCount; ++slot)
            {
                panel.transform.GetChild(slot).Find("Button").GetComponent<Button>().onClick.AddListener(() =>
                {

                });
            }
        }
        playerController.CanMove(false);
    }
}
