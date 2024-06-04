using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QComp.Models
{
    public class SaveItem
    {
        public string Name { get; set; }
        public string Binary { get; set; }
        public DateTime Date { get; set; }

        public SaveItem(string name, string binary, DateTime date)
        {
            Name = name;
            Binary = binary;
            Date = date;
        }

        public override string ToString()
        {
            return $"{Name} [{Binary}] [{Date}]";
        }
    }
}
