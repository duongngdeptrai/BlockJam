using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
public class BlockMove : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private Block block;
    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 targetPos;
    private Camera cam;
    private Rigidbody2D rb;
    private Vector3 lastMousePos;
    private bool hasTarget = false;

    // Các biến phục vụ Auto-Exit
    public bool IsAutoExiting { get; private set; } = false;
    private Vector3 autoExitDirection;
    private Vector3 triggerStartPos;
    private float exitDistance = 3f;
    private float exitSpeed = 6f;
    private void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnMouseDown()
    {
        if (IsAutoExiting) return;

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
        if (IsAutoExiting || !isDragging) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector3 delta = mousePos - lastMousePos;

        targetPos = rb.position + 4f * (Vector2)delta;

        lastMousePos = mousePos;
    }

    private void FixedUpdate()
    {
        if (IsAutoExiting || !isDragging || !hasTarget) return;
        rb.MovePosition(targetPos);
    }
    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[BlockMove] Collision with {collision.collider.name} at {collision.GetContact(0).point}");
    }

    private void OnMouseUp()
    {
        if (IsAutoExiting) return;

        isDragging = false;
        rb.bodyType = RigidbodyType2D.Static;
        hasTarget = false;
        SnapToGrid();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Door"))
        {
            if(block.ColorType == collision.collider.GetComponent<Door>().ColorType)
            {
                collision.collider.isTrigger =  true ; 
            }
        }
        // Debug.Log($"Self: {gameObject.name}");
        // Debug.Log($"Other: {collision.collider.gameObject.name}");
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log($"[BlockMove] Exit collision with {collision.collider.name}");
        if(collision.collider.CompareTag("Door"))
        {
            collision.collider.isTrigger =  false ; 
        }
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

    public void StartAutoExit(Vector3 exitDirection, Vector3 triggerPos)
    {
        IsAutoExiting = true;
        isDragging = false;
        hasTarget = false;
        triggerStartPos = triggerPos;
        autoExitDirection = exitDirection.normalized; // Nhận thẳng hướng từ transform của cửa
        
        // Tắt vật lý, va chạm
        rb.bodyType = RigidbodyType2D.Static;
        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;
    }

    private void Update()
    {
        if (IsAutoExiting)
        {
            // Di chuyển tự động
            transform.position += autoExitDirection * exitSpeed * Time.deltaTime;
            
            // Xóa block cứng khoảng cách exitDistance khỏi DoorTrigger (ví dụ 3 đơn vị)
            if (Vector3.Distance(transform.position, triggerStartPos) >= exitDistance)
            {
                Destroy(gameObject);                    
            }
        }
    }
}
