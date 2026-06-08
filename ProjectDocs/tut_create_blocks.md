# Hướng dẫn tạo Prefab cá khối đặc biệt (L, T, Z) - Cách B (Composite Collider)

Đây là hướng dẫn xây dựng các Block có hình dạng phức tạp từ các ô 1x1, đảm bảo va chạm chính xác và không bị xoay khi kéo thả.

---

## 🏗️ Cấu trúc chung của một Prefab
1.  **Object CHA:**
    - Transform (Reset về 0)
    - Script hình tương ứng (`BlockLShape`, `BlockTShape`, `BlockZShape`)
    - `BlockMove` (Script di chuyển)
    - `Rigidbody2D` (Body Type = **Kinematic**, Constraints -> **Freeze Rotation Z**)
    - `Composite Collider 2D` (Geometry Type = **Polygons**)
2.  **Các Object CON (Cells):**
    - `SpriteRenderer` (Gán hình 1x1, Sorting Order > Grid)
    - `Box Collider 2D` (Size = 1,1; **Composite Operation = Merge**)

---

## 1️⃣ Block chữ L (4 cells)
**Kích thước (Bounding Box):** `sizeX = 2`, `sizeY = 3`

### Tọa độ các ô con (Local Position):
- Cell_0: `(0, 0, 0)` - Dưới trái
- Cell_1: `(1, 0, 0)` - Dưới phải
- Cell_2: `(0, 1, 0)` - Giữa trái
- Cell_3: `(0, 2, 0)` - Trên trái

---

## 2️⃣ Block chữ T (5 cells)
**Kích thước (Bounding Box):** `sizeX = 3`, `sizeY = 3`

### Tọa độ các ô con (Local Position):
- Cell_0: `(1, 0, 0)` - Chân dưới
- Cell_1: `(1, 1, 0)` - Thân giữa
- Cell_2: `(0, 2, 0)` - Ngang trái
- Cell_3: `(1, 2, 0)` - Ngang giữa
- Cell_4: `(2, 2, 0)` - Ngang phải

---

## 3️⃣ Block chữ Z (4 cells)
**Kích thước (Bounding Box):** `sizeX = 3`, `sizeY = 2`

### Tọa độ các ô con (Local Position):
- Cell_0: `(1, 0, 0)` - Dưới trái
- Cell_1: `(2, 0, 0)` - Dưới phải
- Cell_2: `(0, 1, 0)` - Trên trái
- Cell_3: `(1, 1, 0)` - Trên phải

---

## 🔗 Các bước kết nối Reference cuối cùng (QUAN TRỌNG)
Tại Object CHA trong Unity Editor:
1.  Script hình (ví dụ `BlockLShape`): Kéo ĐỦ các SpriteRenderer con vào mảng **Img**.
2.  Script `BlockMove`: 
    - Kéo `Composite Collider 2D` của cha vào field **Box Collider**.
    - Kéo Script hình của cha vào field **Block**.

---

## 📋 Đăng ký vào MapController
Mở Object **MapController** trong Scene, thêm vào danh sách **Block Prefabs**:

| Key | Value (Prefab) |
|---|---|
| `blockLShape` | Prefab L |
| `blockTShape` | Prefab T |
| `blockZShape` | Prefab Z |

---

## 🧪 JSON Level Example
Thêm vào file `level1.json` trong `StreamingAssets/Levels/`:
```json
{
  "blockType": "blockLShape",
  "row": 3,
  "column": 6,
  "color": "Red"
},
{
  "blockType": "blockTShape",
  "row": 5,
  "column": 10,
  "color": "Blue"
},
{
  "blockType": "blockZShape",
  "row": 5,
  "column": 3,
  "color": "Yellow"
}
```
