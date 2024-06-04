using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
