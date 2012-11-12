using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Zvt.Libs.Database.DataLoader.Model;
using Zvt.Libs.Database.DataLoader.Validators;

namespace Zvt.Libs.Database.DataLoader.Tests
{
    [TestClass]
    public class SqlServerDataValidatorTests
    {
        [TestMethod]
        public void Test_ValidateImportedData()
        {
            var mockedDbHelper = new Mock<ISqlServerDatabaseHelper>();
            var realDbUtils = new SqlServerDatabaseUtils(mockedDbHelper.Object);

            var mockedDbUtils = new Mock<ISqlServerDatabaseUtils>();

            mockedDbUtils
                .Setup(x => x.GetTableColumnsInfo("TblFake1"))
                .Returns(
                    new List<TableColumnInfo>() {
                        new TableColumnInfo("StringCol", SqlServerTypes.VARCHAR, 400, false),
                        new TableColumnInfo("DateTimeCol", SqlServerTypes.DATETIME, 1, false),
                        new TableColumnInfo("BoolCol", SqlServerTypes.BIT, 1, false),
                        new TableColumnInfo("IntCol", SqlServerTypes.INT, 20, true),
                        new TableColumnInfo("FkCol", SqlServerTypes.INT, 20, true, fkName: "Fk_name"),
                        new TableColumnInfo("PkCol", SqlServerTypes.INT, 20, true, pkName: "Pk_name"),
                    }
                );

            mockedDbUtils
                .Setup(x => x.GetTableColumnsInfo("TblFake2"))
                .Returns(
                    new List<TableColumnInfo>() {
                        new TableColumnInfo("DecimalCol", SqlServerTypes.DECIMAL, 40, false),
                        new TableColumnInfo("CharCol", SqlServerTypes.CHAR, 1, false)
                    }
                );

            mockedDbUtils
                .Setup(x => x.GetDotNetTypeFromSqlType(It.IsAny<SqlServerTypes>()))
                .Returns((SqlServerTypes sqlType) =>
                {
                    return realDbUtils.GetDotNetTypeFromSqlType(sqlType);
                });

            var data = new Data();

            data.DataContent["TblFake1"] =
                new List<TableRegister>()
                {
                    new TableRegister(
                        new TableRegisterUnit("StringCol", "texto texto texto"),
                        new TableRegisterUnit("DateTimeCol", "2012-11-10 18:53:10"),
                        new TableRegisterUnit("BoolCol", "1"),
                        new TableRegisterUnit("IntCol", "68447"),
                        new TableRegisterUnit("PkCol", "3486")
                    ),
                    new TableRegister(
                        new TableRegisterUnit("StringCol", "texto texto texto texto"),
                        new TableRegisterUnit("DateTimeCol", "2012-12-05 17:03:01"),
                        new TableRegisterUnit("BoolCol", "0"),
                        new TableRegisterUnit("FkCol", "673")
                    )
                };

            data.DataContent["TblFake2"] =
                new List<TableRegister>()
                {
                    new TableRegister(
                        new TableRegisterUnit("DecimalCol", "485.51"),
                        new TableRegisterUnit("CharCol", "F")
                    )
                };

            var validator = new SqlServerDataValidator(mockedDbUtils.Object);
            validator.ValidateImportedData(data);

            Assert.AreEqual(2, data.ReferencedTables.Length);
            Assert.IsTrue(data.ReferencedTables.Contains("TblFake1"));
            Assert.IsTrue(data.ReferencedTables.Contains("TblFake2"));

            Assert.AreEqual(2, data.DataContent["TblFake1"].Count);
            Assert.AreEqual(1, data.DataContent["TblFake2"].Count);

            AssertValidatedData(data, "TblFake1", 0, 0, "StringCol", SqlServerTypes.VARCHAR, 400, false, false, null, false, null, "texto texto texto", "texto texto texto");
            AssertValidatedData(data, "TblFake1", 0, 1, "DateTimeCol", SqlServerTypes.DATETIME, 1, false, false, null, false, null, "2012-11-10 18:53:10", new DateTime(2012, 11, 10, 18, 53, 10));
            AssertValidatedData(data, "TblFake1", 0, 2, "BoolCol", SqlServerTypes.BIT, 1, false, false, null, false, null, "1", true);
            AssertValidatedData(data, "TblFake1", 0, 3, "IntCol", SqlServerTypes.INT, 20, true, false, null, false, null, "68447", (int)68447);
            AssertValidatedData(data, "TblFake1", 0, 4, "PkCol", SqlServerTypes.INT, 20, true, true, "Pk_name", false, null, "3486", (int)3486);

            AssertValidatedData(data, "TblFake1", 1, 0, "StringCol", SqlServerTypes.VARCHAR, 400, false, false, null, false, null, "texto texto texto texto", "texto texto texto texto");
            AssertValidatedData(data, "TblFake1", 1, 1, "DateTimeCol", SqlServerTypes.DATETIME, 1, false, false, null, false, null, "2012-12-05 17:03:01", new DateTime(2012, 12, 05, 17, 03, 01));
            AssertValidatedData(data, "TblFake1", 1, 2, "BoolCol", SqlServerTypes.BIT, 1, false, false, null, false, null, "0", false);
            AssertValidatedData(data, "TblFake1", 1, 3, "FkCol", SqlServerTypes.INT, 20, true, false, null, true, "Fk_name", "673", (int)673);

            AssertValidatedData(data, "TblFake2", 0, 0, "DecimalCol", SqlServerTypes.DECIMAL, 40, false, false, null, false, null, "485.51", (decimal)485.51);
            AssertValidatedData(data, "TblFake2", 0, 1, "CharCol", SqlServerTypes.CHAR, 1, false, false, null, false, null, "F", 'F');
        }

        private void AssertValidatedData(
            Data data, string tblName, int regIndex, int dataUnitIndex, string colName, SqlServerTypes colType,
            int colLength, bool colNullable, bool colIsPk, string colPkName, bool colIsFk, string colFkName,
            string colValueAsString, object colValueAsObject
            )
        {
            Assert.AreEqual(colName, data.DataContent[tblName][regIndex].RegisterData[dataUnitIndex].ColInfo.Name);
            Assert.AreEqual(colType, data.DataContent[tblName][regIndex].RegisterData[dataUnitIndex].ColInfo.Type);
            Assert.AreEqual(colLength, data.DataContent[tblName][regIndex].RegisterData[dataUnitIndex].ColInfo.Length);
            Assert.AreEqual(colNullable, data.DataContent[tblName][regIndex].RegisterData[dataUnitIndex].ColInfo.IsNullable);
            Assert.AreEqual(colIsPk, data.DataContent[tblName][regIndex].RegisterData[dataUnitIndex].ColInfo.IsPk);
            Assert.AreEqual(colPkName, data.DataContent[tblName][regIndex].RegisterData[dataUnitIndex].ColInfo.PkName);
            Assert.AreEqual(colIsFk, data.DataContent[tblName][regIndex].RegisterData[dataUnitIndex].ColInfo.IsFk);
            Assert.AreEqual(colFkName, data.DataContent[tblName][regIndex].RegisterData[dataUnitIndex].ColInfo.FkName);
            Assert.AreEqual(colValueAsString, data.DataContent[tblName][regIndex].RegisterData[dataUnitIndex].ValueAsString);
            Assert.AreEqual(colValueAsObject, data.DataContent[tblName][regIndex].RegisterData[dataUnitIndex].ValueAsObject);
        }
    }
}
