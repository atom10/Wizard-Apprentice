using Ink.Parsed;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public ButtonFunction buttonFunction;
    public GameObject loadGameWindow;

    public Sprite emptySaveSprite;
    public Sprite existingSaveSprite;
    public GameObject saveSlotPrefab;
    public int saveSlots = 3;

    Button button;

    private void Start()
    {
        button = gameObject.GetComponent<Button>();
        switch (buttonFunction)
        {
            case ButtonFunction.new_game:
                button.onClick.AddListener(() => { NewGame(); });
                break;
            case ButtonFunction.load_game:
                button.onClick.AddListener(() => { LoadGame(); });
                break;
            case ButtonFunction.exit:
                button.onClick.AddListener(() => { Exit(); });
                break;
        }
    }

    void NewGame()
    {
        SceneManager.LoadScene("WizardTower");
    }

    void LoadGame()
    {
        loadGameWindow.SetActive(true);
        List<int> activeSlots = PersistanceController.GetInstance().GetSaves();
        GameObject panel = loadGameWindow.transform.Find("Panel").gameObject;
        for (int i = 0; i < panel.transform.childCount; i++)
        {
            Destroy(panel.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < saveSlots; i++) {
            GameObject saveSlot = Instantiate(saveSlotPrefab, panel.transform);
            saveSlot.GetComponent<SimpleValueStorage>().number = i;
            saveSlot.transform.Find("Label/text").GetComponent<TextMeshProUGUI>().text = "Save " + (i+1).ToString();
        }
        for (int slot = 0; slot < panel.transform.childCount; ++slot)
        {
            GameObject saveSlot = panel.transform.GetChild(slot).gameObject;
            saveSlot.transform.Find("Label").GetComponent<Button>().onClick.AddListener(() =>
            {
                PersistanceController.GetInstance().Load(saveSlot.GetComponent<SimpleValueStorage>().number);
            });
            saveSlot.transform.Find("Delete").GetComponent<Button>().onClick.AddListener(() =>
            {
                List<int> saveNumbers = PersistanceController.GetInstance().GetSaves();
                saveNumbers.Remove(saveSlot.GetComponent<SimpleValueStorage>().number);
                PersistanceController.GetInstance().UpdateMetadata(saveNumbers);
                SetSaveSlotEnabled(saveSlot, false);
            });
            SetSaveSlotEnabled(saveSlot, activeSlots.Contains(saveSlot.GetComponent<SimpleValueStorage>().number));
        }
    }

    private void SetSaveSlotEnabled(GameObject saveSlot, bool enabled)
    {
        saveSlot.transform.Find("Label").GetComponent<Button>().enabled = enabled;
        saveSlot.transform.Find("Delete").GetComponent<Button>().enabled = enabled;
        saveSlot.transform.Find("Image").GetComponent<Image>().sprite = enabled ? existingSaveSprite : emptySaveSprite;
    }

    void Exit()
    {
        Application.Quit();
    }
}

public enum ButtonFunction
{
    new_game,
    load_game,
    exit
}