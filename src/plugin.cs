using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PeakBadgeHandbook
{
    [BepInPlugin("peak-badge-handbook", "Peak Badge Handbook", "0.4.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance;
        internal static ManualLogSource Log;

        private ConfigEntry<KeyboardShortcut> _toggleKey;

        private HandbookUI _ui;
        private CursorLockMode _prevLock;
        private Harmony _harmony;
        private bool _pendingOpen;

        internal bool IsHandbookOpen => _ui != null && _ui.IsOpen;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            _toggleKey = Config.Bind("Handbook", "ToggleKey", new KeyboardShortcut(KeyCode.F6), "打开/关闭徽章手册（可改成键盘任意按键） / Toggle handbook (can bind any key)");

            DataProvider.DataDir = Paths.ConfigPath;
            DataProvider.TextureDir = Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location), "Textures");
            Localization.DetectAndPrepare(Paths.ConfigPath, Logger);
            DataProvider.Load(Paths.ConfigPath, Logger);
            DataProvider.Prewarm();
            _ui = new HandbookUI { Log = Logger };
            // ======== 维护提示 ========
            // 新增 UI 配置项：在这里 Bind 后赋值给 _ui 对应字段。
            // HandbookUI 内会自动 clamp 范围，避免字体/按钮溢出。
            // ==========================
            _ui.UiScale = Config.Bind("UI", "UiScale", 1f,
                "整体UI尺寸倍率 / Overall UI scale (0.8 - 1.3, default 1.0; panel/buttons/fonts scale together, panel clamped to 92% screen)").Value;

            _harmony = new Harmony("peak.badgehandbook.ui");
            _harmony.PatchAll();

            Logger.LogInfo("[BadgeHandbook] v0.4.2 已加载。按 " + _toggleKey.Value + " 打开徽章手册");
        }

        private void Update()
        {
            // 隐藏完整预热：等中文字体就绪后，提前生成所有手册文本。
            HandbookUI.PrewarmHiddenUI();

            // 手册打开时按 Esc 只关闭手册，不触发游戏原生 Esc 菜单。
            if (_ui != null && _ui.IsOpen && (Input.GetKeyDown(KeyCode.Escape) || IsEscapePressed()))
            {
                Close();
                return;
            }

            if (_toggleKey.Value.IsDown() || IsShortcutPressed())
            {
                if (_ui.IsOpen) Close();
                else _pendingOpen = true;
            }

            // 数据/导出尚未就绪时先记下打开请求，就绪后自动弹出，避免按前几次被吞掉。
            if (_pendingOpen && DataProvider.Ready && !_ui.IsOpen)
            {
                _pendingOpen = false;
                Open();
            }
        }

        private bool IsEscapePressed()
        {
            try
            {
                var keyboard = UnityEngine.InputSystem.Keyboard.current;
                return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
            }
            catch
            {
                return false;
            }
        }

        private bool IsShortcutPressed()
        {
            // 新输入系统兜底：部分版本/窗口状态下旧 Input 会漏掉首帧按下。
            try
            {
                var keyboard = UnityEngine.InputSystem.Keyboard.current;
                if (keyboard == null) return false;

                var key = ResolveInputKey(_toggleKey.Value.MainKey);
                if (!key.HasValue) return false;

                var ctrl = keyboard[key.Value];
                return ctrl != null && ctrl.wasPressedThisFrame;
            }
            catch
            {
                return false;
            }
        }

        private static UnityEngine.InputSystem.Key? ResolveInputKey(KeyCode keyCode)
        {
            if (keyCode >= KeyCode.Alpha0 && keyCode <= KeyCode.Alpha9)
                return (UnityEngine.InputSystem.Key)((int)UnityEngine.InputSystem.Key.Digit0 + (keyCode - KeyCode.Alpha0));
            if (keyCode >= KeyCode.Keypad0 && keyCode <= KeyCode.Keypad9)
                return (UnityEngine.InputSystem.Key)((int)UnityEngine.InputSystem.Key.Numpad0 + (keyCode - KeyCode.Keypad0));
            if (keyCode >= KeyCode.F1 && keyCode <= KeyCode.F12)
                return (UnityEngine.InputSystem.Key)((int)UnityEngine.InputSystem.Key.F1 + (keyCode - KeyCode.F1));

            if (System.Enum.TryParse<UnityEngine.InputSystem.Key>(keyCode.ToString(), out var result))
                return result;
            return null;
        }

        private void LateUpdate()
        {
            if (_ui == null || !_ui.IsOpen) return;

            // 游戏每帧可能强制锁光标/重新启用输入，这里每帧压住
            SetInput(false);
            if (Cursor.lockState != CursorLockMode.None)
                Cursor.lockState = CursorLockMode.None;
            if (!Cursor.visible)
                Cursor.visible = true;
        }

        internal void Open()
        {
            // 每次打开前重新检测一次游戏界面语言，避免 SettingsHandler 在插件 Awake 时尚未加载完成。
            Localization.DetectAndPrepare(Paths.ConfigPath, Logger);

            if (EventSystem.current == null)
            {
                var es = new GameObject("BadgeHandbookEventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            _ui.Open();
            if (_ui.IsOpen)
            {
                _prevLock = Cursor.lockState;
                SetInput(false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        internal void Close()
        {
            _pendingOpen = false;
            _ui.Close();
            SetInput(true);
            Cursor.lockState = _prevLock;
            Cursor.visible = false;
        }

        private static void SetInput(bool enabled)
        {
            try
            {
                var t = typeof(CharacterInput);
                foreach (var name in new[] { "action_move", "action_look", "action_jump", "action_sprint", "action_interact",
                                             "action_drop", "action_crouch", "action_usePrimary", "action_useSecondary", "action_scroll",
                                             "action_pause" })
                {
                    var f = t.GetField(name, BindingFlags.Public | BindingFlags.Static);
                    if (f?.GetValue(null) is UnityEngine.InputSystem.InputAction a)
                    {
                        if (enabled) { if (!a.enabled) a.Enable(); }
                        else { if (a.enabled) a.Disable(); }
                    }
                }
            }
            catch (System.Exception e)
            {
                Log?.LogWarning("[BadgeHandbook] 输入屏蔽异常: " + e.Message);
            }
        }
    }

    // 打开手册时清零本地角色输入，但保留 FixedUpdate/重力，避免角色浮空。
    [HarmonyPatch(typeof(CharacterInput), "Sample")]
    internal static class CharacterInputSamplePatch
    {
        private static void Postfix(CharacterInput __instance)
        {
            if (Plugin.Instance == null || !Plugin.Instance.IsHandbookOpen) return;
            __instance.movementInput = Vector2.zero;
            __instance.lookInput = Vector2.zero;
            __instance.sprintIsPressed = false;
            __instance.jumpIsPressed = false;
            __instance.interactIsPressed = false;
            __instance.usePrimaryIsPressed = false;
            __instance.useSecondaryIsPressed = false;
        }
    }
}
