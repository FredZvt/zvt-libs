using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zvt.Libs.Database.DataLoader.Model
{
    public class Data
    {
        public Data(Dictionary<string, List<TableRegister>> data = null)
        {
            if (data == null)
            {                
                this.DataContent = new Dictionary<string, List<TableRegister>>();
            }
            else
            {
                this.DataContent = data;
            }
        }

        public Dictionary<string, List<TableRegister>> DataContent { get; set; }

        public string[] ReferencedTables
        {
            get
            {
                return DataContent.Keys.Distinct().ToArray();
            }
        }
    }
}
