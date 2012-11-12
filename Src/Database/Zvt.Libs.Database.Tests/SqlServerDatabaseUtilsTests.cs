using System;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Zvt.Libs.Database;

namespace Zvt.Libs.Database.Tests
{
    [TestClass]
    public class SqlServerDatabaseUtilsTests
    {
        protected Mock<ISqlServerDatabaseHelper> MockedDbHelper { get; set; }

        protected SqlServerDatabaseUtils GetDbUtilsForTests()
        {
            this.MockedDbHelper = new Mock<ISqlServerDatabaseHelper>();
            var dbUtils = new SqlServerDatabaseUtils(this.MockedDbHelper.Object);
            return dbUtils;
        }

        [TestMethod]
        public void Test_MountTableColumnInfoList()
        {
            var dt = new DataTable("TestTable");
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Type", typeof(string));
            dt.Columns.Add("Length", typeof(int));
            dt.Columns.Add("Nullable", typeof(byte));
            dt.Columns.Add("PrimaryKey", typeof(string));
            dt.Columns.Add("ForeignKey", typeof(string));

            var row1 = dt.NewRow();
            row1["Name"] = "Column A";
            row1["Type"] = "varchar";
            row1["Length"] = 40;
            row1["Nullable"] = 1;
            row1["PrimaryKey"] = "PK_TestTable_01";
            row1["ForeignKey"] = DBNull.Value;
            dt.Rows.Add(row1);

            var row2 = dt.NewRow();
            row2["Name"] = "Column B";
            row2["Type"] = "int";
            row2["Length"] = 4;
            row2["Nullable"] = 0;
            row2["PrimaryKey"] = DBNull.Value;
            row2["ForeignKey"] = "FK_01";
            dt.Rows.Add(row2);

            var dbUtils = GetDbUtilsForTests();
            var tblInfo = dbUtils.MountTableColumnInfoList(dt);

            Assert.AreEqual("Column A", tblInfo[0].Name);
            Assert.AreEqual(SqlServerTypes.VARCHAR, tblInfo[0].Type);
            Assert.AreEqual(40, tblInfo[0].Length);
            Assert.AreEqual(true, tblInfo[0].IsNullable);
            Assert.AreEqual(true, tblInfo[0].IsPk);
            Assert.AreEqual("PK_TestTable_01", tblInfo[0].PkName);
            Assert.AreEqual(false, tblInfo[0].IsFk);
            Assert.AreEqual(null, tblInfo[0].FkName);

            Assert.AreEqual("Column B", tblInfo[1].Name);
            Assert.AreEqual(SqlServerTypes.INT, tblInfo[1].Type);
            Assert.AreEqual(4, tblInfo[1].Length);
            Assert.AreEqual(false, tblInfo[1].IsNullable);
            Assert.AreEqual(false, tblInfo[1].IsPk);
            Assert.AreEqual(null, tblInfo[1].PkName);
            Assert.AreEqual(true, tblInfo[1].IsFk);
            Assert.AreEqual("FK_01", tblInfo[1].FkName);
        }

        

        /*
        [TestMethod]
        public void Test_GetTableColumnsInfo()
        {
            var scfm = new SystemConfigurationManagerWrapper();
            var sm = new SettingsManager(scfm);
            ISqlServerDatabaseHelper db = new SqlServerDatabaseHelper(sm, "db");

            var dt = db.GetTableColumnsInfo("DoctorOfflineContacts");
        }
        */
    }
}
