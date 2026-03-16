using UnityEngine;
using System.Collections.Generic;
public class BlockMove : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boxCollider;
    private bool isDragging = false;
    private Vector3 offset;
    private Camera cam;
    private Rigidbody2D rb;
    private void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnMouseDown()
    {
        isDragging = true;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        offset = transform.position - mousePos;
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector3 targetPos = mousePos + offset;
        Vector2 direction = targetPos - transform.position;
        float distance = direction.magnitude;
        if (distance == 0) return;
        direction.Normalize();
        if (!IsColliding(direction, distance))
        {
            rb.MovePosition(targetPos);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        SnapToGrid();
    }
    bool IsColliding(Vector3 direction, float distance)
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        int count = boxCollider.Cast(direction, hits, distance);

        for (int i = 0; i < count; i++)
        {
            Debug.Log("Hit: " + hits[i].collider.gameObject.name);
            if (hits[i].collider.gameObject != gameObject)
            {
                return true;
            }
        }

        return false;
    }
    public void SnapToGrid()
    {
        Vector3 position = transform.position;
        position.x = Mathf.Round(position.x - 0.5f) + 0.5f;
        position.y = Mathf.Round(position.y);
        transform.position = position;
    }
}