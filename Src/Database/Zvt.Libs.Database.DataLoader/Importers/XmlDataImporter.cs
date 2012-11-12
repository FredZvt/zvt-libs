using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Zvt.Libs.Database.DataLoader.Exceptions;
using Zvt.Libs.Database.DataLoader.Model;

namespace Zvt.Libs.Database.DataLoader.Importers
{
    public class XmlDataImporter : IDataImporter
    {
        protected string Xml { get; set; }

        public XmlDataImporter(string xml)
        {
            if (xml == null)
                throw new ArgumentNullException("xml");

            this.Xml = xml;
        }

        public Data ImportData()
        {
            XmlDocument xmlDoc = null;
            try
            {
                xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(this.Xml);
            }
            catch (Exception ex)
            {
                throw new DataImporterException("Invalid xml.", ex);
            }

            var data = new Data();
            foreach (XmlNode tableInsertNodes in xmlDoc.DocumentElement.ChildNodes)
            {
                var targetTableName = tableInsertNodes.Name;

                if (!data.DataContent.ContainsKey(targetTableName))
                    data.DataContent[targetTableName] = new List<TableRegister>();

                var reg = new TableRegister();
                foreach (XmlNode registerColumnNode in tableInsertNodes.ChildNodes)
                {
                    var dataUnit = new TableRegisterUnit();
                    dataUnit.ColInfo.Name = registerColumnNode.Name;
                    dataUnit.ValueAsString = registerColumnNode.InnerText;

                    reg.RegisterData.Add(dataUnit);
                }

                data.DataContent[targetTableName].Add(reg);
            }

            return data;
        }
    }
}
