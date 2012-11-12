using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zvt.Libs.Database.DataLoader.Model;

namespace Zvt.Libs.Database.DataLoader
{
    public interface IDataValidator
    {
        void ValidateImportedData(Data importedData);
    }
}
