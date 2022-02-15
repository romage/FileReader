using FileReader.Core.Enums;
using System.Text;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FileReader.Core.Models;
using FileReader.Core.Interfaces;

namespace FileReader.Core.Services
{
  

    public class DataService : IDataService
    {
        string _constr;
        private readonly ILogger<DataService> _logger;
        private readonly IConfigValues _config;

        public DataService(IConfigValues config, ILogger<DataService> logger)
        {
            _config = config;
            _constr = config.DefaultConnectionString;
            _logger = logger;
        }

        private string getSchema()
        {
            var ds = "dbo";
            var configschema = _config.DefaultSchema;
            if (!string.IsNullOrEmpty(configschema))
            {
                ds = configschema.TrimStart('[').TrimEnd(']');
            }
            return $"[{ds}].";
        }
        private string getPreface()
        {
            
            var configpr = _config.SqlPreface;
            if (!string.IsNullOrEmpty(configpr))
            {
                return $"{configpr.TrimEnd().TrimEnd(';')};";
            }
            return string.Empty;
        }

        public string CreateEmptyTableText(string tableName, List<Column> columns)
        {
            var sql = new StringBuilder();
            //sql.Append("Create alias import.dbo.{tablename}")
            
            sql.AppendLine(getPreface());
            sql.AppendLine($"CREATE TABLE {getSchema()}[{tableName}](");
            var max = columns.Max(e => e.OrdinalPosition);
            foreach (var c in columns.OrderBy(e => e.OrdinalPosition))
            {
                sql.AppendLine(GetColumnString(c, c.OrdinalPosition == max));
            }

            sql.AppendLine(")");

            return sql.ToString();
        }

        public void CreateEmptyTable(string tableName, List<Column> columns)
        {
            var con = new SqlConnection(_constr);
            con.Open();

            string sql = CreateEmptyTableText(tableName, columns);

            con.ExecuteScalar(sql);

        }

        public string PopulateTableString(string tableName, string sourcePath)
        {
            return $@"{getPreface()}
                       BULK INSERT {getSchema()}[{tableName}]
					   FROM '{sourcePath}'
					   WITH
						 (
							 FIRSTROW=2
							 , FIELDTERMINATOR ='|'
							 , ROWTERMINATOR = '\n'
							 , DATAFILETYPE = 'CHAR'
						  );";

        }


        public void PopulateTable(string tableName, string sourcePath)
        {
            var con = new SqlConnection(_constr);
            con.Open();

            string sql = PopulateTableString(tableName, sourcePath);


            con.ExecuteScalar(sql);
        }

        private string GetColumnString(Column col, bool lastColumn)
        {
            string nullable = col.IsNullable ? "NULL" : "NOT NULL";
            string strlen = col.MaxLength > 255 ? "max" : col.MaxLength.ToString();
            string sqlDataType = string.Empty;
            switch (col.HeadingDataType)
            {
                case HeadingDataType.Bool:
                    sqlDataType = "bit";
                    break;
                case HeadingDataType.DateTime:
                    sqlDataType = "DateTime";
                    break;
                case HeadingDataType.Int:
                    sqlDataType = "int";
                    break;
                case HeadingDataType.BigInt:
                    sqlDataType = "bigint";
                    break;
                case HeadingDataType.Decimal:
                    sqlDataType = "Float";
                    break;
                default:
                    sqlDataType = $"Nvarchar({strlen})";
                    break;
            }
            var moreColumns = lastColumn ? "" : ",";


            return $"[{col.Title}] {sqlDataType} { nullable } {moreColumns}";
        }
    }
}
