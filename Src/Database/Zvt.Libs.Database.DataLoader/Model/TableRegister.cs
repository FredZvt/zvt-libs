using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zvt.Libs.Database.DataLoader.Model
{
    public class TableRegister
    {
        public TableRegister(params TableRegisterUnit[] dataBatch)
        {
            this.RegisterData = new List<TableRegisterUnit>();

            if (dataBatch != null)
                this.RegisterData.AddRange(dataBatch);
        }

        public List<TableRegisterUnit> RegisterData { get; set; }
    }
}
