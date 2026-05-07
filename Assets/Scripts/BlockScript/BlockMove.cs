using UnityEngine;
using System;
using System.Collections.Generic;
public class BlockMove : MonoBehaviour
{
    public static event Action<Block> BlockConsumed;
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
    private float doorEnterDepth = 0.1f;
    [SerializeField] private List<Transform> raycastOrigins = new List<Transform>();
    [SerializeField] private float raycastDistance = 20f;

    [Header("Drag Blocking")]
    [SerializeField] private LayerMask dragObstacleMask = ~0;
    [SerializeField] private float dragSkinWidth = 0.02f;

    [SerializeField] private List<BoxCollider2D> dragCastBoxColliders = new List<BoxCollider2D>();

    private readonly List<RaycastHit2D> dragCastHits = new List<RaycastHit2D>(16);
    private ContactFilter2D dragContactFilter;
    //bound của door và vật
    private Bounds doorBounds;
    private Bounds blockBounds;

    private Door pendingConsumeDoor;
    private Collider2D pendingConsumeDoorCollider;
    private float pendingConsumeDoorDistance;

    private RigidbodyInterpolation2D originalInterpolation;
    private CollisionDetectionMode2D originalCollisionDetection;
    private float originalGravityScale;

    private void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();

        originalInterpolation = rb.interpolation;
        originalCollisionDetection = rb.collisionDetectionMode;
        originalGravityScale = rb.gravityScale;

        dragContactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = dragObstacleMask,
            useTriggers = true
        };

        if (dragCastBoxColliders == null)
        {
            dragCastBoxColliders = new List<BoxCollider2D>();
        }

        pendingConsumeDoor = null;
        pendingConsumeDoorCollider = null;
        pendingConsumeDoorDistance = float.PositiveInfinity;
    }

    private void SnapToGridAndConsumeDoor(Door door, Collider2D doorCol)
    {
        if (door == null || doorCol == null) return;

        // Stop dragging immediately.
        isDragging = false;
        hasTarget = false;

        rb.bodyType = RigidbodyType2D.Static;
        rb.interpolation = originalInterpolation;
        rb.collisionDetectionMode = originalCollisionDetection;
        rb.gravityScale = originalGravityScale;

        // Snap to grid first.
        Vector3 position = transform.position;
        position.x = Mathf.Round(position.x - 0.5f) + 0.5f;
        position.y = Mathf.Round(position.y);
        rb.position = position;
        transform.position = position;
        Physics2D.SyncTransforms();

        // Then try to consume.
        doorCollider = doorCol;
        doorCollider.isTrigger = true;
        currentDoor = door;
        closeDoor = true;

        CheckDoorEnterDepth();

        // If not consumed (doorCollider still set), restore trigger.
        if (doorCollider != null)
        {
            doorCollider.isTrigger = false;
        }
    }

    private void EnsureDragCastColliders()
    {
        if (dragCastBoxColliders == null)
        {
            dragCastBoxColliders = new List<BoxCollider2D>();
        }

        // If user didn't assign the list in Inspector, fall back to collecting children.
        if (dragCastBoxColliders.Count == 0)
        {
            GetComponentsInChildren(dragCastBoxColliders);
        }

        dragCastBoxColliders.RemoveAll(c => c == null);
    }

    private bool TryGetBlockBounds(out Bounds bounds, bool includeDisabled)
    {
        EnsureDragCastColliders();

        bool hasBounds = false;
        bounds = default;

        for (int i = 0; i < dragCastBoxColliders.Count; i++)
        {
            BoxCollider2D col = dragCastBoxColliders[i];
            if (col == null)
            {
                continue;
            }

            if (!includeDisabled && !col.enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = col.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(col.bounds);
            }
        }

        return hasBounds;
    }

    private void OnMouseDown()
    {
        if (IsAutoExiting) return;

        isDragging = true;
        // Drag is fully code-driven => kinematic body + direct position set for "dính tay".
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.interpolation = RigidbodyInterpolation2D.None;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        
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

    private Vector2 ResolveDragStep(Vector2 fromPos, Vector2 desiredPos, out bool wasClamped)
    {
        EnsureDragCastColliders();

        pendingConsumeDoor = null;
        pendingConsumeDoorCollider = null;
        pendingConsumeDoorDistance = float.PositiveInfinity;

        Vector2 delta = desiredPos - fromPos;
        const float eps = 0.0001f;
        if (delta.sqrMagnitude <= eps * eps)
        {
            wasClamped = false;
            return desiredPos;
        }

        // First, try the full diagonal step.
        bool fullClamped;
        Vector2 fullResolved = ResolveDragDelta(fromPos, delta, out fullClamped);
        if (!fullClamped)
        {
            wasClamped = false;
            return fullResolved;
        }

        // If diagonal is blocked, allow sliding on the unblocked axis.
        Vector2 stepPos = fromPos;
        bool xClamped = false;
        bool yClamped = false;

        if (Mathf.Abs(delta.x) > eps)
        {
            stepPos = ResolveDragDelta(stepPos, new Vector2(delta.x, 0f), out xClamped);
        }

        if (Mathf.Abs(delta.y) > eps)
        {
            stepPos = ResolveDragDelta(stepPos, new Vector2(0f, delta.y), out yClamped);
        }

        wasClamped = (stepPos - desiredPos).sqrMagnitude > eps * eps;
        return stepPos;
    }

    private Vector2 ResolveDragDelta(Vector2 fromPos, Vector2 delta, out bool wasClamped)
    {
        wasClamped = false;

        const float eps = 0.0001f;
        float distance = delta.magnitude;
        if (distance <= eps)
        {
            return fromPos;
        }

        Vector2 dir = delta / distance;
        float castDistance = distance + Mathf.Max(0f, dragSkinWidth);

        Door blockingDoor;
        Collider2D blockingDoorCollider;
        bool blockingDoorConsumable;
        float nearestBlockingDistance = GetNearestBlockingDistance(dir, castDistance, out blockingDoor, out blockingDoorCollider, out blockingDoorConsumable);

        if (blockingDoorConsumable && blockingDoor != null && blockingDoorCollider != null)
        {
            if (nearestBlockingDistance < pendingConsumeDoorDistance)
            {
                pendingConsumeDoor = blockingDoor;
                pendingConsumeDoorCollider = blockingDoorCollider;
                pendingConsumeDoorDistance = nearestBlockingDistance;
            }
        }
        if (float.IsPositiveInfinity(nearestBlockingDistance))
        {
            return fromPos + delta;
        }

        float allowedDistance = Mathf.Max(0f, nearestBlockingDistance - Mathf.Max(0f, dragSkinWidth));
        float finalDistance = Mathf.Min(distance, allowedDistance);
        wasClamped = finalDistance < (distance - eps);
        return fromPos + dir * finalDistance;
    }

    private float GetNearestBlockingDistance(Vector2 dir, float castDistance, out Door blockingDoor, out Collider2D blockingDoorCollider, out bool blockingDoorConsumable)
    {
        blockingDoor = null;
        blockingDoorCollider = null;
        blockingDoorConsumable = false;

        float nearestBlockingDistance = float.PositiveInfinity;

        if (dragCastBoxColliders == null || dragCastBoxColliders.Count == 0)
        {
            return nearestBlockingDistance;
        }

        for (int colliderIndex = 0; colliderIndex < dragCastBoxColliders.Count; colliderIndex++)
        {
            BoxCollider2D castCollider = dragCastBoxColliders[colliderIndex];
            if (castCollider == null || !castCollider.enabled || castCollider.isTrigger)
            {
                continue;
            }

            dragCastHits.Clear();
            int hitCount = castCollider.Cast(dir, dragContactFilter, dragCastHits, castDistance);
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = dragCastHits[i];
                Collider2D hitCollider = hit.collider;
                if (hitCollider == null)
                {
                    continue;
                }

                // Ignore colliders that belong to this block.
                if (hitCollider.transform.IsChildOf(transform))
                {
                    continue;
                }

                Door hitDoor = hitCollider.GetComponent<Door>();
                if (hitDoor != null)
                {
                    // Doors always block while dragging; consuming is handled after release/snap.
                    if (hit.distance < nearestBlockingDistance)
                    {
                        nearestBlockingDistance = hit.distance;

                        blockingDoor = hitDoor;
                        blockingDoorCollider = hitCollider;
                        blockingDoorConsumable = CheckFitDoorSize(hitDoor);
                    }

                    continue;
                }

                Block hitBlock = hitCollider.GetComponentInParent<Block>();
                if (hitBlock != null)
                {
                    if (hitBlock == block)
                    {
                        continue;
                    }

                    if (hit.distance < nearestBlockingDistance)
                    {
                        nearestBlockingDistance = hit.distance;
                    }

                    continue;
                }

                Wall hitWall = hitCollider.GetComponent<Wall>();
                if (hitWall != null)
                {
                    if (hit.distance < nearestBlockingDistance)
                    {
                        nearestBlockingDistance = hit.distance;
                    }

                    continue;
                }
            }
        }

        return nearestBlockingDistance;
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
        rb.interpolation = originalInterpolation;
        rb.collisionDetectionMode = originalCollisionDetection;
        rb.gravityScale = originalGravityScale;
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
                closeDoor = true;         // Chỉ ghi nhận, KHÔNG auto-exit khi đang kéo

                // Ăn luôn nếu chạm đúng cửa hợp lệ và không bị chặn đường ra.
                TryReleaseCurrentDoor(GetDirectionVector(currentDoor.Direction));
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
                TryReleaseCurrentDoor(exitDir);
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

        if (!TryGetBlockBounds(out Bounds snapBounds, includeDisabled: false))
        {
            if (doorCollider != null) doorCollider.isTrigger = false;
            return;
        }

        // Quét quanh vị trí mới Snap xem có rà trúng cửa không (Giống hệt OnCollisionEnter2D)
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(snapBounds.center, snapBounds.size + new Vector3(0.1f, 0.1f, 0f), 0f);
        
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

                    CheckDoorEnterDepth(); // Kiểm tra độ lún/thẳng hàng và AutoExit ngay!
                    break; 
                }
            }
        }
        if (doorCollider != null) doorCollider.isTrigger = false;
    }
    private bool CheckBlockAlignedWithDoor(Bounds blockB, Bounds doorB, Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
            case Direction.Down:
                return (blockB.min.x >= doorB.min.x - alignTolerance && blockB.max.x <= doorB.max.x + alignTolerance);
            case Direction.Right:
            case Direction.Left:
                return (blockB.min.y >= doorB.min.y - alignTolerance && blockB.max.y <= doorB.max.y + alignTolerance);
        }
        return false;
    }

    private bool CheckBlockDepthReached(Bounds blockB, Bounds doorB, Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:    return (blockB.max.y >= doorB.min.y + doorEnterDepth);
            case Direction.Down:  return (blockB.min.y <= doorB.max.y - doorEnterDepth);
            case Direction.Right: return (blockB.max.x >= doorB.min.x + doorEnterDepth);
            case Direction.Left:  return (blockB.min.x <= doorB.max.x - doorEnterDepth);
        }
        return false;
    }

    public void CheckDoorEnterDepth()
    {
        if (currentDoor == null || doorCollider == null)
        {
            closeDoor = false;
            return;
        }

        if (!TryGetBlockBounds(out Bounds checkBounds, includeDisabled: false))
        {
            closeDoor = false;
            return;
        }

        blockBounds = checkBounds;
        doorBounds = doorCollider.bounds;

        bool isAligned = CheckBlockAlignedWithDoor(blockBounds, doorBounds, currentDoor.Direction);
        bool isDepthReached = CheckBlockDepthReached(blockBounds, doorBounds, currentDoor.Direction);

        if (isAligned)
        {
            TryReleaseCurrentDoor(GetDirectionVector(currentDoor.Direction));
        }
        else if (isDepthReached)
        {
            // Block lún vào cửa (vượt qua depth) nhưng không qua lọt door bounds -> Bắt thả chuột và SnapToGrid
            if (isDragging)
            {
                isDragging = false;
                rb.bodyType = RigidbodyType2D.Static;
                hasTarget = false;
                SnapToGrid();
            }
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
        rb.interpolation = originalInterpolation;
        rb.collisionDetectionMode = originalCollisionDetection;
        rb.gravityScale = originalGravityScale;
        EnsureDragCastColliders();
        for (int i = 0; i < dragCastBoxColliders.Count; i++)
        {
            if (dragCastBoxColliders[i] != null)
            {
                dragCastBoxColliders[i].enabled = false;
            }
        }
    }

    private void Update()
    {
        if (!IsAutoExiting && hasTarget && isDragging)
        {
            // Frame-rate independent smoothing: t = 1 - e^{-k*dt}
            float t = 1f - Mathf.Exp(-15f * Time.deltaTime);
            Vector2 desiredPos = Vector2.Lerp(rb.position, targetPos, t);

            bool wasClamped;
            Vector2 resolvedPos = ResolveDragStep(rb.position, desiredPos, out wasClamped);

            // If we are blocked by a consumable door, snap and consume immediately.
            if (pendingConsumeDoor != null && pendingConsumeDoorCollider != null)
            {
                SnapToGridAndConsumeDoor(pendingConsumeDoor, pendingConsumeDoorCollider);
                return;
            }

            // Immediate movement (no physics-step latency).
            rb.position = resolvedPos;
            Vector3 pos = transform.position;
            pos.x = resolvedPos.x;
            pos.y = resolvedPos.y;
            transform.position = pos;
            Physics2D.SyncTransforms();

            if (wasClamped)
            {
                targetPos.x = resolvedPos.x;
                targetPos.y = resolvedPos.y;
            }
        }

        // VẼ DEBUG BOUNDS LIÊN TỤC ĐỂ BẠN QUAN SÁT TRONG SCENE VIEW
        if (TryGetBlockBounds(out Bounds debugBounds, includeDisabled: true))
        {
            DrawBounds(debugBounds, Color.yellow);
        }

        if (doorCollider != null)
        {
            DrawBounds(doorCollider.bounds, Color.green);
        }

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
        // Chỉ check auto-exit khi KHÔNG đang kéo (đã snap vào grid)
        if(doorCollider != null && !isDragging){
            if(doorCollider.isTrigger == true){
                if(CheckFitDoorSize(currentDoor))
                {
                    if(CloseDoor()){
                        TryReleaseCurrentDoor(GetDirectionVector(currentDoor.Direction));
                    }
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

        Debug.DrawLine(p1, p2, color , 0.02f);
        Debug.DrawLine(p2, p3, color , 0.02f);
        Debug.DrawLine(p3, p4, color , 0.02f);
        Debug.DrawLine(p4, p1, color , 0.02f);

         Vector3 c = b.center;
        float size = 0.1f;

        Debug.DrawLine(c + Vector3.left * size, c + Vector3.right * size, Color.yellow, 0.02f);
        Debug.DrawLine(c + Vector3.up * size, c + Vector3.down * size, Color.yellow, 0.02f);
    }
    bool CloseDoor(){
        if (currentDoor == null || doorCollider == null) return false;
        if (!TryGetBlockBounds(out Bounds closeBounds, includeDisabled: false))
        {
            return false;
        }

        blockBounds = closeBounds;
        doorBounds = doorCollider.bounds;
        DrawBounds(blockBounds, Color.yellow);
        DrawBounds(doorBounds, Color.green);

        switch (currentDoor.Direction)
        {
            case Direction.Up:
                if (blockBounds.min.x >= doorBounds.min.x - alignTolerance && blockBounds.max.x <= doorBounds.max.x + alignTolerance && 
                    blockBounds.max.y >= doorBounds.min.y + doorEnterDepth) return true;
                break;
            case Direction.Down:
                if (blockBounds.min.x >= doorBounds.min.x - alignTolerance && blockBounds.max.x <= doorBounds.max.x + alignTolerance && 
                    blockBounds.min.y <= doorBounds.max.y - doorEnterDepth) return true;
                break;
            case Direction.Right:
                if (blockBounds.min.y >= doorBounds.min.y - alignTolerance && blockBounds.max.y <= doorBounds.max.y + alignTolerance && 
                    blockBounds.max.x >= doorBounds.min.x + doorEnterDepth) return true;
                break;
            case Direction.Left:
                if (blockBounds.min.y >= doorBounds.min.y - alignTolerance && blockBounds.max.y <= doorBounds.max.y + alignTolerance && 
                    blockBounds.min.x <= doorBounds.max.x - doorEnterDepth) return true;
                break;
        }
        return false ; 
    }   
    bool CheckDestroy(){
        if (!TryGetBlockBounds(out Bounds destroyBounds, includeDisabled: true))
        {
            return false;
        }

        blockBounds = destroyBounds; // Cập nhật bounds thực tế theo vị trí hiện tại
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

    private void TryReleaseCurrentDoor(Vector3 exitDir)
    {
        if (CanRaycastReachCurrentDoor())
        {
            ExecuteAutoExit(exitDir);
        }
    }

    private bool CanRaycastReachCurrentDoor()
    {
        if (currentDoor == null || doorCollider == null)
        {
            return false;
        }

        if (raycastOrigins == null || raycastOrigins.Count == 0)
        {
            return false;
        }

        Vector2 dir = GetRayDirection(currentDoor.Direction);
        if (dir == Vector2.zero)
        {
            return false;
        }

        foreach (Transform origin in raycastOrigins)
        {
            if (origin == null)
            {
                continue;
            }

            RaycastHit2D[] hits = Physics2D.RaycastAll(origin.position, dir, raycastDistance);
            Debug.DrawRay(origin.position, dir * raycastDistance, Color.cyan, 0.05f);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null)
                {
                    continue;
                }

                // Bỏ qua collider thuộc chính block hiện tại.
                if (hit.collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                // Block khác chắn trước cửa => fail ray này.
                Block hitBlock = hit.collider.GetComponentInParent<Block>();
                if (hitBlock != null && hitBlock != block)
                {
                    return false;
                }

                Door hitDoor = hit.collider.GetComponent<Door>();
                if (hitDoor != null)
                {
                    if (hitDoor != currentDoor)
                    {
                        return false;
                    }

                    break;
                }

                Wall hitWall = hit.collider.GetComponent<Wall>();
                if (hitWall != null)
                {
                    return false;
                }

                continue;
            }

        }

        return true;
    }

    private Vector2 GetRayDirection(Direction dir)
    {
        return dir switch
        {
            Direction.Up => Vector2.up,
            Direction.Down => Vector2.down,
            Direction.Right => Vector2.right,
            Direction.Left => Vector2.left,
            _ => Vector2.zero
        };
    }

    private void ExecuteAutoExit(Vector3 exitDir)
    {
        if (currentDoor == null) return;

        if (TryGetBlockBounds(out Bounds exitBounds, includeDisabled: true))
        {
            blockBounds = exitBounds; // Cập nhật bounds thực tế
        }
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
