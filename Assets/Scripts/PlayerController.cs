using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public LayerMask solidObjectsLayer;
    public LayerMask interactableObjectsLayer;

    bool can_move = true;
    private bool isMoving;
    private Vector2 input;
    private Animator animator;
    bool just_interacted = false;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    void Start() {
    }

    void Update()
    {
        animator.SetBool("isMoving", isMoving);
        if (can_move)
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

            if(just_interacted)
            {
                if (Input.GetKeyDown(KeyCode.E)) return;
                just_interacted = false;
            }

            if (Input.GetKeyDown(KeyCode.E)) {
                Interact();
            }

            if (Input.GetKeyDown(KeyCode.I)) {
                OpenInventory();
            }
        }
    }

    private void OpenInventory(){
        GameObject managers = GameObject.Find("Managers");
        InventoryManager inventoryManager = managers.GetComponent<InventoryManager>();
        inventoryManager.OpenInventory(this);
    }

    void Interact() {
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + facingDir;
        Debug.DrawLine(transform.position, interactPos, Color.red, 1f);
        var collider = Physics2D.OverlapCircle(interactPos, 0.2f, interactableObjectsLayer);
        if(collider != null){
            collider.GetComponent<Interact>()?.Interact(gameObject);
        }
    }

    bool IsWalkable(Vector3 targetPos)
    {
        if(Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer | interactableObjectsLayer) != null) {
            return false;
        } else {
            return true;
        }
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        while((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        isMoving = false;
    }

    public void CanMove(bool can_move)
    {
        this.can_move = can_move;
        just_interacted = true;
        
    }
}
