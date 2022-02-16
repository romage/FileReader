using FileReader.Core.Enums;
using FileReader.Core.Interfaces;
using FileReader.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileReader.Core.Services
{
    public class TypeProcessingService: ITypeProcessingService
    {

        public void UpdateColumn(string p, Column col)
        {
            
            var isBool = bool.TryParse(p, out var isb);
            if (isBool && p != "True" && p!= "False") return;

            var isDate = DateTime.TryParse(p, out var isda);
            if (isDate)
            {
                if (col.HeadingDataType >= HeadingDataType.DateTime) return;

                col.HeadingDataType = HeadingDataType.DateTime;
                //col.MaxLength = 20;
                return;
            }

            var isInt = int.TryParse(p, out var isi);
            if (isInt)
            {
                if (col.HeadingDataType >= HeadingDataType.Int) return;

                col.HeadingDataType = HeadingDataType.Int;
                return;
            }

            var isBigInt = Int64.TryParse(p, out var isBig);
            if (isBigInt)
            {
                if (col.HeadingDataType >= HeadingDataType.BigInt) return;

                col.HeadingDataType = HeadingDataType.BigInt;
                return;
            }

            var isDecimal = decimal.TryParse(p, out var isdc);
            if (isDecimal)
            {
                if (col.HeadingDataType >= HeadingDataType.Decimal) return;
                col.HeadingDataType = HeadingDataType.Decimal;
                return;
            }

            if (string.IsNullOrWhiteSpace(p))
            {
                if (!col.IsNullable) col.IsNullable = true;
                return;
            }


            var stringLen = p.Length + 5;
            if (stringLen > col.MaxLength)
            {
                col.HeadingDataType = HeadingDataType.String;
                col.MaxLength = stringLen;
            }
        }

    }
}
