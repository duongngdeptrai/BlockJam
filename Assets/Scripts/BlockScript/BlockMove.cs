using UnityEngine;
using System.Collections.Generic;
public class BlockMove : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boxCollider;
    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 targetPos;
    private Camera cam;
    private Rigidbody2D rb;
    private Vector3 lastMousePos;
    private bool hasTarget = false;
    private void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnMouseDown()
    {
        isDragging = true;
        rb.bodyType = RigidbodyType2D.Dynamic ; 
        rb.gravityScale = 0;
        lastMousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        lastMousePos.z = 0;
        targetPos = rb.position;
        hasTarget = true;
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector3 delta = mousePos - lastMousePos;

        targetPos = rb.position + 1.5f * (Vector2)delta;

        lastMousePos = mousePos;
    }

    private void FixedUpdate()
    {
        if (!isDragging || !hasTarget) return;
        rb.MovePosition(targetPos);
    }

    private void OnMouseUp()
    {
        isDragging = false;
        rb.bodyType = RigidbodyType2D.Static;
        hasTarget = false;
        SnapToGrid();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[BlockMove] Collision with {collision.collider.name} at {collision.GetContact(0).point}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[BlockMove] Trigger with {other.name} at {other.ClosestPoint(transform.position)}");
    }
    public void SnapToGrid()
    {
        Vector3 position = transform.position;
        position.x = Mathf.Round(position.x - 0.5f) + 0.5f;
        position.y = Mathf.Round(position.y);
        rb.position = position;
    }
}
