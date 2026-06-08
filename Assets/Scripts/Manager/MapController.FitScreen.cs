using UnityEngine;

public partial class MapController
{
/// <summary>
/// Tạo hoặc đảm bảo mapRoot tồn tại như một root container cho toàn bộ map.
/// </summary>
private void EnsureMapRoot()
{
	if (mapRoot != null) return;

	mapRoot = new GameObject("MapRoot").transform;
	mapRoot.SetParent(transform, false);
}

/// <summary>
/// Tính toán bounds thực tế của toàn bộ map (bao gồm block, door, wall, grid)
/// và scale mapRoot để map vừa tầm nhìn của camera trên mọi thiết bị.
/// </summary>
private void FitMapToScreen()
{
	if (mapRoot == null) return;

	// 1. Tính bounds của tất cả child của mapRoot
	Bounds totalBounds = new Bounds(Vector3.zero, Vector3.zero);
	bool hasBounds = false;

	// Gom bounds từ tất cả Renderer trong map (SpriteRenderer, LineRenderer, etc.)
	// Bỏ qua SpriteMask vì nó không phải phần của map cần căn
	Renderer[] renderers = mapRoot.GetComponentsInChildren<Renderer>(true);
	foreach (var r in renderers)
	{
		if (r.enabled && r.GetComponent<SpriteMask>() == null)
		{
			if (!hasBounds)
			{
				totalBounds = r.bounds;
				hasBounds = true;
			}
			else
			{
				totalBounds.Encapsulate(r.bounds);
			}
		}
	}

	// Nếu không có renderer (rare), thử lấy từ Transform.position của các child
	if (!hasBounds)
	{
		foreach (Transform child in mapRoot)
		{
			if (!hasBounds)
			{
				totalBounds.SetMinMax(child.position, child.position);
				hasBounds = true;
			}
			else
			{
				totalBounds.Encapsulate(child.position);
			}
		}
	}

	if (!hasBounds)
	{
		Debug.LogWarning("FitMapToScreen: Không tìm thấy bounds nào để scale.");
		return;
	}

	// 2. Tính kích thước map thực tế (world units)
	Vector3 mapSize = totalBounds.size;
	float mapWidth = mapSize.x;
	float mapHeight = mapSize.y;

	// Thêm padding nhỏ (5%) để map không dính mép
	const float paddingFactor = 1.05f;
	mapWidth *= paddingFactor;
	mapHeight *= paddingFactor;

	// 3. Lấy thông tin camera và safe area
	Camera cam = Camera.main;
	if (cam == null || !cam.orthographic)
	{
		Debug.LogWarning("FitMapToScreen: Camera.main không tồn tại hoặc không phải orthographic.");
		return;
	}

	// Orthographic visible size (height = 2 * orthographicSize)
	float visibleHeight = cam.orthographicSize * 2f;
	float visibleWidth = visibleHeight * cam.aspect;

	// Kiểm tra safe area (notch, rounded corners)
	Rect safeArea = Screen.safeArea;
	float safeAspect = safeArea.width / safeArea.height;
	// Nếu safeArea nhỏ hơn toàn màn hình, giảm visible area tương ứng
	if (safeArea.width > 0 && safeArea.height > 0)
	{
		// Chỉ sử dụng safeArea nếu nó thực sự nhỏ hơn toàn màn hình
		if (safeArea.width < Screen.width || safeArea.height < Screen.height)
		{
			visibleWidth = visibleHeight * safeAspect;
		}
	}

	// 4. Tính scale để map vừa trong cả width và height
	float scaleX = visibleWidth / mapWidth;
	float scaleY = visibleHeight / mapHeight;
	float fitScale = Mathf.Min(scaleX, scaleY);

	// Giới hạn scale tối thiểu (không cho quá nhỏ) và tối đa (không cho phóng to quá lớn)
	const float minScale = 0.5f;
	const float maxScale = 2.0f;
	fitScale = Mathf.Clamp(fitScale, minScale, maxScale);

	// 5. Áp dụng scale vào mapRoot
	mapRoot.localScale = Vector3.one * fitScale;

	// 6. Tốt nên căn giữa map trong camera view
	// Giữ nguyên localPosition (0,0,0) của mapRoot, nhưng đảm bảo nó ở giữa camera
	Vector3 camPos = cam.transform.position;
	mapRoot.position = new Vector3(camPos.x, camPos.y, 0);

	Debug.Log($"FitMapToScreen: mapSize=({mapWidth:F2},{mapHeight:F2}) scale={fitScale:F3} visible=({visibleWidth:F2},{visibleHeight:F2}) safeAspect={safeAspect:F2}");
}
}
