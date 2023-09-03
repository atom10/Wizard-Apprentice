using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueBox;
    public GameObject customButton;

    TextAsset inkFile;
    bool is_talking = false;
    string startupKnotName = "";
    string speaker_name = "";
    Sprite speaker2_icon;

    GameObject textBox;
    GameObject optionPanel;
    static Story story;
    TextMeshProUGUI nametag;
    TextMeshProUGUI message;
    List<string> tags;
    static Choice choiceSelected;
    private GameObject runningDialogueBox;
    PlayerController playerController;

    Dictionary<int, List<Tuple<string, int>>> choiceSkillRequirements = new Dictionary<int, List<Tuple<string, int>>>();

    void Start()
    {
    }

    public void setup(PlayerController playerController, TextAsset inkFile, String startupKnotName, String speaker_name, Sprite speaker2_icon)
    {
        this.playerController = playerController;
        this.inkFile = inkFile;
        this.startupKnotName = startupKnotName;
        this.speaker_name = speaker_name;
        this.speaker2_icon = speaker2_icon;
    }

    public void Talk()
    {
        playerController.CanMove(false);
        runningDialogueBox = Instantiate(dialogueBox, transform);
        textBox = runningDialogueBox.transform.Find("Text_box").gameObject;
        optionPanel = textBox.transform.Find("Choices").gameObject;
        story = new Story(inkFile.text);
        nametag = textBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        message = textBox.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        nametag.text = speaker_name;
        if (startupKnotName != null && startupKnotName != "") story.ChoosePathString(startupKnotName);
        tags = new List<string>();
        choiceSelected = null;
        is_talking = true;
        runningDialogueBox.transform.Find("speaker_2").gameObject.GetComponent<Image>().sprite = speaker2_icon;
        //AdvanceDialogue();
    }

    public bool isTalking()
    {
        return is_talking;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && is_talking && optionPanel.transform.childCount == 0)
        {
            if (story.canContinue) AdvanceDialogue();
            else FinishDialogue();
        }
    }

    private void FinishDialogue()
    {
        Debug.Log("End of Dialogue!");
        Destroy(runningDialogueBox);
        is_talking = false;
        playerController.CanMove(true);
    }

    void AdvanceDialogue()
    {
        string currentSentence = story.Continue();
        ParseTags();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
        int number_of_childs = optionPanel.transform.childCount;
        for (int i = 0; i < number_of_childs; i++)
            Destroy(optionPanel.transform.GetChild(i).gameObject);
        if (story.currentChoices.Count != 0)
            StartCoroutine(ShowChoices());
    }

    IEnumerator TypeSentence(string sentence)
    {
        message.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            message.text += letter;
            yield return null;
        }
        yield return null;
    }

    IEnumerator ShowChoices()
    {
        Debug.Log("There are choices need to be made here!");
        List<Choice> _choices = story.currentChoices;

        for (int i = 0; i < _choices.Count; i++)
        {
            GameObject temp = Instantiate(customButton, optionPanel.transform);
            TextMeshProUGUI text = temp.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            text.text = _choices[i].text;
            temp.AddComponent<Selectable>();
            temp.GetComponent<Selectable>().element = _choices[i];
            temp.GetComponent<Button>().onClick.AddListener(() => { temp.GetComponent<Selectable>().Decide(); });
            if (choiceSkillRequirements.ContainsKey(i))
                foreach (Tuple<string, int> requirement in choiceSkillRequirements[i])
                {
                    switch (requirement.Item1)
                    {
                        case "charisma":
                            if(playerController.charisma < requirement.Item2)
                            {
                                text.text += " (charisma " + playerController.charisma + "/" + requirement.Item2 + ")";
                                text.color = Color.gray;
                                temp.GetComponent<Button>().enabled = false;
                            }
                            break;
                        default:
                            break;
                    }
                }
        }
        optionPanel.SetActive(true);
        yield return new WaitUntil(() => { return choiceSelected != null; });
        AdvanceFromDecision();
    }

    public static void SetDecision(object element)
    {
        choiceSelected = (Choice)element;
        story.ChooseChoiceIndex(choiceSelected.index);
    }

    void AdvanceFromDecision()
    {
        optionPanel.SetActive(false);
        choiceSelected = null;
        AdvanceDialogue();
    }

    void ParseTags()
    {
        choiceSkillRequirements = new Dictionary<int, List<Tuple<string, int>>>();
        tags = story.currentTags;
        foreach (string t in tags)
        {
            string[] words = t.Split('-');
            switch (words[0]) {
                case "option":
                    int wchich_one = Int32.Parse(words[1]);
                    switch (words[2])
                    {
                        case "requiresSkill":
                            if(!choiceSkillRequirements.ContainsKey(wchich_one)) choiceSkillRequirements[wchich_one] = new List<Tuple<string, int>>();
                            choiceSkillRequirements[wchich_one].Add(new Tuple<string, int>(words[3], int.Parse(words[4])));
                            break;
                        default:
                            break;
                    }
                    break;
                default: break;
            }
        }
    }
    void SetTextColor(string _color)
    {
        switch (_color)
        {
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
