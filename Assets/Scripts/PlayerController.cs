using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class PlayerController : MonoBehaviour
{

    public List<AudioClip> sounds;
    public float health = 50;
    public float max_health = 100;

    public float moveSpeed;
    public LayerMask solidObjectsLayer;
    public LayerMask interactableObjectsLayer;
    public GameObject health_bar_fill;
    public GameObject mapPrefab;
    public Sprite avatar;
    public Material outlineMaterial;

    Dictionary<Renderer, Material> outlinedObjects = new Dictionary<Renderer, Material>(); // outlined object renderer, original material
    int cant_move_sources = 0;
    bool isMoving;
    Vector2 input;
    Animator animator;
    bool just_interacted = false;
    [SerializeField]
    [HideInInspector]
    List<Item_entry> inventory = new List<Item_entry>();
    GameObject map;
    bool playingStepSound = false;

    [HideInInspector]
    public int charisma;
    [HideInInspector]
    public int relation_magnus;
    [HideInInspector]
    public int relation_queen;
    [HideInInspector]
    public int relation_villagers;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnDisable()
    {
        PersistanceController.GetInstance().RememberMe(this);
    }

    public void ChangeScene(string scene)
    {
        PersistanceController persistanceController = PersistanceController.GetInstance();
        persistanceController.RememberMe(this, scene);
        SceneManager.LoadScene(scene);
    }

    IEnumerator Sound(AudioClip sound){
        playingStepSound = true;
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(sound);
        yield return new WaitForSeconds(sound.length);
        playingStepSound = false;
        yield return null;
    }
    void Update()
    {
        
        if(isMoving && !playingStepSound){
        AudioClip chosenSound = sounds[UnityEngine.Random.Range(0, sounds.Count-1)];
        StartCoroutine(Sound(chosenSound));
        }
        if (health_bar_fill != null)
        {
            health_bar_fill.transform.localScale = new Vector3(health / max_health, 1, 1);
        }
        animator.SetBool("isMoving", isMoving);
        if (cant_move_sources == 0)
        {
            if (!isMoving)
            {
                input.x = Input.GetAxisRaw("Horizontal");
                input.y = Input.GetAxisRaw("Vertical");
                if (input != Vector2.zero)
                {
                    var targetPos = transform.position;
                    targetPos.x += input.x;
                    targetPos.y += input.y;
                    animator.SetFloat("moveX", input.x);
                    animator.SetFloat("moveY", input.y);
                    if (input.x < 0) this.transform.localScale = new Vector3(-1, 1, 1);
                    else this.transform.localScale = new Vector3(1, 1, 1);
                    if (IsWalkable(targetPos))
                        StartCoroutine(Move(targetPos));
                }
            }

            if (just_interacted)
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.M)) return;
                just_interacted = false;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
                var interactPos = transform.position + facingDir;
                Debug.DrawLine(transform.position, interactPos, Color.red, 1f);
                var collider = Physics2D.OverlapCircle(interactPos, 0.2f, interactableObjectsLayer);
                if (collider != null)
                {
                    Interact interact = collider.GetComponent<Interact>();
                    if (interact != null) { interact.Interact(gameObject); }
                }
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                OpenInventory();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Pause();
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                if (map == null)
                {
                    cant_move_sources += 1;
                    map = Instantiate(mapPrefab);
                    Canvas canvas = map.GetComponent<Canvas>();
                    canvas.worldCamera = Camera.main;
                } else
                {
                    Destroy(map);
                }
                just_interacted = true;
            }
        } else if (map != null && Input.GetKeyDown(KeyCode.M))
        {
            cant_move_sources -= 1;
            Destroy(map);
        }

        Vector3 camera_new_position = Camera.main.transform.position;
        camera_new_position.x = transform.position.x;
        camera_new_position.y = transform.position.y;
        Camera.main.transform.position = camera_new_position; //Instant camera
        //Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, camera_new_position, moveSpeed * Time.deltaTime); //Smooth camera

        var ip = transform.position + new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        Debug.DrawLine(transform.position, ip, Color.yellow, 1f);
        var cl = Physics2D.OverlapCircle(ip, 0.2f, interactableObjectsLayer);
        if (cl != null)
        {
            Renderer r = cl.GetComponent<SpriteRenderer>();
            if(!outlinedObjects.ContainsKey(r))
            {
                outlinedObjects.Add(r, r.material);
                r.material = outlineMaterial;
            }
        } else
        {
            foreach(var materialToRestore in outlinedObjects)
            {
                if(materialToRestore.Key != null)
                {
                    materialToRestore.Key.material = materialToRestore.Value;
                }
                
            }
            outlinedObjects.Clear();
        }
    }

    private void Pause()
    {
        GameObject managers = GameObject.Find("Managers");
        PauseScreenController pauseScreenController = managers.GetComponent<PauseScreenController>();
        pauseScreenController.ShowPauseScreen(this);
    }

    public ref List<Item_entry> GetInventoryContainer()
    {
        return ref inventory;
    }

    private void OpenInventory()
    {
        GameObject managers = GameObject.Find("Managers");
        InventoryManager inventoryManager = managers.GetComponent<InventoryManager>();
        inventoryManager.OpenInventory(this);
    }

    public bool UseItem(Item item)
    {
        int index = inventory.FindIndex(0, (Item_entry entry) => { return entry.item == item; });
        if (index >= 0)
        {
            Item_entry entry = inventory[index];
            entry.amount -= 1;
            if (entry.amount <= 0) inventory.RemoveAt(index);
            else inventory[index] = entry;
        }

        switch (item.type)
        {
            case Item_types.werable:
                break;
            case Item_types.consumable:
                for (int a = 0; a < item.effects.Count; ++a)
                {
                    switch (item.effects[a])
                    {
                        case what_is_affected.health:
                            health += item.effect_amount[a];
                            if (health > max_health) health = max_health;
                            Debug.Log("Now health is " + health);
                            break;
                        case what_is_affected.speed:
                            moveSpeed += item.effect_amount[a];
                            break;
                    }
                }
                return true;
                //break;
            case Item_types.quest:
                break;
            case Item_types.material:
                break;
            case Item_types.throwable:
                break;
        }
        return false;
    }

    bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer | interactableObjectsLayer) != null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        isMoving = false;
    }

    public void CanMove(bool can_move)
    {
        if (can_move && cant_move_sources > 0)
        {
            cant_move_sources -= 1;
        } else if (!can_move)
        {
            cant_move_sources++;
        }
        just_interacted = true;
    }

    public void setInventory(List<Item_entry> inventory)
    {
        this.inventory = inventory;
    }

    public List<Item_entry> getInventory()
    {
        return inventory;
    }

    public void AddItem(Item item, int amount)
    {
        int index = inventory.FindIndex((Item_entry ie) => { return ie.item.name == item.name; });
        if (index >= 0)
        {
            Item_entry copy = inventory[index];
            copy.amount += amount;
            inventory[index] = copy;
        }
        else inventory.Add(new Item_entry(item, amount));
    }

    public void RemoveItem(Item item, int amount)
    {
        int index = inventory.FindIndex((Item_entry ie) => { return ie.item.name == item.name; });
        if (index >= 0)
        {
            Item_entry copy = inventory[index];
            copy.amount -= amount;
            if(copy.amount <= 0) inventory.RemoveAt(index);
            else inventory[index] = copy;
        }
    }
}