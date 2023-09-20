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

    GameObject textBox;
    GameObject optionPanel;
    static Story story;
    TextMeshProUGUI nametag;
    TextMeshProUGUI message;
    List<string> tags;
    static Choice choiceSelected;
    private GameObject runningDialogueBox;
    PlayerController playerController;
    NpcController npcController;
    Dictionary<int, List<Tuple<string, int>>> choiceRequirements = new Dictionary<int, List<Tuple<string, int>>>();

    public void setup(PlayerController playerController, NpcController npcController)
    {
        this.playerController = playerController;
        this.inkFile = npcController.ink_file;
        this.startupKnotName = npcController.ink_knot_name;
        this.npcController = npcController;
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
        nametag.text = npcController.firstname;
        if (startupKnotName != null && startupKnotName != "") story.ChoosePathString(startupKnotName);
        tags = new List<string>();
        is_talking = true;
        //runningDialogueBox.transform.Find("speaker_1").gameObject.GetComponent<Image>().sprite = playerController.avatar;
        runningDialogueBox.transform.Find("speaker_2").gameObject.GetComponent<Image>().sprite = npcController.avatar;
        AdvanceDialogue();
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
            temp.GetComponent<Selectable>().element = _choices[i];
            temp.GetComponent<Button>().onClick.AddListener(() => { temp.GetComponent<Selectable>().Decide(); });
            if (choiceRequirements.ContainsKey(i))
                foreach (Tuple<string, int> requirement in choiceRequirements[i])
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
                        case "Magnus":
                            if (playerController.relation_magnus < requirement.Item2)
                            {
                                text.text += " (relation to Magnus " + playerController.relation_magnus + "/" + requirement.Item2 + ")";
                                text.color = Color.gray;
                                temp.GetComponent<Button>().enabled = false;
                            }
                            break;
                        case "Queen":
                            if (playerController.relation_queen < requirement.Item2)
                            {
                                text.text += " (relation to queen " + playerController.relation_queen + "/" + requirement.Item2 + ")";
                                text.color = Color.gray;
                                temp.GetComponent<Button>().enabled = false;
                            }
                            break;
                        case "Villagers":
                            if (playerController.relation_villagers < requirement.Item2)
                            {
                                text.text += " (relation to villagers " + playerController.relation_villagers + "/" + requirement.Item2 + ")";
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
        ExecuteTags(true);
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
        tags = story.currentTags;
        ExecuteTags();
    }

    void ExecuteTags(bool postDecision = false)
    {
        if(!postDecision) choiceRequirements = new Dictionary<int, List<Tuple<string, int>>>();
        if (postDecision) Debug.Log("Parsuje wybory po decyzji");
        else Debug.Log("Parsuje wybory przed decyzją");
        foreach (string t in tags)
        {
            string[] words = t.Split('-');
            switch (words[0])
            {
                case "option":
                    int wchich_one = int.Parse(words[1]);
                    if (!choiceRequirements.ContainsKey(wchich_one)) choiceRequirements[wchich_one] = new List<Tuple<string, int>>();
                    switch (words[2])
                    {
                        //#option-1-requiresSkill-charisma-20
                        case "requiresSkill":
                            if(!postDecision) choiceRequirements[wchich_one].Add(new Tuple<string, int>(words[3], int.Parse(words[4])));
                            break;
                        //#option-1-requiresRelation-Magnus-20 (Magnus/Queen/Villagers)
                        case "requiresRelation":
                            if (!postDecision) choiceRequirements[wchich_one].Add(new Tuple<string, int>(words[3], int.Parse(words[4])));
                            break;
                        //# option-1-changesInkFile-Dialogues/sukmadik  (affects next dialogue)
                        case "changesInkFile":
                            if (postDecision)
                            {
                                if (choiceSelected.index == wchich_one)
                                {
                                    npcController.ink_file = Resources.Load(words[3]) as TextAsset;
                                }
                            }
                            break;
                        //# option-1-changesInkKnot-drugSelling  (affects next dialogue)
                        case "changesInkKnot":
                            if (postDecision)
                                if (choiceSelected.index == wchich_one)
                                    npcController.ink_knot_name = words[3];
                            break;
                        //# option-1-advanceTime-20
                        case "advanceTime":
                            if (postDecision)
                            {
                                if (choiceSelected.index == wchich_one)
                                {
                                    PersistanceController persistanceController = PersistanceController.GetInstance();
                                    persistanceController.AdvanceTime(int.Parse(words[3]));
                                }
                            }
                            break;
                        //# option-1-addItem-Items/pierogi-20
                        case "addItem":
                            if (postDecision)
                            {
                                if (choiceSelected.index == wchich_one)
                                {
                                    Item addItem = Resources.Load(words[3]) as Item;
                                    playerController.AddItem(addItem, int.Parse(words[4]));
                                }
                            }
                            break;
                        //# option-1-removeItem-Items/pierogi-20
                        case "removeItem":
                            if (postDecision)
                            {
                                if (choiceSelected.index == wchich_one)
                                {
                                    Item removeItem = Resources.Load(words[3]) as Item;
                                    playerController.RemoveItem(removeItem, int.Parse(words[4]));
                                }
                            }
                            break;
                        //# option-1-changeScene-DomSpokojnejAgonii
                        case "changeScene":
                            if (postDecision)
                            {
                                if (choiceSelected.index == wchich_one)
                                {
                                    FinishDialogue();
                                    playerController.ChangeScene(words[3]);
                                }
                            }
                            break;
                        //# option-1-changePlayerSprite-Images/UwU
                        case "changePlayerSprite":
                            if (postDecision)
                            {
                                if (choiceSelected.index == wchich_one)
                                {
                                    runningDialogueBox.transform.Find("speaker_1").gameObject.GetComponent<Image>().sprite = Resources.Load(words[3]) as Sprite;
                                }
                            }
                            break;
                        //# option-1-changeNpcSprite-Images/UsmiechnietaBara
                        case "changeNpcSprite":
                            if (postDecision)
                            {
                                if (choiceSelected.index == wchich_one)
                                {
                                    runningDialogueBox.transform.Find("speaker_2").gameObject.GetComponent<Image>().sprite = Resources.Load(words[3]) as Sprite;
                                }
                            }
                            break;
                        //# option-1-openAlchemyTable
                        case "openAlchemyTable":
                            if (postDecision)
                            {
                                if (choiceSelected.index == wchich_one)
                                {
                                    GameObject.Find("Managers").GetComponent<AlchemyController>().OpenAlchemyTable(playerController);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                default: break;
            }
        }
    }
}
