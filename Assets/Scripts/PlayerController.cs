using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public float moveSpeed;
    private bool isMoving;
    private Vector2 input;
    private Animator animator;
    public LayerMask solidObjectsLayer;
    public LayerMask interactableObjectsLayer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        animator.SetBool("isMoving", isMoving);

        if (!isMoving) {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            if (input != Vector2.zero) {
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);
                if (input.x < 0) this.transform.localScale = new Vector3(-1, 1, 1);
                else this.transform.localScale = new Vector3(1, 1, 1);
                if(IsWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }

        if(Input.GetKeyDown(KeyCode.E)) {
            Interact();
        }
    }

    void Interact() {
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + facingDir;

        Debug.DrawLine(transform.position, interactPos, Color.red, 1f);
        //Debug.Log(transform.position + " - " + interactPos);

        var collider = Physics2D.OverlapCircle(interactPos, 0.2f, interactableObjectsLayer);
        if(collider != null)
        {
            collider.GetComponent<Interact>()?.Interact();
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
}
