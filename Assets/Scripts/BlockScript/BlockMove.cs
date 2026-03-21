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

    // Door check logic variables
    private bool closeDoor = false;
    private Door currentDoor;
    private Collider2D doorCollider;
    private Collider2D blockCollider;
    private float doorEnterDepth = 0.1f;

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
        if (IsAutoExiting || !hasTarget) return;
        
        if (isDragging)
        {
            rb.MovePosition(targetPos);
        }

        if (closeDoor)
        {
            CheckDoorEnterDepth();
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        //Debug.Log($"[BlockMove] Collision with {collision.collider.name} at {collision.GetContact(0).point}");
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
            Door door = collision.collider.GetComponent<Door>();
            if(door != null && block.ColorType == door.ColorType)
            {
                doorCollider = collision.collider;
                doorCollider.isTrigger = true;
                currentDoor = door;
                closeDoor = true;
                blockCollider = boxCollider;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (closeDoor && other == doorCollider)
        {
            // Kiểm tra xem block vọt qua mặt cửa (vượt ngục) hay lùi lại (quay xe)
            Vector3 exitDir = Vector3.zero;
            switch (currentDoor.Direction)
            {
                case Direction.Up: exitDir = Vector3.up; break;
                case Direction.Down: exitDir = Vector3.down; break;
                case Direction.Right: exitDir = Vector3.right; break;
                case Direction.Left: exitDir = Vector3.left; break;
            }

            Vector3 directionToBlock = transform.position - currentDoor.transform.position;
            
            // Nếu toạ độ đi thuận hướng cửa (vọt rất xa qua cửa)
            if (Vector3.Dot(directionToBlock, exitDir) > 0.1f)
            {
                currentDoor.PlayParticles();
                closeDoor = false;
                doorCollider.isTrigger = false;
                StartAutoExit(exitDir, currentDoor.transform.position);
                doorCollider = null;
                currentDoor = null;
                return;
            }

            // Xử lý fallback cho trường hợp người chơi kéo block lùi lại ra ngoài (quay xe)
            closeDoor = false;
            if (doorCollider != null) doorCollider.isTrigger = false;
            doorCollider = null;
            currentDoor = null;
        }
    }
    public void SnapToGrid()
    {
        Vector3 position = transform.position;
        position.x = Mathf.Round(position.x - 0.5f) + 0.5f;
        position.y = Mathf.Round(position.y);
        rb.position = position;
    }
    public void CheckDoorEnterDepth()
    {
        if (currentDoor == null || doorCollider == null || blockCollider == null)
        {
            closeDoor = false;
            return;
        }

        Bounds blockBounds = blockCollider.bounds;
        Bounds doorBounds = doorCollider.bounds;
        
        bool isEntered = false;
        Vector3 exitDir = Vector3.zero;

        switch (currentDoor.Direction)
        {
            case Direction.Up:
                isEntered = blockBounds.max.y >= doorBounds.min.y + doorEnterDepth;
                exitDir = Vector3.up;
                break;
            case Direction.Down:
                isEntered = blockBounds.min.y <= doorBounds.max.y - doorEnterDepth;
                exitDir = Vector3.down;
                break;
            case Direction.Right:
                isEntered = blockBounds.max.x >= doorBounds.min.x + doorEnterDepth;
                exitDir = Vector3.right;
                break;
            case Direction.Left:
                isEntered = blockBounds.min.x <= doorBounds.max.x - doorEnterDepth;
                exitDir = Vector3.left;
                break;
        }

        if (isEntered)
        {
            currentDoor.PlayParticles();
            closeDoor = false;
            doorCollider.isTrigger = false;
            StartAutoExit(exitDir, currentDoor.transform.position);
            doorCollider = null;
            currentDoor = null;
        }
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
