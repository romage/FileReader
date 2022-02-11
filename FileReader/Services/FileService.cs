using FileReader.Dtos;
using FileReader.Enums;

namespace FileReader.Services
{
    public partial class FileService
    {
        List<Column> columns ;
        private readonly ILogger _logger;

        public FileService(ILogger logger)
        {
            _logger = logger;
        }

        public List<Column> GetColumsMetaData(string path, int sampleSize = 50)
        {
            columns =  new List<Column>();

            using System.IO.StreamReader readingFile = new System.IO.StreamReader(path);

            string readingLine = readingFile.ReadLine();

            var columnsHeaders = readingLine.Split('|');

            for (var i = 0; i < columnsHeaders.Count(); i++)
            {
                var c = new Column { OrdinalPosition = i, Title = columnsHeaders[i] };
                columns.Add(c);
            }

            for(var i=0; i< sampleSize; i++)
            {
               ProcessLine(readingFile);
            }

            return columns;

           
        }

        private void ProcessLine(StreamReader sr)
        {
            string v = string.Empty;
            if (!sr.EndOfStream)
                v = sr.ReadLine();

            var line = v.Split('|');

            for (var i = 0; i < columns.Count; i++)
            {
                var col = columns.Single(e=>e.OrdinalPosition ==i); 

                var p =line[i];

               


                var isBool = bool.TryParse(p, out var isb);
                if (isBool && p!="True" &&p!="False") continue;

                var isDate = DateTime.TryParse(p, out var isda);
                if (isDate)
                {
                    if (col.HeadingDataType >= HeadingDataType.DateTime) continue;

                    col.HeadingDataType = HeadingDataType.DateTime;
                    continue;
                }

                var isInt = int.TryParse(p, out var isi);
                if (isInt )
                {
                    if (col.HeadingDataType >= HeadingDataType.Int) continue;

                    col.HeadingDataType = HeadingDataType.Int;
                    continue;
                }

                var isDecimal = decimal.TryParse(p, out var isdc);
                if (isDecimal)
                {
                    if (col.HeadingDataType >= HeadingDataType.Decimal) continue;
                    col.HeadingDataType = HeadingDataType.Decimal;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(p))
                {
                    if (!col.IsNullable) col.IsNullable = true;
                    continue;
                }


                var stringLen = p.Length + 5;
                if (stringLen > col.MaxLength)
                {
                    col.HeadingDataType = HeadingDataType.String;
                    col.MaxLength = stringLen;
                }
            }

        }

        internal IEnumerable<FileInfo> GetFiles(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            return di.GetFiles("*.csv");
        }
    }
}
