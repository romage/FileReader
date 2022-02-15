using FileReader.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader.Core.Models
{
    public class Column
    {
        public int OrdinalPosition { get; set; }
        public string Title { get; set; } = null!;
        public HeadingDataType HeadingDataType {get;set;}
        public bool IsNullable { get; set; } = false;
        public int MaxLength { get; set;  }
    }
}
