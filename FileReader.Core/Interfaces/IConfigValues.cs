using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader.Core.Interfaces
{
    public  interface IConfigValues
    {
        string ProfileName { get; set; }
        string FolderPath {  get; set; }
        string FileFilter {  get; set; }
        string FileNameCleanerRegex {  get; set; }
        int SampleSize {  get; set; }
        string DefaultConnectionString  {  get; set; }
        string DefaultSchema {  get; set; }
        string SqlPreface {  get; set;  }
        decimal StringLengthPaddingMultiplyer {  get; set; }
        string Codepage {  get; set; }
        string DataFileType {  get; set; }
        string FieldTerminator {  get; set; }
        string RowTerminator {  get; set; }

        void CopySettingsProfileTo(string name);
        List<string> GetProfiles();
        void LoadSettingsProfile(string name);
        Dictionary<string, object> GetAllSettingsProfiles();
        void UpdateSetting(string name, string newValue);
        void LoadDefaultConfig(IConfiguration baseconfig);

    }
}
