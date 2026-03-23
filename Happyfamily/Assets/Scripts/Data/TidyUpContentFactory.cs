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
                DisplayName = "整理前院",
                Levels = new List<TidyUpLevelDefinition>()
            };

            // Level 1: Simple 3x3 grid, 2 layers
            chapter.Levels.Add(CreateLevel1());

            // Level 2: Larger grid, more items
            chapter.Levels.Add(CreateLevel2());

            return chapter;
        }

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
                    new TidyUpItemDefinition
                    {
                        ItemId = "bamboo_chair",
                        DisplayName = "竹椅",
                        SpritePath = "Items/banboo_chair",
                        MemoryText = "爷爷最爱坐的竹椅"
                    },
                    new TidyUpItemDefinition
                    {
                        ItemId = "flower_pot",
                        DisplayName = "花盆",
                        SpritePath = "Items/flower_pot",
                        MemoryText = "奶奶种的茉莉花"
                    },
                    new TidyUpItemDefinition
                    {
                        ItemId = "hand_fan",
                        DisplayName = "蒲扇",
                        SpritePath = "Items/hand_fan",
                        MemoryText = "夏天乘凉的蒲扇"
                    }
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
                MaxLayers = 3,
                Items = new List<TidyUpItemDefinition>
                {
                    new TidyUpItemDefinition
                    {
                        ItemId = "bamboo_chair",
                        DisplayName = "竹椅",
                        SpritePath = "Items/banboo_chair",
                        MemoryText = "爷爷最爱坐的竹椅"
                    },
                    new TidyUpItemDefinition
                    {
                        ItemId = "flower_pot",
                        DisplayName = "花盆",
                        SpritePath = "Items/flower_pot",
                        MemoryText = "奶奶种的茉莉花"
                    },
                    new TidyUpItemDefinition
                    {
                        ItemId = "hand_fan",
                        DisplayName = "蒲扇",
                        SpritePath = "Items/hand_fan",
                        MemoryText = "夏天乘凉的蒲扇"
                    },
                    new TidyUpItemDefinition
                    {
                        ItemId = "wood_table",
                        DisplayName = "木桌",
                        SpritePath = "Items/wood table",
                        MemoryText = "一家人吃饭的方桌"
                    },
                    new TidyUpItemDefinition
                    {
                        ItemId = "wood_chair",
                        DisplayName = "木椅",
                        SpritePath = "Items/wood_chair",
                        MemoryText = "配套的木头椅子"
                    }
                }
            };
        }
    }
}
