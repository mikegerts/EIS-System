﻿using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace EIS.MwsReportsApp
{
    public class MySqlHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <returns>Returns the ID of the record inserted</returns>
        public static long ExecuteNonQuery(MySqlConnection conn, string cmdText, Dictionary<string, object> cmdParms)
        {
            MySqlCommand cmd = conn.CreateCommand();
            using (conn)
            {
                PrepareCommand(cmd, conn, null, CommandType.Text, cmdText, cmdParms);
                cmd.ExecuteNonQuery();
                long val = cmd.LastInsertedId;
                cmd.Parameters.Clear();
                return val;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        /// <returns>Returns the ID of the record inserted</returns>
        public static long ExecuteNonQuery(MySqlConnection conn, CommandType cmdType, string cmdText, Dictionary<string, object> cmdParms)
        {
            MySqlCommand cmd = conn.CreateCommand();
            using (conn)
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
                cmd.ExecuteNonQuery();
                long val = cmd.LastInsertedId;
                cmd.Parameters.Clear();
                return val;
            }
        }


        public static IDataReader ExecuteReader(MySqlConnection conn, CommandType cmdType, string cmdText, Dictionary<string, object> cmdParms)
        {
            MySqlCommand cmd = conn.CreateCommand();
            PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
            var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return rdr;
        }


        public static object ExecuteScalar(MySqlConnection conn, CommandType cmdType, string cmdText, Dictionary<string, object> cmdParms)
        {
            MySqlCommand cmd = conn.CreateCommand();
            PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
            object val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }

        private static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, CommandType cmdType, string cmdText, Dictionary<string, object> cmdParms)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
            {
                cmd.Transaction = trans;
            }
            cmd.CommandType = cmdType;
            if (cmdParms != null)
            {
                foreach (var param in cmdParms)
                {
                    var parameter = cmd.CreateParameter();
                    parameter.ParameterName = param.Key;
                    parameter.Value = param.Value;
                    cmd.Parameters.Add(parameter);
                }

            }
        }
    }
}
