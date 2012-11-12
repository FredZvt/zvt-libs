using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zvt.Libs.Database.DataLoader.InsertScriptWriters;
using Zvt.Libs.Database.DataLoader.Model;

namespace Zvt.Libs.Database.DataLoader.Tests
{
    [TestClass]
    public class SqlServerInsertScriptWriterTests
    {
        protected Data GetFakeData()
        {
            var data = new Data();

            data.DataContent["TblFake1"] =
                new List<TableRegister>()
                {
                    new TableRegister(
                        new TableRegisterUnit("StringCol", SqlServerTypes.VARCHAR, false, "texto texto texto", "texto texto texto"),
                        new TableRegisterUnit("DateTimeCol", SqlServerTypes.DATETIME, false, "2012-11-10 18:53:10", new DateTime(2012,11,10,18,53,10)),
                        new TableRegisterUnit("BoolCol", SqlServerTypes.BIT, false, "1", true),
                        new TableRegisterUnit("IntCol", SqlServerTypes.INT, true, "68447", (int)68447),
                        new TableRegisterUnit("PkCol", SqlServerTypes.INT, true, "3486", (int)3486, ColPkName: "PK_Name_01")
                    ),
                    new TableRegister(
                        new TableRegisterUnit("StringCol", SqlServerTypes.VARCHAR, false, "texto texto texto texto", "texto texto texto texto"),
                        new TableRegisterUnit("DateTimeCol", SqlServerTypes.DATETIME, false, "2012-12-05 17:03:01", new DateTime(2012,12,05,17,03,01)),
                        new TableRegisterUnit("BoolCol", SqlServerTypes.BIT, false, "0", false),
                        new TableRegisterUnit("FkCol", SqlServerTypes.INT, true, "673", (int)673, ColFkName: "FK_Name_01")
                    ),
                    new TableRegister(
                        new TableRegisterUnit("StringCol", SqlServerTypes.VARCHAR, false, "texto texto texto texto texto", "texto texto texto texto texto"),
                        new TableRegisterUnit("DateTimeCol", SqlServerTypes.DATETIME, false, "2012-12-05 16:03:01", new DateTime(2012,12,05,16,03,01)),
                        new TableRegisterUnit("BoolCol", SqlServerTypes.BIT, false, "1", true)
                    )
                };

            data.DataContent["TblFake2"] =
                new List<TableRegister>()
                {
                    new TableRegister(
                        new TableRegisterUnit("DecimalCol", SqlServerTypes.DECIMAL, false, "485.51", 485.51M),
                        new TableRegisterUnit("CharCol", SqlServerTypes.CHAR, false, "F", 'F')
                    )
                };

            return data;
        }

        [TestMethod]
        public void Test_SqlServerInsertScriptWriter()
        {
            var data = GetFakeData();

            var writer = new SqlServerInsertScriptWriter();
            writer.NumberOfSelectsByInsertCommand = 2;

            var sqls = writer.WriteInsertScripts(data);

            var sql1 =
                "SET IDENTITY_INSERT TblFake1 ON" + Environment.NewLine +
                "INSERT INTO [TblFake1] ([StringCol], [DateTimeCol], [BoolCol], [IntCol], [PkCol], [FkCol])" + Environment.NewLine +
                "SELECT 'texto texto texto', '2012-11-10 18:53:10', 1, 68447, 3486, NULL UNION ALL" + Environment.NewLine +
                "SELECT 'texto texto texto texto', '2012-12-05 17:03:01', 0, NULL, NULL, 673" + Environment.NewLine +
                "SET IDENTITY_INSERT TblFake1 OFF" + Environment.NewLine;


            var sql2 =
                "SET IDENTITY_INSERT TblFake1 ON" + Environment.NewLine +
                "INSERT INTO [TblFake1] ([StringCol], [DateTimeCol], [BoolCol], [IntCol], [PkCol], [FkCol])" + Environment.NewLine +
                "SELECT 'texto texto texto texto texto', '2012-12-05 16:03:01', 1, NULL, NULL, NULL" + Environment.NewLine +
                "SET IDENTITY_INSERT TblFake1 OFF" + Environment.NewLine;
                
            var sql3 =
                "INSERT INTO [TblFake2] ([DecimalCol], [CharCol])" + Environment.NewLine +
                "SELECT 485.51, 'F'" + Environment.NewLine;

            Assert.AreEqual(sql1, sqls[0]);
            Assert.AreEqual(sql2, sqls[1]);
            Assert.AreEqual(sql3, sqls[2]);
        }
    }
}
