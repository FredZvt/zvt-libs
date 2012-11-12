using System.Collections.Generic;
using System.Data;

namespace Zvt.Libs.Database
{
    public interface ISqlServerDatabaseHelper
    {
        /// <summary>
        /// Test the connection with the database.
        /// </summary>
        /// <param name="errMsg">If connection fails, returns an error message.</param>
        /// <returns>True if the connection could be established.</returns>
        bool VerifyConnection(out string errMsg);

        /// <summary>
        /// Indicates if a database transaction is active.
        /// </summary>
        bool IsTransactionActive { get; }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Executes a Transact-SQL statement and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">Transact-SQL statement.</param>
        /// <param name="sqlParams">Transact-SQL statement parameters.</param>
        /// <returns>The number of rows affected.</returns>
        int ExecuteNonQuery(string sql, Dictionary<string, object> sqlParams = null);

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
        object ExecuteScalarOrNull(string sql, Dictionary<string, object> sqlParams = null);

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
        TResult ExecuteScalar<TResult>(string sql, Dictionary<string, object> sqlParams = null);

        /// <summary>
        /// Executes the query, and returns the first table in the result set returned by the query, or an empty DataTable if the result set is empty.
        /// Additional tables are ignored.
        /// </summary>
        /// <param name="sql">Transact-SQL statement.</param>
        /// <param name="sqlParams">Transact-SQL statement parameters.</param>
        /// <returns>
        /// The first table in the result set returned by the query, or an empty DataTable if the result set is empty.
        /// </returns>
        DataTable ExecuteQueryAndReturnTable(string sql, Dictionary<string, object> sqlParams = null);

        /// <summary>
        /// Executes the query, and returns all the result set, or an empty SataSet if the result set is empty.
        /// </summary>
        /// <param name="sql">Transact-SQL statement.</param>
        /// <param name="sqlParams">Transact-SQL statement parameters.</param>
        /// <returns>
        /// All the result set, or an empty SataSet if the result set is empty.
        /// </returns>
        DataSet ExecuteQuery(string sql, Dictionary<string, object> sqlParams = null);

        /// <summary>
        /// Executes the stored procedure, and returns the first table in the result set returned by the query, or an empty DataTable if the result set is empty.
        /// Additional tables are ignored.
        /// </summary>
        /// <param name="spName">Stored procedure name.</param>
        /// <param name="sqlParams">Transact-SQL statement parameters.</param>
        /// <returns>
        /// The first table in the result set returned by the query, or an empty DataTable if the result set is empty.
        /// </returns>
        DataTable ExecuteStoredProcedureAndReturnTable(string spName, Dictionary<string, object> sqlParams = null);

        /// <summary>
        /// Executes the stored procedure, and returns all the result set, or an empty SataSet if the result set is empty.
        /// </summary>
        /// <param name="spName">Stored procedure name.</param>
        /// <param name="sqlParams">Transact-SQL statement parameters.</param>
        /// <returns>
        /// All the result set, or an empty SataSet if the result set is empty.
        /// </returns>
        DataSet ExecuteStoredProcedure(string spName, Dictionary<string, object> sqlParams = null);
    }
}
