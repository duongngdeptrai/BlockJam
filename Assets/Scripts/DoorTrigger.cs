using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private GameObject exitVfxPrefab;
    private Door door;

    private void Awake()
    {
        door = GetComponentInParent<Door>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Block block = collision.GetComponent<Block>();
        BlockMove blockMove = collision.GetComponent<BlockMove>();

        if (block != null && blockMove != null && door != null)
        {
            // Cạnh tranh: Nếu block đã auto-exit rồi → không xử lý lại.
            if (blockMove.IsAutoExiting) return;

            // Kiểm tra: Nếu đúng màu -> Gọi block auto-exit
            if (block.ColorType == door.ColorType)
            {
                // Nếu effect prefab trống → chỉ skip spawn, vẫn auto-exit.
                if (exitVfxPrefab != null)
                {
                    // Sinh VFX theo góc quay của DoorTrigger
                    GameObject vfxObj = Instantiate(exitVfxPrefab, transform.position, transform.rotation);
                    
                    float maxDuration = 0f;
                    ParticleSystem[] particleSystems = vfxObj.GetComponentsInChildren<ParticleSystem>();
                    
                    foreach (var ps in particleSystems)
                    {
                        var main = ps.main;
                        main.startColor = ColorManager.GetColor(block.ColorType);
                        ps.Play();

                        float duration = main.duration + main.startLifetime.constantMax;
                        if (duration > maxDuration) maxDuration = duration;
                    }
                    
                    // Dự phòng cho trường hợp ko tìm thấy module PS nào, destroy sau thời gian mặc định
                    if (maxDuration <= 0f) maxDuration = 2f; 

                    Destroy(vfxObj, maxDuration);
                }

                // Gọi auto exit blockMove theo trục quay thật ở ngoài map
                blockMove.StartAutoExit(door.transform.up, transform.position);
            }
        }
    }
}
