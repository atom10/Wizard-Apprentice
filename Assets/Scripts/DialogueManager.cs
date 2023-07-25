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
    public TextAsset inkFile;
    public GameObject textBox;
    public GameObject customButton;
    public GameObject optionPanel;
    public bool isTalking = false;
    public string startupKnotName = null;

    static Story story;
    TextMeshProUGUI nametag;
    TextMeshProUGUI message;
    List<string> tags;
    static Choice choiceSelected;

    void Start() {
        story = new Story(inkFile.text);
        nametag = textBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        message = textBox.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        nametag.text = "eduardo";
        if(startupKnotName != null && startupKnotName != "" ) story.ChoosePathString(startupKnotName);
        tags = new List<string>();
        choiceSelected = null;
        AdvanceDialogue();
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.E)) {
            if(story.canContinue) {
                AdvanceDialogue();
            } else {
                FinishDialogue();
            }
        }
    }

    private void FinishDialogue() {
        Debug.Log("End of Dialogue!");
    }

    void AdvanceDialogue() {
        string currentSentence = story.Continue();
        ParseTags();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
        for (int i = 0; i < optionPanel.transform.childCount; i++) {
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
        //CharacterScript tempSpeaker = GameObject.FindObjectOfType<CharacterScript>();
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
        Debug.Log("activated panel");
        yield return new WaitUntil(() => { return choiceSelected != null; });
        AdvanceFromDecision();
    }

    public static void SetDecision(object element) {
        choiceSelected = (Choice)element;
        story.ChooseChoiceIndex(choiceSelected.index);
    }

    void AdvanceFromDecision() {
        optionPanel.SetActive(false);
        Debug.Log("deactivated panel");
        choiceSelected = null; // Forgot to reset the choiceSelected. Otherwise, it would select an option without player intervention.
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
                Debug.Log(e);
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
