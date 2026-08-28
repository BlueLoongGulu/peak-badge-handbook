using System.Collections.Generic;

namespace PeakBadgeHandbook
{
    // 徽章条目（BadgeData 序列化）
    public class BadgeEntry
    {
        public int visualID;
        public string displayName;
        public string description;
        public string achievementType;   // ACHIEVEMENTTYPE 枚举名
        public bool secret;
        public bool testLocked;
        public string iconFile;          // 徽章图标 PNG（badge_<visualID>.png）
    }

    // 外观选项（CustomizationOption 序列化）
    public class CosmeticEntry
    {
        public string slot;              // skins/eyes/mouths/accessories/fits/hats/sashes/medals/special
        public int index;                // 数组内序号
        public string textureFile;       // PNG 文件名（textures/ 下）
        public string requiredAchievement;   // ACHIEVEMENTTYPE 枚举名
        public string requiredSteamStat;     // STEAMSTATTYPE 枚举名
        public int requiredSteamStatValue;
        public bool requiresAscent;
        public int requiredAscent;
        public string customRequirement;     // CUSTOMREQUIREMENT 枚举名
        public bool isBlank;
        public bool testLocked;
    }

    // 成就解锁需求（AchievementManager 的列表序列化）
    public class AchievementRequirementEntry
    {
        public string achievementType;   // ACHIEVEMENTTYPE
        public string kind;              // "steamstat" / "runbased"
        public string statType;          // STEAMSTATTYPE 或 RUNBASEDVALUETYPE
        public int requiredValue;
    }

    // 完整手册数据
    public class HandbookData
    {
        public List<BadgeEntry> badges = new List<BadgeEntry>();
        public List<CosmeticEntry> cosmetics = new List<CosmeticEntry>();
        public List<AchievementRequirementEntry> achievementRequirements = new List<AchievementRequirementEntry>();
    }
}
