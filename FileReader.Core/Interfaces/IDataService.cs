using FileReader.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader.Core.Interfaces
{
    public interface IDataService
    {
        void CreateEmptyTable(string tableName, List<Column> columns);
        string CreateEmptyTableText(string tableName, List<Column> columns);
        void PopulateTable(string tableName, string sourcePath);
        string PopulateTableString(string tableName, string sourcePath);
    }
}
