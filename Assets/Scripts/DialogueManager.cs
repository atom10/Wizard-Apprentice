using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;
using TMPro;
using Unity.VisualScripting;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueBox;
    public GameObject customButton;

    [HideInInspector]
    public TextAsset inkFile;
    [HideInInspector]
    public bool isTalking = false;
    [HideInInspector]
    public string startupKnotName = "";
    [HideInInspector]
    public string speaker_name = "";
    [HideInInspector]
    public Sprite speaker2_icon;

    GameObject textBox;
    GameObject optionPanel;
    static Story story;
    TextMeshProUGUI nametag;
    TextMeshProUGUI message;
    List<string> tags;
    static Choice choiceSelected;
    private GameObject runningDialogueBox;
    PlayerController playerController;

    void Start() {
        playerController = GetComponent<PlayerController>();
    }

    public void Talk() {
        playerController.can_move = false;
        runningDialogueBox = Instantiate(dialogueBox, transform);
        textBox = runningDialogueBox.transform.Find("Text_box").gameObject;
        optionPanel = textBox.transform.Find("Choices").gameObject;
        story = new Story(inkFile.text);
        nametag = textBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        message = textBox.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        nametag.text = speaker_name;
        if(startupKnotName != null && startupKnotName != "" ) story.ChoosePathString(startupKnotName);
        tags = new List<string>();
        choiceSelected = null;
        isTalking = true;
        runningDialogueBox.transform.Find("speaker_2").gameObject.GetComponent<Image>().sprite=speaker2_icon;
        AdvanceDialogue();
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.X))
        {
            var num = optionPanel.transform.childCount;
            Debug.Log(num);
        }
        if(Input.GetKeyDown(KeyCode.E) && isTalking && optionPanel.transform.childCount == 0) {
            if (story.canContinue) {
                AdvanceDialogue();
            } else {
                FinishDialogue();
            }
        }
    }

    private void FinishDialogue() {
        Debug.Log("End of Dialogue!");
        Destroy(runningDialogueBox);
        isTalking = false;
        playerController.can_move = true;
    }

    void AdvanceDialogue() {
        string currentSentence = story.Continue();
        ParseTags();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
        int number_of_childs = optionPanel.transform.childCount;
        for (int i = 0; i < number_of_childs; i++) {
            Destroy(optionPanel.transform.GetChild(i).gameObject);
        }
        if (story.currentChoices.Count != 0) {
            StartCoroutine(ShowChoices());
        }
    }

    IEnumerator TypeSentence(string sentence) {
        message.text = "";
        foreach(char letter in sentence.ToCharArray()) {
            message.text += letter;
            yield return null;
        }
        yield return null;
    }

    IEnumerator ShowChoices() {
        Debug.Log("There are choices need to be made here!");
        List<Choice> _choices = story.currentChoices;

        for (int i = 0; i < _choices.Count; i++) {
            GameObject temp = Instantiate(customButton, optionPanel.transform);
            temp.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = _choices[i].text;
            temp.AddComponent<Selectable>();
            temp.GetComponent<Selectable>().element = _choices[i];
            temp.GetComponent<Button>().onClick.AddListener(() => { temp.GetComponent<Selectable>().Decide(); });
        }
        optionPanel.SetActive(true);
        yield return new WaitUntil(() => { return choiceSelected != null; });
        AdvanceFromDecision();
    }

    public static void SetDecision(object element) {
        choiceSelected = (Choice)element;
        story.ChooseChoiceIndex(choiceSelected.index);
    }

    void AdvanceFromDecision() {
        optionPanel.SetActive(false);
        choiceSelected = null;
        AdvanceDialogue();
    }

    void ParseTags() {
        tags = story.currentTags;
        foreach (string t in tags) {
            string prefix = t.Split(' ')[0];
            string param = "";
            try {
                param = t.Split(' ')[1];
            } catch(Exception e) {
                Debug.LogException(e);
            }

            switch(prefix.ToLower()) {
                case "color":
                    SetTextColor(param);
                    break;
            }
        }
    }
    void SetTextColor(string _color) {
        switch(_color) {
            case "red":
                message.color = Color.red;
                break;
            case "blue":
                message.color = Color.cyan;
                break;
            case "green":
                message.color = Color.green;
                break;
            case "white":
                message.color = Color.white;
                break;
            default:
                Debug.Log($"{_color} is not available as a text color");
                break;
        }
    }
}
