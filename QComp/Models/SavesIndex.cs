using System.Collections.Generic;

namespace QComp.Models
{
    public class SavesIndex
    {
        public Dictionary<string, List<SaveItem>> Items { get; set; }

        public SavesIndex(Dictionary<string, List<SaveItem>> items)
        {
            Items = items;
        }
    }
}
