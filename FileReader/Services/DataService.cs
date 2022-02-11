using FileReader.Dtos;
using FileReader.Enums;
using System.Text;
using System.Data.SqlClient;
using Dapper;

namespace FileReader.Services
{
    public class DataService
    {
		string _constr;
        private readonly ILogger _logger;

        public DataService(IConfiguration config, ILogger logger)
        {
			_constr = config.GetConnectionString("default");
            _logger = logger;
        }
        public string CreateEmptyTableText(string tableName, List<Column> columns)
        {
			var sql = new StringBuilder($"CREATE TABLE [dbo].[{tableName}](");
			var max = columns.Max(e => e.OrdinalPosition);
			foreach (var c in columns.OrderBy(e => e.OrdinalPosition))
			{
				sql.Append(GetColumnString(c, c.OrdinalPosition == max));
			}
			
			sql.Append(")");

			return sql.ToString();
		}

		public void CreateEmptyTable(string tableName, List<Column> columns)
		{ 
			var con =  new SqlConnection(_constr);
			con.Open();

			string sql = CreateEmptyTableText(tableName, columns);

			con.ExecuteScalar(sql);

		}

		public string PopulateTableString(string tableName, string sourcePath)
		{
			return $@"BULK INSERT {tableName}
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
			string strlen = (col.MaxLength * 2) > 255 ? "max" : (col.MaxLength * 2).ToString();
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
