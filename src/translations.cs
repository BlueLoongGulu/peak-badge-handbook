using System.Collections.Generic;

namespace PeakBadgeHandbook
{
    // ======== 维护提示 ========
    // 1. 修改徽章中文名/条件：编辑 Badges 字典。
    // 2. 修改额外获取提示：编辑 BadgeExtras 字典。
    // 3. 新增徽章类型时，同步在 Badges 和 BadgeExtras 中补一条。
    // 4. 英文版将来可平行维护一套 BadgesEn/BadgeExtrasEn。
    // ==========================
    // 徽章中文翻译表：achievementType -> (中文名, 中文解锁条件)
    // 翻译依据：badges.json 导出的英文 displayName/description（游戏本体数据）
    public static class Translations
    {
        // ======== 数据版本维护 ========
        // 当前依据：Steam 上显示的“补丁 2.03.a”（本地游戏文件未直接查到补丁号）。
        // 每次游戏更新并重新提取数据/发布新模组时，请手动把 DataVersion 改成新补丁号。
        // 标题显示的是“本手册数据基于哪个版本”，而不是“当前游戏版本”，
        // 避免游戏已更新但模组没更新时误导玩家。
        // ==============================
        public const string DataVersion = "2.4.b";

        // 当前语言：由 Localization 在启动时根据游戏界面语言设置。
        // zh-CN = 简体中文，en = English
        public static string CurrentLanguage = "zh-CN";
        public static bool IsChinese => CurrentLanguage == "zh-CN";
        public static readonly Dictionary<string, string[]> Badges = new Dictionary<string, string[]>
        {
            { "TriedYourBestBadge",       new[]{"参与", "让朋友在你不在场的情况下逃离岛屿" } },
            { "EnduranceBadge",           new[]{"耐力", "不接触地面，连续向上攀爬50米" } },
            { "NomadBadge",               new[]{"游牧者", "翻越方山（MESA）" } },
            { "UltimateBadge",            new[]{"极限奖章", "从100米外接住飞盘（代码判定为100米）" } },
            { "CoolCucumberBadge",        new[]{"冷静黄瓜", "热度从未超过10%的情况下翻越方山（MESA）" } },
            { "NeedlepointBadge",         new[]{"针刺", "身上扎满许多仙人掌刺" } },
            { "AeronauticsBadge",         new[]{"飞行", "实现飞行" } },
            { "TwentyFourKaratBadge",     new[]{"24K金", "向熔炉献上值得的祭品" } },
            { "ResourcefulnessBadge",     new[]{"足智多谋", "向饥饿屈服" } },
            { "DaredevilBadge",           new[]{"冒险家", "用侦察兵加农炮射过方山（MESA）峡谷" } },
            { "MegaentomologyBadge",      new[]{"巨型昆虫学", "在蚁狮的攻击中存活" } },
            { "AstronomyBadge",           new[]{"天文学", "太过近距离地注视炽热的太阳" } },
            { "BundledUpBadge",           new[]{"御寒", "寒冷值从未超过20%的情况下翻越雪山（ALPINE）" } },
            { "ForestryBadge",            new[]{"林业", "翻越森蕈（ROOTS）" } },
            { "TreadLightlyBadge",        new[]{"轻手轻脚", "孢子值从未超过25%的情况下翻越森蕈（ROOTS）" } },
            { "WebSecurityBadge",         new[]{"网络安全", "被蜘蛛网缠住后成功挣脱存活" } },
            { "UndeadEncounterBadge",     new[]{"亡灵遭遇", "治愈自己的僵尸咬伤" } },
            { "AdvancedMycologyBadge",    new[]{"高级真菌学", "在一次攀登中尝试全部5种蘑菇莓" } },
            { "DisasterResponseBadge",    new[]{"灾难响应抓钩", "使用救援抓钩将一位昏迷朋友拖行30米" } },
            { "CalciumIntakeBadge",       new[]{"补钙", "用奶白金累计格挡100点伤害" } },
            { "CompetitiveEatingBadge",   new[]{"大胃王", "吃下5根热狗" } },
            { "AppliedEsotericaBadge",    new[]{"应用神秘学", "用非神圣的手段复活队友" } },
            { "MycoacrobaticsBadge",      new[]{"菌类杂技", "借助蘑菇弹跳飞起" } },
            { "CryptogastronomyBadge",    new[]{"神秘烹饪", "烹饪并吃掉一株曼德拉草" } },
            { "WandererBadge",            new[]{"漫游者", "翻越雾沼（GLOOM）" } },
            { "BellringerBadge",          new[]{"敲钟人", "单次攀登中敲响雾沼（GLOOM）的5座钟楼" } },
            { "WellRestedBadge",          new[]{"休息充分", "食用「早起的虫儿」" } },
            { "JesterBadge",              new[]{"小丑", "单次攀登中开启3个小丑行李箱" } },
            { "HangGlidingBadge",         new[]{"滑翔", "用滑翔翼不接触地面飞行100米" } },
            { "MedievalHistoryBadge",     new[]{"中世纪历史", "未被任何陷阱击中翻越城塞（THE CITADEL）" } },
            { "LastResortBadge",          new[]{"最后手段", "用仪式匕首一次性治疗超过75%的伤害" } },
            { "ExorcistBadge",            new[]{"驱魔人", "在雾沼（GLOOM）点亮烛台或钟塔驱散大幽灵（20米内判定）" } },
            { "ArcheryBadge",             new[]{"箭术", "单次攀登中拔掉自己身上10根箭" } },
            { "RuleZeroBadge",            new[]{"规则零", "解放童军领队的灵魂" } },
            { "AlpinistBadge",            new[]{"登山家", "翻越雪山（ALPINE）" } },
            { "AnimalSerenadingBadge",    new[]{"动物小夜曲", "为水豚吹奏军号" } },
            { "ArboristBadge",            new[]{"树艺师", "登上一棵巨树的顶端" } },
            { "AscenderBadge",            new[]{"高空", "累计攀爬5000米" } },
            { "BalloonBadge",             new[]{"气球", "不受坠落伤害逃离岛屿" } },
            { "BeachcomberBadge",         new[]{"海滩拾荒者", "翻越海岸（SHORE）" } },
            { "BingBongBadge",            new[]{"宾邦", "帮助宾邦逃离岛屿" } },
            { "BookwormBadge",            new[]{"书虫", "阅读童军领队迈尔斯的全部日记" } },
            { "BoulderingBadge",          new[]{"攀岩", "放置10个岩钉" } },
            { "ClutchBadge",              new[]{"关键先生", "单次远征中复活3名侦察兵" } },
            { "CookingBadge",             new[]{"烹饪", "在篝火处烹饪20餐" } },
            { "EmergencyPreparednessBadge", new[]{"应急准备", "用物品治疗昏迷队友，将其从死亡边缘救回" } },
            { "EsotericaBadge",           new[]{"神秘学", "获得一件神秘物品" } },
            { "FirstAidBadge",            new[]{"急救", "单次远征中累计治疗队友100点伤势" } },
            { "ForagingBadge",            new[]{"觅食", "单次远征中吃下5种不同浆果" } },
            { "GourmandBadge",            new[]{"美食家", "烹饪并吃下半颗椰子、一块蜂巢、一颗黄雪莓和一枚鸡蛋后逃离岛屿" } },
            { "HappyCamperBadge",         new[]{"快乐露营者", "从篝火获得5次士气鼓舞" } },
            { "KnotTyingBadge",           new[]{"打结", "单次远征中放置100米绳索" } },
            { "LeaveNoTraceBadge",        new[]{"不留痕迹", "不在山上放置任何物品逃离岛屿" } },
            { "LoneWolfBadge",            new[]{"独狼", "单人远征中逃离岛屿" } },
            { "MentorshipBadge",          new[]{"导师", "与童军领队进行一对一对话" } },
            { "MycologyBadge",            new[]{"真菌学", "单次远征中吃下4种不同的无毒蘑菇" } },
            { "NaturalistBadge",          new[]{"自然主义者", "不吃任何包装食品逃离岛屿" } },
            { "PeakBadge",                new[]{"峰顶", "登上峰顶（PEAK）" } },
            { "PlundererBadge",           new[]{"掠夺者", "单次远征中开启15个行李箱" } },
            { "SpeedClimberBadge",        new[]{"仓促", "在一小时内逃离岛屿" } },
            { "SurvivalistBadge",         new[]{"生存专家", "从未失去意识的情况下逃离岛屿" } },
            { "ToxicologyBadge",          new[]{"毒理学", "用物品累计恢复200点中毒" } },
            { "TrailblazerBadge",         new[]{"开拓者", "翻越雨林（TROPICS）" } },
            { "VolcanologyBadge",         new[]{"火山学", "翻越火山（CALDERA）" } },
            { "Ascent1", new[]{"第1次登顶", "完成第1次登顶" } },
            { "Ascent2", new[]{"第2次登顶", "完成第2次登顶" } },
            { "Ascent3", new[]{"第3次登顶", "完成第3次登顶" } },
            { "Ascent4", new[]{"第4次登顶", "完成第4次登顶" } },
            { "Ascent5", new[]{"第5次登顶", "完成第5次登顶" } },
            { "Ascent6", new[]{"第6次登顶", "完成第6次登顶" } },
            { "Ascent7", new[]{"第7次登顶", "完成第7次登顶" } },
            { "Ascent8", new[]{"第8次登顶", "完成第8次登顶" } },
        };
        // English 原版徽章描述（来自游戏本体 badges.json）
        public static readonly Dictionary<string, string[]> BadgesEn = new Dictionary<string, string[]>
        {
            { "TriedYourBestBadge", new[]{"Participation", "Have a friend escape the island without you."} },
            { "EnduranceBadge", new[]{"Endurance", "Climb 50m upwards without touching the ground."} },
            { "NomadBadge", new[]{"Nomad", "Climb past the MESA."} },
            { "UltimateBadge", new[]{"Ultimate", "Catch a Flying Disc from 50m away."} },
            { "CoolCucumberBadge", new[]{"Cool Cucumber", "Climb past the MESA without ever having more than 10% Heat."} },
            { "NeedlepointBadge", new[]{"Needlepoint", "Have a lot of cactuses stuck to you."} },
            { "AeronauticsBadge", new[]{"Aeronautics", "Achieve flight."} },
            { "TwentyFourKaratBadge", new[]{"24 Karat", "Offer The Kiln a worthy sacrifice."} },
            { "ResourcefulnessBadge", new[]{"Resourcefulness", "Give in to your hunger."} },
            { "DaredevilBadge", new[]{"Daredevil", "Shoot across the MESA canyon in a Scout Cannon."} },
            { "MegaentomologyBadge", new[]{"Megaentomology", "Survive an Antlion attack."} },
            { "AstronomyBadge", new[]{"Astronomy", "Look a little too closely at the blazing sun."} },
            { "BundledUpBadge", new[]{"Bundled Up", "Climb past the ALPINE without ever having more than 20% Cold."} },
            { "ForestryBadge", new[]{"Forestry", "Climb past the ROOTS."} },
            { "TreadLightlyBadge", new[]{"Tread Lightly", "Climb past the ROOTS without ever having more than 25% spores."} },
            { "WebSecurityBadge", new[]{"Web Security", "Survive getting caught in a spider's web."} },
            { "UndeadEncounterBadge", new[]{"Undead Encounter", "Cure yourself from a zombie bite."} },
            { "AdvancedMycologyBadge", new[]{"Advanced Mycology", "Try all 5 types of Shroomberry in a run."} },
            { "DisasterResponseBadge", new[]{"Disaster Response", "Pull a passed out friend Xm with the Rescue Claw."} },
            { "CalciumIntakeBadge", new[]{"Calcium Intake", "Block 100 total damage with the Fortified Milk."} },
            { "CompetitiveEatingBadge", new[]{"Competitive Eating", "Eat 5 Hot Dogs."} },
            { "AppliedEsotericaBadge", new[]{"Applied Esoterica", "Ressurect a friend using unholy means."} },
            { "MycoacrobaticsBadge", new[]{"Mycoacrobatics", "Bounce up Xm off a mushroom."} },
            { "CryptogastronomyBadge", new[]{"Cryptogastronomy", "Cook and eat a Mandrake."} },
            { "WandererBadge", new[]{"Wanderer", "Climb past the GLOOM."} },
            { "BellringerBadge", new[]{"Bellringer", "Ring 5 Belltowers in the Gloom in a single run."} },
            { "WellRestedBadge", new[]{"Well Rested", "Consume The Early Worm."} },
            { "JesterBadge", new[]{"Jester", "Open 3 Clown Luggage in one run."} },
            { "HangGlidingBadge", new[]{"Hang Gliding", "Fly 100m with the Glider without touching the ground."} },
            { "MedievalHistoryBadge", new[]{"Medieval History", "Climb past THE CITADEL without being hit by any traps."} },
            { "LastResortBadge", new[]{"Last Resort", "Heal over 75% damage at once by using the Ritual Dagger."} },
            { "ExorcistBadge", new[]{"Exorcist", "Burn up the Big Ghost in the GLOOM."} },
            { "ArcheryBadge", new[]{"Archery", "Remove 10 arrows from yourself in one run."} },
            { "RuleZeroBadge", new[]{"Rule Zero", "Free the Scoutmaster's soul."} },
            { "AlpinistBadge", new[]{"Alpinist", "Climb past the ALPINE."} },
            { "AnimalSerenadingBadge", new[]{"Animal Serenading", "Play the bugle for a capybara."} },
            { "ArboristBadge", new[]{"Arborist", "Reach the top of a really big tree."} },
            { "AscenderBadge", new[]{"High Altitude", "Climb 5000m total."} },
            { "BalloonBadge", new[]{"BalloonBadge", "Escape the island without taking fall damage."} },
            { "BeachcomberBadge", new[]{"Beachcomber", "Climb past the SHORE."} },
            { "BingBongBadge", new[]{"BingBongBadge", "Help Bing Bong escape the island."} },
            { "BookwormBadge", new[]{"Bookworm", "Read all of Scoutmaster Myres's journal entries."} },
            { "BoulderingBadge", new[]{"Bouldering", "Place 10 pitons."} },
            { "ClutchBadge", new[]{"Clutch", "Resurrect 3 scouts in a single expedition. "} },
            { "CookingBadge", new[]{"Cooking", "Cook 20 meals at campfires."} },
            { "EmergencyPreparednessBadge", new[]{"Emergency Preparedness", "Heal an unconscious friend with an item to save them from death."} },
            { "EsotericaBadge", new[]{"Esoterica", "Obtain a mystical item."} },
            { "FirstAidBadge", new[]{"First Aid", "Heal your friends for 100 points of injury in a single expedition."} },
            { "ForagingBadge", new[]{"Foraging", "Eat 5 different berries in a single expedition."} },
            { "GourmandBadge", new[]{"Gourmand", "Escape the island after cooking and eating a coconut half, a honeycomb, a yellow winterberry, and an egg."} },
            { "HappyCamperBadge", new[]{"Happy Camper", "Receive 5 Morale Boosts from campfires."} },
            { "KnotTyingBadge", new[]{"Knot Tying", "Place 100m of rope in a single expedition."} },
            { "LeaveNoTraceBadge", new[]{"Leave No Trace", "Escape the island without placing anything on the mountain."} },
            { "LoneWolfBadge", new[]{"Lone Wolf", "Escape the island in a solo expedition."} },
            { "MentorshipBadge", new[]{"Mentorship", "Have a 1-on-1 with the Scoutmaster."} },
            { "MycologyBadge", new[]{"Mycology", "Eat four different non-toxic mushrooms in a single expedition."} },
            { "NaturalistBadge", new[]{"Naturalist", "Escape the island without eating any packaged food."} },
            { "PeakBadge", new[]{"Peakbadge", "Reach the PEAK."} },
            { "PlundererBadge", new[]{"Plunderer", "Open 15 luggages in a single expedition."} },
            { "SpeedClimberBadge", new[]{"Hasty", "Escape the island in under an hour."} },
            { "SurvivalistBadge", new[]{"Survivalist", "Escape the island without ever losing consciousness."} },
            { "ToxicologyBadge", new[]{"Toxicology", "Restore 200 total poison by using items. "} },
            { "TrailblazerBadge", new[]{"Trailblazer", "Climb past the TROPICS."} },
            { "VolcanologyBadge", new[]{"Volcanology", "Climb past the CALDERA."} },
            { "Ascent1", new[]{"Ascent 1", "Complete Ascent 1."} },
            { "Ascent2", new[]{"Ascent 2", "Complete Ascent 2."} },
            { "Ascent3", new[]{"Ascent 3", "Complete Ascent 3."} },
            { "Ascent4", new[]{"Ascent 4", "Complete Ascent 4."} },
            { "Ascent5", new[]{"Ascent 5", "Complete Ascent 5."} },
            { "Ascent6", new[]{"Ascent 6", "Complete Ascent 6."} },
            { "Ascent7", new[]{"Ascent 7", "Complete Ascent 7."} },
            { "Ascent8", new[]{"Ascent 8", "Complete Ascent 8."} },
        };

        // 额外获取/隐藏条件提示（可后续扩展为英文版）
        public static readonly Dictionary<string, string> BadgeExtras = new Dictionary<string, string>
        {
            { "WellRestedBadge", "获取提示：早起的虫儿（The Early Worm）在雾沼中出现，烹饪/食用后解除困倦。" },
            { "LastResortBadge", "获取提示：仪式匕首在远古行李和雕像中发现，喂食可触发治疗与徽章判定。" },
            { "AppliedEsotericaBadge", "获取提示：使用带骸骨/骷髅行为的物品（Skelleton）复活队友。" },
            { "TwentyFourKaratBadge", "获取提示：将沙漠遗迹中的古代神像投入熔炉。" },
            { "ExorcistBadge", "提示：大幽灵会在雾沼生成并追逐玩家；在雾沼点亮烛台或钟塔，并在20米内驱散即可。" },
            { "AeronauticsBadge", "获取提示：6个气球或2束气球（每束3个），且离地时可触发。" },
            { "MentorshipBadge", "提示：使用童军领队的军号召唤他，在距离足够近的情况下即可解锁。" },
            { "MycoacrobaticsBadge", "获取提示：在弹跳菇上弹跳后进入飞行状态（如飞行类增益的蘑菇或其他拥有浮升能力的道具）。" },
            { "AnimalSerenadingBadge", "提示：水豚在多个地图中生成，不仅限方山；靠近后吹奏军号即可。" },
            { "ArboristBadge", "提示：找到巨树（Jungle Giant Tree），进入顶端树冠区域即可解锁。" },
            { "ResourcefulnessBadge", "提示：开启食人设定后，在极度饥饿状态下食用其他玩家可触发。" },
            { "AstronomyBadge", "获取提示：使用望远镜近距离注视太阳（无特定地图限制）。" },
            { "MegaentomologyBadge", "获取提示：蚁狮在方山（MESA）生成；被咬后拉开足够距离可触发。" },
            { "WebSecurityBadge", "提示：蜘蛛和蛛网在多个地图中都会生成，不仅限森蕈；被网缠住后挣脱并存活即可。" },
            { "UndeadEncounterBadge", "获取提示：僵尸多在森蕈生成；被咬后使用道具治愈僵尸咬伤。" },
            { "BookwormBadge", "获取提示：童军领队的日记页（GuidebookPage）分散在地图各处，全部阅读后解锁。" },
            { "RuleZeroBadge", "获取提示：需要集齐四件童军神秘物品（童军的盛情、童军的毅力、童军的野心、童军的进取），并在顶峰（PEAK）地图后下方给予雕像，通过奇怪水晶合成成童军的荣耀，进入天底通关后解锁。童军神秘物品在每个地图冒蓝光的神秘雕像处获得。" },
        };

        // English 额外提示：保留游戏原文地名/生物名，只把行为逻辑润色成英文。
        public static readonly Dictionary<string, string> BadgeExtrasEn = new Dictionary<string, string>
        {
            { "WellRestedBadge", "Hint: The Early Worm appears in the GLOOM; cook and eat it to cure drowsiness." },
            { "LastResortBadge", "Hint: The Ritual Dagger is found in ancient luggage and statues; feeding with it can trigger healing and the badge." },
            { "AppliedEsotericaBadge", "Hint: Use the Skelleton item to resurrect a friend by unholy means." },
            { "TwentyFourKaratBadge", "Hint: Offer the Ancient Idol from the desert ruins to the Kiln." },
            { "ExorcistBadge", "Hint: The Big Ghost spawns in the GLOOM and chases players; light candles or bell towers and disperse it within 20m." },
            { "AeronauticsBadge", "Hint: 6 balloons or 2 balloon bunches (3 each) while airborne triggers flight." },
            { "MentorshipBadge", "Hint: Summon the Scoutmaster with the Scoutmaster's Bugle, then get close enough." },
            { "MycoacrobaticsBadge", "Hint: Bounce off a bounce mushroom and enter a flight state (e.g. flight-boosting mushroom or other lift ability)." },
            { "AnimalSerenadingBadge", "Hint: Capybaras spawn across multiple maps; play the bugle near one." },
            { "ResourcefulnessBadge", "Hint: Enable cannibalism, then eat another player while starving." },
            { "AstronomyBadge", "Hint: Look too closely at the sun with binoculars (no specific map)." },
            { "MegaentomologyBadge", "Hint: Antlions spawn in the MESA; survive an attack and get far enough away." },
            { "WebSecurityBadge", "Hint: Spiders and webs spawn across multiple maps; break free and survive." },
            { "UndeadEncounterBadge", "Hint: Zombies are mostly found in the ROOTS; cure the zombie bite with an item." },
            { "BookwormBadge", "Hint: Scoutmaster Myres's journal pages are scattered across maps; read them all." },
            { "ArboristBadge", "Hint: Reach the top crown of a giant tree (Jungle Giant Tree)." },
            { "RuleZeroBadge", "Hint: Collect the four mystical Scoutmaster items, bring the statue below PEAK, combine into the Scoutmaster's Glory via strange crystal, and complete the run through the NADIR." },
        };

        // 部位中文名
        public static readonly Dictionary<string, string> Slots = new Dictionary<string, string>
        {
            { "fits", "衣服" }, { "hats", "帽子" }, { "sashes", "饰带" }, { "medals", "奖章" },
            { "eyes", "眼睛" }, { "mouths", "嘴巴" }, { "accessories", "饰品" }, { "skins", "皮肤" },
            { "special", "特殊" }
        };

        // English 部位名
        public static readonly Dictionary<string, string> SlotsEn = new Dictionary<string, string>
        {
            { "fits", "Fits" }, { "hats", "Hats" }, { "sashes", "Sashes" }, { "medals", "Medals" },
            { "eyes", "Eyes" }, { "mouths", "Mouths" }, { "accessories", "Accessories" }, { "skins", "Skins" },
            { "special", "Special" }
        };

        // Steam 统计中文名
        public static readonly Dictionary<string, string> SteamStat = new Dictionary<string, string>
        {
            { "MealsCooked", "烹饪餐数" }, { "MoraleBoosts", "士气鼓舞次数" }, { "PitonsPlaced", "岩钉放置数" },
            { "PoisonHealed", "解毒恢复量" }, { "HeightClimbed", "累计攀爬高度" }, { "MaxAscent", "最高攀登数" },
            { "TimesPeaked", "登顶次数" }, { "BestTime", "最佳时间" }, { "TotalPagesRead", "阅读书页数" },
            { "DamageBlockedByMilk", "奶白金格挡伤害" }
        };

        // 单次攀登统计中文名
        public static readonly Dictionary<string, string> RunStat = new Dictionary<string, string>
        {
            { "RopePlaced", "放置绳索长度" }, { "ScoutsResurrected", "救援侦察兵数" }, { "FallDamageTaken", "坠落伤害" },
            { "PackagedFoodEaten", "食用包装食品数" }, { "TimesPassedOut", "昏倒次数" }, { "LuggageOpened", "开启行李箱数" },
            { "PermanentItemsPlaced", "永久物品放置数" }, { "FriendsHealedAmount", "治疗队友量" }, { "MaxHeightReached", "最高高度" },
            { "MaxHeatTakenInMesa", "方山最高热度" }, { "MaxColdTakenInAlpine", "雪山最低寒冷" }, { "MaxSporesTakenInRoots", "森蕈孢子累积" },
            { "BitByZombie", "被僵尸咬次数" }, { "RangGloomBells", "敲响雾沼钟数" }, { "ClownLuggageOpened", "开启小丑行李箱数" },
            { "HitByTraps", "被陷阱击中数" }, { "ArrowsRemoved", "拔箭数" }
        };

        public static string T(string zh, string en)
        {
            return IsChinese ? zh : en;
        }

        public static string BadgeName(string achievementType)
        {
            var table = IsChinese ? Badges : BadgesEn;
            if (table.TryGetValue(achievementType, out var t)) return t[0];
            return achievementType;
        }

        public static string BadgeDesc(string achievementType)
        {
            var table = IsChinese ? Badges : BadgesEn;
            if (table.TryGetValue(achievementType, out var t)) return t[1];
            return "";
        }

        public static string BadgeExtra(string achievementType)
        {
            var table = IsChinese ? BadgeExtras : BadgeExtrasEn;
            if (table.TryGetValue(achievementType, out var text)) return text;
            return "";
        }

        public static string SlotName(string slot)
        {
            var table = IsChinese ? Slots : SlotsEn;
            if (table.TryGetValue(slot, out var s)) return s;
            return slot;
        }

        public static string StatName(string key)
        {
            if (IsChinese && SteamStat.TryGetValue(key, out var zh)) return zh + " (" + key + ")";
            return key;
        }

        public static string RunStatName(string key)
        {
            if (IsChinese && RunStat.TryGetValue(key, out var zh)) return zh + " (" + key + ")";
            return key;
        }
    }
}
