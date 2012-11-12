using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Zvt.Libs.Configurations;

namespace Zvt.Libs.Database
{
    public class SqlServerDatabaseHelper : ISqlServerDatabaseHelper
    {
        protected ISettingsManager SettingsManager { get; set; }
        protected string ConnectionStringName { get; set; }
        protected SqlConnection TransactionConnection { get; set; }
        protected SqlTransaction Transaction { get; set; }

        /// <summary>
        /// Indicates if a database transaction is active.
        /// </summary>
        public bool IsTransactionActive { get; protected set; }

        public SqlServerDatabaseHelper(
            ISettingsManager settingsManager,
            string connectionStringName
            )
        {
            if (settingsManager == null)
                throw new ArgumentNullException("settingsManager");

            if (connectionStringName == null)
                throw new ArgumentNullException("connectionStringName");

            this.SettingsManager = settingsManager;
            this.ConnectionStringName = connectionStringName;

            this.IsTransactionActive = false;
            this.TransactionConnection = null;
            this.Transaction = null;
        }

        protected SqlConnection GetConnection()
        {
            if (IsTransactionActive)
            {
                return TransactionConnection;
            }
            else
            {
                return
                    new SqlConnection(
                        SettingsManager.GetDatabaseConnectionString(this.ConnectionStringName, true)
                    );
            }
        }
        protected SqlCommand GetCommand(
            string sql,
            Dictionary<string, object> sqlParams = null
            )
        {
            if (sql == null)
                throw new ArgumentNullException("sql");

            var objConn = GetConnection();
            var objCmd = new SqlCommand(sql, objConn);

            if (IsTransactionActive)
                objCmd.Transaction = Transaction;

            if (sqlParams != null)
                foreach (var key in sqlParams.Keys)
                    objCmd.Parameters.AddWithValue(key, sqlParams[key]);

            return objCmd;
        }

        /// <summary>
        /// Test the connection with the database.
        /// </summary>
        /// <param name="errMsg">If connection fails, returns an error message.</param>
        /// <returns>True if the connection could be established.</returns>
        public bool VerifyConnection(out string errMsg)
        {
            errMsg = string.Empty;
            var conn = GetConnection();

            try
            {
                conn.Open();
                conn.Close();
                conn.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        public void BeginTransaction()
        {
            if (IsTransactionActive) return;

            TransactionConnection = GetConnection();
            TransactionConnection.Open();
            Transaction = TransactionConnection.BeginTransaction();
            IsTransactionActive = true;
        }

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        public void CommitTransaction()
        {
            if (!IsTransactionActive) return;

            Transaction.Commit();
            TransactionConnection.Close();
            IsTransactionActive = false;
        }

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        public void RollbackTransaction()
        {
            if (!IsTransactionActive) return;

            Transaction.Rollback();
            TransactionConnection.Close();
            IsTransactionActive = false;
        }

        /// <summary>
        /// Executes a Transact-SQL statement and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Transact-SQL statement.</param>
        /// <param name="sqlParams">Transact-SQL statement parameters.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuery(string sql, Dictionary<string, object> sqlParams = null)
        {
            var objCmd = GetCommand(sql, sqlParams);

            if (!IsTransactionActive)
                objCmd.Connection.Open();

            var affectedRows = objCmd.ExecuteNonQuery();

            if (!IsTransactionActive)
                objCmd.Connection.Close();

            return affectedRows;
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set
        /// returned by the query, or a null reference if the result set is empty.
        /// Additional columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Transact-SQL statement.</param>
        /// <param name="sqlParams">Transact-SQL statement parameters.</param>
        /// <returns>
        /// The first column of the first row in the result set returned by the query, or a null reference if the result set is empty.
        /// </returns>
        public object ExecuteScalarOrNull(string sql, Dictionary<string, object> sqlParams = null)
        {
            var objCmd = GetCommand(sql, sqlParams);

            if (!IsTransactionActive)
                objCmd.Connection.Open();

            var value = objCmd.ExecuteScalar();

            if (!IsTransactionActive)
                objCmd.Connection.Close();

            return value;
        }

        /// <summary>
        /// Executes the query, and returns the typed value of the first column of the first row in the result set
        /// returned by the query, or a null reference if the result set is empty.
        /// Additional columns or rows are ignored.
        /// </summary>
        /// <param name="sql">Transact-SQL statement.</param>
        /// <param name="sqlParams">Transact-SQL statement parameters.</param>
        /// <returns>
        /// The typed value of the first column of the first row in the result set returned by the query,
        /// or a null reference if the result set is empty.
        /// </returns>
        public TResult ExecuteScalar<TResult>(string sql, Dictionary<string, object> sqlParams = null)
        {
            var objValue = ExecuteScalarOrNull(sql, sqlParams);

            try
            {
                var typedValue = Convert.ChangeType(objValue, typeof(TResult));
                return (TResult)typedValue;
            }
            catch (Exception ex)
            {
                throw new Exception("The query result could not be converted to requested type.", ex);
            }
        }

        /// <summary>
        /// Executes the query, and returns the first table in the result set returned by the query, or an empty DataTable if the result set is empty.
        /// Additional tables are ignored.
        /// </summary>
        /// <param name="sql">Transact-SQL statement.</param>
        /// <param name="sqlParams">Transact-SQL statement parameters.</param>
        /// <returns>
        /// The first table in the result set returned by the query, or an empty DataTable if the result set is empty.
        /// </returns>
        public DataTable ExecuteQueryAndReturnTable(string sql, Dictionary<string, object> sqlParams = null)
        {
            var objCmd = GetCommand(sql, sqlParams);
            var objAdapter = new SqlDataAdapter(objCmd);
            var dt = new DataTable();
            objAdapter.Fill(dt);
            objAdapter.Dispose();
            return dt;
        }

        /// <summary>
        /// Executes the query, and returns all the result set, or an empty SataSet if the result set is empty.
        /// </summary>
        /// <param name="sql">Transact-SQL statement.</param>
        /// <param name="sqlParams">Transact-SQL statement parameters.</param>
        /// <returns>
        /// All the result set, or an empty SataSet if the result set is empty.
        /// </returns>
        public DataSet ExecuteQuery(string sql, Dictionary<string, object> sqlParams = null)
        {
            var objCmd = GetCommand(sql, sqlParams);
            var objAdapter = new SqlDataAdapter(objCmd);
            var ds = new DataSet();
            objAdapter.Fill(ds);
            objAdapter.Dispose();
            return ds;
        }

        /// <summary>
        /// Executes the stored procedure, and returns the first table in the result set returned by the query, or an empty DataTable if the result set is empty.
        /// Additional tables are ignored.
        /// </summary>
        /// <param name="spName">Stored procedure name.</param>
        /// <param name="sqlParams">Transact-SQL statement parameters.</param>
        /// <returns>
        /// The first table in the result set returned by the query, or an empty DataTable if the result set is empty.
        /// </returns>
        public DataTable ExecuteStoredProcedureAndReturnTable(string spName, Dictionary<string, object> sqlParams = null)
        {
            var objCmd = GetCommand(spName, sqlParams);
            objCmd.CommandType = CommandType.StoredProcedure;
            var objAdapter = new SqlDataAdapter(objCmd);
            var dt = new DataTable();
            objAdapter.Fill(dt);
            objAdapter.Dispose();
            return dt;
        }

        /// <summary>
        /// Executes the stored procedure, and returns all the result set, or an empty SataSet if the result set is empty.
        /// </summary>
        /// <param name="spName">Stored procedure name.</param>
        /// <param name="sqlParams">Transact-SQL statement parameters.</param>
        /// <returns>
        /// All the result set, or an empty SataSet if the result set is empty.
        /// </returns>
        public DataSet ExecuteStoredProcedure(string spName, Dictionary<string, object> sqlParams = null)
        {
            var objCmd = GetCommand(spName, sqlParams);
            objCmd.CommandType = CommandType.StoredProcedure;
            var objAdapter = new SqlDataAdapter(objCmd);
            var ds = new DataSet();
            objAdapter.Fill(ds);
            objAdapter.Dispose();
            return ds;
        }
    }
}
