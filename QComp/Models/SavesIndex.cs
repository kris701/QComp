using System.Collections.Generic;

namespace QComp.Models
{
    public class SavesIndex
    {
        public Dictionary<string, List<SaveItem>> Items { get; set; }
        public Dictionary<string, List<string>> CachedArguments { get; set; }

        public SavesIndex(Dictionary<string, List<SaveItem>> items, Dictionary<string, List<string>> cachedArguments)
        {
            Items = items;
            CachedArguments = cachedArguments;
        }
    }
}
