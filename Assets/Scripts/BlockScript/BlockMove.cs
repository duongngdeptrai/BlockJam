using UnityEngine;
using System;
public class BlockMove : MonoBehaviour
{
    public static event Action<Block> BlockConsumed;

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
    private Direction autoExitDirectionEnum;
    private Vector3 triggerStartPos;
    private float exitDistance = 3f;
    private float exitSpeed = 6f;
    private float exitThreshold;
    float alignTolerance = 0.1f; // offset cho việc check va chạm   

    // Door check logic variables
    public bool closeDoor = false;
    private Door currentDoor;
    private Collider2D doorCollider;
    private Collider2D blockCollider;
    private float doorEnterDepth = 0.1f;
    //bound của door và vật
    private Bounds doorBounds;
    private Bounds blockBounds;

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
        
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        // Ghi lại điểm lệch tay cầm của chuột so với tâm khối Block
        offset = transform.position - mousePos; 
        
        targetPos = transform.position;
        hasTarget = true;
    }

    private void OnMouseDrag()
    {
        if (IsAutoExiting || !isDragging) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // Neo target chính xác 100% theo con trỏ chuột (Tỉ lệ 1:1) thay vì x4 delta
        targetPos = mousePos + offset;
    }

    private void FixedUpdate()
    {
        if (IsAutoExiting || !hasTarget) return;
        
        if (isDragging)
        {
            // Dùng nội suy (Lerp) để Block trượt kéo theo TargetPos một cách đàn hồi siêu mượt
            Vector2 smoothedPos = Vector2.Lerp(rb.position, targetPos, Time.fixedDeltaTime * 15f);
            rb.MovePosition(smoothedPos);
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
    // Kiểm tra kích thước block có vào được cửa không ở đây sau sửa logic chỉ sửa hàm này 
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
                if(CloseDoor()){
                    ExecuteAutoExit(GetDirectionVector(currentDoor.Direction));
                }
                blockCollider = boxCollider;
            }
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
      
        if (closeDoor && other == doorCollider)
        {
            // Kiểm tra xem block vọt qua mặt cửa (vượt ngục) hay lùi lại (quay xe)
            Vector3 exitDir = GetDirectionVector(currentDoor.Direction);
            Vector3 directionToBlock = transform.position - currentDoor.transform.position;
            
            // Nếu toạ độ đi thuận hướng cửa (vọt rất xa qua cửa)
            if (Vector3.Dot(directionToBlock, exitDir) > 0.1f)
            {
                ExecuteAutoExit(exitDir);
                return;
            }

            // Xử lý fallback cho trường hợp người chơi kéo block lùi lại ra ngoài (quay xe)
            closeDoor = false;
            if (doorCollider != null) doorCollider.isTrigger = false;
            doorCollider = null;
            currentDoor = null;
        }
        if (doorCollider != null)
        {
            doorCollider.isTrigger = false;
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
        if (doorCollider != null) doorCollider.isTrigger = false;
    }
    public void CheckDoorEnterDepth()
    {
        if (currentDoor == null || doorCollider == null || blockCollider == null)
        {
            closeDoor = false;
            return;
        }

        blockBounds = blockCollider.bounds;
        doorBounds = doorCollider.bounds;
        bool isEntered = false;
        switch (currentDoor.Direction)
        {
            case Direction.Up:
                if (blockBounds.min.x >= doorBounds.min.x - alignTolerance && blockBounds.max.x <= doorBounds.max.x + alignTolerance) isEntered = true; // Thẳng hàng trong phạm vi cửa
                else if (blockBounds.max.y >= doorBounds.min.y + doorEnterDepth) isEntered = true; // Ép lấn sâu 0.1
                break;
            case Direction.Down:
                if (blockBounds.min.x >= doorBounds.min.x - alignTolerance && blockBounds.max.x <= doorBounds.max.x + alignTolerance) isEntered = true;
                else if (blockBounds.min.y <= doorBounds.max.y - doorEnterDepth) isEntered = true;
                break;
            case Direction.Right:
                if (blockBounds.min.y >= doorBounds.min.y - alignTolerance && blockBounds.max.y <= doorBounds.max.y + alignTolerance) isEntered = true;
                else if (blockBounds.max.x >= doorBounds.min.x + doorEnterDepth) isEntered = true;
                break;
            case Direction.Left:
                if (blockBounds.min.y >= doorBounds.min.y - alignTolerance && blockBounds.max.y <= doorBounds.max.y + alignTolerance) isEntered = true;
                else if (blockBounds.min.x <= doorBounds.max.x - doorEnterDepth) isEntered = true;
                break;
        }

        if (isEntered)
        {
            ExecuteAutoExit(GetDirectionVector(currentDoor.Direction));
        }
    }
    public void StartAutoExit(Vector3 exitDirectionVector, Door door)
    {
        IsAutoExiting = true;
        isDragging = false;
        hasTarget = false;
        triggerStartPos = door.transform.position;
        autoExitDirection = exitDirectionVector.normalized;
        autoExitDirectionEnum = door.Direction;

        // Tính threshold hủy dựa vào boundary của door
        Bounds dBounds = door.GetComponent<Collider2D>().bounds;
        switch (autoExitDirectionEnum)
        {
            case Direction.Up:    exitThreshold = dBounds.max.y + exitDistance; break;
            case Direction.Down:  exitThreshold = dBounds.min.y - exitDistance; break;
            case Direction.Right: exitThreshold = dBounds.max.x + exitDistance; break;
            case Direction.Left:  exitThreshold = dBounds.min.x - exitDistance; break;
        }
        
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
            if (CheckDestroy()){
                NotifyBlockConsumed();
                Destroy(gameObject);                    
            }
        }
        if(doorCollider != null){
            if(doorCollider.isTrigger == true){
                if(CheckFitDoorSize(currentDoor))
                {
                    if(CloseDoor()){
                        ExecuteAutoExit(GetDirectionVector(currentDoor.Direction));
                    }
                    blockCollider = boxCollider;
                }
            }
        }
    }
    void DrawBounds(Bounds b, Color color)
    {
        Vector3 min = b.min;
        Vector3 max = b.max;

        Vector3 p1 = new Vector3(min.x, min.y, 0);
        Vector3 p2 = new Vector3(max.x, min.y, 0);
        Vector3 p3 = new Vector3(max.x, max.y, 0);
        Vector3 p4 = new Vector3(min.x, max.y, 0);

        Debug.DrawLine(p1, p2, color , 10f);
        Debug.DrawLine(p2, p3, color , 10f);
        Debug.DrawLine(p3, p4, color , 10f);
        Debug.DrawLine(p4, p1, color , 10f);

         Vector3 c = b.center;
        float size = 0.1f;

        Debug.DrawLine(c + Vector3.left * size, c + Vector3.right * size, Color.yellow, 5f);
        Debug.DrawLine(c + Vector3.up * size, c + Vector3.down * size, Color.yellow, 5f);
    }
    bool CloseDoor(){
        if (currentDoor == null || doorCollider == null) return false;
        blockBounds = boxCollider.bounds;
        doorBounds = doorCollider.bounds;
        DrawBounds(blockBounds, Color.yellow);
        DrawBounds(doorBounds, Color.green);

        switch (currentDoor.Direction)
        {
            case Direction.Up:
                if (blockBounds.min.x >= doorBounds.min.x - alignTolerance && blockBounds.max.x <= doorBounds.max.x + alignTolerance) return true; // Thẳng hàng trong phạm vi cửa
                else if (blockBounds.max.y >= doorBounds.min.y + doorEnterDepth) return true; // Ép lấn sâu 0.1
                break;
            case Direction.Down:
                if (blockBounds.min.x >= doorBounds.min.x - alignTolerance && blockBounds.max.x <= doorBounds.max.x + alignTolerance) return true;
                else if (blockBounds.min.y <= doorBounds.max.y - doorEnterDepth) return true;
                break;
            case Direction.Right:
                if (blockBounds.min.y >= doorBounds.min.y - alignTolerance && blockBounds.max.y <= doorBounds.max.y + alignTolerance) return true;
                else if (blockBounds.max.x >= doorBounds.min.x + doorEnterDepth) return true;
                break;
            case Direction.Left:
                if (blockBounds.min.y >= doorBounds.min.y - alignTolerance && blockBounds.max.y <= doorBounds.max.y + alignTolerance) return true;
                else if (blockBounds.min.x <= doorBounds.max.x - doorEnterDepth) return true;
                break;
        }
        return false ; 
    }   
    bool CheckDestroy(){
        blockBounds = boxCollider.bounds; // Cập nhật bounds thực tế theo vị trí hiện tại
        switch(autoExitDirectionEnum){
            case Direction.Up:
                if (blockBounds.max.y >= exitThreshold) return true;
                break;
            case Direction.Down:
                if (blockBounds.min.y <= exitThreshold) return true;
                break;
            case Direction.Right:
                if (blockBounds.max.x >= exitThreshold) return true;
                break;
            case Direction.Left:
                if (blockBounds.min.x <= exitThreshold) return true;
                break;
        }
        return false ; 
    }
    private Vector3 GetDirectionVector(Direction dir)
    {
        return dir switch
        {
            Direction.Up => Vector3.up,
            Direction.Down => Vector3.down,
            Direction.Right => Vector3.right,
            Direction.Left => Vector3.left,
            _ => Vector3.zero
        };
    }

    private void ExecuteAutoExit(Vector3 exitDir)
    {
        if (currentDoor == null) return;

        blockBounds = boxCollider.bounds; // Cập nhật bounds thực tế
        currentDoor.PlayParticles(blockBounds);
        
        closeDoor = false; // Ngừng CheckDoorEnterDepth
        if (doorCollider != null) doorCollider.isTrigger = false;
        
        StartAutoExit(exitDir, currentDoor);

        // Clear references
        doorCollider = null;
        currentDoor = null;
    }

    private void NotifyBlockConsumed()
    {
            BlockConsumed?.Invoke(block);
    }
}
