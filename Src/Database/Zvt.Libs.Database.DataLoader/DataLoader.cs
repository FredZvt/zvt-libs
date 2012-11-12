using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zvt.Libs.Database.DataLoader
{
    public class DataLoader
    {
        protected IDataImporter DataImporter { get; set; }
        protected IInsertScriptWriter SqlWriter { get; set; }

        public DataLoader(IDataImporter dataImporter, IInsertScriptWriter sqlWriter)
        {
            if (dataImporter == null)
                throw new ArgumentNullException("dataImporter");

            if (sqlWriter == null)
                throw new ArgumentNullException("sqlWriter");

            this.DataImporter = dataImporter;
            this.SqlWriter = sqlWriter;
        }
    }
}
