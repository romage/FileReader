using FileReader.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader.Core.Interfaces
{
    public interface ITypeProcessingService
    {
        void UpdateColumn(string p, Column col);
    }
}
