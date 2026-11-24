#if UNITY_EDITOR
using MetalRebels.Core.Managers;
using MetalRebels.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MainMenuSceneBuilder
{
    [MenuItem("MetalRebels/Tạo Scene MainMenu Hoàn Chỉnh")]
    public static void BuildMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 1. Camera nền tối
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.055f, 0.078f, 0.12f, 1f);
            mainCam.orthographic = true;
        }

        // 2. GameManager
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // 3. EventSystem (Chuẩn Input System mới)
        GameObject esObj = new GameObject("EventSystem");
        esObj.AddComponent<EventSystem>();
        esObj.AddComponent<InputSystemUIInputModule>();

        // 4. Hiệu ứng tàn lửa bay lơ lửng
        CreateEmberParticles();

        // 5. Canvas chính
        GameObject canvasObj = new GameObject("MainMenu_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = mainCam;
        canvas.planeDistance = 10f;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        MainMenuController menuController = canvasObj.AddComponent<MainMenuController>();
        TMP_FontAsset fontAsset = FindBestFontAsset();

        // 6. Nền tối + Lưới Grid công nghệ mờ
        GameObject bg = CreatePanel(canvasObj.transform, "Background", Vector2.zero, Vector2.zero, new Color(0.055f, 0.078f, 0.12f, 0.90f), true, false);
        Image bgImg = bg.GetComponent<Image>();
        bgImg.sprite = CreateGridBackgroundSprite();
        bgImg.type = Image.Type.Tiled;

        // 7. Header Group
        GameObject headerGroup = CreateEmptyUI(canvasObj.transform, "Header_Group", new Vector2(0f, 345f), new Vector2(1300f, 350f));

        GameObject topBadgeBg = CreatePanel(headerGroup.transform, "TopBadgeBg", new Vector2(0f, 150f), new Vector2(640f, 36f), new Color(0.11f, 0.14f, 0.20f, 1f));
        CreateTMP(topBadgeBg.transform, "TopBadgeText", "2D SIDE-SCROLLING RUN-AND-GUN SIMULATOR", 17, FontStyles.Bold, new Color(0.92f, 0.52f, 0.25f, 1f), TextAlignmentOptions.Center, fontAsset);

        GameObject titleObj = CreateEmptyUI(headerGroup.transform, "TitleText", new Vector2(0f, 32f), new Vector2(1200f, 210f));
        var titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "METAL\nREBELS";
        titleTMP.fontSize = 112;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.lineSpacing = -28f;
        titleTMP.characterSpacing = 12f;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;
        titleTMP.raycastTarget = false;
        titleTMP.enableVertexGradient = true;
        titleTMP.colorGradient = new VertexGradient(
            Color.white, Color.white,
            new Color(0.55f, 0.62f, 0.72f, 1f), new Color(0.55f, 0.62f, 0.72f, 1f)
        );
        if (fontAsset != null) titleTMP.font = fontAsset;

        CreatePanel(headerGroup.transform, "GlowLine", new Vector2(0f, -86f), new Vector2(1180f, 3f), new Color(0.95f, 0.35f, 0.08f, 0.9f));

        GameObject creditBanner = CreatePanel(headerGroup.transform, "CreditBanner", new Vector2(0f, -130f), new Vector2(1180f, 50f), new Color(0.08f, 0.12f, 0.18f, 0.96f));
        CreateIcon(creditBanner.transform, "CreditIcon", new Vector2(-535f, 0f), new Vector2(24f, 24f), CreateIconSprite("layers"), new Color(1f, 0.55f, 0.15f, 1f));
        CreateTMP(
            creditBanner.transform,
            "CreditText",
            "<color=#FF9E2C>APEX GROUP (NHÓM 8)</color>  •  <color=#CBD5E1>THIẾT KẾ VÀ PHÁT TRIỂN TRÒ CHƠI</color>  •  <color=#38BDF8>ĐẠI HỌC THỦY LỢI</color>",
            21, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, fontAsset
        );

        // 8. Cụm 5 nút chính
        GameObject btnGroup = CreateEmptyUI(canvasObj.transform, "MainButtons_Group", new Vector2(0f, -125f), new Vector2(780f, 470f));

        Button btnNew = CreateStyledMenuButton(btnGroup.transform, "Btn_NewCampaign", new Vector2(0f, 184f),
            "CHIẾN DỊCH MỚI", "START MÀN 1",
            new Color(0.91f, 0.24f, 0.08f, 1f), new Color(1f, 0.34f, 0.12f, 1f),
            Color.white, new Color(0.98f, 0.80f, 0.20f, 1f),
            CreateIconSprite("play"), new Color(1f, 0.85f, 0.20f, 1f), true, fontAsset);

        Button btnMission = CreateStyledMenuButton(btnGroup.transform, "Btn_MissionSelect", new Vector2(0f, 92f),
            "CHỌN MÀN CHƠI", "3 MISSIONS",
            new Color(0.09f, 0.13f, 0.20f, 0.96f), new Color(0.14f, 0.20f, 0.31f, 1f),
            Color.white, new Color(0.65f, 0.72f, 0.82f, 1f),
            CreateIconSprite("map"), new Color(0.96f, 0.62f, 0.18f, 1f), false, fontAsset);

        Button btnSettings = CreateStyledMenuButton(btnGroup.transform, "Btn_Settings", new Vector2(0f, 0f),
            "CÀI ĐẶT & HƯỚNG DẪN", "AUDIO & GUIDE",
            new Color(0.09f, 0.13f, 0.20f, 0.96f), new Color(0.14f, 0.20f, 0.31f, 1f),
            Color.white, new Color(0.22f, 0.74f, 0.97f, 1f),
            CreateIconSprite("sliders"), new Color(0.22f, 0.74f, 0.97f, 1f), false, fontAsset);

        Button btnTeam = CreateStyledMenuButton(btnGroup.transform, "Btn_TeamInfo", new Vector2(0f, -92f),
            "THÔNG TIN NHÓM", "APEX GROUP",
            new Color(0.09f, 0.13f, 0.20f, 0.96f), new Color(0.14f, 0.20f, 0.31f, 1f),
            Color.white, new Color(0.20f, 0.83f, 0.60f, 1f),
            CreateIconSprite("team"), new Color(0.20f, 0.83f, 0.60f, 1f), false, fontAsset);

        Button btnExit = CreateStyledMenuButton(btnGroup.transform, "Btn_Exit", new Vector2(0f, -184f),
            "THOÁT TRÒ CHƠI", "EXIT",
            new Color(0.07f, 0.10f, 0.16f, 0.96f), new Color(0.25f, 0.11f, 0.14f, 1f),
            new Color(0.65f, 0.72f, 0.80f, 1f), new Color(0.45f, 0.52f, 0.62f, 1f),
            CreateIconSprite("power"), new Color(0.55f, 0.62f, 0.72f, 1f), false, fontAsset);

        // 9. Thanh Footer dưới đáy (Đã thêm icon dấu vô cực ∞ sắc nét cho Pistol)
        GameObject footer = CreatePanel(canvasObj.transform, "FooterInfo_Bar", new Vector2(0f, -468f), new Vector2(1180f, 50f), new Color(0.07f, 0.10f, 0.16f, 0.96f));

        CreateFooterItem(footer.transform, new Vector2(-385f, 0f), CreateIconSprite("hero"), new Color(0.95f, 0.45f, 0.20f, 1f),
            "<color=#94A3B8>HERO:</color> <color=#FFFFFF>MARCO (HP: 100/3 LIVES)</color>", fontAsset);

        CreatePistolFooterItem(footer.transform, new Vector2(10f, 0f), fontAsset);

        CreateFooterItem(footer.transform, new Vector2(395f, 0f), CreateIconSprite("target"), new Color(0.94f, 0.27f, 0.27f, 1f),
            "<color=#94A3B8>TARGET:</color> <color=#EF4444>PROJECT TITAN</color>", fontAsset);

        // =========================================================================
        // 10. POPUP 1: CHỌN MÀN CHƠI (MissionSelectPanel)
        // =========================================================================
        GameObject missionPanel = CreatePopupBase(canvasObj.transform, "MissionSelectPanel", "CHỌN NHIỆM VỤ (MISSIONS)", new Vector2(920f, 560f), fontAsset, menuController);
        Button m1 = CreatePopupActionButton(missionPanel.transform, "Btn_Mission1", new Vector2(0f, 80f), "MÀN 1: ĐỘT NHẬP CĂN CỨ RAVEN", fontAsset);
        Button m2 = CreatePopupActionButton(missionPanel.transform, "Btn_Mission2", new Vector2(0f, -10f), "MÀN 2: VƯỢT QUA RỪNG RẬM TỬ THẦN", fontAsset);
        Button m3 = CreatePopupActionButton(missionPanel.transform, "Btn_Mission3", new Vector2(0f, -100f), "MÀN 3: TỔNG TẤN CÔNG", fontAsset);
        UnityEventTools.AddIntPersistentListener(m1.onClick, menuController.OnSelectMission, 0);
        UnityEventTools.AddIntPersistentListener(m2.onClick, menuController.OnSelectMission, 1);
        UnityEventTools.AddIntPersistentListener(m3.onClick, menuController.OnSelectMission, 2);
        missionPanel.SetActive(false);

        // =========================================================================
        // 11. POPUP 2: CÀI ĐẶT & HƯỚNG DẪN (Chia 2 cột hiện đại)
        // =========================================================================
        GameObject settingsPanel = CreatePopupBase(canvasObj.transform, "SettingsPanel", "CÀI ĐẶT & HƯỚNG DẪN", new Vector2(1220f, 680f), fontAsset, menuController);
        Transform settingsWin = settingsPanel.transform.Find("Window");

        // --- CỘT TRÁI: CÀI ĐẶT ÂM THANH ---
        GameObject leftCol = CreatePanel(settingsWin, "Left_AudioColumn", new Vector2(-295f, 10f), new Vector2(540f, 450f), new Color(0.10f, 0.15f, 0.23f, 1f));
        CreatePanel(leftCol.transform, "TopAccent", new Vector2(0f, 223f), new Vector2(540f, 4f), new Color(0.95f, 0.35f, 0.08f, 1f));

        GameObject audioTitleObj = CreateEmptyUI(leftCol.transform, "SectionTitle", new Vector2(0f, 180f), new Vector2(480f, 45f));
        CreateTMP(audioTitleObj.transform, "Text", "CÀI ĐẶT ÂM THANH", 23, FontStyles.Bold, new Color(1f, 0.62f, 0.17f, 1f), TextAlignmentOptions.Left, fontAsset);

        GameObject volLabelObj = CreateEmptyUI(leftCol.transform, "VolumeLabel", new Vector2(-85f, 100f), new Vector2(310f, 40f));
        CreateTMP(volLabelObj.transform, "Text", "Âm lượng tổng (Master Volume)", 19, FontStyles.Bold, new Color(0.85f, 0.90f, 0.96f, 1f), TextAlignmentOptions.Left, fontAsset);

        GameObject volValObj = CreateEmptyUI(leftCol.transform, "VolumePercentText", new Vector2(190f, 100f), new Vector2(100f, 40f));
        var volPercentTMP = CreateTMP(volValObj.transform, "Text", "100%", 21, FontStyles.Bold, new Color(0.22f, 0.74f, 0.97f, 1f), TextAlignmentOptions.Right, fontAsset);

        Slider volumeSlider = CreateStyledSlider(leftCol.transform, "MasterVolumeSlider", new Vector2(0f, 42f), new Vector2(480f, 28f));

        GameObject audioNoteObj = CreateEmptyUI(leftCol.transform, "AudioNote", new Vector2(0f, -85f), new Vector2(480f, 150f));
        CreateTMP(audioNoteObj.transform, "Text",
            "<color=#94A3B8>• Kéo thanh trượt để điều chỉnh âm lượng nhạc nền và hiệu ứng cháy nổ trong trận đánh.\n\n" +
            "• Thiết lập âm lượng sẽ được lưu tự động cho toàn bộ chiến dịch.</color>",
            17, FontStyles.Normal, Color.white, TextAlignmentOptions.TopLeft, fontAsset);

        // --- CỘT PHẢI: HƯỚNG DẪN PHÍM THAO TÁC (Chuẩn phím của dự án) ---
        GameObject rightCol = CreatePanel(settingsWin, "Right_GuideColumn", new Vector2(295f, 10f), new Vector2(540f, 450f), new Color(0.10f, 0.15f, 0.23f, 1f));
        CreatePanel(rightCol.transform, "TopAccent", new Vector2(0f, 223f), new Vector2(540f, 4f), new Color(0.22f, 0.74f, 0.97f, 1f));

        GameObject guideTitleObj = CreateEmptyUI(rightCol.transform, "SectionTitle", new Vector2(0f, 180f), new Vector2(480f, 45f));
        CreateTMP(guideTitleObj.transform, "Text", "HƯỚNG DẪN PHÍM THAO TÁC", 23, FontStyles.Bold, new Color(0.22f, 0.74f, 0.97f, 1f), TextAlignmentOptions.Left, fontAsset);

        CreateKeyRow(rightCol.transform, new Vector2(0f, 126f),  "A / D",       "Di chuyển sang trái / phải", fontAsset);
        CreateKeyRow(rightCol.transform, new Vector2(0f, 80f),   "W",           "Ngắm bắn lên trên", fontAsset);
        CreateKeyRow(rightCol.transform, new Vector2(0f, 34f),   "S",           "Ngắm bắn xuống dưới", fontAsset);
        CreateKeyRow(rightCol.transform, new Vector2(0f, -12f),  "C",           "Ngồi né đạn", fontAsset);
        CreateKeyRow(rightCol.transform, new Vector2(0f, -58f),  "SPACE",       "Nhảy vượt chướng ngại vật", fontAsset);
        CreateKeyRow(rightCol.transform, new Vector2(0f, -104f), "G",           "Ném lựu đạn", fontAsset);
        CreateKeyRow(rightCol.transform, new Vector2(0f, -150f), "1 / 2 / 3 / 4", "Đổi vũ khí chiến đấu", fontAsset);
        CreateKeyRow(rightCol.transform, new Vector2(0f, -196f), "J / R / ESC", "Bắn súng / Nạp đạn / Tạm dừng", fontAsset);

        settingsPanel.SetActive(false);

        // =========================================================================
        // 12. POPUP 3: THÔNG TIN NHÓM (3 Thẻ thành viên)
        // =========================================================================
        GameObject teamPanel = CreatePopupBase(canvasObj.transform, "TeamInfoPanel", "THÔNG TIN NHÓM PHÁT TRIỂN", new Vector2(1040f, 640f), fontAsset, menuController);
        Transform teamWin = teamPanel.transform.Find("Window");

        GameObject teamHeaderCard = CreatePanel(teamWin, "TeamSubHeader", new Vector2(0f, 165f), new Vector2(920f, 76f), new Color(0.10f, 0.15f, 0.23f, 1f));
        CreateTMP(teamHeaderCard.transform, "Text",
            "<color=#FF9E2C>APEX GROUP (NHÓM 8)</color>  —  <color=#E2E8F0>MÔN: THIẾT KẾ VÀ PHÁT TRIỂN TRÒ CHƠI</color>\n" +
            "<color=#38BDF8>KHOA CÔNG NGHỆ THÔNG TIN  •  ĐẠI HỌC THỦY LỢI (TLU)</color>",
            19, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, fontAsset);

        CreateMemberCard(teamWin, new Vector2(0f, 68f),  "01", "Trương Quang Việt Hoàng", "2351060450", new Color(0.95f, 0.35f, 0.08f, 1f), fontAsset);
        CreateMemberCard(teamWin, new Vector2(0f, -24f), "02", "Triệu Quang Hoàng",        "2351060449", new Color(0.22f, 0.74f, 0.97f, 1f), fontAsset);
        CreateMemberCard(teamWin, new Vector2(0f, -116f),"03", "Lê Hữu Hiệu",              "2351060446", new Color(0.20f, 0.83f, 0.60f, 1f), fontAsset);

        teamPanel.SetActive(false);

        // 13. Nối toàn bộ tham chiếu vào MainMenuController
        SerializedObject so = new SerializedObject(menuController);
        so.FindProperty("missionSelectPanel").objectReferenceValue = missionPanel;
        so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        so.FindProperty("teamInfoPanel").objectReferenceValue = teamPanel;
        so.FindProperty("masterVolumeSlider").objectReferenceValue = volumeSlider;
        so.FindProperty("volumePercentText").objectReferenceValue = volPercentTMP;

        SerializedProperty btnArray = so.FindProperty("missionButtons");
        btnArray.arraySize = 3;
        btnArray.GetArrayElementAtIndex(0).objectReferenceValue = m1;
        btnArray.GetArrayElementAtIndex(1).objectReferenceValue = m2;
        btnArray.GetArrayElementAtIndex(2).objectReferenceValue = m3;
        so.ApplyModifiedProperties();

        UnityEventTools.AddPersistentListener(btnNew.onClick, menuController.OnNewCampaignClicked);
        UnityEventTools.AddPersistentListener(btnMission.onClick, menuController.OnOpenMissionSelectClicked);
        UnityEventTools.AddPersistentListener(btnSettings.onClick, menuController.OnOpenSettingsClicked);
        UnityEventTools.AddPersistentListener(btnTeam.onClick, menuController.OnOpenTeamInfoClicked);
        UnityEventTools.AddPersistentListener(btnExit.onClick, menuController.OnQuitGameClicked);

        string savePath = "Assets/_Project/Module_SystemUI/Scenes/MainMenu.unity";
        System.IO.Directory.CreateDirectory("Assets/_Project/Module_SystemUI/Scenes");
        EditorSceneManager.SaveScene(scene, savePath);
        AssetDatabase.Refresh();
        Debug.Log("Đã cập nhật hoàn chỉnh Scene MainMenu tại: " + savePath);
    }

    private static TMP_FontAsset FindBestFontAsset()
    {
        string[] guids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
        if (guids.Length > 0)
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        return null;
    }

    private static void CreateEmberParticles()
    {
        GameObject particleObj = new GameObject("EmberParticles");
        particleObj.transform.position = new Vector3(0f, -5.5f, 0f);
        var ps = particleObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 6f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.28f, 0.05f, 0.85f),
            new Color(1f, 0.78f, 0.15f, 0.85f)
        );
        main.maxParticles = 80;
        var emission = ps.emission;
        emission.rateOverTime = 14f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(18f, 1f, 1f);
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.4f;
        noise.frequency = 0.5f;
        var renderer = particleObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
        renderer.sortingOrder = 1;
    }

    private static GameObject CreateEmptyUI(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return obj;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 pos, Vector2 size, Color color, bool stretch = false, bool raycastTarget = false)
    {
        GameObject obj = CreateEmptyUI(parent, name, pos, size);
        Image img = obj.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = raycastTarget;
        if (stretch)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        return obj;
    }

    private static void CreateIcon(Transform parent, string name, Vector2 pos, Vector2 size, Sprite sprite, Color color)
    {
        GameObject obj = CreateEmptyUI(parent, name, pos, size);
        Image img = obj.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
    }

    private static void CreateFooterItem(Transform parent, Vector2 pos, Sprite iconSprite, Color iconColor, string labelText, TMP_FontAsset font)
    {
        GameObject item = CreateEmptyUI(parent, "FooterItem", pos, new Vector2(360f, 40f));
        CreateIcon(item.transform, "Icon", new Vector2(-155f, 0f), new Vector2(20f, 20f), iconSprite, iconColor);
        var tmp = CreateTMP(item.transform, "Text", labelText, 17, FontStyles.Bold, Color.white, TextAlignmentOptions.Left, font);
        tmp.margin = new Vector4(42f, 0f, 0f, 0f);
    }

    // Cụm Pistol ở Footer có Icon súng + Icon vô cực (∞) vẽ pixel-art sắc nét
    private static void CreatePistolFooterItem(Transform parent, Vector2 pos, TMP_FontAsset font)
    {
        GameObject item = CreateEmptyUI(parent, "FooterPistolItem", pos, new Vector2(360f, 40f));
        CreateIcon(item.transform, "GunIcon", new Vector2(-155f, 0f), new Vector2(20f, 20f), CreateIconSprite("gun"), new Color(0.22f, 0.74f, 0.97f, 1f));

        var leftText = CreateTMP(item.transform, "PistolLabel", "<color=#94A3B8>PISTOL:</color>", 17, FontStyles.Bold, Color.white, TextAlignmentOptions.Left, font);
        leftText.margin = new Vector4(42f, 0f, 0f, 0f);

        // Icon dấu vô cực ∞ nằm ngay sau chữ PISTOL:
        CreateIcon(item.transform, "InfinityIcon", new Vector2(-46f, 0f), new Vector2(24f, 14f), CreateIconSprite("infinity"), Color.white);

        var rightText = CreateTMP(item.transform, "AmmoLabel", "<color=#FFFFFF>AMMO (KEY: J)</color>", 17, FontStyles.Bold, Color.white, TextAlignmentOptions.Left, font);
        rightText.margin = new Vector4(152f, 0f, 0f, 0f);
    }

    private static TextMeshProUGUI CreateTMP(Transform parent, string name, string content, float size, FontStyles style, Color color, TextAlignmentOptions align, TMP_FontAsset font)
    {
        GameObject obj = CreateEmptyUI(parent, name, Vector2.zero, Vector2.zero);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        if (font != null) tmp.font = font;
        return tmp;
    }

    private static Button CreateStyledMenuButton(Transform parent, string name, Vector2 pos, string mainText, string badgeText, Color normalBg, Color hoverBg, Color mainColor, Color badgeColor, Sprite iconSprite, Color iconColor, bool isPrimary, TMP_FontAsset font)
    {
        GameObject btnObj = CreatePanel(parent, name, pos, new Vector2(760f, 76f), normalBg, false, true);
        Button btn = btnObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;

        var effect = btnObj.AddComponent<MenuButtonEffect>();
        effect.SetupColors(btnObj.GetComponent<Image>(), normalBg, hoverBg);

        if (isPrimary)
        {
            GameObject accent = CreatePanel(btnObj.transform, "LeftAccent", Vector2.zero, new Vector2(8f, 0f), new Color(1f, 0.82f, 0.15f, 1f));
            RectTransform accRt = accent.GetComponent<RectTransform>();
            accRt.anchorMin = new Vector2(0f, 0f);
            accRt.anchorMax = new Vector2(0f, 1f);
            accRt.pivot = new Vector2(0f, 0.5f);
            accRt.sizeDelta = new Vector2(8f, 0f);
        }

        CreateIcon(btnObj.transform, "BtnIcon", new Vector2(-325f, 0f), new Vector2(26f, 26f), iconSprite, iconColor);

        var leftTMP = CreateTMP(btnObj.transform, "MainLabel", mainText, 26, FontStyles.Bold, mainColor, TextAlignmentOptions.Left, font);
        leftTMP.margin = new Vector4(82f, 0f, 0f, 0f);

        if (isPrimary)
        {
            GameObject tagBg = CreatePanel(btnObj.transform, "BadgeBox", new Vector2(265f, 0f), new Vector2(165f, 38f), new Color(0.45f, 0.10f, 0.02f, 0.92f));
            CreateTMP(tagBg.transform, "SubLabel", badgeText, 18, FontStyles.Bold, badgeColor, TextAlignmentOptions.Center, font);
        }
        else
        {
            var rightTMP = CreateTMP(btnObj.transform, "SubLabel", badgeText, 18, FontStyles.Bold, badgeColor, TextAlignmentOptions.Right, font);
            rightTMP.margin = new Vector4(0f, 0f, 38f, 0f);
        }

        return btn;
    }

    private static GameObject CreatePopupBase(Transform parent, string name, string title, Vector2 windowSize, TMP_FontAsset font, MainMenuController controller)
    {
        GameObject overlay = CreatePanel(parent, name, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.82f), true, true);
        GameObject box = CreatePanel(overlay.transform, "Window", Vector2.zero, windowSize, new Color(0.07f, 0.10f, 0.16f, 0.99f), false, true);

        float halfH = windowSize.y * 0.5f;

        GameObject header = CreatePanel(box.transform, "Header", new Vector2(0f, halfH - 38f), new Vector2(windowSize.x, 76f), new Color(0.11f, 0.16f, 0.25f, 1f));
        CreatePanel(header.transform, "BottomLine", new Vector2(0f, -37f), new Vector2(windowSize.x, 2f), new Color(0.95f, 0.35f, 0.08f, 0.8f));
        CreateTMP(header.transform, "Title", title, 28, FontStyles.Bold, new Color(1f, 0.62f, 0.17f, 1f), TextAlignmentOptions.Center, font);

        GameObject closeObj = CreatePanel(box.transform, "Btn_Close", new Vector2(0f, -halfH + 42f), new Vector2(260f, 52f), new Color(0.88f, 0.22f, 0.10f, 1f), false, true);
        Button closeBtn = closeObj.AddComponent<Button>();
        var closeEffect = closeObj.AddComponent<MenuButtonEffect>();
        closeEffect.SetupColors(closeObj.GetComponent<Image>(), new Color(0.88f, 0.22f, 0.10f, 1f), new Color(1f, 0.32f, 0.16f, 1f));
        CreateTMP(closeObj.transform, "Text", "ĐÓNG (CLOSE)", 21, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, font);
        UnityEventTools.AddPersistentListener(closeBtn.onClick, controller.CloseAllSubPanels);

        return overlay;
    }

    private static Button CreatePopupActionButton(Transform popupOverlay, string name, Vector2 pos, string label, TMP_FontAsset font)
    {
        Transform box = popupOverlay.transform.Find("Window");
        GameObject btnObj = CreatePanel(box, name, pos, new Vector2(640f, 68f), new Color(0.11f, 0.17f, 0.27f, 1f), false, true);
        Button btn = btnObj.AddComponent<Button>();
        var effect = btnObj.AddComponent<MenuButtonEffect>();
        effect.SetupColors(btnObj.GetComponent<Image>(), new Color(0.11f, 0.17f, 0.27f, 1f), new Color(0.18f, 0.27f, 0.42f, 1f));
        CreateTMP(btnObj.transform, "Text", label, 23, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, font);
        return btn;
    }

    private static Slider CreateStyledSlider(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        GameObject sliderObj = CreateEmptyUI(parent, name, pos, size);
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        GameObject bg = CreatePanel(sliderObj.transform, "Background", Vector2.zero, new Vector2(0f, 12f), new Color(0.18f, 0.24f, 0.35f, 1f), false, true);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.5f);
        bgRt.anchorMax = new Vector2(1f, 0.5f);
        bgRt.sizeDelta = new Vector2(0f, 12f);

        GameObject fillArea = CreateEmptyUI(sliderObj.transform, "Fill Area", Vector2.zero, Vector2.zero);
        RectTransform faRt = fillArea.GetComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0f, 0.5f);
        faRt.anchorMax = new Vector2(1f, 0.5f);
        faRt.sizeDelta = new Vector2(0f, 12f);

        GameObject fill = CreatePanel(fillArea.transform, "Fill", Vector2.zero, Vector2.zero, new Color(0.95f, 0.35f, 0.08f, 1f), true, false);
        slider.fillRect = fill.GetComponent<RectTransform>();

        GameObject handleArea = CreateEmptyUI(sliderObj.transform, "Handle Slide Area", Vector2.zero, Vector2.zero);
        RectTransform haRt = handleArea.GetComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero;
        haRt.anchorMax = Vector2.one;
        haRt.offsetMin = Vector2.zero;
        haRt.offsetMax = Vector2.zero;

        GameObject handle = CreatePanel(handleArea.transform, "Handle", Vector2.zero, new Vector2(24f, 28f), Color.white, false, true);
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handle.GetComponent<Image>();

        return slider;
    }

    private static void CreateKeyRow(Transform parent, Vector2 pos, string keyText, string descText, TMP_FontAsset font)
    {
        GameObject row = CreatePanel(parent, "KeyRow_" + keyText, pos, new Vector2(480f, 40f), new Color(0.07f, 0.11f, 0.17f, 0.95f));

        GameObject badge = CreatePanel(row.transform, "KeyBadge", new Vector2(-155f, 0f), new Vector2(148f, 30f), new Color(0.16f, 0.24f, 0.36f, 1f));
        CreateTMP(badge.transform, "Key", keyText, 15, FontStyles.Bold, new Color(1f, 0.78f, 0.20f, 1f), TextAlignmentOptions.Center, font);

        var descTMP = CreateTMP(row.transform, "Desc", descText, 17, FontStyles.Normal, new Color(0.88f, 0.92f, 0.97f, 1f), TextAlignmentOptions.Left, font);
        descTMP.margin = new Vector4(180f, 0f, 10f, 0f);
    }

    private static void CreateMemberCard(Transform parent, Vector2 pos, string indexStr, string fullName, string studentId, Color accentColor, TMP_FontAsset font)
    {
        GameObject card = CreatePanel(parent, "MemberCard_" + studentId, pos, new Vector2(920f, 76f), new Color(0.10f, 0.15f, 0.23f, 1f));

        CreatePanel(card.transform, "AccentBar", new Vector2(-456f, 0f), new Vector2(8f, 76f), accentColor);

        GameObject numBox = CreatePanel(card.transform, "NumBox", new Vector2(-400f, 0f), new Vector2(54f, 46f), new Color(0.07f, 0.10f, 0.16f, 1f));
        CreateTMP(numBox.transform, "Num", indexStr, 20, FontStyles.Bold, accentColor, TextAlignmentOptions.Center, font);

        var nameTMP = CreateTMP(card.transform, "FullName", fullName.ToUpper(), 24, FontStyles.Bold, Color.white, TextAlignmentOptions.Left, font);
        nameTMP.margin = new Vector4(115f, 0f, 0f, 0f);

        GameObject idBadge = CreatePanel(card.transform, "IdBadge", new Vector2(325f, 0f), new Vector2(220f, 42f), new Color(0.07f, 0.10f, 0.16f, 1f));
        CreateTMP(idBadge.transform, "StudentId", $"MSV: {studentId}", 19, FontStyles.Bold, new Color(0.38f, 0.82f, 0.98f, 1f), TextAlignmentOptions.Center, font);
    }

    private static Sprite CreateGridBackgroundSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color baseCol = new Color(1f, 1f, 1f, 1f);
        Color lineCol = new Color(0.75f, 0.85f, 1f, 1f);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, (x == 0 || y == 0) ? lineCol : baseCol);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private static Sprite CreateIconSprite(string type)
    {
        int s = 24;
        Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color w = Color.white;

        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                tex.SetPixel(x, y, clear);

        void FillRect(int x0, int y0, int x1, int y1)
        {
            for (int y = Mathf.Max(0, y0); y <= Mathf.Min(s - 1, y1); y++)
                for (int x = Mathf.Max(0, x0); x <= Mathf.Min(s - 1, x1); x++)
                    tex.SetPixel(x, y, w);
        }

        switch (type)
        {
            case "play":
                for (int x = 5; x <= 19; x++)
                {
                    int half = (19 - x) * 8 / 14;
                    FillRect(x, 12 - half, x, 12 + half);
                }
                break;
            case "layers":
                FillRect(4, 16, 19, 18);
                FillRect(4, 11, 19, 13);
                FillRect(4, 6, 19, 8);
                break;
            case "map":
                FillRect(3, 5, 8, 18);
                FillRect(10, 6, 14, 19);
                FillRect(16, 5, 20, 18);
                break;
            case "sliders":
                FillRect(3, 17, 20, 18); FillRect(7, 15, 10, 20);
                FillRect(3, 11, 20, 12); FillRect(14, 9, 17, 14);
                FillRect(3, 5, 20, 6);   FillRect(9, 3, 12, 8);
                break;
            case "team":
                FillRect(10, 14, 13, 17); FillRect(8, 7, 15, 12);
                FillRect(4, 12, 6, 14);   FillRect(3, 7, 7, 10);
                FillRect(17, 12, 19, 14); FillRect(16, 7, 20, 10);
                break;
            case "power":
                FillRect(11, 11, 12, 20);
                FillRect(6, 5, 17, 6);
                FillRect(5, 6, 6, 15);
                FillRect(17, 6, 18, 15);
                break;
            case "hero":
                FillRect(9, 14, 14, 19);
                FillRect(6, 5, 17, 12);
                break;
            case "gun":
                FillRect(5, 12, 19, 16);
                FillRect(6, 6, 10, 12);
                break;
            case "target":
                FillRect(11, 3, 12, 20);
                FillRect(3, 11, 20, 12);
                FillRect(7, 7, 16, 8); FillRect(7, 15, 16, 16);
                FillRect(7, 7, 8, 16); FillRect(15, 7, 16, 16);
                break;
            case "infinity": // Vẽ biểu tượng vô cực ∞ sắc nét 2 vòng khép kín
                // Vòng trái
                FillRect(3, 15, 8, 16);
                FillRect(3, 7, 8, 8);
                FillRect(2, 9, 3, 14);
                // Vòng phải
                FillRect(15, 15, 20, 16);
                FillRect(15, 7, 20, 8);
                FillRect(20, 9, 21, 14);
                // Nhánh chéo giao nhau ở giữa
                FillRect(9, 13, 10, 14);
                FillRect(11, 11, 12, 12);
                FillRect(13, 9, 14, 10);
                FillRect(9, 9, 10, 10);
                FillRect(13, 13, 14, 14);
                break;
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f));
    }
}
#endif