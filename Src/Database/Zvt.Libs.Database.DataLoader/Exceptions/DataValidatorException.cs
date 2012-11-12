using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zvt.Libs.Database.DataLoader.Exceptions
{
    public class DataValidatorException : Exception
    {
        public DataValidatorException(string message) : base(message) { }
        public DataValidatorException(string message, Exception innerException) : base(message, innerException) { }
    }
}
