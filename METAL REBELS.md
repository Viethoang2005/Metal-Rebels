TRƯỜNG ĐẠI HỌC THUỶ LỢI

KHOA CÔNG NGHỆ THÔNG TIN

![](Aspose.Words.c9231e24-7b4e-4f38-9d74-4652ab6b8d21.001.jpeg)

ĐỀ CƯƠNG BÀI TẬP CUỐI MÔN

“THIẾT KẾ VÀ PHÁT TRIỂN TRÒ CHƠI” 

**Tên đề tài:**
**\
` `THIẾT KẾ VÀ PHÁT TRIỂN TRÒ CHƠI “ METAL REBELS”

**Nhóm thực hiện**: Apex Group ( Nhóm 8 )

|Trương Quang Việt Hoàng|2351060450|
| :-: | :-: |
|Triệu Quang Hoàng|2351060449|
|Lê Hữu Hiệu||
|Lê Thái Hiếu||

**Giảng viên phụ trách môn học**: ThS. Trương Xuân Nam 


**Hà Nội, 2026**
# <a name="_g3goc5qplqhm"></a>**Phân chia công việc**
**Bảng phân chia công việc**

|STT|Thành viên|Công việc|
| :- | :- | :- |
|1|Trương Quang Việt Hoàng (nhóm trưởng)|<p>Hệ thống nhân vật Marco và chiến đấu phía người chơi </p><p>HP, mạng sống, nhận sát thương và hồi sinh của Marco </p><p>Quản lý Git/GitHub và điều phối tích hợp </p>|
|2|Triệu Quang Hoàng|<p>Hệ thống kẻ địch và boss </p><p>AI, HP, sát thương, đạn địch, hành vi tấn công, các giai đoạn chiến đấu của boss và xử lý khi kẻ địch bị tiêu diệt </p>|
|3|Lê Hữu Hiệu|<p>Thiết kế và lập trình màn chơi </p><p></p><p>Tilemap, camera, Event Lock, spawn quái, checkpoint, con tin, thùng phá hủy và vật phẩm trong màn. </p>|
|4|Lê Thái Hiếu|<p>UI/HUD, Game Manager, luồng thắng/thua và chuyển màn </p><p>Tích hợp âm thanh, hiệu ứng gameplay cơ bản, kiểm thử luồng game và đóng gói bản build. </p>|


# <a name="_ejnhhtutkcrv"></a>**Mục lục**
[**Phân chia công việc**	](#_g3goc5qplqhm)**2**

[**Mục lục	3**](#_ejnhhtutkcrv)

[**Phần 1: Giới thiệu	5**](#_ko2yf5w7jguq)

[1. High concept	5](#_g1140zc90vkm)

[2. Giới thiệu nhóm làm game	5](#_9hlzdzx2911v)

[**Phần 2: Tổng quan về Game	6**](#_rn6t5lh917dt)

[1. Giới thiệu chung:	6](#_oro4mqt08n1)

[2. Yếu tố:	6](#_5tbvtzfeompu)

[3. Nội dung:	6](#_ccf7tfnxeea)

[4. Nhóm người chơi game được nhắm đến:	7](#_boxxljbh2ype)

[5. Game Flow:	7](#_lr3gsz69m4jd)

[6. Look & Feel:	8](#_q8a2c8llbl4h)

[7. Các khía cạnh tác động vào người chơi	8](#_ih5x3ap3v5i2)

[8. Mục tiêu trải nghiệm	9](#_m562lqb418um)

[9. Các trải nghiệm được lồng vào màn chơi	9](#_huqyx2i6ryg4)

[**Phần 3: Gameplay and Mechanics	9**](#_vk8eaypdffjo)

[1. Gameplay	9](#_snd8sujptnn2)

[a. Core game loop:	9](#_etuyjftyhkvw)

[b. Điều khiển và di chuyển	10](#_z2kbyc3i753b)

[c. Chiến đấu	10](#_4ka5c7aqbymc)

[d. Thu thập	10](#_p11lsapm2ggs)

[2. Mechanics:	11](#_afmw798lpevj)

[Cơ chế di chuyển	11](#_6sudn5wsdufi)

[Cơ chế bắn	11](#_r7hdd6y6760t)

[Cơ chế ngắm và hướng bắn	12](#_a98n3scn8u2j)

[Cơ chế vũ khí	12](#_1d3vst645ocp)

[Cơ chế nhặt và thay đổi vũ khí	12](#_y32fm2nm43ga)

[Cơ chế máu và sát thương	12](#_b0fj2unywf51)

[Cơ chế bất tử tạm thời	14](#_fratcwwm6lc)

[Cơ chế né tránh	14](#_8nemlzlsn1le)

[Cơ chế kẻ địch	14](#_yz6tczm849d4)

[Cơ chế AI của kẻ địch	14](#_mt0049777ro7)

[Cơ chế đạn và vật thể tấn công	14](#_pui0oxf0tbcp)

[Cơ chế lựu đạn và vụ nổ	15](#_hpz8whwteivo)

[Cơ chế vật thể có thể phá hủy	15](#_96rfzo6n6rkx)

[Cơ chế giải cứu con tin	15](#_3xv8yu7v9abm)

[Cơ chế khóa khu vực và đợt kẻ địch	15](#_aq4toxwtkiin)

[Cơ chế boss	16](#_5impvulr79u2)

[Cơ chế vật phẩm	16](#_tjqedmjg8gp8)

[Cơ chế Checkpoint	16](#_d3ykzzxk1yn4)

[Cơ chế camera	17](#_jed081z5xnuh)

[Cơ chế Pause	17](#_dgry1q24j2vy)

[Cơ chế chiến thắng	17](#_rmxeghtjv1uv)

[**Phần 4: Cốt truyện, bối cảnh và nhân vật trong game	17**](#_sf6ewo6nwsc)

[1. Story	17](#_xblpdjh44ha5)

[2. Characters	21](#_hxyipk3b06l1)

[**Phần 5: Các màn chơi	23**](#_sdsvyytfrs34)

[**Phần 6: Giao diện	24**](#_jusj4b5gi06)

[**Phần 7: Game matrix	24**](#_jtxpn6ajc6j6)
# <a name="_sirh4pdf2qr2"></a>
# <a name="_ko2yf5w7jguq"></a>**Phần 1: Giới thiệu**
1. ## <a name="_g1140zc90vkm"></a>High concept
**METAL REBELS** là game hành động 2D Run-and-Gun với góc nhìn Side-Scrolling, nơi người chơi vào vai một chiến binh nổi dậy đột nhập vào căn cứ quân sự của tập đoàn độc tài, sử dụng kho vũ khí đa dạng để tiêu diệt kẻ địch.

Ảnh minh họa cho game:

![](Aspose.Words.c9231e24-7b4e-4f38-9d74-4652ab6b8d21.002.jpeg)

![](Aspose.Words.c9231e24-7b4e-4f38-9d74-4652ab6b8d21.003.jpeg)
1. ## <a name="_9hlzdzx2911v"></a>Giới thiệu nhóm làm game
   Tên nhóm: Apex Group ( nhóm 8 )

   Thành viên:

   Trương Quang Việt Hoàng

   Triệu Quang Hoàng

   Lê Hữu Hiệu

   Lê Thái Hiếu
# <a name="_rn6t5lh917dt"></a> **Phần 2: Tổng quan về Game**
1. ## <a name="_oro4mqt08n1"></a>Giới thiệu chung:
1. **Thể loại game**: 2D Side-scrolling Run & Gun (Hành động bắn súng góc nhìn ngang) kết hợp Action Platformer. 
1. **Chủ đề**: Chiến tranh hiện đại kết hợp khoa học viễn tưởng
1. **Phong cách**: Sôi động, dồn dập mang lại cảm giác bùng nổ và đã tay trong từng pha xả súng; đồng thời cài cắm các yếu tố mạo hiểm, căn cứ ngầm bí ẩn để kích thích tò mò và thôi thúc người chơi tiến lên khám phá. 
1. ## <a name="_5tbvtzfeompu"></a>Yếu tố:
- Hành động dồn dập: Nhịp độ game nhanh, đòi hỏi người chơi phải liên tục di chuyển, nhảy né đạn và bắn trả kẻ địch.

- ​Hệ thống kho vũ khí phong phú: Cho phép đổi từ súng lục mặc định sang các loại súng hạng nặng ngẫu nhiên nhặt trên đường.

- ​Cơ chế tương tác: Linh hoạt ngắm bắn ở nhiều góc độ (đứng, ngồi, nhảy bắn).

- ​Cơ chế phụ trợ - Giải cứu con tin: Tăng tính tương tác khi người chơi giải cứu con tin để nhận vật phẩm tiếp tế và tích điểm thưởng.

- ​Trận chiến Boss đa giai đoạn: Các màn chơi kết thúc bằng những trận đánh trùm với cơ chế tấn công thay đổi theo từng phase.
1. ## <a name="_ccf7tfnxeea"></a>Nội dung:
   Trong bối cảnh thế giới đứng trước nguy cơ bị đe dọa bởi các tổ chức quân sự cực đoan, Black Viper đã chiếm đóng đảo Raven và bắt giữ nhiều nhà khoa học nhằm hoàn thiện PROJECT TITAN – một siêu vũ khí có sức công phá khủng khiếp. Với tham vọng thống trị thế giới bằng sức mạnh quân sự, tổ chức này đang chuẩn bị kích hoạt vũ khí và triển khai kế hoạch tấn công các mục tiêu chiến lược.

   Người chơi vào vai Marco, một chiến binh đặc nhiệm tinh nhuệ được giao nhiệm vụ đột nhập vào căn cứ Raven, giải cứu các nhà khoa học và phá hủy PROJECT TITAN  trước khi thảm họa xảy ra. Không có lực lượng hỗ trợ trực tiếp, Marco phải đơn độc vượt qua, đối đầu với những cỗ máy chiến đấu hiện đại và lực lượng tinh nhuệ của địch.

   Bằng lòng dũng cảm, kỹ năng chiến đấu và ý chí kiên định, Marco phá hủy lõi năng lượng của TITAN, giải cứu các con tin và chấm dứt âm mưu của Black Viper. Tuy nhiên, những dữ liệu bí mật còn sót lại cho thấy một dự án nguy hiểm hơn có thể đang được chuẩn bị, mở ra khả năng cho những cuộc chiến tiếp theo.


1. ## <a name="_boxxljbh2ype"></a>Nhóm người chơi game được nhắm đến:
   Người chơi độ tuổi 12+ trở lên, những người từng gắn liền tuổi thơ với các dòng game huyền thoại như Metal Slug (Rambo Lùn), Contra, Mega Man và những người yêu thích các game hành động tốc độ cao, dồn dập, đòi hỏi phản xạ tay/mắt nhanh nhạy (né đạn, nhảy, đổi súng liên tục).
1. ## <a name="_lr3gsz69m4jd"></a>Game Flow:

- Khởi đầu & Nhập cuộc:** Người chơi xuất hiện ở đầu màn hình, di chuyển từ trái sang phải, làm quen với địa hình, nhảy qua các hố vực và phá hủy các vật cản (thùng gỗ, bao cát) trên đường.

- Giao tranh tự do & Thu thập:** Người chơi chạm trán các nhóm lính gác nhỏ lẻ. Vừa xả súng tiêu diệt địch, người chơi vừa né tránh đạn và tìm kiếm các hòm viện trợ hoặc từ con tin để phục hồi máu hoặc đổi sang các loại súng mạnh hơn.

- Sự kiện Khóa màn hình (Event Lock):** Tại các điểm mốc quan trọng, ống kính Camera sẽ dừng cuộn và khóa cứng màn hình không cho người chơi tiến tiếp. Kẻ địch và xe bọc thép sẽ tràn ra liên tục từ hai bên. Người chơi bắt buộc phải sinh tồn và tiêu diệt sạch toàn bộ các đợt quái (Waves) này thì rào chắn mới mở ra để tiếp tục hành trình.

- Trận chiến với Boss cuối màn:** Cuối màn chơi, người chơi bước vào vùng pháo đài của Boss. Camera khóa chặt khu vực thi đấu. Màn giao tranh diễn ra qua 2 giai đoạn:
+ Giai đoạn 1: Boss sử dụng các đòn đánh cơ bản (bắn pháo, xả súng máy), người chơi tập trung quan sát quy luật để né đòn và tỉa máu.
+ Giai đoạn 2 (Báo động/Nổi giận): Khi máu Boss xuống dưới 50%, Boss đổi trạng thái, tăng tốc độ tấn công và tung ra các kỹ năng nguy hiểm hơn (gọi lính trợ giúp, xả laser diện rộng).

- Trong quá trình chơi: Người chơi có thể bấm Tạm dừng (Pause Menu) bất kỳ lúc nào để tiếp tục chơi, chỉnh cài đặt hoặc thoát ra Menu chính.

- Khi kết thúc màn chơi:
+ **Thất bại (Game Over):** Nếu mạng nhân vật về 0, màn hình Game Over xuất hiện cho phép người chơi chọn *Thử lại (Retry)* hoặc *Thoát* về Menu.
+ **Chiến thắng (Level Clear):** Nếu hạ gục được Boss, màn hình mừng chiến thắng xuất hiện và chuyển người chơi sang màn tiếp theo (hoặc màn hình **Mission Completed** nếu là màn cuối).

1. ## <a name="_q8a2c8llbl4h"></a>Look & Feel:
   Định hướng đề xuất: hình ảnh 2D side-scrolling với bối cảnh căn cứ quân sự, đảo hoang, kho vũ khí và công nghệ quân sự. Màu sắc và hiệu ứng nền tạo cảm giác chiến đấu hiện đại, rõ ràng khi có đạn, vụ nổ và vật phẩm.
1. ## <a name="_ih5x3ap3v5i2"></a>Các khía cạnh tác động vào người chơi
Phản xạ: né đạn và xử lý kẻ địch trong thời gian ngắn.

Nhịp độ: các đợt địch liên tục tạo áp lực hành động.

Lựa chọn: quyết định sử dụng vũ khí nào tùy tình huống.

Khám phá: tìm con tin và vật phẩm tiếp tế trong màn.

Thử thách: Boss thay đổi cách tấn công theo từng giai đoạn.
1. ## <a name="_m562lqb418um"></a>Mục tiêu trải nghiệm
Tạo trải nghiệm hành động nhanh, dễ hiểu về cơ chế cơ bản nhưng vẫn có thử thách về phản xạ, di chuyển, ngắm bắn và lựa chọn vũ khí. Người chơi cần có cảm giác mình đang thực sự đột nhập vào một căn cứ quân sự và từng bước phá vỡ kế hoạch của kẻ địch.
1. ## <a name="_huqyx2i6ryg4"></a>Các trải nghiệm được lồng vào màn chơi
Giải cứu con tin nhận phần thưởng

Vũ khí mới giúp thay đổi cách xử lý tình huống.

Boss nhiều phase tạo cao trào ở cuối màn.

Khám phá & Nhập cuộc (Low Tension)**:** Đoạn đầu màn chơi có mật độ quái thưa. Người chơi chủ yếu di chuyển, nhảy qua hố vực. Trải nghiệm này tạo sự thư giãn và chuẩn bị tâm lý.

Căng thẳng tăng dần (Building Tension)**:** Xuất hiện các loại lính kết hợp (lính cận chiến lao vào + lính ném lựu đạn từ xa). Người chơi phải liên tục đổi hướng ngắm và di chuyển linh hoạt.

Đỉnh điểm Kịch tính**:**

- Sự kiện Khóa màn hình (Event Lock): Màn hình dừng cuộn, kẻ địch tràn ra liên tục từ hai bên. Người chơi bị ép vào không gian hẹp, đòi hỏi phản xạ nhanh và xả súng tối đa.
- Đấu Boss: Camera khóa chặt, nhạc nền chuyển sang điệp khúc dồn dập, Boss xả chiêu diện rộng đòi hỏi người chơi thuộc quy luật đòn đánh để né tránh.


# <a name="_vk8eaypdffjo"></a>Phần 3: Gameplay and Mechanics
1. ## <a name="_snd8sujptnn2"></a>Gameplay
1. ### <a name="_etuyjftyhkvw"></a>Core game loop:
   Di chuyển → Quan sát → Bắn → Né tránh → Tiêu diệt kẻ địch → Thu thập vật phẩm → Vượt qua màn → Đánh Boss

   Người chơi **chủ động quyết định thời điểm bắn bằng nút**, không tự động bắn
1. ### <a name="_z2kbyc3i753b"></a>Điều khiển và di chuyển
   Người chơi có thể:

- Di chuyển trái, phải
- Nhảy
- Ngồi
- Di chuyển khi đang bắn
- Thay đổi hướng bắn

Điểm đặc trưng là **người chơi chủ động điều khiển việc bắn**, game không tự động bắn 
1. ### <a name="_4ka5c7aqbymc"></a>Chiến đấu
`	`Người chơi có thể:

- Bắn kẻ địch
- Thay đổi vũ khí
- Sử dụng lựu đạn
- Né đạn
- Tận dụng địa hình
- Phá hủy một số vật thể trong môi trường
1. ### <a name="_p11lsapm2ggs"></a>Thu thập
   Trong màn chơi xuất hiện:

- Vũ khí
- Đạn
- HP
- Lựu đạn

Người chơi có thể nhận thêm vật phẩm thông qua việc phá hủy vật thể hoặc giải cứu con tin 
1. ## <a name="_afmw798lpevj"></a>Mechanics:
### <a name="_6sudn5wsdufi"></a>**Cơ chế di chuyển**
Người chơi điều khiển nhân vật Marco di chuyển sang trái hoặc phải trong môi trường 2D Side-Scrolling. Nhân vật có thể chạy, nhảy và ngồi xuống để vượt qua địa hình hoặc tránh các đòn tấn công của kẻ địch. Khi di chuyển, nhân vật chịu tác động của trọng lực và va chạm với mặt đất, tường, vật cản và các đối tượng trong môi trường.

Cơ chế di chuyển được thiết kế đơn giản để người chơi có thể nhanh chóng làm quen, đồng thời tạo điều kiện cho việc kết hợp di chuyển với chiến đấu.
### <a name="_r7hdd6y6760t"></a>**Cơ chế bắn**
**Marco** sử dụng vũ khí để tấn công kẻ địch. Người chơi phải chủ động nhấn nút bắn, nhân vật không tự động khai hỏa. Khi người chơi thực hiện thao tác bắn, viên đạn được tạo ra theo hướng mà nhân vật đang ngắm và di chuyển về phía mục tiêu.

Người chơi có thể bắn theo nhiều hướng khác nhau như phía trước, phía trên hoặc phía dưới. Việc vừa di chuyển vừa bắn giúp tạo ra nhịp độ chiến đấu nhanh và yêu cầu người chơi phản ứng liên tục trước các tình huống trên màn chơi.

|Tình huống|Hành vi|
| :- | :- |
|Nhấn nút bán|Bắn ngay 1 phát|
|Giữ nút bắn|Tiếp tục bắn theo nhịp tối đa của vũ khí|
|Nhả nút|Dừng bắn|
|Hết đạn|Tự động nạp đạn|
|Đang nạp đạn|Không thể bắn|
|Nạp xong khi vẫn giữ nút|Tiếp tục bắn|
|Hết đạn dự trữ|Không thể nạp tiếp|

### <a name="_a98n3scn8u2j"></a>**Cơ chế ngắm và hướng bắn**
Hướng bắn phụ thuộc vào hướng di chuyển và thao tác điều khiển của người chơi. Marco có thể bắn sang trái, phải, lên trên hoặc xuống dưới tùy thuộc vào vị trí của kẻ địch.

Khi đang đứng, người chơi có thể bắn theo hướng ngang hoặc hướng lên. Khi đang ngồi, nhân vật có thể bắn thấp để tấn công những mục tiêu ở vị trí thấp hơn. Trong khi nhảy, người chơi vẫn có thể điều chỉnh hướng bắn để tấn công kẻ địch ở các vị trí khác nhau.
### <a name="_1d3vst645ocp"></a>**Cơ chế vũ khí**
Người chơi bắt đầu màn chơi với một khẩu súng cơ bản có lượng đạn ổn định và có thể sử dụng trong phần lớn tình huống. Trong quá trình chơi, người chơi có thể tìm thấy hoặc nhận được các loại vũ khí mạnh hơn.

Các loại vũ khí dự kiến gồm súng lục, súng trường, shotgun và súng phóng rocket. Mỗi loại có đặc điểm khác nhau về sát thương, tốc độ bắn và lượng đạn.

Việc lựa chọn vũ khí phù hợp giúp người chơi xử lý từng loại kẻ địch hiệu quả hơn. Ví dụ, shotgun phù hợp khi chiến đấu ở khoảng cách gần, trong khi rocket launcher có thể được sử dụng để gây sát thương lớn lên kẻ địch hoặc phương tiện bọc thép.
### <a name="_y32fm2nm43ga"></a>**Cơ chế nhặt và thay đổi vũ khí**
Vũ khí có thể xuất hiện trong các thùng tiếp tế hoặc được nhận sau khi giải cứu con tin. Khi nhặt một vũ khí mới, Marco sẽ chuyển sang sử dụng loại vũ khí đó.

Vũ khí đặc biệt có lượng đạn giới hạn. Khi hết đạn, người chơi có thể tìm thêm đạn trong màn chơi hoặc quay lại sử dụng khẩu súng cơ bản.
### <a name="_b0fj2unywf51"></a>**Cơ chế máu và sát thương**
Nhân vật Marco: 

- Có 3 mạng
- HP tối đa mỗi mạng: 100
- Thời gian bất tử sau khi hồi sinh tại checkpoint: 2 giây
- Thời gian bất tử sau khi nhận sát thương: 1 giây
- HP hồi sinh tại checkpoint: 100
- HP hồi lại sau khi hồi máu: 30

Sát thương của vũ khí người chơi:

|Vũ khí|Sát thương|Nhịp bắn tối đa|Đạn tối đa (viên)|Thời gian nạp đạn (giây)|Số đạn/ 1 băng đạn|
| :- | :- | :- | :- | :- | :- |
|Súng lục|20/viên|2 viên/giây|Vô hạn|0|Không áp dụng|
|Shotgun|50/phát|1 phát/giây|30|3|8 viên|
|Rocket|100/vụ nổ|1 quả / 2 giây|5|2|1 quả|
|Súng trường|15/viên|5 viên/giây|120|1,5|30 viên|
|Lựu đạn|30/quả||5 quả|0|Mỗi lần ném 1 quả|

HP và sát thương của kẻ địch:

|Kẻ địch|HP|Sát thương|
| :- | :- | :- |
|Lính thường|30|20|
|Lính xe|70|30|

HP và sát thương của boss:

|Boss|HP|Sát thương|
| :- | :- | :- |
|Boss 1 - Viper|600|Đạn súng máy: 20, đạn pháo: 35|
|Boss 2 - Hunter|900|Cận chiến: 25, tên lửa: 35, laser: 45|
|Boss cuối - PROJECT TITAN|1500|Tên lửa: 35, laser: 45|

### <a name="_fratcwwm6lc"></a>**Cơ chế bất tử tạm thời**
Sau khi bị trúng đòn, Marco sẽ có một khoảng thời gian ngắn không thể tiếp tục nhận sát thương. Trong khoảng thời gian này, nhân vật có hiệu ứng nhấp nháy để báo hiệu trạng thái bất tử tạm thời và không thể bắn.

Cơ chế này giúp người chơi có thời gian phản ứng và tránh việc nhân vật bị mất toàn bộ máu liên tục khi đứng gần nhiều kẻ địch.
### <a name="_8nemlzlsn1le"></a>**Cơ chế né tránh**
Người chơi có thể sử dụng khả năng di chuyển, nhảy và ngồi xuống để tránh các đòn tấn công. Đạn của kẻ địch thường bay theo quỹ đạo xác định, vì vậy người chơi cần quan sát hướng tấn công và nhanh chóng thay đổi vị trí.

Cơ chế né tránh kết hợp với bắn giúp tạo nên lối chơi đặc trưng của thể loại Run-and-Gun, trong đó người chơi phải vừa tấn công vừa liên tục di chuyển để duy trì lợi thế.
### <a name="_yz6tczm849d4"></a>**Cơ chế kẻ địch**
Kẻ địch xuất hiện ở nhiều vị trí khác nhau trong màn chơi và có cách tấn công khác nhau. Một số loại kẻ địch có thể di chuyển và bắn từ xa, trong khi một số khác sẽ tiếp cận và tấn công trực tiếp. Sự kết hợp giữa nhiều loại kẻ địch buộc người chơi phải thay đổi cách di chuyển và lựa chọn mục tiêu trong từng tình huống.
### <a name="_mt0049777ro7"></a>**Cơ chế AI của kẻ địch**
Kẻ địch hoạt động dựa trên trạng thái AI. Khi chưa phát hiện người chơi, chúng có thể đứng canh hoặc tuần tra khu vực. Khi Marco tiến vào phạm vi phát hiện, kẻ địch chuyển sang trạng thái tấn công. Khi bị trúng đòn, chúng có phản ứng nhận sát thương và khi máu giảm xuống 0 sẽ bị tiêu diệt.
### <a name="_pui0oxf0tbcp"></a>**Cơ chế đạn và vật thể tấn công**
Đạn của người chơi và kẻ địch là những vật thể di chuyển theo một hướng nhất định. Khi đạn va chạm với nhân vật, kẻ địch hoặc vật thể có thể phá hủy, nó sẽ tạo ra hiệu ứng tương ứng và gây sát thương nếu mục tiêu có thể nhận sát thương.

Các loại đạn khác nhau có thể có tốc độ, sát thương và phạm vi ảnh hưởng khác nhau.
### <a name="_hpz8whwteivo"></a>**Cơ chế lựu đạn và vụ nổ**
Người chơi có thể sử dụng lựu đạn để tấn công nhiều kẻ địch cùng lúc. Sau khi được ném, lựu đạn sẽ phát nổ sau một khoảng thời gian. Vụ nổ gây sát thương trong một phạm vi nhất định và có thể tác động đến nhiều kẻ địch hoặc các vật thể xung quanh. 

Lựu đạn sẽ nổ sau khi ném 3 giây, bán kính 3m, gây sát thương diện rộng cho các mục tiêu trong phạm vi bán kính.
### <a name="_96rfzo6n6rkx"></a>**Cơ chế vật thể có thể phá hủy**
Trong màn chơi xuất hiện vật thể như thùng gỗ. Người chơi có thể phá hủy bằng cách bắn hoặc sử dụng vũ khí phù hợp.

Vật thể có thể chứa vật phẩm hoặc tạo ra hiệu ứng khi bị phá hủy. Điều này khuyến khích người chơi quan sát và tương tác với môi trường thay vì chỉ di chuyển theo một đường thẳng.
### <a name="_3xv8yu7v9abm"></a>**Cơ chế giải cứu con tin**
Con tin xuất hiện tại một số khu vực trong căn cứ quân sự và được bảo vệ bởi kẻ địch. Người chơi cần tiêu diệt hoặc vượt qua khu vực bảo vệ để tiếp cận con tin. Sau khi giải cứu, người chơi có thể nhận được phần thưởng như máu, đạn, lựu đạn, vũ khí. 
### <a name="_aq4toxwtkiin"></a>**Cơ chế khóa khu vực và đợt kẻ địch**
Tại một số vị trí quan trọng, màn chơi sẽ kích hoạt một sự kiện khóa khu vực. Khi đó, camera dừng di chuyển và lối đi có thể bị chặn lại.

Một hoặc nhiều đợt kẻ địch sẽ xuất hiện từ các vị trí khác nhau và tấn công người chơi. Người chơi phải tiêu diệt toàn bộ kẻ địch trước khi khu vực được mở lại. Sau khi hoàn thành, chướng ngại vật được gỡ bỏ và người chơi có thể tiếp tục tiến về phía trước.

Cơ chế này tạo ra những khoảng cao trào trong màn chơi và kiểm tra khả năng chiến đấu của người chơi trước khi chuyển sang khu vực tiếp theo.
### <a name="_5impvulr79u2"></a>**Cơ chế boss**
Cuối mỗi màn chơi có thể xuất hiện một Boss với lượng máu lớn và cách tấn công phức tạp hơn kẻ địch thông thường. Người chơi phải quan sát hành vi của Boss để tìm thời điểm thích hợp tấn công và tránh các đòn đánh.

Trận đấu có thể được chia thành nhiều giai đoạn. Khi lượng máu của Boss giảm xuống một mức nhất định, Boss sẽ thay đổi cách tấn công, tăng tốc độ hoặc gọi thêm kẻ địch hỗ trợ.
### <a name="_tjqedmjg8gp8"></a>**Cơ chế vật phẩm**
Trong quá trình chơi, người chơi có thể thu thập nhiều loại vật phẩm khác nhau. Vật phẩm có thể xuất hiện từ vật thể có thể phá hủy hoặc sau khi giải cứu con tin.

Các vật phẩm chính gồm hồi máu, đạn, lựu đạn và vũ khí. Việc thu thập vật phẩm giúp người chơi duy trì khả năng chiến đấu và chuẩn bị cho những khu vực khó hơn.

|Vật phẩm|Tác dụng|
| :- | :- |
|HP|Hồi 30 HP cho Marco|
|Đạn shotgun|Cho Marco 20 viên shotgun|
|Đạn súng trường|Cho Marco 30 viên|
|Rocket|Marco nhận được vũ khí mới - Rocket và có sẵn 1 viên đạn|
|Đạn Rocket|Cho Marco 2 viên|
|Lựu đạn|Cho Marco 2 quả|

### <a name="_d3ykzzxk1yn4"></a>**Cơ chế Checkpoint**
Một số vị trí trong màn chơi được thiết lập làm checkpoint. Khi người chơi đi qua hoặc kích hoạt checkpoint, vị trí đó sẽ trở thành điểm hồi sinh mới.

Nếu Marco bị tiêu diệt, người chơi có thể tiếp tục màn chơi từ checkpoint gần nhất thay vì phải chơi lại toàn bộ màn.
### <a name="_jed081z5xnuh"></a>**Cơ chế camera** 
Camera di chuyển theo nhân vật khi Marco tiến về phía trước trong màn chơi. Camera có giới hạn để tránh việc người chơi nhìn thấy các khu vực chưa được thiết kế hoặc chưa được kích hoạt.

Trong những sự kiện quan trọng như Event Lock hoặc Boss Battle, camera có thể tạm dừng di chuyển để tập trung vào khu vực chiến đấu.

Các vụ nổ lớn hoặc đòn tấn công mạnh của Boss có thể tạo hiệu ứng rung camera nhằm tăng cảm giác tác động trong chiến đấu.
### <a name="_dgry1q24j2vy"></a>**Cơ chế Pause**
Người chơi có thể tạm dừng trò chơi bất cứ lúc nào trong quá trình chơi. Khi Pause được kích hoạt, hoạt động của nhân vật, kẻ địch, đạn và các sự kiện trong màn chơi sẽ tạm dừng.

Menu Pause cho phép người chơi tiếp tục chơi, khởi động lại màn chơi, thay đổi một số thiết lập hoặc quay về màn hình chính.
### <a name="_rmxeghtjv1uv"></a>**Cơ chế chiến thắng**
Khi người chơi đánh bại Boss và hoàn thành các mục tiêu chính của màn chơi, màn chơi sẽ kết thúc. Sau đó, người chơi có thể chuyển sang màn chơi tiếp theo hoặc kết thúc trò chơi nếu đã hoàn thành toàn bộ.
# <a name="_sf6ewo6nwsc"></a>**Phần 4: Cốt truyện, bối cảnh và nhân vật trong game**
1. ## <a name="_xblpdjh44ha5"></a>Story
Một tổ chức quân sự phản động mang tên Black Viper bất ngờ chiếm đóng đảo Raven, một hòn đảo biệt lập từng được sử dụng làm khu nghiên cứu quân sự bí mật. Sau khi kiểm soát toàn bộ hòn đảo, chúng bắt giữ nhiều nhà khoa học và ép buộc họ chế tạo PROJECT TITAN – một siêu vũ khí có khả năng phá hủy các căn cứ quân sự quy mô lớn.

Black Viper dự định hoàn thành quá trình thử nghiệm và kích hoạt TITAN trong vòng vài giờ tới. Nếu kế hoạch thành công, tổ chức này sẽ sở hữu sức mạnh đủ để đe dọa nhiều quốc gia và gây ra một cuộc khủng hoảng toàn cầu.

Do hòn đảo được bảo vệ bởi hệ thống phòng thủ dày đặc, việc triển khai một lực lượng quân sự quy mô lớn là quá nguy hiểm. Bộ chỉ huy quyết định cử Marco, một chiến binh đặc nhiệm tinh nhuệ, thực hiện nhiệm vụ đột nhập đơn độc vào đảo Raven.

Nhiệm vụ của Marco gồm ba mục tiêu chính:

- Xâm nhập hòn đảo và phá vỡ tuyến phòng thủ bên ngoài.
- Vượt qua khu rừng rậm để tiếp cận căn cứ quân sự trung tâm.
- Tiêu diệt lực lượng địch, giải cứu các nhà khoa học và phá hủy PROJECT TITAN.

Marco chỉ có một mình, lượng đạn dược có hạn và không có lực lượng hỗ trợ trực tiếp. Anh phải chiến đấu xuyên suốt hòn đảo, đối đầu với hàng loạt binh lính, phương tiện chiến đấu và những cỗ máy quân sự nguy hiểm để hoàn thành nhiệm vụ.

**Màn 1: Đột nhập đảo RAVEN**

Trong đêm tối, Marco bí mật tiếp cận đảo Raven bằng một chiếc xuồng cao su. Sau khi cập bờ, anh nhận được thông tin cuối cùng từ bộ chỉ huy rằng toàn bộ hệ thống liên lạc của hòn đảo đã bị Black Viper kiểm soát.

Khi Marco vừa tiến vào đất liền, một toán lính gác phát hiện ra sự hiện diện của anh và lập tức tấn công anh. 

Không còn lựa chọn nào khác, Marco buộc phải chiến đấu xuyên qua bãi biển, các chiến hào và những công sự phòng thủ của địch. Trên đường đi, anh phá hủy các trạm liên lạc, tiêu diệt những đơn vị canh gác và thu thập thông tin về vị trí căn cứ chính.

Khi tiến đến cổng kiểm soát cuối cùng, Marco bị một xe bọc thép hạng nặng chặn đường. Đây là tuyến phòng thủ quan trọng nhất trước khi tiến vào khu vực rừng rậm.

Sau trận chiến quyết liệt, Marco phá hủy chiếc xe bọc thép và mở được lối đi vào bên trong hòn đảo.

**Màn 2: Vượt qua rừng rậm tử thần**

Sau khi phá vỡ tuyến phòng thủ bên ngoài của đảo Raven, Marco tiếp tục tiến sâu vào hòn đảo và bước vào một khu rừng nhiệt đới rộng lớn. Đây là vùng đệm giữa bãi biển và căn cứ quân sự trung tâm, được Black Viper tận dụng để bố trí các chốt gác, đội tuần tra và hệ thống phòng thủ tự động nhằm ngăn chặn kẻ địch xâm nhập.

Ngay khi Marco tiến vào rừng, anh bị một nhóm lính Black Viper phát hiện. Tiếng súng vang lên giữa những tán cây, buộc anh phải chiến đấu để mở đường tiến về phía trước. Càng đi sâu vào rừng, lực lượng địch càng trở nên đông đảo và được trang bị tốt hơn. Những toán lính và các cỗ máy liên tục xuất hiện, tạo thành những đợt tấn công dồn dập.

Không chỉ phải đối đầu với binh lính, Marco còn chạm trán những cỗ máy chiến đấu được Black Viper triển khai để bảo vệ tuyến đường dẫn vào căn cứ. Các robot tuần tra và phương tiện chiến đấu cơ giới liên tục khai hỏa và tìm cách chặn đứng bước tiến của anh. Marco phải tận dụng kỹ năng chiến đấu, khả năng di chuyển linh hoạt và các loại vũ khí thu thập được trên đường để vượt qua từng đợt phòng thủ.

Sau khi đánh bại các đơn vị địch, Marco tiến đến một khoảng rừng trống gần lối vào căn cứ. Tại đây, Black Viper đã bố trí một cỗ máy chiến đấu hạng nhẹ chuyên dùng để tuần tra và tiêu diệt những kẻ xâm nhập. Khi phát hiện Marco, cỗ máy lập tức kích hoạt hệ thống vũ khí và lao vào trận chiến.

Marco buộc phải đối đầu với cỗ máy trong một trận chiến quyết liệt. Sau nhiều đợt tấn công, anh tìm ra điểm yếu của đối thủ và phá hủy nó, mở đường tiến về phía căn cứ quân sự.

**Màn 3: Tổng tấn công**

Sau khi vượt qua khu rừng rậm và đánh bại robot chiến đấu Hunter, Marco cuối cùng cũng tiếp cận được căn cứ quân sự trung tâm của Black Viper. Trước mắt anh là một pháo đài được bảo vệ nghiêm ngặt với hàng rào thép gai, tháp canh, ụ súng máy và các phương tiện chiến đấu hạng nặng. Đây cũng là nơi tổ chức Black Viper đặt trung tâm chỉ huy và phòng thí nghiệm phát triển PROJECT TITAN.

Marco bí mật xâm nhập qua cổng phụ của căn cứ. Tuy nhiên, hệ thống cảnh báo nhanh chóng phát hiện sự hiện diện của anh, khiến toàn bộ căn cứ chuyển sang trạng thái báo động. Đại tá Viktor Kane, thủ lĩnh Black Viper, lập tức ra lệnh cho binh lính ngăn chặn kẻ xâm nhập bằng mọi giá.

Marco buộc phải chiến đấu xuyên qua các tuyến phòng thủ bên trong căn cứ. Trên đường đi, anh đối mặt với những toán lính tinh nhuệ được trang bị vũ khí hạng nặng và những cỗ máy chiến đấu được triển khai để bảo vệ khu chỉ huy. Các đợt tấn công ngày càng dữ dội khi Marco tiến gần đến trung tâm căn cứ.

Không còn thời gian để chần chừ, Marco tiến thẳng đến trung tâm chỉ huy. Tại đây, Đại tá Viktor Kane xuất hiện và đối mặt trực tiếp với anh. Kane tin rằng PROJECT TITAN sẽ giúp Black Viper thống trị thế giới, vì vậy hắn quyết định tự mình tiêu diệt Marco để bảo vệ thành quả của tổ chức.

**Trận chiến cuối cùng**

Kane kích hoạt hệ thống vũ khí TITAN và điều khiển cỗ máy chiến đấu khổng lồ để tấn công Marco. Trận chiến diễn ra giữa trung tâm căn cứ đang rung chuyển bởi những vụ nổ liên tiếp.

Marco phải né tránh các loạt đạn, tên lửa và những đòn tấn công diện rộng của TITAN, đồng thời tìm cách phá hủy các bộ phận phòng vệ của cỗ máy. Khi lớp giáp bảo vệ bị phá hủy, lõi năng lượng của siêu vũ khí dần lộ ra. Đây là cơ hội cuối cùng để Marco kết thúc trận chiến.

Bằng những phát bắn chính xác, Marco phá hủy lõi năng lượng, khiến TITAN ngừng hoạt động. Cỗ máy chiến đấu phát nổ, hệ thống điện trong căn cứ tắt hoàn toàn và toàn bộ cơ sở bắt đầu sụp đổ. Đại tá Kane thất bại, còn âm mưu sử dụng siêu vũ khí của Black Viper chính thức bị chặn đứng.

**Kết thúc**

Marco nhanh chóng đưa những người được giải cứu rời khỏi trung tâm căn cứ trước khi vụ nổ lan rộng. Khi anh thoát ra ngoài, một loạt tiếng nổ lớn vang lên phía sau, phá hủy phòng thí nghiệm và những cơ sở quân sự quan trọng của Black Viper.

Bình minh dần xuất hiện trên đảo Raven. Lực lượng cứu hộ tiến vào khu vực và đưa những người sống sót đến nơi an toàn. Marco hoàn thành nhiệm vụ, giải cứu các nhà khoa học và ngăn chặn một thảm họa có thể đe dọa toàn thế giới.

Khi chiếc trực thăng rời khỏi hòn đảo, Marco nhìn về phía căn cứ đang chìm trong khói lửa và nói:

*“Mục tiêu đã bị vô hiệu hóa. Đưa mọi người về nhà thôi.”*
1. ## <a name="_hxyipk3b06l1"></a>Characters

|![](Aspose.Words.c9231e24-7b4e-4f38-9d74-4652ab6b8d21.004.png)|<p></p><p>Nhân vật chính - Marco</p><p></p><p>Một chiến binh đặc nhiệm nổi tiếng với khả năng chiến đấu độc lập và ý chí không bao giờ khuất phục.</p>|
| :-: | :-: |
|![](Aspose.Words.c9231e24-7b4e-4f38-9d74-4652ab6b8d21.005.png)|<p></p><p></p><p></p><p>Lính bộ của Black Viper</p>|
|![](Aspose.Words.c9231e24-7b4e-4f38-9d74-4652ab6b8d21.006.png)|<p></p><p></p><p>Lính xe của Black Viper</p>|
|![](Aspose.Words.c9231e24-7b4e-4f38-9d74-4652ab6b8d21.007.png)|<p></p><p></p><p>Con tin</p>|
|![](Aspose.Words.c9231e24-7b4e-4f38-9d74-4652ab6b8d21.008.png)|<p></p><p></p><p></p><p>Boss màn 1</p>|
|![](Aspose.Words.c9231e24-7b4e-4f38-9d74-4652ab6b8d21.009.png)|<p></p><p></p><p></p><p>Boss màn 2</p>|
|![](Aspose.Words.c9231e24-7b4e-4f38-9d74-4652ab6b8d21.010.png)|<p></p><p></p><p>Boss màn cuối</p><p>PROJECT TITAN</p>|

# <a name="_sdsvyytfrs34"></a>**Phần 5: Các màn chơi**
**5.1. Màn 1: Đột nhập căn cứ RAVEN**

Marco đổ bộ lên bãi biển, chiến đấu với lính gác và vượt qua các công sự phòng thủ bên ngoài của Black Viper. Cuối màn, Marco đối đầu với xe bọc thép Viper-01 để mở đường tiến vào khu rừng.

**5.2. Màn 2: Vượt qua rừng rậm tử thần**

Marco tiến sâu vào khu rừng nhiệt đới, tiêu diệt các toán lính địch và những cỗ máy chiến đấu được bố trí dọc đường. Cuối màn, anh đối đầu với robot Hunter, đánh bại nó và tiếp cận căn cứ quân sự trung tâm.

**5.3. Màn 3: Tổng tấn công**

Marco đột nhập vào căn cứ quân sự, vượt qua các tuyến phòng thủ, giải cứu các nhà khoa học và tiến đến trung tâm chỉ huy. Tại đây, anh đối đầu với Đại tá Viktor Kane và phá hủy PROJECT TITAN, chấm dứt âm mưu của Black Viper.

# <a name="_jusj4b5gi06"></a>**Phần 6: Giao diện**
# <a name="_jtxpn6ajc6j6"></a>**Phần 7: Game matrix**

|Thành phần|Màn 1|Màn 2|Màn 3|
| :- | :- | :- | :- |
|Bối cảnh|Bãi biển, chiến hào, công sự|Rừng nhiệt đới tử thần chứa đầy nguy hiểm|Căn cứ trung tâm của tổ chức quân sự |
|Kẻ địch|Lính gác, lính xe|Lính gác, robot|Lính gác, robot|
|Event Lock|Có |Có |Có|
|Vật phẩm, thùng phá hủy|Có |Có |Có|
|Giải cứu con tin|Có |Có|Có|
|Boss|Viper-01|Hunter|PROJECT TITAN|
|Mục tiêu|Tiêu diệt địch và mở đường tiến vào đảo|Tiếp tục tiêu diệt dịch và tiến sâu hơn vào bên trong căn cứ trung tâm|Đánh bại PROJECT TITAN và phá hủy lõi|


