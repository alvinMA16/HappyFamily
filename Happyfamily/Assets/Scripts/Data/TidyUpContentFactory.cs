using System.Collections.Generic;

namespace HappyFamily.Data
{
    public static class TidyUpContentFactory
    {
        public static TidyUpChapterDefinition CreateTidyUpChapter()
        {
            var chapter = new TidyUpChapterDefinition
            {
                Id = "tidyup_chapter_01",
                DisplayName = "整理老屋",
                Levels = new List<TidyUpLevelDefinition>()
            };

            // 目前只有5种素材，通过调整网格和层数来增加难度
            chapter.Levels.Add(CreateLevel1()); // 3x3, 2层, 3种 = 9张
            chapter.Levels.Add(CreateLevel2()); // 4x3, 2层, 4种 = 12张
            chapter.Levels.Add(CreateLevel3()); // 4x3, 3层, 5种 = 15张
            chapter.Levels.Add(CreateLevel4()); // 4x4, 3层, 5种 = 15张 (更密集)
            chapter.Levels.Add(CreateLevel5()); // 5x4, 3层, 5种 = 15张
            chapter.Levels.Add(CreateLevel6()); // 5x4, 4层, 5种 = 15张
            chapter.Levels.Add(CreateLevel7()); // 5x5, 4层, 5种 = 15张
            chapter.Levels.Add(CreateLevel8()); // 6x5, 4层, 5种 = 15张

            return chapter;
        }

        // ============== 物品定义（只用已有的5个素材）==============
        private static TidyUpItemDefinition ItemBambooChair => new TidyUpItemDefinition
        {
            ItemId = "bamboo_chair",
            DisplayName = "竹椅",
            SpritePath = "Items/banboo_chair",
            MemoryText = "爷爷最爱坐的竹椅"
        };

        private static TidyUpItemDefinition ItemFlowerPot => new TidyUpItemDefinition
        {
            ItemId = "flower_pot",
            DisplayName = "花盆",
            SpritePath = "Items/flower_pot",
            MemoryText = "奶奶种的茉莉花"
        };

        private static TidyUpItemDefinition ItemHandFan => new TidyUpItemDefinition
        {
            ItemId = "hand_fan",
            DisplayName = "蒲扇",
            SpritePath = "Items/hand_fan",
            MemoryText = "夏天乘凉的蒲扇"
        };

        private static TidyUpItemDefinition ItemWoodTable => new TidyUpItemDefinition
        {
            ItemId = "wood_table",
            DisplayName = "木桌",
            SpritePath = "Items/wooden_table",
            MemoryText = "一家人吃饭的方桌"
        };

        private static TidyUpItemDefinition ItemWoodChair => new TidyUpItemDefinition
        {
            ItemId = "wood_chair",
            DisplayName = "木椅",
            SpritePath = "Items/wooden_chair",
            MemoryText = "配套的木头椅子"
        };

        // ============== 关卡定义 ==============

        private static TidyUpLevelDefinition CreateLevel1()
        {
            return new TidyUpLevelDefinition
            {
                Id = "tidyup_level_01",
                DisplayName = "杂物间",
                Description = "把散落的物品整理好",
                GridWidth = 3,
                GridHeight = 3,
                MaxLayers = 2,
                Items = new List<TidyUpItemDefinition>
                {
                    ItemBambooChair,
                    ItemFlowerPot,
                    ItemHandFan
                }
            };
        }

        private static TidyUpLevelDefinition CreateLevel2()
        {
            return new TidyUpLevelDefinition
            {
                Id = "tidyup_level_02",
                DisplayName = "老屋客厅",
                Description = "整理客厅里的老物件",
                GridWidth = 4,
                GridHeight = 3,
                MaxLayers = 2,
                Items = new List<TidyUpItemDefinition>
                {
                    ItemBambooChair,
                    ItemFlowerPot,
                    ItemWoodTable,
                    ItemWoodChair
                }
            };
        }

        private static TidyUpLevelDefinition CreateLevel3()
        {
            return new TidyUpLevelDefinition
            {
                Id = "tidyup_level_03",
                DisplayName = "厨房角落",
                Description = "厨房里堆满了老物件",
                GridWidth = 4,
                GridHeight = 3,
                MaxLayers = 3,
                Items = new List<TidyUpItemDefinition>
                {
                    ItemBambooChair,
                    ItemFlowerPot,
                    ItemHandFan,
                    ItemWoodTable,
                    ItemWoodChair
                }
            };
        }

        private static TidyUpLevelDefinition CreateLevel4()
        {
            return new TidyUpLevelDefinition
            {
                Id = "tidyup_level_04",
                DisplayName = "卧室衣柜",
                Description = "衣柜里塞满了回忆",
                GridWidth = 4,
                GridHeight = 4,
                MaxLayers = 3,
                Items = new List<TidyUpItemDefinition>
                {
                    ItemBambooChair,
                    ItemFlowerPot,
                    ItemHandFan,
                    ItemWoodTable,
                    ItemWoodChair
                }
            };
        }

        private static TidyUpLevelDefinition CreateLevel5()
        {
            return new TidyUpLevelDefinition
            {
                Id = "tidyup_level_05",
                DisplayName = "阁楼储物",
                Description = "尘封多年的阁楼",
                GridWidth = 5,
                GridHeight = 4,
                MaxLayers = 3,
                Items = new List<TidyUpItemDefinition>
                {
                    ItemBambooChair,
                    ItemFlowerPot,
                    ItemHandFan,
                    ItemWoodTable,
                    ItemWoodChair
                }
            };
        }

        private static TidyUpLevelDefinition CreateLevel6()
        {
            return new TidyUpLevelDefinition
            {
                Id = "tidyup_level_06",
                DisplayName = "书房整理",
                Description = "爷爷的书房乱成一团",
                GridWidth = 5,
                GridHeight = 4,
                MaxLayers = 4,
                Items = new List<TidyUpItemDefinition>
                {
                    ItemBambooChair,
                    ItemFlowerPot,
                    ItemHandFan,
                    ItemWoodTable,
                    ItemWoodChair
                }
            };
        }

        private static TidyUpLevelDefinition CreateLevel7()
        {
            return new TidyUpLevelDefinition
            {
                Id = "tidyup_level_07",
                DisplayName = "地下室",
                Description = "地下室里堆满了宝贝",
                GridWidth = 5,
                GridHeight = 5,
                MaxLayers = 4,
                Items = new List<TidyUpItemDefinition>
                {
                    ItemBambooChair,
                    ItemFlowerPot,
                    ItemHandFan,
                    ItemWoodTable,
                    ItemWoodChair
                }
            };
        }

        private static TidyUpLevelDefinition CreateLevel8()
        {
            return new TidyUpLevelDefinition
            {
                Id = "tidyup_level_08",
                DisplayName = "老宅大清理",
                Description = "整个老宅的回忆都在这里",
                GridWidth = 6,
                GridHeight = 5,
                MaxLayers = 4,
                Items = new List<TidyUpItemDefinition>
                {
                    ItemBambooChair,
                    ItemFlowerPot,
                    ItemHandFan,
                    ItemWoodTable,
                    ItemWoodChair
                }
            };
        }
    }
}
