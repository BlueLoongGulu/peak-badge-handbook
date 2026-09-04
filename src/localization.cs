using System.IO;
using BepInEx.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace PeakBadgeHandbook
{
    // ============================================================
    // 语言检测 + 本地化 JSON 缓存 + 版本校验覆盖
    // 启动流程：
    //   1. 检测游戏界面语言
    //   2. 确保当前语言的外部 JSON 存在且版本匹配（缺失/损坏/旧版则用 DLL 内嵌默认值重建）
    //   3. 加载外部 JSON 覆盖翻译（版本匹配时允许玩家/维护者改 JSON）
    // 文件直接放在 BepInEx/config/ 下：
    //   peak-badge-handbook.json          = English
    //   peak-badge-handbook.zh-CN.json    = Simplified Chinese
    // 只生成当前检测到的语言文件；切语言后再按需生成另一份。
    // ============================================================
    public static class Localization
    {
        public static void DetectAndPrepare(string dataDir, ManualLogSource log)
        {
            try
            {
                // 游戏 2.4.b 更新：LanguageSetting.Language 嵌套枚举已被移除，
                // 语言全局状态改用 LocalizedText.CURRENT_LANGUAGE（public static Language 字段）。
                // ⚠️ 插件 Awake 早期游戏设置系统可能尚未加载（SettingsHandler.Instance == null），
                // 此时 LocalizedText.CURRENT_LANGUAGE 还是默认值（English），直接读会误判语言，
                // 导致中文界面玩家走英文预热路径、中文字形没预生成 → F6 首次打开卡顿（交付文档 7.12）。
                // 因此仅在 SettingsHandler 就绪时才信任 CURRENT_LANGUAGE，否则按系统语言兜底。
                var settings = SettingsHandler.Instance;
                if (settings != null)
                {
                    var lang = LocalizedText.CURRENT_LANGUAGE;
                    if (lang == LocalizedText.Language.SimplifiedChinese || lang == LocalizedText.Language.TraditionalChinese)
                    {
                        Translations.CurrentLanguage = "zh-CN";
                    }
                    else
                    {
                        Translations.CurrentLanguage = "en";
                    }
                }
                else
                {
                    // 游戏设置系统未加载（插件 Awake 早期）：按系统语言兜底
                    if (Application.systemLanguage == SystemLanguage.ChineseSimplified ||
                        Application.systemLanguage == SystemLanguage.ChineseTraditional)
                        Translations.CurrentLanguage = "zh-CN";
                    else
                        Translations.CurrentLanguage = "en";
                }
            }
            catch
            {
                // 读取失败时按系统语言兜底
                if (Application.systemLanguage == SystemLanguage.ChineseSimplified ||
                    Application.systemLanguage == SystemLanguage.ChineseTraditional)
                    Translations.CurrentLanguage = "zh-CN";
                else
                    Translations.CurrentLanguage = "en";
            }

            log?.LogInfo("[BadgeHandbook] 检测到界面语言: " + Translations.CurrentLanguage);
            EnsureSelectedJson(dataDir, log);
            LoadSelectedJson(dataDir, log);
        }

        private static void EnsureSelectedJson(string dataDir, ManualLogSource log)
        {
            try
            {
                string dir = dataDir;
                Directory.CreateDirectory(dir);

                string file = Translations.IsChinese ? "peak-badge-handbook.zh-CN.json" : "peak-badge-handbook.json";
                string path = Path.Combine(dir, file);
                string currentVersion = ReadVersion(path);

                // 缺失、损坏、版本落后：用 DLL 内嵌默认值重建
                if (currentVersion != Translations.DataVersion)
                {
                    WriteSelectedJson(path);
                    log?.LogInfo("[BadgeHandbook] 已生成/重建本地化文件: " + path + " (version=" + Translations.DataVersion + ")");
                }
            }
            catch (System.Exception e)
            {
                log?.LogWarning("[BadgeHandbook] 本地化 JSON 生成失败: " + e.Message);
            }
        }

        private static void WriteSelectedJson(string path)
        {
            if (Translations.IsChinese)
            {
                var zh = new
                {
                    version = Translations.DataVersion,
                    badges = Translations.Badges,
                    extras = Translations.BadgeExtras,
                    slots = Translations.Slots
                };
                File.WriteAllText(path, JsonConvert.SerializeObject(zh, Formatting.Indented));
            }
            else
            {
                var en = new
                {
                    version = Translations.DataVersion,
                    badges = Translations.BadgesEn,
                    extras = Translations.BadgeExtrasEn,
                    slots = Translations.SlotsEn
                };
                File.WriteAllText(path, JsonConvert.SerializeObject(en, Formatting.Indented));
            }
        }

        private static void LoadSelectedJson(string dataDir, ManualLogSource log)
        {
            try
            {
                string dir = dataDir;
                string file = Translations.IsChinese ? "peak-badge-handbook.zh-CN.json" : "peak-badge-handbook.json";
                string path = Path.Combine(dir, file);
                if (!File.Exists(path)) return;

                var root = JObject.Parse(File.ReadAllText(path));
                string version = root["version"]?.ToString();
                if (version != Translations.DataVersion)
                    return; // 版本不匹配：保留 DLL 内嵌默认值，下次启动会重建

                var badges = root["badges"] as JObject;
                var extras = root["extras"] as JObject;
                var slots = root["slots"] as JObject;

                if (Translations.IsChinese)
                {
                    Translations.Badges.Clear();
                    Translations.BadgeExtras.Clear();
                    Translations.Slots.Clear();
                    LoadStringArrayDict(badges, Translations.Badges);
                    LoadStringDict(extras, Translations.BadgeExtras);
                    LoadStringDict(slots, Translations.Slots);
                }
                else
                {
                    Translations.BadgesEn.Clear();
                    Translations.BadgeExtrasEn.Clear();
                    Translations.SlotsEn.Clear();
                    LoadStringArrayDict(badges, Translations.BadgesEn);
                    LoadStringDict(extras, Translations.BadgeExtrasEn);
                    LoadStringDict(slots, Translations.SlotsEn);
                }

                log?.LogInfo("[BadgeHandbook] 已加载外部本地化 JSON: " + path);
            }
            catch (System.Exception e)
            {
                log?.LogWarning("[BadgeHandbook] 本地化 JSON 读取失败，使用 DLL 内嵌默认值: " + e.Message);
            }
        }

        private static string ReadVersion(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                var root = JObject.Parse(File.ReadAllText(path));
                return root["version"]?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static void LoadStringArrayDict(JObject source, System.Collections.Generic.Dictionary<string, string[]> target)
        {
            if (source == null) return;
            foreach (var pair in source)
            {
                if (pair.Value is JArray arr && arr.Count >= 2)
                    target[pair.Key] = new[] { arr[0].ToString(), arr[1].ToString() };
            }
        }

        private static void LoadStringDict(JObject source, System.Collections.Generic.Dictionary<string, string> target)
        {
            if (source == null) return;
            foreach (var pair in source)
            {
                if (pair.Value != null)
                    target[pair.Key] = pair.Value.ToString();
            }
        }
    }
}
