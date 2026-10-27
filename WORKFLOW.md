# Quy chuẩn làm việc - Metal Rebels

Tài liệu này quy định cách tổ chức mã nguồn, phân chia quyền sở hữu và tích hợp thay đổi để các thành viên có thể phát triển song song. Khi có quyết định mới ảnh hưởng đến nhiều phần, cập nhật tài liệu này và thông báo cho cả nhóm.

## 1. Nguyên tắc chung

1. Mỗi tính năng, scene và prefab có một người chịu trách nhiệm chính.
2. Không sửa trực tiếp thay đổi của thành viên khác nếu chưa trao đổi.
3. Phần dùng chung chỉ được thay đổi theo quy trình ở mục [Hợp đồng giữa các hệ thống](#5-hợp-đồng-giữa-các-hệ-thống).
4. Mỗi thành viên phải có thể chạy và kiểm thử phần việc của mình bằng scene test hoặc dữ liệu mẫu, không chờ scene hoàn chỉnh.
5. Tích hợp thường xuyên bằng các thay đổi nhỏ; không dồn toàn bộ việc ghép game vào cuối dự án.
6. File Unity `.meta` là một phần của asset: luôn đưa file này lên Git cùng asset tương ứng.

## 2. Cấu trúc thư mục hiện tại

Giữ cấu trúc module hiện có trong `Assets/_Project`; đặt mã nguồn, prefab và animation của mỗi thành viên trong module tương ứng. Không chuyển asset hoặc tạo một cây `Assets/Project` khác song song.

```text
Assets/
├── _Project/
│   ├── Core/
│   │   ├── Data/
│   │   ├── Events/
│   │   ├── Interfaces/
│   │   └── Shared/
│   ├── Game/
│   │   └── Scenes/
│   ├── Module_Enemies/
│   │   ├── Animations/
│   │   ├── Prefabs/
│   │   └── Scripts/
│   ├── Module_Environment/
│   │   ├── Maps/
│   │   ├── Prefabs/
│   │   └── Scripts/
│   ├── Module_Player/
│   │   ├── Animations/
│   │   ├── Prefabs/
│   │   └── Scripts/
│   └── Module_SystemUI/
│       ├── Audio/
│       ├── Prefabs/
│       └── Scripts/
├── Scenes/       # Hiện có SampleScene; Build Settings đang tham chiếu scene này
├── Settings/     # Input System và cấu hình URP/Unity
└── Welcome/      # Tài nguyên template
```

`Assets/_Project` đã có các hợp đồng/data ban đầu trong `Core`; các module gameplay và `Game/Scenes` hiện chưa có script, prefab hoặc scene game. Không ghi tài liệu hoặc giao task như thể các thư mục con khác đã tồn tại. Nếu phát sinh nhu cầu thêm thư mục, đặt trong module sở hữu tính năng đó, thống nhất với nhóm trước và commit file `.meta` do Unity tạo.

### Quy tắc đặt tên

- Dùng tiếng Anh, PascalCase cho tên class, method, property và file C#; tên file C# phải trùng tên class `MonoBehaviour` hoặc class chính.
- Dùng tên rõ chức năng, ví dụ `PlayerHealth.cs`, `EnemyProjectile.prefab`, `Level01_RavenBeach.unity`.
- Đặt script, prefab và animation trong module sở hữu tính năng; tránh tạo bản sao cùng chức năng ở nhiều module.
- Scene chính do Lê Hữu Hiệu quản lý trong `Assets/_Project/Game/Scenes`. Scene kiểm thử cá nhân đặt ở thư mục gốc module sở hữu, theo tên `Sandbox_<TenThanhVien>.unity`; các scene này không được commit.
- `Assets/Scenes/SampleScene.unity` hiện là scene duy nhất trong Build Settings. Giữ nguyên cho đến khi nhóm quyết định scene khởi động thay thế và cập nhật Build Settings.

## 3. Phân công sở hữu

| Thành viên | Thư mục/phần sở hữu chính | Trách nhiệm |
|---|---|---|
| Trương Quang Việt Hoàng | `Assets/_Project/Module_Player` | Điều khiển Marco, chiến đấu, vũ khí, HP, mạng sống, sát thương nhận vào và hồi sinh |
| Triệu Quang Hoàng | `Assets/_Project/Module_Enemies` | AI, HP và sát thương kẻ địch, đạn địch, hành vi tấn công, phase boss, xử lý kẻ địch bị hạ |
| Lê Hữu Hiệu | `Assets/_Project/Module_Environment`; toàn bộ scene chính trong `Assets/_Project/Game/Scenes` | Tilemap, camera, Event Lock, waves, checkpoint, con tin, thùng phá hủy, bố trí vật phẩm và quản lý scene chính |
| Lê Thái Hiếu | `Assets/_Project/Module_SystemUI`; phối hợp với `Assets/_Project/Core` | HUD, menu, luồng thắng/thua và chuyển màn, âm thanh, kiểm thử luồng game và đóng gói build |

Nhóm trưởng điều phối tích hợp và quản lý các phần dùng chung. Quyền sở hữu không cấm hỗ trợ nhau, nhưng người sở hữu cần được thông báo trước khi thay đổi file thuộc phần của họ.

### Tài nguyên dùng chung

Các nội dung dưới đây có nguy cơ xung đột cao và cần một người phụ trách thay đổi tại một thời điểm:

- `Assets/_Project/Core/Interfaces`, `Assets/_Project/Core/Events`, `Assets/_Project/Core/Data`, `Assets/_Project/Core/Shared`.
- Input Actions, Tag/Layer, Project Settings, Build Settings và cấu hình Render Pipeline.
- Scene trong `Assets/_Project/Game/Scenes`, prefab hoặc asset được nhiều module tham chiếu.

Trước khi sửa các tài nguyên này, thông báo phạm vi thay đổi, người thực hiện và thời điểm dự kiến. Người phụ trách chịu trách nhiệm thông báo cho các phần bị ảnh hưởng.

## 4. Quy chuẩn Git

Hiện trạng ban đầu của project cần được kiểm tra và kết nối với Git repository trước khi nhóm làm việc song song. Sau khi repository được thiết lập, áp dụng quy trình sau:

1. Nhánh `main` luôn ở trạng thái có thể mở/chạy và tạo build; không commit trực tiếp lên `main`.
2. Mỗi task dùng một nhánh ngắn hạn, ví dụ `feature/player-health`, `feature/boss-hunter`, `feature/level-event-lock`, `fix/hud-ammo`.
3. Tạo pull request để đưa thay đổi vào nhánh tích hợp của nhóm (ví dụ `develop`) hoặc `main` nếu nhóm không dùng nhánh `develop`. Nhờ ít nhất một thành viên khác xem xét thay đổi trước khi merge.
4. Kéo thay đổi mới nhất về nhánh của mình thường xuyên. Tránh để nhánh tồn tại quá lâu hoặc gom nhiều tính năng không liên quan vào một pull request.
5. Commit nhỏ, mô tả đúng mục đích, ví dụ `Add player health and damage handling`.
6. Không đưa vào Git các thư mục sinh tự động như `Library`, `Temp`, `Logs`, `obj`, `Build` và file cá nhân của IDE. Dùng `.gitignore` chuẩn Unity; không bỏ qua thư mục `Assets` hoặc các file `.meta`.
7. Không tự ý đổi GUID, xóa `.meta`, đổi tên asset ngoài Unity Editor, hoặc tạo lại asset đã có. Khi đổi tên/di chuyển asset, dùng Unity Editor và commit cả thay đổi asset lẫn `.meta`.
8. Với file nhị phân lớn, nhóm thống nhất trước cách lưu trữ; chỉ cấu hình Git LFS khi repository đã hỗ trợ và cả nhóm cài LFS.

Nếu có xung đột scene hoặc prefab, người sở hữu file đó chủ trì giải quyết trong Unity Editor và kiểm tra lại các tham chiếu trước khi merge. Không chọn một phiên bản theo kiểu “ours/theirs” nếu chưa xác nhận các thay đổi cần giữ.

## 5. Hợp đồng giữa các hệ thống

### Nguyên tắc

- Dùng interface hoặc API nhỏ cho **yêu cầu/hành động**; dùng event cho **thông báo sự kiện đã xảy ra**.
- Hệ thống tạo và sở hữu trạng thái là nơi duy nhất được thay đổi trạng thái đó. Ví dụ: `Health` cập nhật HP; HUD chỉ hiển thị.
- Không để một hệ thống gọi trực tiếp các chi tiết nội bộ của hệ thống khác nếu có thể dùng hợp đồng chung.
- Hợp đồng phải ghi rõ kiểu dữ liệu, đơn vị, thời điểm phát event, điều kiện biên và bên chịu trách nhiệm.
- Không thêm event bus toàn cục hoặc abstraction mới nếu event C# hoặc interface tại chỗ đã giải quyết được nhu cầu.

### Hợp đồng tối thiểu

| Hợp đồng | Bên phát/cung cấp | Bên nhận/sử dụng | Quy ước |
|---|---|---|---|
| Nhận sát thương | Projectile, vụ nổ hoặc vùng sát thương | Player, enemy, boss, vật thể phá hủy | Dùng chung kiểu dữ liệu sát thương; lượng sát thương không âm; mục tiêu tự quyết định HP và trạng thái chết |
| Thay đổi HP / chết | Thành phần Health của đối tượng | HUD, hiệu ứng, hệ thống hồi sinh hoặc wave | HP nằm trong khoảng `0..Maximum`; thông báo đổi HP phát sau khi cập nhật; thông báo chết chỉ phát một lần |
| Nhặt vật phẩm | Pickup trên màn chơi | Player hoặc thành phần kho đồ | Trả kết quả nhận thành công/thất bại; chỉ thu hồi pickup khi nhận thành công |
| Địch bị hạ | Enemy/Boss | Wave/Event Lock và hiệu ứng liên quan | Báo một lần; không tự điều khiển scene hoặc menu |
| Hoàn tất khu vực/wave | Level/Event Lock | Game Flow hoặc Level Controller | Hoàn tất sau khi điều kiện được định nghĩa (ví dụ mọi địch của wave đã bị hạ); không tự tải scene |
| Trạng thái màn chơi | Game Flow | UI | Game Flow sở hữu thắng/thua, retry và chuyển màn; UI hiển thị trạng thái và gửi yêu cầu, không tự thay đổi gameplay |

Trước khi triển khai, nhóm trưởng và các thành viên liên quan ghi thống nhất kiểu dữ liệu và chữ ký cụ thể trong `Assets/_Project/Core/Interfaces` và `Assets/_Project/Core/Events`. Dữ liệu dùng chung đặt trong `Assets/_Project/Core/Data` hoặc `Assets/_Project/Core/Shared` theo thống nhất của nhóm. Không tạo nhiều kiểu `DamageInfo`, `HealthChanged` hoặc pickup trùng mục đích ở các module riêng.

### Vòng đời event

- Đăng ký event khi component được kích hoạt, thường trong `OnEnable`; hủy đăng ký trong `OnDisable`.
- Không phát event sau khi đối tượng đã bị hủy hoặc component không còn hoạt động.
- Event không thay thế API cần trả kết quả đồng bộ. Ví dụ, kết quả nhặt vật phẩm nên trả thành công/thất bại trực tiếp.
- Nếu thay đổi tên, tham số hoặc ý nghĩa của hợp đồng, báo các bên dùng hợp đồng đó và cập nhật các bên liên quan trong cùng đợt tích hợp.

## 6. Làm việc độc lập và tích hợp

1. Mỗi thành viên có thể tạo scene test riêng tại thư mục gốc module mình sở hữu, dùng placeholder nếu art hoặc prefab thật chưa sẵn sàng. Đặt tên `Sandbox_<TenThanhVien>.unity`; các scene sandbox này bị `.gitignore` loại khỏi Git.
2. Scene test chỉ chứa các đối tượng cần cho tính năng đó; không phụ thuộc scene chính hoặc toàn bộ Game Manager.
3. Người làm Player kiểm thử di chuyển, bắn, HP và hồi sinh độc lập; người làm enemy/boss kiểm thử AI, sát thương và chết độc lập; người làm level kiểm thử bố trí, Event Lock và checkpoint bằng prefab đại diện; người làm UI kiểm thử với dữ liệu mẫu.
4. Bàn giao prefab kèm script, dữ liệu cấu hình, dependency và hướng dẫn setup ngắn. Người nhận không phải sửa trực tiếp code nội bộ của prefab để sử dụng.
5. Tích hợp từng lát dọc nhỏ, ví dụ Player nhận sát thương và HUD cập nhật, hoặc wave kết thúc và Event Lock mở. Không đợi toàn bộ game hoàn tất mới ghép.
6. Chỉ Lê Hữu Hiệu chỉnh sửa các scene chính trong `Assets/_Project/Game/Scenes`. Thành viên khác dựng/test trong sandbox cá nhân, sau đó bàn giao prefab và cấu hình để Hữu Hiệu đặt vào scene chính.
7. Thay đổi Input Actions, Tag/Layer, Build Settings hoặc cấu hình project phải được thông báo và thống nhất trước khi áp dụng.

## 7. Kiểm tra trước khi tạo pull request

- Project mở được trong đúng phiên bản Unity mà nhóm thống nhất; không có lỗi compile mới.
- Tính năng đã được kiểm tra bằng scene test hoặc luồng liên quan; ghi rõ các bước kiểm tra trong pull request.
- Không có tham chiếu asset bị mất; prefab và scene thay đổi đã được mở kiểm tra trong Unity.
- Các thay đổi `.meta` đi kèm asset tương ứng.
- Không vô tình đưa `Library`, `Temp`, `Logs`, build output hoặc file cá nhân vào thay đổi.
- Pull request chỉ chứa một mục tiêu rõ ràng, có mô tả hành vi thay đổi và ảnh/video nếu thay đổi hình ảnh hoặc UI.
- Người sở hữu hệ thống liên quan đã xem xét thay đổi; nếu sửa hợp đồng chung, đã thông báo và cập nhật các bên phụ thuộc.

## 8. Quyết định cần thống nhất trong nhóm

Trước khi bắt đầu tích hợp chung, nhóm cần chốt và ghi lại:

- Phiên bản Unity Editor dùng chung.
- Tên và quy trình các nhánh Git (`main` và có/không có `develop`).
- Danh sách scene được đưa vào Build Settings và người quản lý danh sách này.
- Tag/Layer, Input Actions và các quy ước cấu hình dùng chung.
- Chữ ký cụ thể của các hợp đồng trong mục 5.
