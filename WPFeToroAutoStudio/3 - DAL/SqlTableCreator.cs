using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace Applenium
{
    /// <summary>
    ///     SqlTableCreator - public class to deal with Tables creating in DB
    /// </summary>
    public class SqlTableCreator
    {
        private string _tableName;

        /// <summary>
        ///     Empty consructor for TableCreator class
        /// </summary>
        public SqlTableCreator()
        {
        }

        /// <summary>
        ///     Consructor for TableCreator class
        /// </summary>
        public SqlTableCreator(SqlConnection connection)
            : this(connection, null)
        {
        }

        /// <summary>
        ///     Consructor for TableCreator class
        /// </summary>
        private SqlTableCreator(SqlConnection connection, SqlTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }

        /// <summary>
        ///     Connection - public property for connection string
        /// </summary>
        public SqlConnection Connection { private get; set; }

        /// <summary>
        ///     Transaction - public property for transaction
        /// </summary>
        private SqlTransaction Transaction { get; set; }

        /// <summary>
        ///     DestinationTable - public property for DestinationTable to create
        /// </summary>
        public string DestinationTableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }

        /// <summary>
        ///     Create schema in DB
        /// </summary>
        public object Create(DataTable schema)
        {
            return Create(schema, null);
        }

        /// <summary>
        ///     Create schema in DB with primary key
        /// </summary>
        public object Create(DataTable schema, int numKeys)
        {
            var primaryKeys = new int[numKeys];
            for (int i = 0; i < numKeys; i++)
            {
                primaryKeys[i] = i;
            }
            return Create(schema, primaryKeys);
        }

        /// <summary>
        ///     Create schema in DB with set of primary keys
        /// </summary>
        private object Create(DataTable schema, int[] primaryKeys)
        {
            string sql = GetCreateSql(_tableName, schema, primaryKeys);

            SqlCommand cmd;
            if (Transaction != null && Transaction.Connection != null)
            {
                cmd = new SqlCommand(sql, Connection, Transaction);
            }
            else
            {
                cmd = new SqlCommand(sql, Connection);
            }
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        ///     Create table based on DataTable
        /// </summary>
        public object CreateFromDataTable(DataTable table)
        {
            string sql = GetCreateFromDataTableSql(_tableName, table);

            SqlCommand cmd;
            if (Transaction != null && Transaction.Connection != null)
            {
                cmd = new SqlCommand(sql, Connection, Transaction);
            }
            else
            {
                cmd = new SqlCommand(sql, Connection);
            }
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        ///     Create table in DN based on SQl query
        /// </summary>
        private static string GetCreateSql(string tableName, DataTable schema, int[] primaryKeys)
        {
            string sql = "CREATE TABLE [" + tableName + "] (\n";

            foreach (DataRow column in schema.Rows)
            {
                if (!(schema.Columns.Contains("IsHidden") && (bool) column["IsHidden"]))
                {
                    sql += "\t[" + column["ColumnName"] + "] " + SqlGetType(column);

                    if (schema.Columns.Contains("AllowDBNull") && (bool) column["AllowDBNull"] == false)
                    {
                        sql += " NOT NULL";
                    }
                    sql += ",\n";
                }
            }
            sql = sql.TrimEnd(new[] {',', '\n'}) + "\n";

            string pk = ", CONSTRAINT PK_" + tableName + " PRIMARY KEY CLUSTERED (";
            bool hasKeys = (primaryKeys != null && primaryKeys.Length > 0);
            if (hasKeys)
            {
                foreach (int key in primaryKeys)
                {
                    pk += schema.Rows[key]["ColumnName"] + ", ";
                }
            }
            else
            {
                string keys = string.Join(", ", GetPrimaryKeys(schema));
                pk += keys;
                hasKeys = keys.Length > 0;
            }
            pk = pk.TrimEnd(new[] {',', ' ', '\n'}) + ")\n";
            if (hasKeys)
            {
                sql += pk;
            }
            sql += ")";

            return sql;
        }

        /// <summary>
        ///     Creates table in DB with certain number of columns
        /// </summary>
        public object Create(string tableName, int columnsNumber)
        {
            int numofCol = columnsNumber;
            string sql = "CREATE TABLE " + tableName + " (";
            sql += "[rowID] " + "[int] IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED" + ",";
            for (int column = 1; column <= numofCol; column++)
            {
                sql += "[column" + column + "] " + "[varchar](255) NULL" + ",";
            }
            sql = sql.TrimEnd(new[] {',', '\n'});

            if (!sql.EndsWith(")"))
            {
                sql += ")";
            }


            SqlCommand cmd;
            if (Transaction != null && Transaction.Connection != null)
            {
                cmd = new SqlCommand(sql, Connection, Transaction);
            }
            else
            {
                cmd = new SqlCommand(sql, Connection);
            }
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        ///     Return SQL based on Data table and Table name
        /// </summary>
        private static string GetCreateFromDataTableSql(string tableName, DataTable table)
        {
            string sql = "CREATE TABLE " + tableName + " (";
            foreach (DataColumn column in table.Columns)
            {
                sql += "[" + column.ColumnName + "] " + "[" + SqlGetType(column).ToLower() + "]" + ",";
            }
            sql = sql.TrimEnd(new[] {',', '\n'});
            if (table.PrimaryKey.Length > 0)
            {
                sql += "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED (";
                foreach (DataColumn column in table.PrimaryKey)
                {
                    sql += "[" + column.ColumnName + "],";
                }
                sql = sql.TrimEnd(new[] {','}) + "))\n";
            }

            if ((table.PrimaryKey.Length == 0) && (!sql.EndsWith(")")))
            {
                sql += ")";
            }

            return sql;
        }

        /// <summary>
        ///     Get primary key from DB
        /// </summary>
        private static string[] GetPrimaryKeys(DataTable schema)
        {
            var keys = new List<string>();

            foreach (DataRow column in schema.Rows)
            {
                if (schema.Columns.Contains("IsKey") && (bool) column["IsKey"])
                {
                    keys.Add(column["ColumnName"].ToString());
                }
            }

            return keys.ToArray();
        }

        /// <summary>
        ///     Return DB column type
        /// </summary>
        private static string SqlGetType(object type, int columnSize, int numericPrecision, int numericScale)
        {
            switch (type.ToString())
            {
                case "System.String":
                    return "VARCHAR(" +
                           ((columnSize == -1)
                                ? "255"
                                : (columnSize > 8000) ? "MAX" : columnSize.ToString(CultureInfo.InvariantCulture)) + ")";

                case "System.Decimal":
                    if (numericScale > 0)
                    {
                        return "REAL";
                    }
                    else
                    {
                        if (numericPrecision > 10)
                        {
                            return "BIGINT";
                        }
                        else
                        {
                            return "INT";
                        }
                    }
                case "System.Double":
                case "System.Single":
                    return "REAL";

                case "System.Int64":
                    return "BIGINT";

                case "System.Int16":
                case "System.Int32":
                    return "INT";

                case "System.DateTime":
                    return "DATETIME";

                case "System.Boolean":
                    return "BIT";

                case "System.Byte":
                    return "TINYINT";

                case "System.Guid":
                    return "UNIQUEIDENTIFIER";

                default:
                    throw new Exception(type + " not implemented.");
            }
        }

        /// <summary>
        ///     SQLGetType
        /// </summary>
        private static string SqlGetType(DataRow schemaRow)
        {
            return SqlGetType(schemaRow["DataType"],
                              int.Parse(schemaRow["ColumnSize"].ToString()),
                              int.Parse(schemaRow["NumericPrecision"].ToString()),
                              int.Parse(schemaRow["NumericScale"].ToString()));
        }

        /// <summary>
        ///     Return type of Column in Table from Data Base
        /// </summary>
        private static string SqlGetType(DataColumn column)
        {
            return SqlGetType(column.DataType, column.MaxLength, 10, 2);
        }

        /// <summary>
        ///     Insert data to Table in Data Base ( table already created)
        /// </summary>
        public void BulkInsertDataTable(string connectionString, string tableName, DataTable table)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var bulkCopy =
                    new SqlBulkCopy
                        (
                        connection,
                        SqlBulkCopyOptions.TableLock |
                        SqlBulkCopyOptions.FireTriggers |
                        SqlBulkCopyOptions.UseInternalTransaction,
                        null
                        ) {DestinationTableName = tableName};

                connection.Open();

                bulkCopy.WriteToServer(table);
                connection.Close();
            }
        }
    }
}