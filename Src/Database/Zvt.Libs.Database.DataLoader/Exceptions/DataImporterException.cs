using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zvt.Libs.Database.DataLoader.Exceptions
{
    public class DataImporterException : Exception
    {
        public DataImporterException(string message) : base(message) { }
        public DataImporterException(string message, Exception innerException) : base(message, innerException) { }
    }
}
