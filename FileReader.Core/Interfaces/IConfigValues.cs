using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader.Core.Interfaces
{
    public  interface IConfigValues
    {
        string FolderPath { get; }
        string FileFilter { get; }
        string FileNameCleanerRegex { get; }
        int SampleSize { get; }
        string DefaultConnectionString  { get; }
    }
}
