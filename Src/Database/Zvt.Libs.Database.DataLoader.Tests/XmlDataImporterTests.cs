using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zvt.Libs.Database.DataLoader.Importers;

namespace Zvt.Libs.Database.DataLoader.Tests
{
    [TestClass]
    public class XmlDataImporterTests
    {
        [TestMethod]
        public void Test_XmlDataImporter()
        {
            var xml = @"
                <root>
                    <table_A>
                        <colA>String value 1.</colA>
                        <colB>1234</colB>
                        <colC>NULL</colC>
                        <colD>2012-11-08</colD>
                    </table_A>
                    <table_A>
                        <colA>String value 2.</colA>
                        <colC>852.45</colC>
                        <colD>2012-12-01</colD>
                    </table_A>
                    <table_B>
                        <colA>String value to add to table table_B</colA>
                    </table_B>
                </root>
            ";

            var dataImporter = new XmlDataImporter(xml);
            var data = dataImporter.ImportData();

            Assert.AreEqual(2, data.ReferencedTables.Length);
            Assert.IsTrue(data.DataContent.Keys.Contains("table_A"));
            Assert.IsTrue(data.DataContent.Keys.Contains("table_B"));

            Assert.AreEqual(2, data.DataContent["table_A"].Count);
            Assert.AreEqual(1, data.DataContent["table_B"].Count);

            Assert.AreEqual("colA", data.DataContent["table_A"][0].RegisterData[0].ColInfo.Name);
            Assert.AreEqual("String value 1.", data.DataContent["table_A"][0].RegisterData[0].ValueAsString);
            Assert.AreEqual("colB", data.DataContent["table_A"][0].RegisterData[1].ColInfo.Name);
            Assert.AreEqual("1234", data.DataContent["table_A"][0].RegisterData[1].ValueAsString);
            Assert.AreEqual("colC", data.DataContent["table_A"][0].RegisterData[2].ColInfo.Name);
            Assert.AreEqual("NULL", data.DataContent["table_A"][0].RegisterData[2].ValueAsString);
            Assert.AreEqual("colD", data.DataContent["table_A"][0].RegisterData[3].ColInfo.Name);
            Assert.AreEqual("2012-11-08", data.DataContent["table_A"][0].RegisterData[3].ValueAsString);

            Assert.AreEqual("colA", data.DataContent["table_A"][1].RegisterData[0].ColInfo.Name);
            Assert.AreEqual("String value 2.", data.DataContent["table_A"][1].RegisterData[0].ValueAsString);
            Assert.AreEqual("colC", data.DataContent["table_A"][1].RegisterData[1].ColInfo.Name);
            Assert.AreEqual("852.45", data.DataContent["table_A"][1].RegisterData[1].ValueAsString);
            Assert.AreEqual("colD", data.DataContent["table_A"][1].RegisterData[2].ColInfo.Name);
            Assert.AreEqual("2012-12-01", data.DataContent["table_A"][1].RegisterData[2].ValueAsString);

            Assert.AreEqual("colA", data.DataContent["table_B"][0].RegisterData[0].ColInfo.Name);
            Assert.AreEqual("String value to add to table table_B", data.DataContent["table_B"][0].RegisterData[0].ValueAsString);
        }
    }
}
