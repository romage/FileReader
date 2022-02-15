using FileReader.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader.Core.Interfaces
{
    public  interface IFileService
    {
        List<Column> GetColumsMetaData(string path, int sampleSize = 50);
        IEnumerable<FileInfo> GetFiles(string path);
        string GetTableName(string filename);
    }
}
