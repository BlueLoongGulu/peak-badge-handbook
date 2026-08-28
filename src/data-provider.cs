using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace PeakBadgeHandbook
{
    // ============================================================
    // 数据提供者：加载内置/外部 JSON + 贴图，提供查询
    // 纯只读，供护照注入使用
    // ============================================================
    public static class DataProvider
    {
        public static bool Ready { get; private set; }
        public static string DataDir;
        public static string TextureDir;
        public static ManualLogSource Log;

        private static HandbookData _data;
        private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, CosmeticEntry> CosmeticIndex = new Dictionary<string, CosmeticEntry>();
        private static Dictionary<string, BadgeData> _badgeDataByAchievement;
        private static Dictionary<int, BadgeData> _badgeDataByVisual;
        private static Material _eyeMaterial;

        public static void Load(string dataDir, ManualLogSource log)
        {
            DataDir = dataDir;
            Log = log;
            Textures.Clear();
            CosmeticIndex.Clear();
            try
            {
                _data = new HandbookData();

                // 外部文件优先；但如果外部文件缺失/损坏/比 DLL 内嵌数据少，
                // 就回退到 DLL 内嵌完整默认数据，避免半成品导出导致徽章丢失。
                var externalBadges = LoadPart<List<BadgeEntry>>(Path.Combine(DataDir, "badges.json"));
                var embeddedBadges = JsonConvert.DeserializeObject<List<BadgeEntry>>(EmbeddedData.BadgesJson) ?? new List<BadgeEntry>();
                bool badgesUsable = externalBadges != null && externalBadges.Count >= embeddedBadges.Count && HasAnyIcon(externalBadges);
                _data.badges = badgesUsable ? externalBadges : embeddedBadges;

                var externalCosmetics = LoadPart<List<CosmeticEntry>>(Path.Combine(DataDir, "cosmetics.json"));
                var embeddedCosmetics = JsonConvert.DeserializeObject<List<CosmeticEntry>>(EmbeddedData.CosmeticsJson) ?? new List<CosmeticEntry>();
                bool cosmeticsUsable = externalCosmetics != null && externalCosmetics.Count >= embeddedCosmetics.Count && HasAnyTextureFile(externalCosmetics);
                _data.cosmetics = cosmeticsUsable ? externalCosmetics : embeddedCosmetics;

                var externalAchievements = LoadPart<List<AchievementRequirementEntry>>(Path.Combine(DataDir, "achievements.json"));
                var embeddedAchievements = JsonConvert.DeserializeObject<List<AchievementRequirementEntry>>(EmbeddedData.AchievementsJson) ?? new List<AchievementRequirementEntry>();
                _data.achievementRequirements = (externalAchievements != null && externalAchievements.Count >= embeddedAchievements.Count) ? externalAchievements : embeddedAchievements;

                string texDir = string.IsNullOrEmpty(TextureDir) ? Path.Combine(DataDir, "textures") : TextureDir;
                foreach (var c in _data.cosmetics)
                {
                    CosmeticIndex[Key(c.slot, c.index)] = c;
                    if (!string.IsNullOrEmpty(c.textureFile)) LoadTexture(Path.Combine(texDir, c.textureFile), c.textureFile);
                }
                foreach (var b in _data.badges)
                    if (!string.IsNullOrEmpty(b.iconFile)) LoadTexture(Path.Combine(texDir, b.iconFile), b.iconFile);

                Ready = _data.badges.Count > 0 || _data.cosmetics.Count > 0;
                Log.LogInfo($"[BadgeHandbook] 数据加载完成: {_data.badges.Count} 徽章 / {_data.cosmetics.Count} 外观");
            }
            catch (Exception e)
            {
                Log.LogError("[BadgeHandbook] 数据加载失败: " + e);
            }
        }

        // 预热：提前构建徽章数据缓存/眼睛材质，减少首次翻页卡顿。
        public static void Prewarm()
        {
            try { FindBadgeData(new BadgeEntry { achievementType = "NONE" }); } catch { }
        }

        public static string Key(string slot, int index) => slot + "_" + index;

        public static CosmeticEntry FindCosmetic(string slot, int index)
        {
            if (!Ready) return null;
            CosmeticIndex.TryGetValue(Key(slot, index), out var c);
            return c;
        }

        public static Texture2D GetTexture(string file) => Textures.TryGetValue(file, out var t) ? t : null;

        // 直接读取游戏内徽章图标，优先于 PNG 缓存。
        public static Texture GetBadgeTexture(BadgeEntry b)
        {
            if (b == null) return null;
            var data = FindBadgeData(b);
            if (data != null && data.icon != null)
                ForceMaxMipmap(data.icon);
            return data != null ? data.icon : null;
        }

        // 直接读取游戏内外观贴图，优先于 PNG 缓存。
        public static Color GetCosmeticColor(CosmeticEntry c)
        {
            if (c == null) return Color.white;
            var custom = Customization.Instance;
            if (custom == null) return Color.white;

            CustomizationOption[] arr = null;
            switch (c.slot)
            {
                case "fits": arr = custom.fits; break;
                case "hats": arr = custom.hats; break;
                case "sashes": arr = custom.sashes; break;
                case "medals": arr = custom.medals; break;
                case "accessories": arr = custom.accessories; break;
                case "skins": arr = custom.skins; break;
                case "eyes": arr = custom.eyes; break;
                case "mouths": arr = custom.mouths; break;
            }
            if (arr == null || c.index < 0 || c.index >= arr.Length) return Color.white;
            var option = arr[c.index];
            return option != null ? option.color : Color.white;
        }

        // 获取游戏内护照界面用于眼睛的专用材质，从而让通道图按游戏内管线渲染。
        public static Material GetEyeMaterial()
        {
            if (_eyeMaterial != null) return _eyeMaterial;
            try
            {
                var manager = PassportManager.instance;
                if (manager != null && manager.buttons != null)
                {
                    foreach (var b in manager.buttons)
                    {
                        if (b != null && b.eyeMaterial != null)
                        {
                            _eyeMaterial = b.eyeMaterial;
                            return _eyeMaterial;
                        }
                    }
                }

                foreach (var b in Resources.FindObjectsOfTypeAll<PassportButton>())
                {
                    if (b != null && b.eyeMaterial != null)
                    {
                        _eyeMaterial = b.eyeMaterial;
                        return _eyeMaterial;
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        public static Texture GetCosmeticTexture(CosmeticEntry c)
        {
            if (c == null) return null;
            var custom = Customization.Instance;
            if (custom == null) return null;

            CustomizationOption[] arr = null;
            switch (c.slot)
            {
                case "fits": arr = custom.fits; break;
                case "hats": arr = custom.hats; break;
                case "sashes": arr = custom.sashes; break;
                case "medals": arr = custom.medals; break;
                case "accessories": arr = custom.accessories; break;
                case "skins": arr = custom.skins; break;
                case "eyes": arr = custom.eyes; break;
                case "mouths": arr = custom.mouths; break;
            }
            if (arr == null || c.index < 0 || c.index >= arr.Length) return null;
            var option = arr[c.index];
            if (option != null && option.texture != null)
                ForceMaxMipmap(option.texture);
            return option != null ? option.texture : null;
        }

        // 只让 RawImage 显示该纹理时忽略 mipmap 限制，强制最高分辨率，不修改全局画质设置。
        private static void ForceMaxMipmap(Texture tex)
        {
            var t2 = tex as Texture2D;
            if (t2 == null) return;
            try
            {
                t2.ignoreMipmapLimit = true;
                t2.requestedMipmapLevel = 0;
            }
            catch
            {
                // 某些纹理不可设置时保持原样
            }
        }

        private static BadgeData FindBadgeData(BadgeEntry b)
        {
            if (_badgeDataByAchievement == null)
            {
                _badgeDataByAchievement = new Dictionary<string, BadgeData>();
                _badgeDataByVisual = new Dictionary<int, BadgeData>();
                foreach (var data in Resources.FindObjectsOfTypeAll<BadgeData>())
                {
                    if (data == null) continue;
                    if (!_badgeDataByAchievement.ContainsKey(data.linkedAchievement.ToString()))
                        _badgeDataByAchievement[data.linkedAchievement.ToString()] = data;
                    if (!_badgeDataByVisual.ContainsKey(data.visualID))
                        _badgeDataByVisual[data.visualID] = data;
                }
            }

            if (!string.IsNullOrEmpty(b.achievementType) && _badgeDataByAchievement.TryGetValue(b.achievementType, out var byType))
                return byType;
            if (_badgeDataByVisual.TryGetValue(b.visualID, out var byVisual))
                return byVisual;
            return null;
        }

        public static List<CosmeticEntry> AllCosmetics() => _data?.cosmetics ?? new List<CosmeticEntry>();
        public static List<BadgeEntry> AllBadges() => _data?.badges ?? new List<BadgeEntry>();

        public static string BadgeName(string achievementType) => Translations.BadgeName(achievementType);
        public static string BadgeDesc(string achievementType) => Translations.BadgeDesc(achievementType);

        public static bool IsDefaultUnlock(CosmeticEntry c)
        {
            if (c == null) return true;
            return (c.requiredAchievement == null || c.requiredAchievement == "NONE")
                && (c.requiredSteamStat == null || c.requiredSteamStat == "NONE")
                && !c.requiresAscent
                && (c.customRequirement == null || c.customRequirement == "None");
        }

        // 查询游戏当前是否已解锁该外观（只读，不修改任何进度）。
        // 只读检查徽章是否已在游戏中解锁。
        public static bool IsBadgeUnlocked(string achievementType)
        {
            if (string.IsNullOrEmpty(achievementType) || achievementType == "NONE")
                return false;

            try
            {
                var enumValue = (ACHIEVEMENTTYPE)Enum.Parse(typeof(ACHIEVEMENTTYPE), achievementType);
                var manager = AchievementManager.Instance;
                return manager != null && manager.IsAchievementUnlocked(enumValue);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsCosmeticUnlocked(CosmeticEntry c)
        {
            if (c == null) return false;
            if (IsDefaultUnlock(c)) return true;

            var custom = Customization.Instance;
            if (custom == null) return false;

            CustomizationOption[] arr = null;
            switch (c.slot)
            {
                case "fits": arr = custom.fits; break;
                case "hats": arr = custom.hats; break;
                case "sashes": arr = custom.sashes; break;
                case "medals": arr = custom.medals; break;
                case "accessories": arr = custom.accessories; break;
                case "skins": arr = custom.skins; break;
                case "eyes": arr = custom.eyes; break;
                case "mouths": arr = custom.mouths; break;
            }

            if (arr == null || c.index < 0 || c.index >= arr.Length) return false;
            var option = arr[c.index];
            return option != null && !option.IsLocked;
        }

        // 通道图（眼睛/嘴巴）转白色蒙版：非透明像素染白，保留 alpha —— 与游戏内显示一致
        public static Texture2D GetPreviewTexture(string slot, string file)
        {
            if (string.IsNullOrEmpty(file)) return null;
            if ((slot == "eyes" || slot == "mouths") && Textures.TryGetValue("white_" + file, out var white))
                return white;
            return GetTexture(file);
        }

        public static string UnlockText(CosmeticEntry c)
        {
            if (c == null) return "";
            if (c.isBlank) return Translations.T("（空白占位，无实际外观）", "(Blank placeholder, no actual appearance)");
            if (c.testLocked) return Translations.T("（测试锁定物品）", "(Test-locked item)");
            var parts = new List<string>();
            if (c.requiredAchievement != null && c.requiredAchievement != "NONE")
            {
                parts.Add(Translations.T("◆ 需要徽章：「", "- Requires badge: ") + Translations.BadgeName(c.requiredAchievement) + Translations.T("」", ""));
                string d = Translations.BadgeDesc(c.requiredAchievement);
                if (!string.IsNullOrEmpty(d)) parts.Add(Translations.T("   徽章条件: ", "   Badge condition: ") + d);
            }
            if (c.requiredSteamStat != null && c.requiredSteamStat != "NONE")
                parts.Add(Translations.T("◆ Steam统计: ", "- Steam stat: ") + Translations.StatName(c.requiredSteamStat) + Translations.T(" 达到 ", " reach ") + c.requiredSteamStatValue);
            if (c.requiresAscent)
                parts.Add(Translations.T("◆ 攀登次数: ", "- Ascents: ") + c.requiredAscent);
            if (c.customRequirement != null && c.customRequirement != "None")
            {
                if (c.customRequirement == "Goat")
                    parts.Add(Translations.T("◆ 特殊条件: 最大攀登次数达到8（山羊帽）", "- Special: Reach max ascent 8 (Goat hat)"));
                else if (c.customRequirement == "Crown")
                    parts.Add(Translations.T("◆ 特殊条件: 完成全部基础徽章（皇冠帽）", "- Special: Unlock all base badges (Crown hat)"));
                else
                    parts.Add(Translations.T("◆ 特殊条件: ", "- Special: ") + c.customRequirement);
            }
            if (parts.Count == 0) return Translations.T("默认解锁（无需任何条件）", "Unlocked by default (no requirements)");
            return string.Join("\n", parts);
        }

        public static string BadgeNameFor(string achievementType) => Translations.BadgeName(achievementType);
        public static string BadgeDescFor(string achievementType) => Translations.BadgeDesc(achievementType);

        // Customization.Type 枚举值 -> 部位名
        public static string SlotFromType(int typeValue)
        {
            switch (typeValue)
            {
                case 0: return "skins";
                case 10: return "accessories";
                case 20: return "eyes";
                case 30: return "mouths";
                case 40: return "fits";
                case 50: return "hats";
                case 60: return "sashes";
                case 70: return "medals";
                default: return null;
            }
        }

        private static bool HasAnyIcon(List<BadgeEntry> list)
        {
            if (list == null) return false;
            foreach (var b in list)
                if (b != null && !string.IsNullOrEmpty(b.iconFile)) return true;
            return false;
        }

        private static bool HasAnyTextureFile(List<CosmeticEntry> list)
        {
            if (list == null) return false;
            foreach (var c in list)
                if (c != null && !string.IsNullOrEmpty(c.textureFile)) return true;
            return false;
        }

        private static T LoadPart<T>(string path) where T : class
        {
            try
            {
                if (!File.Exists(path)) return null;
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            }
            catch
            {
                return null; // 文件损坏时走 DLL 内嵌默认数据
            }
        }

        private static void LoadTexture(string path, string key)
        {
            if (!File.Exists(path)) return;
            var bytes = File.ReadAllBytes(path);
            var t2d = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!t2d.LoadImage(bytes)) return;
            Textures[key] = t2d;

            // 通道图（眼睛/嘴巴）额外生成白色蒙版：非透明像素染白，保留 alpha
            if (key.StartsWith("eyes_") || key.StartsWith("mouths_"))
            {
                try
                {
                    var pixels = t2d.GetPixels32();
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        if (pixels[i].a > 10) pixels[i] = new Color32(255, 255, 255, pixels[i].a);
                        else pixels[i] = new Color32(0, 0, 0, 0);
                    }
                    var white = new Texture2D(t2d.width, t2d.height, TextureFormat.RGBA32, false);
                    white.SetPixels32(pixels);
                    white.Apply();
                    Textures["white_" + key] = white;
                }
                catch { }
            }
        }
    }
}
