using System;
using System.Collections.Generic;

namespace HappyFamily.Data
{
    [Serializable]
    public class TidyUpItemDefinition
    {
        public string ItemId;
        public string DisplayName;
        public string SpritePath;       // Path in Resources folder
        public string MemoryText;       // Short nostalgic text when matched
    }

    [Serializable]
    public class TidyUpLevelDefinition
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public int GridWidth;           // Width of the stacking grid
        public int GridHeight;          // Height of the stacking grid
        public int MaxLayers;           // Maximum stacking layers
        public List<TidyUpItemDefinition> Items = new List<TidyUpItemDefinition>();
    }

    [Serializable]
    public class TidyUpChapterDefinition
    {
        public string Id;
        public string DisplayName;
        public List<TidyUpLevelDefinition> Levels = new List<TidyUpLevelDefinition>();
    }
}
