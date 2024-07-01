using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;

using System.IO;
using System.Data.SQLite;

namespace MadTomDev.Data
{

    public class SQLiteHelper : IDisposable
    {
        public SQLiteConnection sqliteConn;
        private Timer closeConnTimer;

        private string _dbFileFullName, _password;
        private bool _readOnly;
        private SyncModes _syncMode;
        public enum SyncModes
        {
            /// <summary>
            /// flush after critical sections
            /// </summary>
            Normal = 0,
            /// <summary>
            /// flush after every write
            /// </summary>
            Full = 1,
            /// <summary>
            /// flush by os
            /// </summary>
            Off = 2,
        }

        /// <summary>
        /// 创建一个新的Sqlite数据库实例；
        /// * 只有4中基本数据类型INTEGER、REAL、TEXT 和 BLOB；
        /// </summary>
        /// <param name="dbFileFullName">数据库文件路径，为空且打开模式为创建时创建</param>
        /// <param name="password">* 仅当本地sqlite（默认不支持）支持加密时才有效</param>
        /// <param name="readOnly"></param>
        /// <param name="syncMode"></param>
        /// <param name="enableForeignKeys">默认null-不发送外键约束设定，看sqlite默认行为； true-启用外键约束；false-不启用外键约束</param>
        /// <param name="recursiveTriggers">默认false-不启用触发器；true-启用触发器</param>
        /// <param name="isAutoCloseConn"></param>
        /// <param name="autoCloseConnTimeoutMiliSec"></param>
        public SQLiteHelper(string dbFileFullName, string password = null,
            bool readOnly = false, SyncModes syncMode = SyncModes.Off,
            bool enableForeignKeys = false, bool recursiveTriggers = false,
            bool isAutoCloseConn = false, uint autoCloseConnTimeoutMiliSec = 5000)
        {
            _dbFileFullName = dbFileFullName;
            _password = password;
            _readOnly = readOnly;
            _syncMode = syncMode;
            _EnableForeignKeys = enableForeignKeys;
            _EnableRecursiveTriggers = recursiveTriggers;

            TryReOpenConnection();

            AutoCloseConnectionTimeoutMiliSec = autoCloseConnTimeoutMiliSec;
            IsAutoCloseConnection = isAutoCloseConn;
        }


        private bool _EnableForeignKeys = false;
        public bool EnableForeignKeys
        {
            get => _EnableForeignKeys;
            set
            {
                if (_EnableForeignKeys == value)
                    return;

                if (value)
                    ExecuteNonQuery("PRAGMA foreign_keys = ON;");
                else
                    ExecuteNonQuery("PRAGMA foreign_keys = OFF;");
                _EnableForeignKeys = value;
            }
        }

        private bool _EnableRecursiveTriggers = false;
        public bool EnableRecursiveTriggers
        {
            get => _EnableRecursiveTriggers;
            set
            {
                if (_EnableRecursiveTriggers == value)
                    return;

                if (value)
                    ExecuteNonQuery("PRAGMA recursive_triggers = true;");
                else
                    ExecuteNonQuery("PRAGMA recursive_triggers = false;");
                _EnableRecursiveTriggers = value;
            }
        }

        private void TryReOpenConnection()
        {
            if (sqliteConn == null)
            {
                SQLiteConnectionStringBuilder conStrBdr = new SQLiteConnectionStringBuilder()
                {
                    DataSource = _dbFileFullName,
                    Password = _password,
                    ReadOnly = _readOnly,
                    SyncMode = (SynchronizationModes)_syncMode,

                    ForeignKeys = _EnableForeignKeys,
                    RecursiveTriggers = _EnableRecursiveTriggers,
                };
                sqliteConn = new SQLiteConnection(conStrBdr.ToString());
                sqliteConn.Open();
            }
            DBLastActTime = DateTime.Now;
        }

        private DateTime DBLastActTime;
        public bool IsAutoCloseConnection
        {
            set
            {
                if (value && closeConnTimer == null)
                {
                    closeConnTimer = new Timer(new TimerCallback(CloseConnTimerCallback), this,
                        1000, 1000);
                }
                else if (closeConnTimer != null)
                {
                    closeConnTimer.Dispose();
                    closeConnTimer = null;
                }
            }
            get
            {
                return closeConnTimer != null;
            }
        }
        private void CloseConnTimerCallback(object args)
        {
            if (sqliteConn != null
                && (DateTime.Now - DBLastActTime).TotalMilliseconds > AutoCloseConnectionTimeoutMiliSec)
            {
                sqliteConn?.Close();
                sqliteConn = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public uint AutoCloseConnectionTimeoutMiliSec
        { set; get; } = 5000;
        public void KeepConnect()
        {
            DBLastActTime = DateTime.Now;
            TryReOpenConnection();
        }

        public SQLiteCommand GetNewCommand()
        {
            if (IsAutoCloseConnection)
            {
                KeepConnect();
            }
            return sqliteConn.CreateCommand();
        }
        public SQLiteTransaction GetNewTransaction()
        {
            if (IsAutoCloseConnection)
            {
                KeepConnect();
            }
            return sqliteConn.BeginTransaction();
        }
        private SQLiteTransaction transaction;
        public void BeginTransaction()
        {
            if (IsAutoCloseConnection)
            {
                KeepConnect();
            }
            transaction = sqliteConn.BeginTransaction();
        }
        public void EndTransaction(bool commit = true)
        {
            if (transaction != null)
            {
                if (commit)
                    transaction.Commit();
                else
                    transaction.Rollback();
            }
            transaction = null;
        }

        public void ExecuteNonQuery(string cmdText)
        {
            if (IsAutoCloseConnection)
            {
                KeepConnect();
            }
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            cmd.CommandText = cmdText;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        #region basic check, get

        public SQLiteDataReader ExcuteQueryReader(string cmdText)
        {
            if (IsAutoCloseConnection)
            {
                KeepConnect();
            }
            SQLiteCommand cmd = sqliteConn.CreateCommand();
            cmd.CommandText = cmdText;
            return cmd.ExecuteReader();
        }
        public string[] VirtualTableNameList
        {
            get
            {
                SQLiteDataReader reader = ExcuteQueryReader("SELECT name FROM sqlite_master WHERE type='view';"); //ORDER BY name
                List<string> result = new List<string>();
                while (reader.Read())
                {
                    result.Add(reader["name"].ToString());
                }
                reader.Close();
                return result.ToArray();
            }
        }
        public bool HaveVirtualTable(string vtName)
        {
            SQLiteDataReader reader = ExcuteQueryReader("SELECT count(*) FROM sqlite_master WHERE type='view' and name='" + vtName + "';");
            if (reader.Read())
            {
                bool result = (reader.GetInt16(0) > 0);
                reader.Close();
                return result;
            }
            reader.Close();
            return false;
        }

        public string[] TableNameList
        {
            get
            {
                SQLiteDataReader reader = ExcuteQueryReader("SELECT name FROM sqlite_master WHERE type='table';"); //ORDER BY name
                List<string> result = new List<string>();
                while (reader.Read())
                {
                    result.Add(reader["name"].ToString());
                }
                reader.Close();
                return result.ToArray();
            }
        }
        public bool HaveTable(string tabName)
        {
            SQLiteDataReader reader = ExcuteQueryReader("SELECT count(*) FROM sqlite_master WHERE type='table' and name='" + tabName + "';");
            if (reader.Read())
            {
                bool result = (reader.GetInt16(0) > 0);
                reader.Close();
                return result;
            }
            reader.Close();
            return false;
        }
        public bool HaveIndex(string idxName)
        {
            SQLiteDataReader reader = ExcuteQueryReader("SELECT count(*) FROM sqlite_master WHERE type='index' and name='" + idxName + "';");
            if (reader.Read())
            {
                bool result = (reader.GetInt16(0) > 0);
                reader.Close();
                return result;
            }
            reader.Close();
            return false;
        }

        /// <summary>
        /// returns the row count of a table
        /// -1, no table found; 0-n rows of the table;
        /// </summary>
        /// <param name="tabName"></param>
        /// <returns></returns>
        public long TableRowCount(string tabName)
        {
            long result = -1;
            using (SQLiteDataReader reader
                = ExcuteQueryReader(
                    "SELECT count(*) FROM " + tabName + ";"))
            {
                if (reader.Read())
                {
                    result = reader.GetInt64(0);
                }
            }
            return result;
        }
        public long TableRowCount(string tabName, string conditionField, object conditionValue)
        {
            return TableRowCount(tabName, new string[] { conditionField }, new object[] { conditionValue });
        }
        public long TableRowCount(string tabName, string[] conditionFields, object[] conditionValues)
        {
            long result = -1;
            using (SQLiteCommand cmd = GetNewCommand())
            {
                StringBuilder cmdBdr = new StringBuilder();
                cmdBdr.Append("SELECT count(*) FROM ");
                cmdBdr.Append(tabName);
                cmdBdr.Append(" where ");
                int iv = Math.Min(conditionFields.Length, conditionValues.Length);
                string cTag;
                for (int i = 0; i < iv; i++)
                {
                    cmdBdr.Append(conditionFields[i]);
                    cmdBdr.Append("=");
                    cTag = "@c" + i;
                    cmdBdr.Append(cTag);
                    cmdBdr.Append(" and ");
                    cmd.Parameters.AddWithValue(cTag, conditionValues[i]);
                }
                if (iv > 0)
                    cmdBdr.Remove(cmdBdr.Length - 5, 5);

                cmdBdr.Append(";");
                cmd.CommandText = cmdBdr.ToString();
                using (DataTable dt = new DataTable())
                {
                    FillDataTable(cmd, dt);
                    result = (long)dt.Rows[0][0];
                }
            }
            return result;
        }

        public long TableRowCount(string tabName, string conditionField, object[] conditionValueRange)
        {
            return TableRowCount(tabName, new string[] { conditionField }, new object[][] { conditionValueRange });
        }
        public long TableRowCount(string tabName, string[] conditionFields, object[][] conditionValueRangeList)
        {
            long result = -1;
            using (SQLiteCommand cmd = GetNewCommand())
            {
                StringBuilder cmdBdr = new StringBuilder();
                cmdBdr.Append("SELECT count(*) FROM ");
                cmdBdr.Append(tabName);
                cmdBdr.Append(" where ");
                int iv = Math.Min(conditionFields.Length, conditionValueRangeList.Length);
                string cTag;
                int ci = 0;
                for (int i = 0; i < iv; i++)
                {
                    if (conditionValueRangeList[i].Length <= 0)
                        continue;

                    cmdBdr.Append(conditionFields[i]);
                    cmdBdr.Append(" in (");
                    foreach (object c in conditionValueRangeList[i])
                    {
                        cTag = "@c" + ci++;
                        cmdBdr.Append(cTag + ", ");
                        cmd.Parameters.AddWithValue(cTag, c);
                    }
                    cmdBdr.Remove(cmdBdr.Length - 2, 2);
                    cmdBdr.Append(") and ");
                }
                if (iv > 0)
                    cmdBdr.Remove(cmdBdr.Length - 5, 5);

                cmdBdr.Append(";");
                cmd.CommandText = cmdBdr.ToString();
                using (DataTable dt = new DataTable())
                {
                    FillDataTable(cmd, dt);
                    result = (long)dt.Rows[0][0];
                }
            }
            return result;
        }

        private void FillDataTable(SQLiteCommand cmd, DataTable dt)
        {
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.HasRows)
                    return;
                int iv = reader.FieldCount;
                for (int i = 0; i < iv; i++)
                {
                    dt.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                }


                DataRow newDR;
                while (reader.Read())
                {
                    newDR = dt.NewRow();
                    for (int i = 0; i < iv; i++)
                        newDR[i] = reader[i];

                    dt.Rows.Add(newDR);
                }
            }
        }

        public DataRow GetTableRowTemplete(string tableName)
        {
            DataTable dt = new DataTable(tableName);
            using (SQLiteCommand cmd = GetNewCommand())
            {
                cmd.CommandText = $"pragma table_info('{tableName}');";
                DataTable infoDt = new DataTable();
                FillDataTable(cmd, infoDt);
                string typeName;
                Type type = null;
                foreach (DataRow infoDr in infoDt.Rows)
                {
                    typeName = ((string)infoDr["type"]).ToLower();
                    if (typeName == "guid")
                        type = typeof(Guid);
                    else if (typeName == "text")
                        type = typeof(string);
                    else if (typeName == "int")
                        type = typeof(int);
                    else if (typeName == "float")
                        type = typeof(float);
                    else if (typeName == "double")
                        type = typeof(double);
                    else if (typeName == "decimal")
                        type = typeof(decimal);
                    else if (typeName == "bool")
                        type = typeof(bool);
                    else if (typeName == "datetime")
                        type = typeof(DateTime);
                    else
                        throw new Exception("Unknow type.");
                    dt.Columns.Add((string)infoDr["name"], type);
                }
            }
            return dt.NewRow();
        }

        #endregion

        // 写入stream大数据
        //        var insertCommand = connection.CreateCommand();
        //        insertCommand.CommandText =
        //@"
        //    INSERT INTO data(value)
        //    VALUES (zeroblob($length));

        //    SELECT last_insert_rowid();
        //";
        //insertCommand.Parameters.AddWithValue("$length", inputStream.Length);
        //var rowid = (long)insertCommand.ExecuteScalar();
        //        插入行后，打开一个流，以使用 SqliteBlob 写入大型对象。

        //C#

        //复制
        //using (var writeStream = new SqliteBlob(connection, "data", "value", rowid))
        //{
        //    // NB: Although SQLite doesn't support async, other types of streams do
        //    await inputStream.CopyToAsync(writeStream);
        //}


        // 读取stream大数据
        //        var selectCommand = connection.CreateCommand();
        //        selectCommand.CommandText =
        //@"
        //    SELECT id, value
        //    FROM data
        //    LIMIT 1
        //";
        //using (var reader = selectCommand.ExecuteReader())
        //{
        //    while (reader.Read())
        //    {
        //        using (var readStream = reader.GetStream(1))
        //        {
        //            await readStream.CopyToAsync(outputStream);
        //    }
        //}
        //}









        #region select, insert, update, delete
        public DataTable SelectAll(string tableName)
        {
            DataTable dt = new DataTable();
            using (SQLiteCommand cmd = GetNewCommand())
            {
                cmd.CommandText = $"select * from {tableName};";
                FillDataTable(cmd, dt);
            }
            return dt;
        }
        public DataTable SelectAll(string tableName, string[] returnFields)
        {
            return SelectAll(tableName, returnFields, null, null);
        }
        public DataTable SelectAll(string tableName, string conditionField, object conditionValue, int offSet = 0, int limit = 0)
        {
            return SelectAll(tableName, new string[] { conditionField }, new object[] { conditionValue }, offSet, limit);
        }
        public DataTable SelectAll(string tableName, string[] returnFields, string conditionField, object conditionValue, int offSet = 0, int limit = 0)
        {
            return SelectAll(tableName, returnFields, new string[] { conditionField }, new object[] { conditionValue }, offSet, limit);
        }

        public DataTable SelectAll(string tableName, string[] conditionFields, object[] conditionValues, int offSet = 0, int limit = 0)
        {
            return SelectAll(tableName, null, conditionFields, conditionValues, offSet, limit);
        }

        public DataTable SelectAll(string tableName, string[] returnFields, string[] conditionFields, object[] conditionValues, int offSet = 0, int limit = 0)
        {
            DataTable dt = new DataTable();
            using (SQLiteCommand cmd = GetNewCommand())
            {
                StringBuilder cmdBdr = new StringBuilder();
                cmdBdr.Append($"select ");
                if (returnFields == null || returnFields.Length <= 0)
                {
                    cmdBdr.Append("*");
                }
                else
                {
                    for (int i = 0, iv = returnFields.Length; i < iv; i++)
                        cmdBdr.Append(returnFields[i] + ",");
                    cmdBdr.Remove(cmdBdr.Length - 1, 1);
                }
                cmdBdr.Append($" from {tableName} ");
                if (conditionFields != null && conditionFields.Length > 0)
                {
                    cmdBdr.Append("where ");
                    StringBuilder cBdr = new StringBuilder();
                    int vIdx = 0;
                    string vFlag;
                    for (int i = 0, iv = conditionFields.Length; i < iv; i++)
                    {
                        cBdr.Append(conditionFields[i]);
                        cBdr.Append("=");
                        vFlag = "@c" + (vIdx++).ToString();
                        cBdr.Append(vFlag);
                        cBdr.Append(" and ");
                        cmd.Parameters.AddWithValue(vFlag, conditionValues[i]);
                    }
                    if (cBdr.Length > 5)
                        cBdr.Remove(cBdr.Length - 5, 5);
                    cmdBdr.Append(cBdr.ToString());
                }

                if (limit > 0)
                    cmdBdr.Append($" limit {limit}");
                if (offSet > 0)
                    cmdBdr.Append($" offset {offSet}");
                cmdBdr.Append(";");
                cmd.CommandText = cmdBdr.ToString();
                FillDataTable(cmd, dt);
            }
            return dt;
        }


        public DataTable SelectAll(string tableName, string conditionField, object[] conditionValueRange, int offSet = 0, int limit = 0)
        {
            return SelectAll(tableName, null, new string[] { conditionField }, new object[][] { conditionValueRange }, offSet, limit);
        }
        public DataTable SelectAll(string tableName, string[] returnFields, string conditionField, object[] conditionValueRange, int offSet = 0, int limit = 0)
        {
            return SelectAll(tableName, returnFields, new string[] { conditionField }, new object[][] { conditionValueRange }, offSet, limit);
        }
        public DataTable SelectAll(string tableName, string[] conditionFields, object[][] conditionValueRangeList, int offSet = 0, int limit = 0)
        {
            return SelectAll(tableName, null, conditionFields, conditionValueRangeList, offSet, limit);
        }
        public DataTable SelectAll(string tableName, string[] returnFields, string[] conditionFields, object[][] conditionValueRangeList, int offSet = 0, int limit = 0)
        {
            DataTable dt = new DataTable();
            using (SQLiteCommand cmd = GetNewCommand())
            {
                StringBuilder cmdBdr = new StringBuilder();
                cmdBdr.Append($"select ");
                if (returnFields == null || returnFields.Length <= 0)
                {
                    cmdBdr.Append("*");
                }
                else
                {
                    for (int i = 0, iv = returnFields.Length; i < iv; i++)
                        cmdBdr.Append(returnFields[i] + ",");
                    cmdBdr.Remove(cmdBdr.Length - 1, 1);
                }
                cmdBdr.Append($" from {tableName} ");
                if (conditionFields != null && conditionFields.Length > 0)
                {
                    cmdBdr.Append("where ");
                    StringBuilder cBdr = new StringBuilder();
                    int vIdx = 0;
                    string vFlag;
                    for (int i = 0, iv = conditionFields.Length; i < iv; i++)
                    {
                        if (conditionValueRangeList[i].Length <= 0)
                            continue;

                        cBdr.Append(conditionFields[i]);
                        cBdr.Append(" in (");

                        foreach (object c in conditionValueRangeList[i])
                        {
                            vFlag = "@c" + (vIdx++).ToString();
                            cBdr.Append(vFlag + ", ");
                            cmd.Parameters.AddWithValue(vFlag, c);
                        }
                        cBdr.Remove(cBdr.Length - 2, 2);

                        cBdr.Append(") and ");
                    }
                    if (cBdr.Length > 5)
                        cBdr.Remove(cBdr.Length - 5, 5);
                    cmdBdr.Append(cBdr.ToString());
                }

                if (limit > 0)
                    cmdBdr.Append($" limit {limit}");
                if (offSet > 0)
                    cmdBdr.Append($" offset {offSet}");
                cmdBdr.Append(";");
                cmd.CommandText = cmdBdr.ToString();
                FillDataTable(cmd, dt);
            }
            return dt;
        }


        public DataRow SelectSingle(string tableName)
        {
            return SelectSingle(tableName, new string[] { }, new object[] { });
        }
        public DataRow SelectSingle(string tableName, string conditionField, object conditionValue)
        {
            return SelectSingle(tableName, new string[] { conditionField }, new object[] { conditionValue });
        }
        public DataRow SelectSingle(string tableName, string[] conditionFields, object[] conditionValues)
        {
            DataTable dt = SelectAll(tableName, conditionFields, conditionValues, 0, 1);
            if (dt.Rows.Count > 0)
                return dt.Rows[0];
            else
                return null;
        }

        public int Insert(string tableName, DataRow dataRow)
        {
            int result = 0;
            using (SQLiteCommand cmd = GetNewCommand())
            {
                StringBuilder cmdBdr = new StringBuilder();
                cmdBdr.Append("insert into ");
                cmdBdr.Append(tableName);
                cmdBdr.Append(" (");
                StringBuilder vBdr = new StringBuilder();
                int vIdx = 0;
                string vFlag;
                foreach (DataColumn dc in dataRow.Table.Columns)
                {
                    cmdBdr.Append(dc.ColumnName);
                    cmdBdr.Append(",");
                    vFlag = "@v" + (vIdx++).ToString();
                    vBdr.Append(vFlag);
                    vBdr.Append(",");
                    cmd.Parameters.AddWithValue(vFlag, dataRow[dc]);
                }
                cmdBdr.Remove(cmdBdr.Length - 1, 1);
                vBdr.Remove(vBdr.Length - 1, 1);

                cmdBdr.Append(") values (");
                cmdBdr.Append(vBdr.ToString());
                cmdBdr.Append(");");
                cmd.CommandText = cmdBdr.ToString();
                result = cmd.ExecuteNonQuery();
            }
            return result;
        }

        /// <summary>
        /// 可更新多条记录，按照1个相等条件；
        /// 此方法只使用相等条件。。
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dataRow"></param>
        /// <param name="conditionField"></param>
        /// <param name="conditionValue"></param>
        /// <returns></returns>
        public int Update(string tableName, DataRow dataRow, string conditionField, object conditionValue)
        {
            return Update(tableName, dataRow,
                new string[] { conditionField }, new object[] { conditionValue });
        }
        /// <summary>
        /// 可更新多条记录，按照多个相等条件；
        /// 此方法只使用相等条件。。
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dataRow"></param>
        /// <param name="conditionFields"></param>
        /// <param name="conditionValues"></param>
        /// <returns></returns>
        public int Update(string tableName, DataRow dataRow, string[] conditionFields, object[] conditionValues)
        {
            int result = 0;
            using (SQLiteCommand cmd = GetNewCommand())
            {
                StringBuilder cmdBdr = new StringBuilder();
                cmdBdr.Append("update ");
                cmdBdr.Append(tableName);
                cmdBdr.Append(" set ");
                StringBuilder vBdr = new StringBuilder();
                int vIdx = 0;
                string vFlag;
                foreach (DataColumn dc in dataRow.Table.Columns)
                {
                    vBdr.Append(dc.ColumnName);
                    vBdr.Append("=");
                    vFlag = "@v" + (vIdx++).ToString();
                    vBdr.Append(vFlag);
                    vBdr.Append(", ");
                    cmd.Parameters.AddWithValue(vFlag, dataRow[dc]);
                }
                vBdr.Remove(vBdr.Length - 2, 2);

                cmdBdr.Append(vBdr.ToString());
                cmdBdr.Append(" where ");

                vIdx = 0;
                vBdr.Clear();
                for (int i = 0, iv = conditionFields.Length; i < iv; i++)
                {
                    vBdr.Append(conditionFields[i]);
                    vBdr.Append("=");
                    vFlag = "@c" + (vIdx++).ToString();
                    vBdr.Append(vFlag);
                    vBdr.Append(" and ");
                    cmd.Parameters.AddWithValue(vFlag, conditionValues[i]);
                }
                vBdr.Remove(vBdr.Length - 5, 5);
                cmdBdr.Append(vBdr.ToString());
                cmdBdr.Append(";");
                cmd.CommandText = cmdBdr.ToString();
                result = cmd.ExecuteNonQuery();
            }
            return result;
        }

        /// <summary>
        /// 替换或插入记录；
        /// 如果找到主键或唯一键值的记录，则先删除这条记录；
        /// 随后插入新纪录；
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dataRow"></param>
        /// <returns></returns>
        public int Replace(string tableName, DataRow dataRow)
        {
            List<string> fields = new List<string>();
            List<object> values = new List<object>();
            foreach (DataColumn dc in dataRow.Table.Columns)
            {
                fields.Add(dc.ColumnName);
                values.Add(dataRow[dc]);
            }
            return Replace(tableName, fields.ToArray(), values.ToArray());
        }
        /// <summary>
        /// 替换或插入记录；
        /// 如果找到主键或唯一键值的记录，则先删除这条记录；
        /// 随后插入新纪录；
        /// 注意未给出的键值将会置空（默认值？）
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int Replace(string tableName, string[] fields, object[] values)
        {
            int result = 0;
            using (SQLiteCommand cmd = GetNewCommand())
            {
                StringBuilder cmdBdr = new StringBuilder();
                cmdBdr.Append("insert or replace into ");
                cmdBdr.Append(tableName);
                cmdBdr.Append(" (");
                int vIdx = 0;
                string vFlag;
                StringBuilder vBdr = new StringBuilder();
                for (int i = 0, iv = fields.Length; i < iv; i++)
                {
                    cmdBdr.Append(fields[i]);
                    cmdBdr.Append(",");
                    vFlag = "@v" + (vIdx++).ToString();
                    vBdr.Append(vFlag);
                    vBdr.Append(",");
                    cmd.Parameters.AddWithValue(vFlag, values[i]);
                }
                cmdBdr.Remove(cmdBdr.Length - 1, 1);
                vBdr.Remove(vBdr.Length - 1, 1);

                cmdBdr.Append(") values (");
                cmdBdr.Append(vBdr.ToString());
                cmdBdr.Append(");");
                cmd.CommandText = cmdBdr.ToString();
                result = cmd.ExecuteNonQuery();
            }
            return result;
        }



        /// <summary>
        /// 可删除多条记录，按照1个相等条件；
        /// 此方法只使用相等条件。
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="conditionField"></param>
        /// <param name="conditionValue"></param>
        /// <returns></returns>
        public int Delete(string tableName, string conditionField, object conditionValue)
        {
            return Delete(tableName, new string[] { conditionField }, new object[] { conditionValue });
        }
        /// <summary>
        /// 可删除多条记录，按照多个相等条件；
        /// 此方法只使用相等条件。。
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="conditionFields"></param>
        /// <param name="conditionValues"></param>
        /// <returns></returns>
        public int Delete(string tableName, string[] conditionFields, object[] conditionValues)
        {
            int result = 0;
            using (SQLiteCommand cmd = GetNewCommand())
            {
                StringBuilder cmdBdr = new StringBuilder();
                cmdBdr.Append("delete from ");
                cmdBdr.Append(tableName);
                cmdBdr.Append(" where ");

                int vIdx = 0;
                string vFlag;
                StringBuilder vBdr = new StringBuilder();
                for (int i = 0, iv = conditionFields.Length; i < iv; i++)
                {
                    vBdr.Append(conditionFields[i]);
                    vBdr.Append("=");
                    vFlag = "@c" + (vIdx++).ToString();
                    vBdr.Append(vFlag);
                    vBdr.Append(" and ");
                    cmd.Parameters.AddWithValue(vFlag, conditionValues[i]);
                }
                vBdr.Remove(vBdr.Length - 5, 5);
                cmdBdr.Append(vBdr.ToString());
                cmdBdr.Append(";");
                cmd.CommandText = cmdBdr.ToString();
                result = cmd.ExecuteNonQuery();
            }
            return result;
        }


        #endregion






        public void Dispose()
        {
            if (sqliteConn != null)
            {
                sqliteConn.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public void Vacuume()
        {
            ExecuteNonQuery("VACUUM;");
        }
    }
}
