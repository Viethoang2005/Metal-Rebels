# METAL REBELS

Game hành động 2D side-scrolling Run-and-Gun kết hợp Action Platformer, phát triển bằng Unity. Người chơi điều khiển Marco đột nhập đảo Raven, chiến đấu với Black Viper, giải cứu con tin và phá hủy siêu vũ khí PROJECT TITAN.

> **Trạng thái hiện tại:** project đang ở giai đoạn triển khai nền tảng. Đã có cấu trúc module và một số hợp đồng/dữ liệu trong `Core`; các module gameplay, prefab và scene chính vẫn đang được phát triển. `Assets/Scenes/SampleScene.unity` là scene mẫu duy nhất trong Build Settings, chưa phải màn chơi của game.

## Mục lục

- [Thông tin project](#thông-tin-project)
- [Bắt đầu làm việc](#bắt-đầu-làm-việc)
- [Tổng quan game](#tổng-quan-game)
- [Gameplay và quy tắc thiết kế](#gameplay-và-quy-tắc-thiết-kế)
- [Cấu trúc project](#cấu-trúc-project)
- [Phân công và quyền sở hữu](#phân-công-và-quyền-sở-hữu)
- [Hợp đồng giữa các hệ thống](#hợp-đồng-giữa-các-hệ-thống)
- [Quy trình làm việc và Git](#quy-trình-làm-việc-và-git)
- [Tình trạng và việc còn thiếu](#tình-trạng-và-việc-còn-thiếu)
- [Tài liệu tham khảo](#tài-liệu-tham-khảo)

## Thông tin project

| Thuộc tính | Giá trị |
|---|---|
| Tên game | Metal Rebels |
| Nhóm | Apex Group - Nhóm 8 |
| Thể loại | 2D Side-scrolling Run-and-Gun / Action Platformer |
| Engine | Unity **6000.6.4f1** |
| Render pipeline | Universal Render Pipeline 2D |
| Input | Unity Input System |
| Scene đang có trong Build Settings | `Assets/Scenes/SampleScene.unity` |

Các package và phiên bản được khai báo tại `Packages/manifest.json`, khóa phiên bản dependency tại `Packages/packages-lock.json`. Không tự ý đổi phiên bản Unity hoặc package dùng chung nếu chưa trao đổi với nhóm.

## Bắt đầu làm việc

### Yêu cầu

- Unity Hub và Unity Editor **6000.6.4f1**.
- Git.
- Git LFS. `.gitattributes` bật LFS cho một số loại asset như PNG, JPG, PSD, audio và model.
- Kết nối mạng khi Unity tải các package khai báo trong manifest.

### Clone và mở project

Sau khi quản trị viên cung cấp URL GitHub và cấp quyền:

```powershell
git lfs install
git clone <URL-repository>
cd MetalRebels
git lfs pull
```

Mở Unity Hub → **Add project from disk** → chọn thư mục vừa clone → mở bằng Unity **6000.6.4f1**. Chờ Unity import asset và khôi phục package trước khi bắt đầu chỉnh sửa.

Để cập nhật nhánh hiện tại:

```powershell
git pull
git lfs pull
```

Trước khi pull, commit hoặc lưu riêng thay đổi đang làm dở. Không xóa thư mục `Library` trong khi Unity đang mở project.

### Input Actions hiện có

Asset `Assets/Settings/InputSystem_Actions.inputactions` có các action map `Player` và `UI`. Các binding Player hiện gồm:

| Action | Binding bàn phím hiện có |
|---|---|
| `Move` | WAD hoặc phím mũi tên |
| `Attack` | J |
| `Jump` | Space |
| `Crouch` | S hoặc mũi tên xuống |
| `Interact` | E | (cứu con tin)
| `Grenade` | G |
| `Select Weapon` | 1 / 2 / 3 / 4| (1: Súng lục, 2: Shotgun, 3: Súng trường, 4: Rocket(nếu có), 5: Lựu đạn )
| `Pause` | ESC hoặc P |

Input Actions cũng có binding gamepad/thiết bị khác. Hãy dùng asset hiện có thay vì tạo bộ input trùng lặp. Nếu cần đổi action hoặc binding, báo nhóm trước vì Player, UI và scene có thể dùng chung asset này. Cách xử lý aim, bắn nhiều hướng và điều khiển trên từng thiết bị cần được kiểm tra trong gameplay thực tế.

## Tổng quan game

### Cốt truyện

Tổ chức quân sự Black Viper chiếm đảo Raven, bắt giữ các nhà khoa học và ép họ hoàn thiện PROJECT TITAN, một siêu vũ khí có thể phá hủy các căn cứ quân sự. Marco, một chiến binh đặc nhiệm, được cử đột nhập đơn độc vào đảo, phá vỡ hệ thống phòng thủ, giải cứu con tin và phá hủy lõi TITAN trước khi vũ khí được kích hoạt.

### Vòng lặp chính

**Di chuyển → Quan sát → Bắn → Né tránh → Tiêu diệt địch → Thu thập vật phẩm → Vượt khu vực → Đánh boss.**

Người chơi chủ động nhấn nút để bắn; nhân vật không tự động khai hỏa. Người chơi có thể di chuyển, nhảy, ngồi, đổi hướng bắn, đổi vũ khí, dùng lựu đạn, phá vật thể và giải cứu con tin.

### Tiến trình màn chơi dự kiến

1. **Màn 1 - Đột nhập đảo Raven:** bãi biển, chiến hào và công sự; kết thúc bằng boss xe bọc thép Viper-01.
2. **Màn 2 - Rừng rậm tử thần:** rừng nhiệt đới, lính và robot; kết thúc bằng robot Hunter.
3. **Màn 3 - Tổng tấn công:** căn cứ trung tâm Black Viper; đánh bại PROJECT TITAN và phá hủy lõi năng lượng.

Mỗi màn dự kiến kết hợp giao tranh tự do, vật phẩm/thùng phá hủy, con tin, Event Lock và boss. Event Lock dừng camera/khóa lối đi cho đến khi các wave được xử lý. Boss dự kiến có ít nhất hai phase; phase sau bắt đầu khi HP xuống dưới 50%.

## Gameplay và quy tắc thiết kế

Các thông số dưới đây là **mục tiêu thiết kế trong đề cương**, chưa đồng nghĩa chúng đã được triển khai hoặc cân bằng trong Unity. Nếu gameplay thực tế cần thay đổi, ghi nhận thay đổi để nhóm cập nhật đề cương và README.

### Marco

- 3 mạng; HP tối đa mỗi mạng: 100.
- Mất HP khi trúng đòn; hồi sinh tại checkpoint với 100 HP.
- Bất tử sau khi nhận sát thương: 1 giây; sau hồi sinh: 2 giây.
- Trong thời gian bất tử có hiệu ứng nhấp nháy; đề cương quy định Marco không thể bắn trong thời gian này.
- Vật phẩm hồi máu khôi phục 30 HP.

### Vũ khí và đạn

| Vũ khí | Sát thương thiết kế | Nhịp bắn tối đa | Đạn tối đa | Băng đạn | Nạp đạn |
|---|---:|---:|---:|---:|---:|
| Súng lục | 20/viên | 2 viên/giây | Vô hạn | Không áp dụng | Không áp dụng |
| Shotgun | 50/phát | 1 phát/giây | 30 | 8 | 3 giây |
| Súng trường | 15/viên | 5 viên/giây | 120 | 30 | 1,5 giây |
| Rocket | 100/vụ nổ | 1 quả/2 giây | 5 | 1 | 2 giây |
| Lựu đạn | 30/quả | Theo lần ném | 5 quả | Mỗi lần ném 1 quả | Không áp dụng |

Lựu đạn nổ sau 3 giây, bán kính thiết kế 3 m. Vũ khí đặc biệt có đạn giới hạn; khi hết đạn, người chơi có thể quay về súng lục cơ bản.

### Kẻ địch và boss

| Đối tượng | HP thiết kế | Sát thương/đòn đánh thiết kế |
|---|---:|---|
| Lính thường | 30 | 20 |
| Lính xe | 70 | 30 |
| Viper-01 (boss màn 1) | 600 | Súng máy: 20; pháo: 35 |
| Hunter (boss màn 2) | 900 | Cận chiến: 25; tên lửa: 35; laser: 45 |
| PROJECT TITAN (boss cuối) | 1500 | Tên lửa: 35; laser: 45 |

AI địch có thể gồm trạng thái canh/tuần tra, phát hiện Marco, tấn công, nhận sát thương và bị hạ. Hành vi cụ thể tùy từng loại; không để UI hoặc Level sửa trực tiếp trạng thái nội bộ của AI.

### Vật phẩm

| Vật phẩm | Tác dụng thiết kế |
|---|---|
| HP | Hồi 30 HP |
| Đạn shotgun | +20 viên shotgun |
| Đạn súng trường | +30 viên |
| Rocket | Nhận vũ khí Rocket và 1 viên |
| Đạn Rocket | +2 viên |
| Lựu đạn | +2 quả |

Vật phẩm có thể xuất hiện từ thùng phá hủy hoặc phần thưởng giải cứu con tin. Checkpoint lưu vị trí hồi sinh gần nhất. Pause phải tạm dừng gameplay và cho phép tiếp tục, khởi động lại, chỉnh thiết lập hoặc quay về menu.

## Cấu trúc project

```text
MetalRebels/
├── .gitattributes
├── .gitignore
├── README.md
├── WORKFLOW.md
├── METAL REBELS.md
├── Packages/
│   ├── manifest.json
│   └── packages-lock.json
├── ProjectSettings/
└── Assets/
    ├── _Project/
    │   ├── Core/
    │   │   ├── Interfaces/     # Hợp đồng giữa gameplay, hiện có IDamageable
    │   │   ├── Events/         # Thông báo dùng chung, hiện có GameEvents, PlayerEvents
    │   │   ├── Data/           # ScriptableObject/data, hiện có WeaponData
    │   │   └── Shared/         # Asset dùng chung; thống nhất trước khi thêm/sửa
    │   ├── Module_Player/
    │   │   ├── Scripts/
    │   │   ├── Prefabs/
    │   │   ├── Animations/
    │   │   └── Sandbox_VietHoang.unity       # Scene test cục bộ, không commit
    │   ├── Module_Enemies/
    │   │   ├── Scripts/
    │   │   ├── Prefabs/
    │   │   ├── Animations/
    │   │   └── Sandbox_QuangHoang.unity      # Scene test cục bộ, không commit
    │   ├── Module_Environment/
    │   │   ├── Scripts/
    │   │   ├── Prefabs/
    │   │   ├── Maps/
    │   │   └── Sandbox_HuuHieu.unity         # Scene test cục bộ, không commit
    │   ├── Module_SystemUI/
    │   │   ├── Scripts/
    │   │   ├── Prefabs/
    │   │   ├── Audio/
    │   │   └── Sandbox_ThaiHieu.unity        # Scene test cục bộ, không commit
    │   └── Game/
    │       └── Scenes/                       # Scene chính; chỉ Lê Hữu Hiệu chỉnh sửa
    │           ├── MainMenu.unity            # Dự kiến
    │           ├── Level_01.unity            # Dự kiến
    │           ├── Level_02.unity            # Dự kiến
    │           └── Level_03.unity            # Dự kiến
    ├── Scenes/
    │   └── SampleScene.unity                 # Scene hiện tại trong Build Settings
    ├── Settings/                              # URP, Input Actions và cấu hình scene
    └── Welcome/                               # Tài nguyên template Unity
```

Các tên `Sandbox_*.unity` và scene chính liệt kê ở trên là quy ước/kế hoạch; chúng chưa được tạo trong repository. `.gitignore` loại trừ sandbox cá nhân cùng `.meta` của chúng. Không đưa scene sandbox vào Build Settings.

Asset chỉ được đặt theo module sở hữu. Nếu cần một loại thư mục mới, thành viên sở hữu module tạo trong module đó bằng Unity Editor. Không tạo cây `Project/` thứ hai; nội dung game thuộc `Assets/_Project/`.

### Tài nguyên Core đã có

- `Core/Interfaces/IDamageable.cs`: có `IDamageable.TakeDamage(int damageAmount)`.
- `Core/Data/WeaponData.cs`: ScriptableObject cấu hình loại/tên vũ khí, projectile, sát thương, fire rate, đạn, nạp đạn và vụ nổ. Tạo asset bằng menu **MetalRebels → Data → WeaponData**.
- `Core/Events/PlayerEvents.cs`: các Action thông báo HP, mạng, chết/hồi sinh, vũ khí và đạn.
- `Core/Events/GameEvents.cs`: các Action cho Event Lock, boss, game over và pause.

`WeaponData` hiện chỉ có giá trị mặc định chung trong code, chưa có asset cấu hình cho từng loại súng. Hãy dùng bảng thông số ở trên làm mục tiêu khi tạo data; không xem giá trị mặc định là cân bằng cuối cùng. Các event trong `PlayerEvents`/`GameEvents` hiện khai báo bằng `Action` field tĩnh; khi đăng ký, luôn hủy đăng ký tương ứng và trao đổi trước khi đổi tên/tham số.

## Phân công và quyền sở hữu

Mỗi module có một người chịu trách nhiệm chính. Thành viên có thể phối hợp, nhưng không tự ý sửa file hoặc scene đang do người khác sở hữu.

| Thành viên | Vùng sở hữu | Công việc và bàn giao |
|---|---|---|
| Trương Quang Việt Hoàng (nhóm trưởng) | `Assets/_Project/Module_Player/` | Điều khiển Marco, chiến đấu, vũ khí, projectile người chơi, HP, mạng sống, sát thương và hồi sinh. Tạo/duy trì asset player trong `Core` cùng nhóm. |
| Triệu Quang Hoàng | `Assets/_Project/Module_Enemies/` | Lính/xe/robot, AI, HP, đạn địch, boss và phase. Bàn giao prefab có collider, damage, scripts và dependency rõ ràng. |
| Lê Hữu Hiệu | `Assets/_Project/Module_Environment/` và `Assets/_Project/Game/Scenes/` | Tilemap/map, camera, Event Lock, wave spawner, checkpoint, hostage, breakable, item placement; là người duy nhất chỉnh sửa scene chính và quản lý danh sách scene build. |
| Lê Thái Hiếu | `Assets/_Project/Module_SystemUI/` | HUD, menu, Game Manager/Game Flow, thắng/thua, pause, chuyển scene, audio, kiểm thử luồng tổng và đóng gói build. Phối hợp với owner Core khi cần event/data chung. |

`Core` là vùng dùng chung, không thuộc quyền sửa độc quyền của một module. Nhóm trưởng điều phối thay đổi interface, event và data dùng chung; người sửa cần thông báo trước cho các module phụ thuộc.

### Quy tắc về scene

- Scene test cá nhân đặt ở gốc module, tên `Sandbox_<TenThanhVien>.unity`; dùng để thử prefab/hành vi độc lập. Các scene này không được commit.
- `Assets/_Project/Game/Scenes/` chỉ chứa scene chính như Main Menu và ba level. Chỉ Lê Hữu Hiệu chỉnh sửa scene chính.
- Các thành viên khác phát triển prefab/config riêng và bàn giao cho Hữu Hiệu tích hợp vào scene chính.
- `Assets/Scenes/SampleScene.unity` là scene template hiện được bật trong Build Settings. Chỉ đổi danh sách scene build sau khi nhóm thống nhất scene khởi động thay thế.

## Hợp đồng giữa các hệ thống

### Nguyên tắc

1. Interface/API dùng cho yêu cầu cần phản hồi; event dùng để thông báo việc đã xảy ra.
2. Mỗi trạng thái chỉ có một chủ sở hữu. Health cập nhật HP; HUD chỉ đọc và hiển thị. Wave/Event Lock sở hữu trạng thái wave; enemy chỉ báo đã chết.
3. Không để Player/Enemy/UI/Level phụ thuộc trực tiếp vào chi tiết triển khai nội bộ của nhau.
4. Thay đổi hợp đồng trong `Core` phải được báo trước và tích hợp cùng các bên phụ thuộc.
5. Đăng ký event trong `OnEnable`, hủy đăng ký trong `OnDisable`; tránh gọi tĩnh từ object đã bị hủy.

### Luồng giao tiếp dự kiến

| Từ | Đến | Giao tiếp |
|---|---|---|
| Đạn/vụ nổ | Player, enemy, boss hoặc vật thể phá hủy | Gọi `IDamageable.TakeDamage(...)` |
| Player Health/Inventory | HUD | Báo HP, mạng, vũ khí, đạn qua event; HUD không tự sửa gameplay |
| Enemy/Boss | Wave/Event Lock | Báo bị hạ đúng một lần; không tự mở cổng hoặc tải scene |
| Event Lock/Level | Game Flow | Báo khu vực/wave hoàn tất; Level không tự chuyển scene |
| Game Flow | UI | Báo pause, game over, clear; UI gửi yêu cầu nút bấm tới Game Flow |
| Pickup | Player/Inventory | Yêu cầu nhận vật phẩm và chỉ biến mất khi được chấp nhận |

Các hợp đồng hiện có mới là nền tảng ban đầu. Chữ ký cụ thể và điều kiện như HP giới hạn `0..Maximum`, chết chỉ báo một lần, cách phân loại nguồn sát thương cần được thống nhất khi triển khai từng tính năng; không giả định những hành vi đó đã tự có trong các file hiện tại.

## Quy trình làm việc và Git

### Quy tắc phối hợp

1. Chia task theo chức năng và module sở hữu; mỗi file/scene có một người chịu trách nhiệm chính.
2. Làm và kiểm tra trong sandbox cá nhân bằng placeholder, không chờ asset hoặc scene của thành viên khác.
3. Bàn giao prefab/script kèm dependency, cách setup, các event/interface dùng và các bước kiểm tra.
4. Tích hợp từng luồng nhỏ: ví dụ Player nhận damage và HUD đổi HP; wave hết địch và Event Lock mở; boss chết và màn chơi clear.
5. Scene chính, Input Actions, Tag/Layer, Build Settings, `Core` và prefab dùng chung cần báo nhóm trước khi sửa.

### Git và Unity

- Mỗi task dùng nhánh ngắn hạn, ví dụ `feature/player-combat`, `feature/boss-hunter`, `feature/level-event-lock`, `fix/hud-ammo`.
- Không commit trực tiếp lên `main`; tạo Pull Request nhỏ, mô tả thay đổi và cách kiểm thử; nhờ thành viên khác review.
- Commit asset Unity cùng file `.meta`; đổi tên/di chuyển asset bằng Unity Editor, không xóa hoặc tạo lại `.meta`.
- Không đưa `Library/`, `Temp/`, `Logs/`, `obj/`, `Build/`, `UserSettings/`, file IDE hoặc scene sandbox vào commit.
- Không sửa scene chính đồng thời. Nếu có xung đột prefab/scene, báo owner file và giải quyết trong Unity Editor; không chọn mù `ours` hoặc `theirs`.
- Dùng Unity Version Control / Smart Merge khi giải quyết YAML conflict nếu có công cụ phù hợp.
- Cài Git LFS và chạy `git lfs install` trước khi clone; không commit file LFS pointer thay cho nội dung asset.

### Trạng thái GitHub

Tại thời điểm README này được cập nhật, thư mục project hiện tại **chưa có `.git` và chưa có remote GitHub**. Vì vậy chưa thể clone/pull từ repo này. Nhóm trưởng cần tạo hoặc chọn repository, cấp quyền thành viên, push các file nguồn Unity (`Assets` và `.meta`, `Packages`, `ProjectSettings`, `.gitignore`, `.gitattributes`, README và tài liệu) rồi cung cấp URL clone. Không push thư mục cache/build nêu trên.

## Tình trạng và việc còn thiếu

- [x] Cấu trúc module `Core`, `Module_Player`, `Module_Enemies`, `Module_Environment`, `Module_SystemUI`, `Game/Scenes`.
- [x] Bước đầu có `WeaponData`, `IDamageable`, `GameEvents`, `PlayerEvents`.
- [ ] Tạo các scene sandbox cá nhân và gameplay scripts/prefabs cho từng module.
- [ ] Tạo asset WeaponData cho từng loại súng và kiểm thử với chỉ số thiết kế.
- [ ] Tạo scene chính `MainMenu`, `Level_01`, `Level_02`, `Level_03`; cập nhật Build Settings sau khi thống nhất.
- [ ] Khởi tạo/kết nối GitHub remote, push commit ban đầu và kiểm thử clone + Git LFS trên máy thành viên.
- [ ] Các ảnh được tham chiếu trong `METAL REBELS.md` hiện chưa có trong project; cần bổ sung nếu muốn hình minh họa hiển thị trên GitHub.
- [ ] Xác nhận các package prerelease trong `Packages/manifest.json` có cần cho game và hoạt động trên máy tất cả thành viên.
- [ ] Đổi `companyName: DefaultCompany` và cấu hình application identifier trước khi đóng gói bản phát hành.

## Tài liệu tham khảo

- [WORKFLOW.md](./WORKFLOW.md): quy chuẩn phối hợp, quyền sở hữu và hợp đồng làm việc.
- [METAL REBELS.md](./METAL%20REBELS.md): đề cương môn học, gameplay, cơ chế, cốt truyện và chỉ số thiết kế.

