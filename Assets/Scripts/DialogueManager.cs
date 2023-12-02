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
    Dictionary<int, List<Tuple<string, int>>> choiceGenericRequirements = new Dictionary<int, List<Tuple<string, int>>>(); //(requirementName, value)
    Dictionary<int, List<Tuple<string, int>>> choiceItemRequirements = new Dictionary<int, List<Tuple<string, int>>>(); //(resourcePath, amount)
    Dictionary<int, List<Tuple<int, int>>> choiceHourRequirements = new Dictionary<int, List<Tuple<int, int>>>(); //(from, to)
    Dictionary<int, string> alternativeKnot = new Dictionary<int, string>();

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
        runningDialogueBox = Instantiate(dialogueBox);
        textBox = runningDialogueBox.transform.Find("Text_box").gameObject;
        optionPanel = textBox.transform.Find("Choices/Viewport/Content").gameObject;
        story = new Story(inkFile.text);
        nametag = textBox.transform.Find("npc_name").GetComponent<TextMeshProUGUI>();
        message = textBox.transform.Find("npc_sentence/Viewport/Content").GetComponent<TextMeshProUGUI>();
        nametag.text = npcController.firstname;
        if (startupKnotName != null && startupKnotName != "") story.ChoosePathString(startupKnotName);
        tags = new List<string>();
        is_talking = true;
        runningDialogueBox.transform.Find("speaker_1").gameObject.GetComponent<Image>().sprite = playerController.avatar;
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
    void AdvanceDialogue() {
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
    IEnumerator TypeSentence(string sentence) {
        message.text = "";
        foreach (char letter in sentence.ToCharArray()) {
            message.text += letter;
            yield return null;
        }
        yield return null;
    }
    IEnumerator ShowChoices() {
        List<Choice> _choices = story.currentChoices;
        Dictionary<int, bool> choiceDisabledButHadAlternateRoute = new Dictionary<int, bool>();
        for (int i = 0; i < _choices.Count; i++) {
            GameObject temp = Instantiate(customButton, optionPanel.transform);
            TextMeshProUGUI text = temp.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            text.text = _choices[i].text;
            string originalText = text.text;
            temp.GetComponent<Selectable>().element = _choices[i];
            temp.GetComponent<Button>().onClick.AddListener(() => { temp.GetComponent<Selectable>().Decide(); });
            if (choiceGenericRequirements.ContainsKey(i))
            {
                foreach (Tuple<string, int> requirement in choiceGenericRequirements[i])
                {
                    switch (requirement.Item1)
                    {
                        case "charisma":
                            if (playerController.charisma < requirement.Item2)
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
                        case "requiresFlag":
                            if (!PersistanceController.GetInstance().CheckEventFlag(requirement.Item2))
                            {
                                text.color = Color.gray;
                                temp.GetComponent<Button>().enabled = false;
                            }
                            break;
                        case "beforeDay":
                            if (PersistanceController.GetInstance().currentSave.day >= requirement.Item2)
                            {
                                text.text += " (too late)";
                                text.color = Color.gray;
                                temp.GetComponent<Button>().enabled = false;
                            }
                            break;
                        case "afterDay":
                            if (PersistanceController.GetInstance().currentSave.day <= requirement.Item2)
                            {
                                text.text += " (too early)";
                                text.color = Color.gray;
                                temp.GetComponent<Button>().enabled = false;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            if (choiceItemRequirements.ContainsKey(i))
            {
                foreach (Tuple<string, int> requirement in choiceItemRequirements[i])
                {
                    Debug.Log("Item required: " + requirement.Item1);
                    Item item = Resources.Load<Item>(requirement.Item1);
                    if(item == null) { Debug.Log("Wymagany przedmiot nie znaleziony w zasobach gry"); }
                    List<Item_entry> inventory = playerController.GetInventoryContainer();
                    int index = inventory.FindIndex((Item_entry item_entry) => { return item_entry.item == item; });
                    if (index != -1)
                    {
                        if (inventory[index].amount >= requirement.Item2) continue;
                    }
                    text.text += " (" + item.name + " " + (index == -1 ? 0 : inventory[index].amount) + "/" + requirement.Item2 + ")";
                    text.color = Color.gray;
                    temp.GetComponent<Button>().enabled = false;
                }
            }
            if (choiceHourRequirements.ContainsKey(i)) {
                bool requiremrntMet = false;
                if (choiceHourRequirements[i].Count == 0) requiremrntMet = true;
                int hour = PersistanceController.GetInstance().currentSave.hour;
                foreach (Tuple<int, int> requirement in choiceHourRequirements[i])
                {
                    if (requirement.Item1 <= hour && hour <= requirement.Item2)
                        requiremrntMet = true;
                }
                if(!requiremrntMet)
                {
                    text.text += " (Can't tell right now)";
                    text.color = Color.gray;
                    temp.GetComponent<Button>().enabled = false;
                }
            }
            if (alternativeKnot.ContainsKey(i) && text.enabled == false)
            {
                text.text = originalText + " (!)";
                text.enabled = true;
                choiceDisabledButHadAlternateRoute[i] = true;
            }
        }
        optionPanel.SetActive(true);
        yield return new WaitUntil(() => { return choiceSelected != null; });
        if (choiceDisabledButHadAlternateRoute.ContainsKey(choiceSelected.index)) story.ChoosePathString(alternativeKnot[choiceSelected.index]);
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
        if (!postDecision)
        {
            choiceGenericRequirements = new Dictionary<int, List<Tuple<string, int>>>();
            choiceItemRequirements = new Dictionary<int, List<Tuple<string, int>>>(); //Clean old requirements before parsing
            choiceHourRequirements = new Dictionary<int, List<Tuple<int, int>>>();
        }
        foreach (string t in tags)
        {
            string[] words = t.Split('-');
            switch (words[0])
            {
				//#changeSpeakerName-Magnus
				case "changeSpeakerName":
					if(!postDecision)
						nametag.text = words[1];
					break;
                //Option cases
                case "option":
                    int wchich_one = int.Parse(words[1]);
                    if (!choiceGenericRequirements.ContainsKey(wchich_one))
                    {
                        choiceGenericRequirements[wchich_one] = new List<Tuple<string, int>>();
                        choiceItemRequirements[wchich_one] = new List<Tuple<string, int>>();
                        choiceHourRequirements[wchich_one] = new List<Tuple<int, int>>();
                    }
                    switch (words[2])
                    {
                        //#option-1-requiresSkill-charisma-20 (only charisma for now ¯\_(ツ)_/¯ )
                        case "requiresSkill":
                            if(!postDecision) choiceGenericRequirements[wchich_one].Add(new Tuple<string, int>(words[3], int.Parse(words[4])));
                            break;
                        //#option-1-requiresRelation-Magnus-20 (Magnus/Queen/Villagers)
                        case "requiresRelation":
                            if (!postDecision) choiceGenericRequirements[wchich_one].Add(new Tuple<string, int>(words[3], int.Parse(words[4])));
                            break;
                        //#option-1-changesRelation-Magnus-20-(minus/plus) (Magnus/Queen/Villagers)
                        case "changesRelation":
                            
                                if (postDecision)
                                {
                                    if (choiceSelected.index == wchich_one)
                                    {
                                        int relationInfluence = int.Parse(words[4]) * (words[5].Equals("minus") ? -1 : 1);
                                        switch(words[3])
                                        {
                                            case "Magnus":
                                                playerController.relation_magnus += relationInfluence;
                                                break;
                                            case "Queen":
                                                playerController.relation_queen += relationInfluence;
                                                break;
                                            case "Villagers":
                                                playerController.relation_villagers += relationInfluence;
                                                break;
                                        }
                                    }
                                }
                            
                            break;
                        //# option-1-changesInkFile-Dialogues/sukmadik  (affects next dialogue chain)
                        case "changesInkFile":
                            if (postDecision)
                            {
                                if (choiceSelected.index == wchich_one)
                                {
                                    npcController.ink_file = Resources.Load(words[3]) as TextAsset;
                                }
                            }
                            break;
                        //# option-1-forceCloseDialogue
                        case "forceCloseDialogue":
                            if (postDecision)
                            {
                                FinishDialogue();
                            }
                            break;
                        //# option-1-changesInkKnot-powitanie  (affects next dialogue chain)
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
                            } else
                            {
                                choiceItemRequirements[wchich_one].Add(new Tuple<string, int>(words[3], int.Parse(words[4])));
                            }
                            break;
                        //# option-1-requireItem-Items/pierogi-20
                        case "requireItem":
                            if (!postDecision)
                            {
                                choiceItemRequirements[wchich_one].Add(new Tuple<string, int>(words[3], int.Parse(words[4])));
                            }
                            break;
                        //# option-1-changeScene-DomSpokojnejStarosci
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
                        //# option-1-requiresFlag-124
                        case "requiresFlag":
                            if (!postDecision) choiceGenericRequirements[wchich_one].Add(new Tuple<string, int>(words[2], int.Parse(words[3])));
                            break;
                        //# option-1-setsFlag-124-true (true/false)
                        case "setsFlag":
                            //Może i mało czytelne ale za to podatne na błędy
                            if (postDecision)
                                if (choiceSelected.index == wchich_one) PersistanceController.GetInstance().SetEventFlag(wchich_one, words[4] == "true" ? true : false);
                            break;
                        //# option-1-requiresTime-beforeDay-3 (beforeDay/afterDay)
                        //# option-1-requiresTime-betweenHours-12-16 (can be used multiple times for different ranges eq. 2-5 7-11)
                        case "requiresTime":
                            switch (words[3])
                            {
                                case "beforeDay":
                                    if (!postDecision) choiceGenericRequirements[wchich_one].Add(new Tuple<string, int>("beforeDay", int.Parse(words[4])));
                                    break;
                                case "afterDay":
                                    if (!postDecision) choiceGenericRequirements[wchich_one].Add(new Tuple<string, int>("afterDay", int.Parse(words[4])));
                                    break;
                                case "betweenHours":
                                    if (!postDecision) choiceHourRequirements[wchich_one].Add(new Tuple<int, int>(int.Parse(words[4]), int.Parse(words[5])));
                                    break;
                                default:
                                    break;
                            }
                            break;
                        //# option-1-alternativeKnotName-gtfo (knot to be loaded when requirements are not met)
                        case "alternativeKnotName":
                            alternativeKnot[wchich_one] = words[3];
                            break;
                        //# option-1-addJournalEntry-content
                        case "addJournalEntry":
                            if(postDecision)
                                if (choiceSelected.index == wchich_one)
                                    PersistanceController.GetInstance().currentSave.journal.Add(words[3]);
                            break;
                        //# option-1-addGatherQuest-(resource/path)
                        case "addGatherQuest":
                            if (postDecision)
                                if (choiceSelected.index == wchich_one)
                                    PersistanceController.GetInstance().currentSave.gatherQuests.Add(Resources.Load<GatherQuest>(words[3]));
                            break;
                        //# option-1-removeGatherQuest-(resource/path)
                        case "removeGatherQuest":
                            if (postDecision)
                                if (choiceSelected.index == wchich_one)
                                    if(PersistanceController.GetInstance().currentSave.gatherQuests.Contains(Resources.Load<GatherQuest>(words[3])))
                                    {
                                        PersistanceController.GetInstance().currentSave.gatherQuests.Remove(Resources.Load<GatherQuest>(words[3]));
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
