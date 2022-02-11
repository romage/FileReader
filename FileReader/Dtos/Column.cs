using FileReader.Enums;

namespace FileReader.Dtos
{
        public class Column
        { 
            public int OrdinalPosition { get; set; }
            public string Title { get; set; }
            public HeadingDataType HeadingDataType { get; set; } = HeadingDataType.Bool;
            public bool IsNullable { get; set; } = false;
            public int MaxLength { get; set; }
        }
}
