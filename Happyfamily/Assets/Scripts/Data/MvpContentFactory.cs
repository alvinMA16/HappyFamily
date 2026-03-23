using System.Collections.Generic;

namespace HappyFamily.Data
{
    public static class MvpContentFactory
    {
        public static MvpChapterDefinition CreateFrontYardChapter()
        {
            return new MvpChapterDefinition
            {
                Id = "chapter_front_yard",
                DisplayName = "第一章：前院起头",
                Levels = new List<MvpLevelDefinition>
                {
                    new MvpLevelDefinition
                    {
                        Id = "level_front_yard_01",
                        DisplayName = "第 1 关：先把院口理顺",
                        Description = "把相同物件配成一对，先把前院入口收拾顺当。",
                        StepBudget = 10,
                        StarReward = 1,
                        PairLabels = new[]
                        {
                            "暖壶", "搪瓷缸", "花盆", "线团", "收音机", "菜篮子"
                        }
                    },
                    new MvpLevelDefinition
                    {
                        Id = "level_front_yard_02",
                        DisplayName = "第 2 关：给花架腾出地方",
                        Description = "继续整理常用物件，为前院添一点生气。",
                        StepBudget = 9,
                        StarReward = 1,
                        PairLabels = new[]
                        {
                            "花盆", "竹椅", "月饼盒", "线团", "保温壶", "旧相框"
                        }
                    },
                    new MvpLevelDefinition
                    {
                        Id = "level_front_yard_03",
                        DisplayName = "第 3 关：前院收拾齐整",
                        Description = "完成这一关，Slice 的基础玩法就跑通了。",
                        StepBudget = 8,
                        StarReward = 1,
                        PairLabels = new[]
                        {
                            "收音机", "竹椅", "搪瓷脸盆", "旧挂钟", "菜篮子", "台灯"
                        }
                    }
                },
                RenovationNodes = new List<MvpRenovationNodeDefinition>
                {
                    new MvpRenovationNodeDefinition
                    {
                        Id = "front_yard_gate",
                        DisplayName = "焕新节点：修院门",
                        RequiredStars = 1,
                        MemoryTitle = "院门边的风",
                        MemoryText = "门口收拾利落了，院子里一下就有了人气。旧木门推开时，像把从前那些平常日子也一并推回来了。",
                        Options = new List<MvpRenovationOptionDefinition>
                        {
                            new MvpRenovationOptionDefinition
                            {
                                Id = "gate_warm_wood",
                                DisplayName = "暖木院门",
                                Description = "更朴素，像熟悉的老院子。"
                            },
                            new MvpRenovationOptionDefinition
                            {
                                Id = "gate_olive_green",
                                DisplayName = "墨绿院门",
                                Description = "更清爽，院口显得精神一些。"
                            }
                        }
                    }
                }
            };
        }
    }
}
