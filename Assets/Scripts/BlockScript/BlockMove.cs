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

    private bool CheckFitDoorSize(Door door)
    {
        if (door == null) return false;
        if (block.ColorType != door.ColorType) return false;

        int actualDoorSize = door.Size <= 0 ? 1 : door.Size; 
        
        if (door.Direction == Direction.Up || door.Direction == Direction.Down)
            if (block.SizeX > actualDoorSize) return false; 
                
        if (door.Direction == Direction.Right || door.Direction == Direction.Left)
            if (block.SizeY > actualDoorSize) return false; 

        return true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Door"))
        {
            Door door = collision.collider.GetComponent<Door>();
            if(CheckFitDoorSize(door))
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
        transform.position = position; // Ép tọa độ ghim lập tức để Bounds không bị lag 1 frame

        Physics2D.SyncTransforms(); // Cập nhật hitbox BoxCollider2D ngay tức khắc

        // Quét quanh vị trí mới Snap xem có rà trúng cửa không (Giống hệt OnCollisionEnter2D)
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(boxCollider.bounds.center, boxCollider.bounds.size + new Vector3(0.1f, 0.1f, 0f), 0f);
        
        foreach (var col in hitColliders)
        {
            if (col.CompareTag("Door"))
            {
                Door door = col.GetComponent<Door>();
                if (CheckFitDoorSize(door))
                {
                    doorCollider = col;
                    doorCollider.isTrigger = true;
                    currentDoor = door;
                    closeDoor = true;
                    blockCollider = boxCollider;

                    CheckDoorEnterDepth(); // Kiểm tra độ lún/thẳng hàng và AutoExit ngay!
                    break; 
                }
            }
        }
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

        // BƯỚC 2: Tự động AutoExit ngay nếu Block đứng Ở NGOÀI GẦN CỬA, và THẲNG HÀNG (Lọt qua khe).
        // Cho sai số lệch tọa độ 0.35 unit để linh động.
        float alignTolerance = 0.1f;

        switch (currentDoor.Direction)
        {
            case Direction.Up:
                exitDir = Vector3.up;
                if (Mathf.Abs(blockBounds.center.x - doorBounds.center.x) < alignTolerance) isEntered = true; // Chạm ngoài + thẳng hàng
                else if (blockBounds.max.y >= doorBounds.min.y + doorEnterDepth) isEntered = true; // Ép lấn sâu 0.1
                break;
            case Direction.Down:
                exitDir = Vector3.down;
                if (Mathf.Abs(blockBounds.center.x - doorBounds.center.x) < alignTolerance) isEntered = true;
                else if (blockBounds.min.y <= doorBounds.max.y - doorEnterDepth) isEntered = true;
                break;
            case Direction.Right:
                exitDir = Vector3.right;
                if (Mathf.Abs(blockBounds.center.y - doorBounds.center.y) < alignTolerance) isEntered = true;
                else if (blockBounds.max.x >= doorBounds.min.x + doorEnterDepth) isEntered = true;
                break;
            case Direction.Left:
                exitDir = Vector3.left;
                if (Mathf.Abs(blockBounds.center.y - doorBounds.center.y) < alignTolerance) isEntered = true;
                else if (blockBounds.min.x <= doorBounds.max.x - doorEnterDepth) isEntered = true;
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
