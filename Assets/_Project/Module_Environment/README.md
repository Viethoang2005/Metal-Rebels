# Module Environment - Metal Rebels

**Người sở hữu:** Lê Hữu Hiệu  
**Vùng phụ trách:** `Assets/_Project/Module_Environment` & `Assets/_Project/Game/Scenes`

---

## 1. Danh sách Scripts đã triển khai

| Script | Chức năng | Hợp đồng / Liên kết |
|---|---|---|
| [`CameraFollow2D.cs`](./Scripts/CameraFollow2D.cs) | Follow nhân vật Marco theo trục X, hỗ trợ ranh giới map (Min/Max Bounds), khóa camera tại vị trí cố định (`LockAt`/`Unlock`). | Tự tìm tag `Player` hoặc gán thủ công `Transform target`. |
| [`CameraShaker.cs`](./Scripts/CameraShaker.cs) | Tạo hiệu ứng rung màn hình khi có bom nổ, boss ra chiêu hoặc va đập mạnh. | Gọi qua singleton `CameraShaker.Instance.Shake(...)` hoặc static `CameraShaker.ShakeCurrent(...)`. |
| [`BreakableObject.cs`](./Scripts/BreakableObject.cs) | Thùng gỗ, bao cát, rào cản nhận sát thương từ súng/lựu đạn/vụ nổ, vỡ ra và rơi vật phẩm theo bảng tỷ lệ `DropTable`. | Kế thừa `MetalRebels.Core.Interfaces.IDamageable`. |
| [`ItemPickup.cs`](./Scripts/ItemPickup.cs) | Vật phẩm tiếp tế (Máu, Đạn Shotgun/Rifle/Rocket, Súng Rocket, Lựu đạn) trôi nổi bồng bềnh và tự thu hồi khi Player nhặt. | Gọi qua interface `MetalRebels.Core.Interfaces.IPickupCollector`. |
| [`HostageController.cs`](./Scripts/HostageController.cs) | Con tin bị trói, hiện gợi ý nút `[E] Cứu con tin`, tương tác qua Action `Interact`, nhả đồ tiếp tế và bỏ chạy. | Sử dụng Input System action `Interact` hoặc fallback phím `E`. |
| [`EventLockBarrier.cs`](./Scripts/EventLockBarrier.cs) | Rào chắn hai đầu ngăn người chơi khi kích hoạt sự kiện khóa màn hình. | Có animation nâng/hạ hoặc bật/tắt collider mượt mà. |
| [`WaveManager.cs`](./Scripts/WaveManager.cs) | Quản lý các đợt quái (waves) trong khu vực Event Lock, đếm số lượng quái còn sống và báo khi clear xong toàn bộ. | Bắn event `OnAllWavesCleared`. |
| [`EventLockTrigger.cs`](./Scripts/EventLockTrigger.cs) | Trigger khóa màn hình: khóa camera, hạ rào chắn, gọi `WaveManager`, phát event toàn cục. | Phát `GameEvents.OnEventLockStart` và `GameEvents.OnEventLockCleared`. |
| [`Checkpoint.cs`](./Scripts/Checkpoint.cs) | Cột mốc lưu vị trí hồi sinh gần nhất khi Marco chạm vào. | Cập nhật `Checkpoint.LastActiveCheckpointPosition` và cờ `Checkpoint.HasActiveCheckpoint`. |
| [`DeathZone.cs`](./Scripts/DeathZone.cs) | Vực chết hoặc bẫy chông ở đáy hố sâu. Gây sát thương chí tử cho mọi đối tượng có `IDamageable` rơi xuống. | Gọi `IDamageable.TakeDamage(9999)`. |
| [`LevelManager.cs`](./Scripts/LevelManager.cs) | Điều phối điểm xuất phát ban đầu, ranh giới map cho Camera và cung cấp tọa độ hồi sinh chính xác cho Game Manager. | Cung cấp hàm `GetRespawnPosition()`. |

---

## 2. Danh sách Prefabs đã tạo sẵn (`Module_Environment/Prefabs`)

- `Pickup_Health.prefab`: Hồi 30 HP
- `Pickup_ShotgunAmmo.prefab`: +20 viên Shotgun
- `Pickup_RifleAmmo.prefab`: +30 viên Rifle
- `Pickup_Rocket.prefab`: Nhận súng Rocket + 1 viên
- `Pickup_RocketAmmo.prefab`: +2 viên Rocket
- `Pickup_Grenade.prefab`: +2 quả lựu đạn
- `Breakable_Crate.prefab`: Thùng gỗ nhận sát thương, vỡ rơi máu/đạn
- `Checkpoint.prefab`: Cột mốc lưu checkpoint
- `DeathZone.prefab`: Trigger tử vong đáy vực
- `Hostage.prefab`: Con tin giải cứu (phím E)
- `EventLockBarrier.prefab`: Rào chắn chặn đường
- `EventLockZone.prefab`: Vùng kích hoạt khóa màn hình hoàn chỉnh

### Prefabs Mockup phục vụ Playtest độc lập (`Module_Environment/Prefabs/Mock`)
- `Mock_Player.prefab`: Nhân vật test có di chuyển (A/D/W/Space), bắn đạn (J), nhặt vật phẩm (`IPickupCollector`), nhận sát thương (`IDamageable`) và hồi sinh tại Checkpoint.
- `Mock_Bullet.prefab`: Đạn mockup của người chơi, bay thẳng và gây 20 sát thương.
- `Mock_Enemy.prefab`: Kính quái lính 30 HP, có tuần tra, trúng đạn nhấp nháy đỏ, tự hủy khi chết để `WaveManager` ghi nhận.
- `Mock_Boss_Viper.prefab`: Boss mockup 600 HP, bắn hạ sẽ phát `GameEvents.OnBossKilled` & `OnGameOver(true)`.

---

## 3. Danh sách Scene chính trong Game (`Assets/_Project/Game/Scenes`)

Đã thiết lập sẵn và đăng ký theo thứ tự trong **Build Settings**:
1. `MainMenu.unity`: Menu khởi động chính (Index 0).
2. `Level_01.unity`: Màn 1 - Đột nhập bãi biển đảo Raven, Boss Viper-01 (Index 1).
3. `Level_02.unity`: Màn 2 - Vượt rừng rậm tử thần, Boss Robot Hunter (Index 2).
4. `Level_03.unity`: Màn 3 - Tổng tấn công căn cứ trung tâm, Boss PROJECT TITAN (Index 3).

---

## 4. Công cụ Editor Menu

Trong Unity Editor, bạn có thể vào menu:
`MetalRebels -> Environment -> Generate All Prefabs and Scenes (Levels 1-3 & MainMenu)` để tự động khởi tạo hoặc cập nhật lại toàn bộ Prefabs và các Scene bất cứ lúc nào.

---

## 5. Hướng dẫn phối hợp với các Module khác

- **Với Trương Quang Việt Hoàng (`Module_Player`):**
  - Component Marco Player cần implement giao diện `MetalRebels.Core.Interfaces.IPickupCollector` để tiếp nhận các loại vật phẩm từ `ItemPickup`.
  - Khi hồi sinh, Player có thể lấy tọa độ qua `Checkpoint.LastActiveCheckpointPosition` (hoặc gọi qua `LevelManager.GetRespawnPosition()`).
- **Với Triệu Quang Hoàng (`Module_Enemies`):**
  - Các Prefab lính/quái cần được kéo vào danh sách `waves` trong `WaveManager` của từng Event Lock hoặc bố trí trực tiếp trên màn chơi.
- **Với Lê Thái Hiếu (`Module_SystemUI`):**
  - UI HUD và Game Manager lắng nghe `GameEvents.OnEventLockStart` và `GameEvents.OnEventLockCleared` để bật nhạc cao trào hoặc thông báo "AREA LOCKED" / "CLEAR".

