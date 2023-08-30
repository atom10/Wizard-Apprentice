using Ink.Parsed;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public ButtonFunction buttonFunction;
    public GameObject loadGameWindow;

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
        for (int slot = 0; slot < panel.transform.childCount; ++slot)
        {
            GameObject saveSlot = panel.transform.GetChild(slot).gameObject;
            saveSlot.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
            {
                PersistanceController.GetInstance().Load(saveSlot.GetComponent<SimpleValueStorage>().number);
            });
            if (!activeSlots.Contains(saveSlot.GetComponent<SimpleValueStorage>().number))
            {
                saveSlot.transform.Find("Button").GetComponent<Button>().enabled = false;
            }
        }
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