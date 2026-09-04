using System;
using System.Collections.Generic;
using BepInEx.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeakBadgeHandbook
{
    // ============================================================
    // 徽章手册窗口 —— 独立 UGUI 界面（护照风格配色，中文字体，精致布局）
    // 纯预览：只读数据展示，不调用任何解锁/换装/进度 API
    // ============================================================
    public class HandbookUI
    {
        public bool IsOpen { get; private set; }
        public ManualLogSource Log;

        // cfg 可调 UI 参数
        public float UiScale = 1f;       // 整体 UI 尺寸倍率（面板/按钮/字体联动）

        private float _contentBottomOffset = 58f;
        private GameObject _root;
        private GameObject _detailLayer;
        private TextMeshProUGUI _pageLabel;
        private readonly List<GameObject> _dynamic = new List<GameObject>();
        private readonly Dictionary<int, int> _pages = new Dictionary<int, int>();
        private readonly List<Button> _tabButtons = new List<Button>();
        private bool _trackDynamic = true;

        private int _currentTab;   // 0..Slots.Length-1 部位；Slots.Length = 徽章图鉴
        private int _page;

        // 配色（护照风格）
        private static readonly Color ColPaper  = new Color(0.93f, 0.88f, 0.74f, 1f);
        private static readonly Color ColCard   = new Color(1f, 0.97f, 0.88f, 1f);
        private static readonly Color ColBorder = new Color(0.18f, 0.25f, 0.46f, 1f);
        private static readonly Color ColText   = new Color(0.24f, 0.18f, 0.10f, 1f);
        private static readonly Color ColTextDim = new Color(0.45f, 0.40f, 0.32f, 1f);
        private static readonly Color ColLocked = new Color(0.80f, 0.33f, 0.18f, 1f);
        private static readonly Color ColUnlock = new Color(0.25f, 0.60f, 0.30f, 1f);
        private static readonly Color ColTabOn  = new Color(0.18f, 0.25f, 0.46f, 1f);
        private static readonly Color ColTabOff = new Color(0.78f, 0.71f, 0.56f, 1f);

        // 展示的部位（眼睛/嘴巴是通道图，不展示贴图）
        private static readonly string[] Slots = { "fits", "hats", "sashes", "medals", "accessories", "eyes", "mouths" };

        private static TMP_FontAsset _chineseFont;
        private static TMP_FontAsset _latinFont;
        private static Sprite _roundedSprite;

        // ============ UI 预热（减少首次 F6 打开/首次翻页卡顿） ============
        private static bool _hiddenPrewarmDone;
        private static string _hiddenPrewarmLanguage;

        public static bool HiddenPrewarmDone => _hiddenPrewarmDone;

        // 隐藏完整预热：等中文字体就绪后，隐藏生成所有手册文本，避免首次 F6/翻页卡顿。
        public static void PrewarmHiddenUI()
        {
            // 语言可能因启动早期检测不准而后期修正（如 SettingsHandler 就绪后从 en 改回 zh-CN），
            // 检测到语言变化时重置预热标志，让预热按新语言重新跑一遍（delivery doc 7.12 保护）。
            if (_hiddenPrewarmDone && _hiddenPrewarmLanguage != Translations.CurrentLanguage)
            {
                _hiddenPrewarmDone = false;
            }
            if (_hiddenPrewarmDone) return;

            // 语言通用等待逻辑：当前语言对应的字体未就绪就下一帧再试。
            // 中文（简/繁）等待简体中文字体；英文/其他语言等待默认字体，默认字体缺失则回退中文字体。
            if (!IsCurrentLanguageFontReady()) return;

            try
            {
                var canvasGo = new GameObject("BadgeHandbookHiddenPrewarmCanvas", typeof(Canvas), typeof(CanvasGroup));
                var canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = -30000;
                var cg = canvasGo.GetComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;

                PrewarmText(canvasGo.transform,
                    Translations.T(
                        "徽章手册解锁图鉴衣服帽子饰带奖章饰品眼睛嘴巴徽章条件数据提取自游戏本体仅预览不改变解锁状态默认解锁已解锁未解锁上一页下一页解锁条件需要徽章徽章条件Steam统计达到攀登次数特殊条件占位已通过行动获取点我看如何解锁暂无徽章数据该部位暂无数据外观需在游戏内预览此处略如何解锁第页共个",
                        "Badge Handbook Unlock Gallery Fits Hats Sashes Medals Accessories Eyes Mouths Previous Next Unlocked Locked Unlock Condition Requires Badge Steam Stat Ascents Special Placeholder"),
                    24);

                foreach (var badge in DataProvider.AllBadges())
                {
                    PrewarmText(canvasGo.transform, DataProvider.BadgeName(badge.achievementType), 20);
                    PrewarmText(canvasGo.transform, DataProvider.BadgeDesc(badge.achievementType), 14);
                    PrewarmText(canvasGo.transform, Translations.BadgeExtra(badge.achievementType), 14);
                }

                foreach (var cosmetic in DataProvider.AllCosmetics())
                {
                    PrewarmText(canvasGo.transform, Translations.SlotName(cosmetic.slot), 18);
                    PrewarmText(canvasGo.transform, DataProvider.UnlockText(cosmetic), 16);
                    PrewarmText(canvasGo.transform, Translations.T("已解锁", "Unlocked"), 14);
                    PrewarmText(canvasGo.transform, Translations.T("未解锁", "Locked"), 14);
                }

                UnityEngine.Object.Destroy(canvasGo);
                _hiddenPrewarmDone = true;
                _hiddenPrewarmLanguage = Translations.CurrentLanguage;
            }
            catch
            {
            }
        }

        // 语言通用字体就绪判断，后续新增语言只需在此扩展。
        private static bool IsCurrentLanguageFontReady()
        {
            if (Translations.IsChinese)
            {
                TMP_FontAsset zh = null;
                try { if (FontFallbackSwapper.instance != null) zh = FontFallbackSwapper.instance.simplifiedChineseFont; } catch { }
                return zh != null;
            }

            if (TMP_Settings.defaultFontAsset != null) return true;
            TMP_FontAsset fallback = null;
            try { if (FontFallbackSwapper.instance != null) fallback = FontFallbackSwapper.instance.mainBaseFont; } catch { }
            return fallback != null;
        }

        private static void PrewarmText(Transform parent, string text, float size)
        {
            if (string.IsNullOrEmpty(text)) return;
            try
            {
                var go = new GameObject("PrewarmText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                go.transform.SetParent(parent, false);
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.font = Font();
                tmp.text = text;
                tmp.fontSize = Mathf.Clamp(size, 8f, 80f);
                tmp.ForceMeshUpdate();
            }
            catch { }
        }

        private static TMP_FontAsset Font()
        {
            // 中文界面：优先用游戏简体中文字体
            if (Translations.IsChinese)
            {
                if (_chineseFont != null) return _chineseFont;
                try
                {
                    if (FontFallbackSwapper.instance != null && FontFallbackSwapper.instance.simplifiedChineseFont != null)
                        return _chineseFont = FontFallbackSwapper.instance.simplifiedChineseFont;
                }
                catch { }
                try
                {
                    if (TMP_Settings.defaultFontAsset != null)
                        return TMP_Settings.defaultFontAsset;
                }
                catch { }
                return null;
            }

            // 英文/其他非中文界面：优先用默认字体（通常更适配拉丁字符）
            if (_latinFont != null) return _latinFont;
            try
            {
                if (TMP_Settings.defaultFontAsset != null)
                    return _latinFont = TMP_Settings.defaultFontAsset;
            }
            catch { }
            try
            {
                // 如果拿不到默认字体，再退回游戏英文字体
                if (FontFallbackSwapper.instance != null && FontFallbackSwapper.instance.mainBaseFont != null)
                    return _latinFont = FontFallbackSwapper.instance.mainBaseFont;
            }
            catch { }
            return null;
        }

        // ============ 生命周期 ============
        public void Open()
        {
            if (IsOpen) return;
            if (!DataProvider.Ready) { DataProvider.Load(DataProvider.DataDir, Log); }
            if (!DataProvider.Ready) { Log?.LogWarning("[BadgeHandbook] 数据未就绪"); return; }
            IsOpen = true;
            if (_pages.TryGetValue(_currentTab, out var savedPage))
                _page = savedPage;
            else
                _page = 0;
            BuildUI();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            DestroyUI();
        }

        // ============ UI 构建 ============
        private void BuildUI()
        {
            if (_root != null) DestroyUI();
            _root = new GameObject("BadgeHandbookUI");
            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            _root.AddComponent<CanvasScaler>();
            _root.AddComponent<GraphicRaycaster>();
            StretchCanvas(_root);

            // 以下为常驻 UI（不随翻页清除）
            _trackDynamic = false;
            _tabButtons.Clear();

            // 遮罩
            var mask = MakeImage("Mask", _root.transform, 0, 0, Screen.width, Screen.height, new Color(0f, 0f, 0f, 0.55f));
            var maskBtn = mask.gameObject.AddComponent<Button>();
            maskBtn.transition = Selectable.Transition.None;
            maskBtn.onClick.AddListener(Close);

            float uiScale = Mathf.Clamp(UiScale, 0.8f, 1.3f);
            // 面板随 UI 整体放大，但最大不超过屏幕的 92%，始终留出外部留白。
            float pw = Mathf.Min(Mathf.Min(1080f, Screen.width * 0.94f) * uiScale, Screen.width * 0.92f);
            float ph = Mathf.Min(Mathf.Min(720f, Screen.height * 0.94f) * uiScale, Screen.height * 0.92f);
            float px = (Screen.width - pw) / 2f;
            float py = (Screen.height - ph) / 2f;

            // 边框 + 纸张
            var border = MakeImage("Border", _root.transform, px - 12, py - 12, pw + 24, ph + 24, ColBorder);
            ApplyRoundedCorners(border);
            var paper = MakeImage("Paper", _root.transform, px, py, pw, ph, ColPaper);
            ApplyRoundedCorners(paper);

            // 标题条
            MakeImage("Header", _root.transform, px, py, pw, 56, ColBorder);
            string versionText = Translations.T("徽章手册 · 解锁图鉴", "Badge Handbook · Unlock Gallery");
            versionText += Translations.T("（基于 v" + Translations.DataVersion + " 数据）", "(based on v" + Translations.DataVersion + " data)");
            MakeText("Title", _root.transform, px + 20, py + 10, pw - 160, 38, versionText, new Color(1f, 0.98f, 0.90f, 1f), 24, FontStyles.Bold);
            var close = MakeButton("Close", _root.transform, px + pw - 52, py + 10, 40, 40, "✕", new Color(0.12f, 0.17f, 0.34f, 1f), 24);
            close.onClick.AddListener(Close);

            // Tab 行
            float tabH = 44f * uiScale;
            float navH = 60f * uiScale;
            float navW = 180f * uiScale;
            _contentBottomOffset = navH + 16f;

            int tabCount = Slots.Length + 1;
            float tabW = (pw - 36f) / tabCount;
            float tabY = py + 66;
            float contentTop = tabY + tabH + 10f;
            for (int i = 0; i < Slots.Length; i++)
            {
                int idx = i;
                string name = Translations.SlotName(Slots[i]);
                var b = MakeButton("Tab" + i, _root.transform, px + 12 + i * tabW, tabY, tabW - 6, tabH, name, _currentTab == i ? ColTabOn : ColTabOff, 18);
                _tabButtons.Add(b);
                b.onClick.AddListener(() =>
                {
                    _pages[_currentTab] = _page;
                    _currentTab = idx;
                    _page = _pages.TryGetValue(idx, out var saved) ? saved : 0;
                    RefreshTabColors();
                    RebuildContent(px, py, pw, ph, contentTop);
                });
            }
            int badgeTabIdx = Slots.Length;
            var bt = MakeButton("TabBadges", _root.transform, px + 12 + Slots.Length * tabW, tabY, tabW - 6, tabH, Translations.T("徽章图鉴", "Badges"), _currentTab == badgeTabIdx ? ColTabOn : ColTabOff, 18);
            _tabButtons.Add(bt);
            bt.onClick.AddListener(() =>
            {
                _pages[_currentTab] = _page;
                _currentTab = badgeTabIdx;
                _page = _pages.TryGetValue(badgeTabIdx, out var saved) ? saved : 0;
                RefreshTabColors();
                RebuildContent(px, py, pw, ph, contentTop);
            });

            // 翻页（底部）
            float pageY = py + ph - navH - 16f;
            var prev = MakeButton("Prev", _root.transform, px + 16, pageY, navW, navH, Translations.T("上一页", "Prev"), ColBorder, 18);
            prev.onClick.AddListener(() => { _page--; _pages[_currentTab] = _page; RebuildContent(px, py, pw, ph, contentTop); });
            var next = MakeButton("Next", _root.transform, px + pw - navW - 16, pageY, navW, navH, Translations.T("下一页", "Next"), ColBorder, 18);
            next.onClick.AddListener(() => { _page++; _pages[_currentTab] = _page; RebuildContent(px, py, pw, ph, contentTop); });
            _pageLabel = MakeText("Page", _root.transform, px + pw / 2f - 150, pageY, 300, navH, "", ColBorder, 17, FontStyles.Bold);

            // 以下为可翻页内容，需要随 Tab/翻页清除重建
            _trackDynamic = true;
            RefreshTabColors();

            RebuildContent(px, py, pw, ph, contentTop);
        }

        private void RefreshTabColors()
        {
            for (int i = 0; i < _tabButtons.Count; i++)
            {
                var btn = _tabButtons[i];
                var img = btn?.targetGraphic as Image;
                if (btn == null || img == null) continue;

                // 直接禁用 ColorTint，避免按钮状态切换把选中色覆盖掉。
                // 高亮完全由 Image.color 控制，这样切换栏目一定立刻显示。
                btn.transition = Selectable.Transition.None;
                var targetColor = (i == _currentTab) ? ColTabOn : ColTabOff;
                img.color = targetColor;
                img.CrossFadeColor(targetColor, 0f, true, true);
            }
        }

        private void RebuildContent(float px, float py, float pw, float ph, float tabY)
        {
            for (int i = _dynamic.Count - 1; i >= 0; i--)
            {
                var go = _dynamic[i];
                if (go == null || go == _root) continue;
                _dynamic.RemoveAt(i);
                UnityEngine.Object.Destroy(go);
            }
            float contentY = tabY;
            float contentH = (py + ph - _contentBottomOffset) - contentY;
            if (_currentTab == Slots.Length) DrawBadgeGrid(px, contentY, pw, contentH);
            else DrawCosmeticGrid(px, contentY, pw, contentH, Slots[_currentTab]);
        }

        // ============ 外观格子 ============
        private void DrawCosmeticGrid(float px, float contentY, float pw, float contentH, string slot)
        {
            var items = new List<CosmeticEntry>();
            foreach (var c in DataProvider.AllCosmetics())
                if (c.slot == slot) items.Add(c);
            if (items.Count == 0) { MakeText("Empty", _root.transform, px + 40, contentY - contentH / 2f, pw - 80, 50, Translations.T("该部位暂无数据", "No data for this slot"), ColText, 22, FontStyles.Bold); return; }

            const int cols = 4, rows = 2;
            int perPage = cols * rows;
            int total = (items.Count + perPage - 1) / perPage;
            if (_page < 0) _page = 0;
            if (_page >= total) _page = total - 1;
            _pages[_currentTab] = _page;

            float gw = (pw - 40 - (cols - 1) * 14) / cols;
            float gh = (contentH - 14 - (rows - 1) * 14) / rows;
            int start = _page * perPage;

            for (int i = 0; i < perPage; i++)
            {
                int idx = start + i;
                if (idx >= items.Count) break;
                var c = items[idx];
                float cx = px + 20 + (i % cols) * (gw + 14);
                float cy = contentY + (i / cols) * (gh + 14);
                DrawCard(c, cx, cy, gw, gh);
            }
            if (_pageLabel != null) _pageLabel.text = Translations.T("第 ", "Page ") + (_page + 1) + " / " + total + Translations.T(" 页  ·  ", " · ") + Translations.SlotName(slot);
        }

        private void DrawCard(CosmeticEntry c, float x, float y, float w, float h)
        {
            var card = MakeImage("Card_" + c.slot + "_" + c.index, _root.transform, x, y, w, h, ColCard);
            var btn = card.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            btn.targetGraphic = card;
            var cb = new ColorBlock { normalColor = Color.white, highlightedColor = new Color(0.95f, 0.92f, 0.85f, 1f), pressedColor = new Color(0.9f, 0.86f, 0.78f, 1f), selectedColor = Color.white, disabledColor = Color.grey, colorMultiplier = 1f, fadeDuration = 0.08f };
            btn.colors = cb;

            // 贴图（通道图部位不显示贴图）
            if (c.slot == "eyes")
            {
                var eyeTex = DataProvider.GetTexture(c.textureFile ?? "") ?? DataProvider.GetCosmeticTexture(c);
                if (eyeTex != null)
                {
                    var img = MakeRawImage("Tex_" + c.slot + "_" + c.index, _root.transform, x + 10, y + h - (w - 20), w - 20, w - 20);
                    img.texture = eyeTex;
                    var eyeMat = DataProvider.GetEyeMaterial();
                    if (eyeMat != null) img.material = eyeMat;
                    img.color = Color.white;
                }
                else
                {
                    MakeText("NoTex_" + c.slot + "_" + c.index, _root.transform, x + 10, y + 30, w - 20, h - 60, Translations.T("（外观需在游戏内\n预览，此处略）", "(Preview only in game\nnot shown here)"), ColTextDim, 14, FontStyles.Normal).alignment = TextAlignmentOptions.Center;
                }
            }
            else
            {
                var tex = DataProvider.GetTexture(c.textureFile ?? "") ?? DataProvider.GetCosmeticTexture(c);
                if (tex != null)
                {
                    var img = MakeRawImage("Tex_" + c.slot + "_" + c.index, _root.transform, x + 10, y + h - (w - 20), w - 20, w - 20);
                    img.texture = tex;
                    img.color = Color.white;
                }
            }

            // 底部状态条
            string status;
            Color sc;
            if (c.isBlank) { status = Translations.T("占位", "Placeholder"); sc = ColTextDim; }
            else if (DataProvider.IsDefaultUnlock(c)) { status = Translations.T("默认解锁", "Unlocked by default"); sc = ColUnlock; }
            else if (DataProvider.IsCosmeticUnlocked(c)) { status = Translations.T("已通过行动获取", "Earned"); sc = ColUnlock; }
            else { status = Translations.T("点我看如何解锁", "Click to see unlock"); sc = ColLocked; }
            MakeText("St_" + c.slot + "_" + c.index, _root.transform, x + 10, y + 8, w - 20, 24, status, sc, 14, FontStyles.Bold).alignment = TextAlignmentOptions.Center;

            btn.onClick.AddListener(() => ShowDetail(c));
        }

        private void ShowDetail(CosmeticEntry c)
        {
            if (_detailLayer != null) UnityEngine.Object.Destroy(_detailLayer);
            _detailLayer = new GameObject("DetailLayer");
            var canvas = _detailLayer.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            _detailLayer.AddComponent<GraphicRaycaster>();
            StretchCanvas(_detailLayer);

            // 详情层整层销毁，不需要进入 _dynamic 列表
            _trackDynamic = false;

            var mask = MakeImage("DMask", _detailLayer.transform, 0, 0, Screen.width, Screen.height, new Color(0f, 0f, 0f, 0.45f));
            var mb = mask.gameObject.AddComponent<Button>();
            mb.transition = Selectable.Transition.None;
            mb.onClick.AddListener(CloseDetail);

            float dw = Mathf.Min(620, Screen.width * 0.8f);
            float dh = 380;
            float dx = (Screen.width - dw) / 2f;
            float dy = (Screen.height - dh) / 2f;

            MakeImage("DBorder", _detailLayer.transform, dx - 8, dy - 8, dw + 16, dh + 16, ColBorder);
            MakeImage("DPaper", _detailLayer.transform, dx, dy, dw, dh, ColPaper);
            MakeImage("DHeader", _detailLayer.transform, dx, dy, dw, 48, ColBorder);
            MakeText("DTitle", _detailLayer.transform, dx + 16, dy + 8, dw - 90, 34, Translations.T("如何解锁 · ", "How to unlock · ") + Translations.SlotName(c.slot) + " " + c.index, new Color(1f, 0.98f, 0.90f, 1f), 22, FontStyles.Bold);
            var cb2 = MakeButton("DClose", _detailLayer.transform, dx + dw - 44, dy + 8, 36, 34, "✕", new Color(0.12f, 0.17f, 0.34f, 1f), 20);
            cb2.onClick.AddListener(CloseDetail);

            // 大图（通道图不显示）
            if (c.slot == "eyes")
            {
                var eyeTex = DataProvider.GetTexture(c.textureFile ?? "") ?? DataProvider.GetCosmeticTexture(c);
                if (eyeTex != null)
                {
                    var big = MakeRawImage("DBig", _detailLayer.transform, dx + 24, dy + 66, 200, 200);
                    big.texture = eyeTex;
                    var eyeMat = DataProvider.GetEyeMaterial();
                    if (eyeMat != null) big.material = eyeMat;
                    big.color = Color.white;
                }
            }
            else
            {
                var tex = DataProvider.GetTexture(c.textureFile ?? "") ?? DataProvider.GetCosmeticTexture(c);
                if (tex != null)
                {
                    var big = MakeRawImage("DBig", _detailLayer.transform, dx + 24, dy + 66, 200, 200);
                    big.texture = tex;
                    big.color = Color.white;
                }
            }

            // 解锁条件
            MakeText("DUnlock", _detailLayer.transform, dx + 250, dy + 66, dw - 274, dh - 90, DataProvider.UnlockText(c), ColText, 17, FontStyles.Normal);

            // 底部说明
            MakeText("DFooter", _detailLayer.transform, dx + 16, dy + dh - 34, dw - 32, 22, Translations.T("数据提取自游戏本体 · 仅预览，不改变解锁状态", "Data from the game · Preview only, no unlock changes"), ColTextDim, 12, FontStyles.Normal);

            _trackDynamic = true;
        }

        private void ShowBadgeDetail(BadgeEntry b)
        {
            if (_detailLayer != null) UnityEngine.Object.Destroy(_detailLayer);
            _detailLayer = new GameObject("BadgeDetailLayer");
            var canvas = _detailLayer.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            _detailLayer.AddComponent<GraphicRaycaster>();
            StretchCanvas(_detailLayer);
            _trackDynamic = false;

            var mask = MakeImage("BDMask", _detailLayer.transform, 0, 0, Screen.width, Screen.height, new Color(0f, 0f, 0f, 0.45f));
            var mb = mask.gameObject.AddComponent<Button>();
            mb.transition = Selectable.Transition.None;
            mb.onClick.AddListener(CloseDetail);

            float dw = Mathf.Min(680, Screen.width * 0.86f);
            float dh = 420;
            float dx = (Screen.width - dw) / 2f;
            float dy = (Screen.height - dh) / 2f;

            MakeImage("BDBorder", _detailLayer.transform, dx - 8, dy - 8, dw + 16, dh + 16, ColBorder);
            MakeImage("BDPaper", _detailLayer.transform, dx, dy, dw, dh, ColPaper);
            MakeImage("BDHeader", _detailLayer.transform, dx, dy, dw, 48, ColBorder);
            MakeText("BDTitle", _detailLayer.transform, dx + 16, dy + 8, dw - 90, 34, Translations.T("徽章 · ", "Badge · ") + DataProvider.BadgeName(b.achievementType), new Color(1f, 0.98f, 0.90f, 1f), 22, FontStyles.Bold);
            var cb2 = MakeButton("BDClose", _detailLayer.transform, dx + dw - 44, dy + 8, 36, 34, "✕", new Color(0.12f, 0.17f, 0.34f, 1f), 20);
            cb2.onClick.AddListener(CloseDetail);

            var tex = DataProvider.GetTexture(b.iconFile ?? "") ?? DataProvider.GetBadgeTexture(b);
            if (tex != null)
            {
                var big = MakeRawImage("BDBig", _detailLayer.transform, dx + 24, dy + 66, 200, 200);
                big.texture = tex;
            }

            string desc = DataProvider.BadgeDesc(b.achievementType);
            if (string.IsNullOrEmpty(desc)) desc = b.description ?? "";
            string extra = Translations.BadgeExtra(b.achievementType);
            string body = Translations.T("解锁条件：", "Unlock condition: ") + desc;
            if (!string.IsNullOrEmpty(extra)) body += "\n\n" + extra;
            MakeText("BDUnlock", _detailLayer.transform, dx + 250, dy + 66, dw - 274, dh - 90, body, ColText, 17, FontStyles.Normal);

            MakeText("BDFooter", _detailLayer.transform, dx + 16, dy + dh - 34, dw - 32, 22, Translations.T("数据提取自游戏本体 · 仅预览，不改变解锁状态", "Data from the game · Preview only, no unlock changes"), ColTextDim, 12, FontStyles.Normal);

            _trackDynamic = true;
        }

        private void CloseDetail()
        {
            if (_detailLayer != null) { UnityEngine.Object.Destroy(_detailLayer); _detailLayer = null; }
        }

        // ============ 徽章图鉴 ============
        private void DrawBadgeGrid(float px, float contentY, float pw, float contentH)
        {
            var badges = DataProvider.AllBadges();
            if (badges.Count == 0) { MakeText("Empty", _root.transform, px + 40, contentY - contentH / 2f, pw - 80, 50, Translations.T("暂无徽章数据", "No badge data"), ColText, 22, FontStyles.Bold); return; }

            const int cols = 2, rows = 3;
            int perPage = cols * rows;
            int total = (badges.Count + perPage - 1) / perPage;
            if (_page < 0) _page = 0;
            if (_page >= total) _page = total - 1;
            _pages[_currentTab] = _page;

            float gw = (pw - 40 - (cols - 1) * 16) / cols;
            float gh = (contentH - 14 - (rows - 1) * 14) / rows;
            int start = _page * perPage;

            for (int i = 0; i < perPage; i++)
            {
                int idx = start + i;
                if (idx >= badges.Count) break;
                var b = badges[idx];
                float bx = px + 20 + (i % cols) * (gw + 16);
                float by = contentY + (i / cols) * (gh + 14);
                var card = MakeImage("BCard_" + b.visualID, _root.transform, bx, by, gw, gh, ColCard);
                var btn = card.gameObject.AddComponent<Button>();
                btn.transition = Selectable.Transition.ColorTint;
                btn.targetGraphic = card;
                btn.colors = new ColorBlock { normalColor = Color.white, highlightedColor = new Color(0.95f, 0.92f, 0.85f, 1f), pressedColor = new Color(0.9f, 0.86f, 0.78f, 1f), selectedColor = Color.white, disabledColor = Color.grey, colorMultiplier = 1f, fadeDuration = 0.08f };

                var tex = DataProvider.GetTexture(b.iconFile ?? "") ?? DataProvider.GetBadgeTexture(b);
                if (tex != null)
                {
                    var icon = MakeRawImage("BIcon_" + b.visualID, _root.transform, bx + 14, by + gh - 78, 64, 64);
                    icon.texture = tex;
                }
                string name = DataProvider.BadgeName(b.achievementType);
                string desc = DataProvider.BadgeDesc(b.achievementType);
                if (string.IsNullOrEmpty(desc)) desc = b.description ?? "";
                bool badgeUnlocked = DataProvider.IsBadgeUnlocked(b.achievementType);
                MakeText("BName_" + b.visualID, _root.transform, bx + 90, by + gh - 40, gw - 210, 34, name, ColBorder, 19, FontStyles.Bold);
                MakeText("BStat_" + b.visualID, _root.transform, bx + gw - 110, by + gh - 40, 100, 34,
                    badgeUnlocked ? Translations.T("已解锁", "Unlocked") : Translations.T("未解锁", "Locked"),
                    badgeUnlocked ? ColUnlock : ColLocked, 14, FontStyles.Bold);
                MakeText("BDesc_" + b.visualID, _root.transform, bx + 14, by + 12, gw - 28, gh - 60, Translations.T("解锁条件：", "Unlock condition: ") + desc, ColText, 14, FontStyles.Normal);
                btn.onClick.AddListener(() => ShowBadgeDetail(b));
            }
            if (_pageLabel != null) _pageLabel.text = Translations.T("第 ", "Page ") + (_page + 1) + " / " + total + Translations.T(" 页  ·  徽章图鉴（共 ", " · Badge Gallery (") + badges.Count + Translations.T(" 个）", ")");
        }

        // ============ UGUI 工具 ============

        // ============ 圆角处理 ============
        private static void ApplyRoundedCorners(Image img, float radius = 16f)
        {
            if (_roundedSprite == null)
            {
                const int size = 64;
                int r = Mathf.RoundToInt(radius);
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        bool inside = true;
                        // 四个圆角外区域置透明
                        if (x < r && y < r) inside = Sqr(x - r) + Sqr(y - r) <= r * r;
                        else if (x >= size - r && y < r) inside = Sqr(x - (size - r)) + Sqr(y - r) <= r * r;
                        else if (x < r && y >= size - r) inside = Sqr(x - r) + Sqr(y - (size - r)) <= r * r;
                        else if (x >= size - r && y >= size - r) inside = Sqr(x - (size - r)) + Sqr(y - (size - r)) <= r * r;

                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, inside ? 1f : 0f));
                    }
                }
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.Apply();
                _roundedSprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, size, size),
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0,
                    SpriteMeshType.FullRect,
                    new Vector4(r, r, r, r));
            }
            img.sprite = _roundedSprite;
            img.type = Image.Type.Sliced;
        }

        private static int Sqr(int v) { return v * v; }

        // ============ 全屏 Canvas 基础处理 ============
        private static void StretchCanvas(GameObject canvasGo)
        {
            var rt = canvasGo.GetComponent<RectTransform>();
            if (rt == null) rt = canvasGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        private TextMeshProUGUI MakeText(string name, Transform parent, float x, float y, float w, float h,
            string text, Color color, float size, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = color;
            tmp.fontSize = Mathf.Clamp(size * Mathf.Clamp(UiScale, 0.8f, 1.3f), 8f, 80f);
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            var font = Font();
            if (font != null) tmp.font = font;
            if (_trackDynamic) _dynamic.Add(go);
            return tmp;
        }

        private Image MakeImage(string name, Transform parent, float x, float y, float w, float h, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            var img = go.AddComponent<Image>();
            img.color = color;
            if (_trackDynamic) _dynamic.Add(go);
            return img;
        }

        private RawImage MakeRawImage(string name, Transform parent, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            var raw = go.AddComponent<RawImage>();
            raw.color = Color.white;
            raw.raycastTarget = false;
            if (_trackDynamic) _dynamic.Add(go);
            return raw;
        }

        private Button MakeButton(string name, Transform parent, float x, float y, float w, float h,
            string text, Color bg, float size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            var img = go.AddComponent<Image>();
            img.color = bg;
            var btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            btn.targetGraphic = img;
            btn.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(1.25f, 1.25f, 1.35f, 1f),
                pressedColor = new Color(0.72f, 0.76f, 0.88f, 1f),
                selectedColor = new Color(1.12f, 1.12f, 1.18f, 1f),
                disabledColor = Color.grey,
                colorMultiplier = 1f,
                fadeDuration = 0.12f
            };
            MakeText("T_" + name, go.transform, 0, 0, w, h, text, Color.white, size, FontStyles.Bold).alignment = TextAlignmentOptions.Center;
            if (_trackDynamic) _dynamic.Add(go);
            return btn;
        }

        private void DestroyUI()
        {
            if (_root != null) { UnityEngine.Object.Destroy(_root); _root = null; }
            if (_detailLayer != null) { UnityEngine.Object.Destroy(_detailLayer); _detailLayer = null; }
            _dynamic.Clear();
            _tabButtons.Clear();
            _pageLabel = null;
            _trackDynamic = true;
        }
    }
}
