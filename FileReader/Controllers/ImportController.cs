using FileReader.Dtos;
using FileReader.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileReader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {

        private readonly ILogger<ImportController> _logger;
        private readonly IConfiguration config;
        public ImportController(ILogger<ImportController> logger, IConfiguration config)
        {
            _logger = logger;
            this.config = config;
        }

        //[HttpGet(Name = "ColumnInfo")]
        //public List<Column> ColumnInfo()
        //{
        //    string path = @"D:\projects\test\FileReader\File2.csv";
        //    var fs = new FileService(_logger);
        //    var columnInfo = fs.GetColumsMetaData(path, 300);
        //    DataService? ds = new DataService(config, _logger);
        //    ds.CreateEmptyTable("File2", columnInfo);
        //    ds.PopulateTable("File2", path);

        //    return columnInfo;
        //}

        //[HttpGet(Name = "FilesInImport")]
        //public IEnumerable<FileInfo> FilesInImport()
        //{
        //    string path = @"D:\projects\test\FileReader";
        //    var fs = new FileService(_logger);
        //    var fileInfo = fs.GetFiles(path);

        //    return fileInfo;
        //}

        [HttpGet(Name = "ProcessAllFiles")]
        public string ProcessAllFiles()
        {
            string path = config.GetValue<string>("FolderPath");

            var fs = new FileService(_logger);
            var fileInfo = fs.GetFiles(path);

            foreach (var f in fileInfo)
            {
                var columnInfo = fs.GetColumsMetaData(f.FullName, 300);
                var tableName = Path.GetFileNameWithoutExtension(f.Name);
                DataService? ds = new DataService(config, _logger);
                ds.CreateEmptyTable(tableName, columnInfo);
                ds.PopulateTable(tableName, f.FullName);
            }

            return "message goes here";

        }




    }
}
