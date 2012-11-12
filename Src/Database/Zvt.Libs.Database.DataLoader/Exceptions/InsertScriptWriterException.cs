using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zvt.Libs.Database.DataLoader.Exceptions
{
    public class InsertScriptWriterException : Exception
    {
        public InsertScriptWriterException(string message) : base(message) { }
        public InsertScriptWriterException(string message, Exception innerException) : base(message, innerException) { }
    }
}
