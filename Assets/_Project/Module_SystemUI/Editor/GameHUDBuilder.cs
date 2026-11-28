#if UNITY_EDITOR
using MetalRebels.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

public class GameHUDBuilder
{
    [MenuItem("MetalRebels/Tạo Prefab HUD, Pause & GameOver")]
    public static void BuildGameHUDPrefab()
    {
        TMP_FontAsset fontAsset = FindLiberationSansFont();

        GameObject canvasObj = new GameObject("GameHUD_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        HUDController hud = canvasObj.AddComponent<HUDController>();

        // 1. CỤM HUD GÓC TRÊN TRÁI (Máu, Mạng, Vũ khí, Đạn/Icon Vô cực, Lựu đạn)
        GameObject topLeft = CreatePanel(canvasObj.transform, "TopLeft_StatusBox", new Vector2(-690f, 455f), new Vector2(480f, 120f), new Color(0.06f, 0.09f, 0.14f, 0.88f));
        CreatePanel(topLeft.transform, "LeftBar", new Vector2(-237f, 0f), new Vector2(6f, 120f), new Color(0.95f, 0.35f, 0.08f, 1f));

        CreateTMP(topLeft.transform, "HeroLabel", new Vector2(-135f, 36f), new Vector2(160f, 30f), "MARCO", 20, FontStyles.Bold, new Color(1f, 0.62f, 0.17f, 1f), TextAlignmentOptions.Left, fontAsset);
        var livesTMP = CreateTMP(topLeft.transform, "LivesText", new Vector2(-25f, 36f), new Vector2(80f, 30f), "x3", 20, FontStyles.Bold, new Color(0.20f, 0.83f, 0.60f, 1f), TextAlignmentOptions.Left, fontAsset);

        Slider hpSlider = CreateHPBar(topLeft.transform, new Vector2(-50f, 2f), new Vector2(330f, 22f));
        var hpTMP = CreateTMP(topLeft.transform, "HPText", new Vector2(170f, 2f), new Vector2(95f, 26f), "100/100", 16, FontStyles.Bold, Color.white, TextAlignmentOptions.Right, fontAsset);

        var weaponTMP = CreateTMP(topLeft.transform, "WeaponNameText", new Vector2(-135f, -34f), new Vector2(160f, 28f), "PISTOL", 18, FontStyles.Bold, new Color(0.22f, 0.74f, 0.97f, 1f), TextAlignmentOptions.Left, fontAsset);

        // Chữ số đạn (hiện khi dùng HMG/Shotgun/Rocket)
        var ammoTMP = CreateTMP(topLeft.transform, "AmmoText", new Vector2(20f, -34f), new Vector2(120f, 28f), "200/200", 18, FontStyles.Bold, Color.white, TextAlignmentOptions.Left, fontAsset);
        ammoTMP.gameObject.SetActive(false);

        // Icon Vô cực ∞ (hiện mặc định khi dùng Pistol)
        GameObject infIconObj = CreateEmptyUI(topLeft.transform, "InfinityAmmoIcon", new Vector2(-18f, -34f), new Vector2(30f, 16f));
        Image infImg = infIconObj.AddComponent<Image>();
        infImg.sprite = CreateInfinitySprite();
        infImg.color = Color.white;
        infImg.raycastTarget = false;

        // Icon Quả Lựu Đạn pixel-art đặt ngay bên trái số lượng lựu đạn
        GameObject grenadeIconObj = CreateEmptyUI(topLeft.transform, "GrenadeIcon", new Vector2(112f, -34f), new Vector2(20f, 22f));
        Image grenadeImg = grenadeIconObj.AddComponent<Image>();
        grenadeImg.sprite = CreateGrenadeSprite();
        grenadeImg.color = new Color(0.98f, 0.80f, 0.20f, 1f);
        grenadeImg.raycastTarget = false;

        var grenadeTMP = CreateTMP(topLeft.transform, "GrenadeText", new Vector2(160f, -34f), new Vector2(110f, 28f), "10/10", 18, FontStyles.Bold, new Color(0.98f, 0.80f, 0.20f, 1f), TextAlignmentOptions.Right, fontAsset);

        // 2. CỤM HUD GÓC TRÊN PHẢI (Tên Màn Chơi + Nút Pause nhanh)
        GameObject topRight = CreatePanel(canvasObj.transform, "TopRight_MissionBox", new Vector2(700f, 465f), new Vector2(460f, 84f), new Color(0.06f, 0.09f, 0.14f, 0.88f));
        CreateTMP(topRight.transform, "MissionTag", new Vector2(-42f, 18f), new Vector2(340f, 24f), "ACTIVE MISSION", 14, FontStyles.Bold, new Color(1f, 0.62f, 0.17f, 1f), TextAlignmentOptions.Left, fontAsset);
        var missionTitleTMP = CreateTMP(topRight.transform, "MissionTitleText", new Vector2(-42f, -12f), new Vector2(340f, 30f), "MÀN 1: ĐỘT NHẬP CĂN CỨ RAVEN", 18, FontStyles.Bold, Color.white, TextAlignmentOptions.Left, fontAsset);

        GameObject pauseBtnObj = CreatePanel(topRight.transform, "Btn_PauseHUD", new Vector2(182f, 0f), new Vector2(58f, 58f), new Color(0.12f, 0.18f, 0.28f, 1f), true);
        Button pauseHudBtn = pauseBtnObj.AddComponent<Button>();
        var pauseHover = pauseBtnObj.AddComponent<MenuButtonEffect>();
        pauseHover.SetupColors(pauseBtnObj.GetComponent<Image>(), new Color(0.12f, 0.18f, 0.28f, 1f), new Color(0.91f, 0.24f, 0.08f, 1f));
        CreateTMP(pauseBtnObj.transform, "Text", Vector2.zero, new Vector2(58f, 58f), "II", 22, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, fontAsset);
        UnityEventTools.AddPersistentListener(pauseHudBtn.onClick, hud.TogglePauseMenu);

        // 3. PAUSE MENU PANEL
        GameObject pausePanel = CreateOverlayWindow(canvasObj.transform, "PausePanel", "TRÒ CHƠI TẠM DỪNG", "NHẤN ESC HOẶC CHỌN THAO TÁC BÊN DƯỚI", new Color(0.22f, 0.74f, 0.97f, 1f), new Vector2(720f, 500f), fontAsset);
        Transform pauseWin = pausePanel.transform.Find("Window");

        Button btnResume = CreateActionBtn(pauseWin, "Btn_Resume", new Vector2(0f, 45f), "TIẾP TỤC CHIẾN ĐẤU", new Color(0.91f, 0.24f, 0.08f, 1f), new Color(1f, 0.34f, 0.12f, 1f), fontAsset);
        Button btnPauseRestart = CreateActionBtn(pauseWin, "Btn_Restart", new Vector2(0f, -45f), "CHƠI LẠI MÀN NÀY", new Color(0.11f, 0.17f, 0.27f, 1f), new Color(0.18f, 0.27f, 0.42f, 1f), fontAsset);
        Button btnPauseMenu = CreateActionBtn(pauseWin, "Btn_MainMenu", new Vector2(0f, -135f), "VỀ MENU CHÍNH", new Color(0.08f, 0.12f, 0.19f, 1f), new Color(0.25f, 0.11f, 0.14f, 1f), fontAsset);

        UnityEventTools.AddPersistentListener(btnResume.onClick, hud.OnResumeClicked);
        UnityEventTools.AddPersistentListener(btnPauseRestart.onClick, hud.OnRestartClicked);
        UnityEventTools.AddPersistentListener(btnPauseMenu.onClick, hud.OnMainMenuClicked);
        pausePanel.SetActive(false);

        // 4. GAME OVER PANEL (Không còn khung điểm số)
        GameObject gameOverPanel = CreateOverlayWindow(canvasObj.transform, "GameOverPanel", "NHIỆM VỤ THẤT BẠI", "CHIẾN BINH MARCO ĐÃ HY SINH TRÊN CHIẾN TRƯỜNG", new Color(0.94f, 0.27f, 0.27f, 1f), new Vector2(720f, 440f), fontAsset);
        Transform goWin = gameOverPanel.transform.Find("Window");

        Button btnGoRestart = CreateActionBtn(goWin, "Btn_Retry", new Vector2(0f, 15f), "THỬ LẠI MÀN CHƠI", new Color(0.91f, 0.24f, 0.08f, 1f), new Color(1f, 0.34f, 0.12f, 1f), fontAsset);
        Button btnGoMenu = CreateActionBtn(goWin, "Btn_MainMenu", new Vector2(0f, -80f), "THOÁT VỀ MENU CHÍNH", new Color(0.11f, 0.17f, 0.27f, 1f), new Color(0.18f, 0.27f, 0.42f, 1f), fontAsset);

        UnityEventTools.AddPersistentListener(btnGoRestart.onClick, hud.OnRestartClicked);
        UnityEventTools.AddPersistentListener(btnGoMenu.onClick, hud.OnMainMenuClicked);
        gameOverPanel.SetActive(false);

        // 5. VICTORY PANEL (Tự đổi giữa "CHIẾN THẮNG" ở Màn 1-2 và "HOÀN THÀNH NHIỆM VỤ" ở Màn 3)
        GameObject victoryPanel = CreateOverlayWindow(canvasObj.transform, "VictoryPanel", "CHIẾN THẮNG", "KHU VỰC ĐÃ ĐƯỢC KIỂM SOÁT — SẴN SÀNG CHO CHẶNG TIẾP THEO", new Color(0.20f, 0.83f, 0.60f, 1f), new Vector2(760f, 500f), fontAsset);
        Transform vicWin = victoryPanel.transform.Find("Window");
        var vicTitleTMP = vicWin.Find("Title").GetComponent<TextMeshProUGUI>();
        var vicSubtitleTMP = vicWin.Find("Subtitle").GetComponent<TextMeshProUGUI>();

        Button btnNextMission = CreateActionBtn(vicWin, "Btn_NextMission", new Vector2(0f, 45f), "MÀN TIẾP THEO", new Color(0.12f, 0.68f, 0.45f, 1f), new Color(0.16f, 0.80f, 0.54f, 1f), fontAsset);
        Button btnVicRestart = CreateActionBtn(vicWin, "Btn_Replay", new Vector2(0f, -45f), "CHƠI LẠI MÀN NÀY", new Color(0.11f, 0.17f, 0.27f, 1f), new Color(0.18f, 0.27f, 0.42f, 1f), fontAsset);
        Button btnVicMenu = CreateActionBtn(vicWin, "Btn_MainMenu", new Vector2(0f, -135f), "VỀ MENU CHÍNH", new Color(0.08f, 0.12f, 0.19f, 1f), new Color(0.18f, 0.27f, 0.42f, 1f), fontAsset);

        UnityEventTools.AddPersistentListener(btnNextMission.onClick, hud.OnNextMissionClicked);
        UnityEventTools.AddPersistentListener(btnVicRestart.onClick, hud.OnRestartClicked);
        UnityEventTools.AddPersistentListener(btnVicMenu.onClick, hud.OnMainMenuClicked);
        victoryPanel.SetActive(false);

        // 6. Gán tham chiếu vào HUDController & Lưu Prefab
        SerializedObject so = new SerializedObject(hud);
        so.FindProperty("hpSlider").objectReferenceValue = hpSlider;
        so.FindProperty("hpText").objectReferenceValue = hpTMP;
        so.FindProperty("livesText").objectReferenceValue = livesTMP;
        so.FindProperty("weaponNameText").objectReferenceValue = weaponTMP;
        so.FindProperty("ammoText").objectReferenceValue = ammoTMP;
        so.FindProperty("infinityAmmoIcon").objectReferenceValue = infIconObj;
        so.FindProperty("grenadeText").objectReferenceValue = grenadeTMP;
        so.FindProperty("missionTitleText").objectReferenceValue = missionTitleTMP;

        so.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        so.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        so.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;
        so.FindProperty("victoryTitleText").objectReferenceValue = vicTitleTMP;
        so.FindProperty("victorySubtitleText").objectReferenceValue = vicSubtitleTMP;
        so.FindProperty("nextMissionButton").objectReferenceValue = btnNextMission.gameObject;
        so.ApplyModifiedProperties();

        string prefabFolder = "Assets/_Project/Module_SystemUI/Prefabs";
        System.IO.Directory.CreateDirectory(prefabFolder);
        string prefabPath = prefabFolder + "/GameHUD_Canvas.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvasObj, prefabPath);
        Object.DestroyImmediate(canvasObj);
        AssetDatabase.Refresh();

        Debug.Log("Đã tạo lại Prefab GameHUD_Canvas (bỏ tính điểm & dùng Icon vô cực ∞)!");
    }

    private static TMP_FontAsset FindLiberationSansFont()
    {
        string[] guids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
        if (guids.Length > 0)
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        return null;
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

    private static GameObject CreatePanel(Transform parent, string name, Vector2 pos, Vector2 size, Color color, bool raycast = false)
    {
        GameObject obj = CreateEmptyUI(parent, name, pos, size);
        Image img = obj.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = raycast;
        return obj;
    }

    private static TextMeshProUGUI CreateTMP(Transform parent, string name, Vector2 pos, Vector2 size, string content, float fontSize, FontStyles style, Color color, TextAlignmentOptions align, TMP_FontAsset font)
    {
        GameObject obj = CreateEmptyUI(parent, name, pos, size);
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        if (font != null) tmp.font = font;
        return tmp;
    }

    private static Slider CreateHPBar(Transform parent, Vector2 pos, Vector2 size)
    {
        GameObject sliderObj = CreateEmptyUI(parent, "HPSlider", pos, size);
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 100f;

        GameObject bg = CreatePanel(sliderObj.transform, "Background", Vector2.zero, size, new Color(0.16f, 0.22f, 0.32f, 1f));
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;

        GameObject fillArea = CreateEmptyUI(sliderObj.transform, "Fill Area", Vector2.zero, Vector2.zero);
        RectTransform faRt = fillArea.GetComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero;
        faRt.anchorMax = Vector2.one;
        faRt.offsetMin = new Vector2(2f, 2f);
        faRt.offsetMax = new Vector2(-2f, -2f);

        GameObject fill = CreatePanel(fillArea.transform, "Fill", Vector2.zero, Vector2.zero, new Color(0.95f, 0.35f, 0.08f, 1f));
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.sizeDelta = Vector2.zero;
        slider.fillRect = fillRt;

        return slider;
    }

    private static GameObject CreateOverlayWindow(Transform parent, string name, string title, string subtitle, Color accentColor, Vector2 windowSize, TMP_FontAsset font)
    {
        GameObject overlay = CreatePanel(parent, name, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.84f), true);
        RectTransform ovRt = overlay.GetComponent<RectTransform>();
        ovRt.anchorMin = Vector2.zero;
        ovRt.anchorMax = Vector2.one;
        ovRt.sizeDelta = Vector2.zero;

        GameObject box = CreatePanel(overlay.transform, "Window", Vector2.zero, windowSize, new Color(0.07f, 0.10f, 0.16f, 0.99f), true);
        float halfH = windowSize.y * 0.5f;

        CreatePanel(box.transform, "TopAccent", new Vector2(0f, halfH - 3f), new Vector2(windowSize.x, 6f), accentColor);
        CreateTMP(box.transform, "Title", new Vector2(0f, halfH - 55f), new Vector2(windowSize.x - 60f, 50f), title, 36, FontStyles.Bold, accentColor, TextAlignmentOptions.Center, font);
        CreateTMP(box.transform, "Subtitle", new Vector2(0f, halfH - 98f), new Vector2(windowSize.x - 60f, 34f), subtitle, 17, FontStyles.Bold, new Color(0.65f, 0.72f, 0.82f, 1f), TextAlignmentOptions.Center, font);

        return overlay;
    }

    private static Button CreateActionBtn(Transform parent, string name, Vector2 pos, string label, Color normalColor, Color hoverColor, TMP_FontAsset font)
    {
        GameObject btnObj = CreatePanel(parent, name, pos, new Vector2(540f, 68f), normalColor, true);
        Button btn = btnObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;

        var effect = btnObj.AddComponent<MenuButtonEffect>();
        effect.SetupColors(btnObj.GetComponent<Image>(), normalColor, hoverColor);

        CreateTMP(btnObj.transform, "Text", Vector2.zero, new Vector2(520f, 60f), label, 23, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, font);
        return btn;
    }

    private static Sprite CreateInfinitySprite()
    {
        string spriteDir = "Assets/_Project/Module_SystemUI/Sprites";
        System.IO.Directory.CreateDirectory(spriteDir);
        string pngPath = spriteDir + "/icon_infinity.png";

        int w = 32;
        int h = 16;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color white = Color.white;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, clear);

        void FillRect(int x0, int y0, int x1, int y1)
        {
            for (int y = Mathf.Max(0, y0); y <= Mathf.Min(h - 1, y1); y++)
                for (int x = Mathf.Max(0, x0); x <= Mathf.Min(w - 1, x1); x++)
                    tex.SetPixel(x, y, white);
        }

        // Vẽ biểu tượng vô cực ∞ sắc nét 32x16
        // Vòng trái
        FillRect(4, 12, 11, 13);
        FillRect(4, 2, 11, 3);
        FillRect(2, 4, 3, 11);
        // Vòng phải
        FillRect(20, 12, 27, 13);
        FillRect(20, 2, 27, 3);
        FillRect(28, 4, 29, 11);
        // Nhánh chéo giao nhau ở giữa
        FillRect(12, 10, 13, 11);
        FillRect(14, 8, 17, 9);
        FillRect(18, 6, 19, 7);
        FillRect(12, 4, 13, 5);
        FillRect(14, 6, 17, 7);
        FillRect(18, 8, 19, 9);

        tex.Apply();
        System.IO.File.WriteAllBytes(pngPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(pngPath);
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
    }

    private static Sprite CreateGrenadeSprite()
    {
        string spriteDir = "Assets/_Project/Module_SystemUI/Sprites";
        System.IO.Directory.CreateDirectory(spriteDir);
        string pngPath = spriteDir + "/icon_grenade.png";

        int w = 20;
        int h = 24;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color white = Color.white;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, clear);

        void FillRect(int x0, int y0, int x1, int y1)
        {
            for (int y = Mathf.Max(0, y0); y <= Mathf.Min(h - 1, y1); y++)
                for (int x = Mathf.Max(0, x0); x <= Mathf.Min(w - 1, x1); x++)
                    tex.SetPixel(x, y, white);
        }

        // Chốt và cần gạt trên đầu lựu đạn
        FillRect(8, 19, 11, 22);
        FillRect(12, 16, 15, 21);
        FillRect(14, 12, 15, 16);

        // Thân quả lựu đạn (chia rãnh đặc trưng)
        FillRect(5, 3, 14, 17);
        FillRect(4, 5, 15, 15);
        FillRect(6, 2, 13, 18);

        tex.Apply();
        System.IO.File.WriteAllBytes(pngPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(pngPath);
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
    }
}
#endif