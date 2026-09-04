# Changelog

---

## v1.0.2

### 中文

- 无修复改动：仅修改部分注释内容，与 v1.0.1 功能一致

### English

- No functional changes: only comment/documentation cleanup. Functionally identical to v1.0.1.

---

## v1.0.1

### 中文

- 适配游戏 2.4.b 更新：修复游戏更新后模组无法加载的问题（F6 无响应）
- 跟随游戏新版本调整语言检测逻辑（本地化数据版本更新为 2.4.b）

### English

- Compatible with game update 2.4.b: fixed mod failing to load after the game update (F6 unresponsive)
- Adjusted language detection for the new game version (localization data version updated to 2.4.b)

---

## v1.0.0

### 中文

- 首个正式发布：F6 独立徽章手册 UI
- 徽章图鉴：64 个徽章的名称 / 解锁条件 / 额外提示 / 解锁状态 / 详情弹窗
- 外观图鉴：衣服 / 帽子 / 饰带 / 奖章 / 饰品 / 眼睛 / 嘴巴 七大栏位
- 每个栏位独立记忆翻页位置
- 中英文界面自动切换（跟随游戏语言）
- 高清贴图优先，缺失时回退游戏内贴图
- 内置 JSON 数据兜底（外部 JSON 缺失/损坏时自动重建）
- UiScale 整体缩放调节
- 启动隐藏预热，避免首次打开卡顿

### English

- First official release: standalone handbook UI toggled with F6
- Badge gallery: all 64 badges with names / unlock conditions / extra tips / unlock status / detail popup
- Cosmetic gallery: seven slots — Fits / Hats / Sashes / Medals / Accessories / Eyes / Mouths
- Each slot remembers its own page position
- UI language auto-switches between Chinese and English (follows game language)
- HD textures preferred, falls back to in-game textures when missing
- Built-in JSON data as fallback (auto-rebuilds when external JSON is missing or corrupted)
- UiScale for adjusting overall UI size
- Hidden prewarm at startup to avoid first-open lag