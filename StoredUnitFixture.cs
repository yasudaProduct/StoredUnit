using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace UnitStored
{
    public class StoredUnitFixture : IDisposable
    {
        private readonly string _connectionString; // TODO 呼び出しもとで設定できるようにする
        public OracleConnection Connection { get; private set; }
        private readonly bool _insertMockData = false;

        public StoredUnitFixture()
        {
            Console.WriteLine("StoredUnitFixture: 初期化処理");

            Connection = new OracleConnection(_connectionString);
            Connection.Open();

        }

        public T ExecuteFunction<T>(string functionName, params OracleParameter[] parameters)
        {
            using (var command = new OracleCommand($"BEGIN :result := {functionName}; END;", Connection))
            {

                var resultParam = new OracleParameter("result", GetOracleDbType(typeof(T)),300)
                {
                    Direction = ParameterDirection.ReturnValue,
                };

                command.Parameters.Add(resultParam);
                command.Parameters.AddRange(parameters);

                command.ExecuteNonQuery();

                return ConvertOracleValue<T>(resultParam.Value);
            }
        }

        public void ExecuteProcedure<T>(string procedureName, params OracleParameter[] parameters)
        {
            using (var command = new OracleCommand($"BEGIN {procedureName}; END;", Connection))
            {
                var resultParam = new OracleParameter("result", GetOracleDbType(typeof(T)))
                {
                    Direction = ParameterDirection.ReturnValue
                };

                command.Parameters.AddRange(parameters);

                command.ExecuteNonQuery();
            }
        }

        public Dictionary<string, object> ExecuteStoredProcedureOut(
        string procedureName,
        Dictionary<string, (object value, OracleDbType dbType, ParameterDirection direction)> parameters)
        {
            var results = new Dictionary<string, object>();

            using (var command = new OracleCommand($"BEGIN {procedureName}; END;", Connection))
            {
                // パラメータの追加
                foreach (var param in parameters)
                {
                    var oracleParam = new OracleParameter(param.Key, param.Value.dbType)
                    {
                        Direction = param.Value.direction,
                        Value = param.Value.value ?? DBNull.Value
                    };

                    if (param.Value.dbType == OracleDbType.Varchar2 && param.Value.direction == ParameterDirection.Output)
                    {
                        oracleParam.Size = 50; // VARCHAR2のサイズ指定
                    }

                    command.Parameters.Add(oracleParam);


                }

                command.ExecuteNonQuery();

                // OUTパラメータ取得
                foreach (var param in parameters)
                {
                    if (param.Value.direction == ParameterDirection.Output)
                    {
                        var oracleValue = command.Parameters[param.Key].Value;
                        results[param.Key] = oracleValue;
                    }
                }

            }

            return results;
        }

        public T ExecuteScalar<T>(string query, params OracleParameter[] parameters)
        {
            using (var command = new OracleCommand(query, Connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                var result = command.ExecuteScalar();
                return ConvertSystemValue<T>(result);
            }
        }

        public List<Dictionary<string, object>> ExecuteQuery(string query, params OracleParameter[] parameters)
        {
            using (var command = new OracleCommand(query, Connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (var reader = command.ExecuteReader())
                {
                    var results = new List<Dictionary<string, object>>();
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        results.Add(row);
                    }
                    return results;
                }
            }
        }

        public void ExecuteNonQuery(string query, params OracleParameter[] parameters)
        {
            using (var command = new OracleCommand(query, Connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                var result = command.ExecuteNonQuery();
            }
        }

        private OracleDbType GetOracleDbType(Type type)
        {
            if (type == typeof(bool))
                return OracleDbType.Boolean;
            if (type == typeof(int))
                return OracleDbType.Int32;
            if (type == typeof(decimal))
                return OracleDbType.Decimal;
            if (type == typeof(string))
                return OracleDbType.Varchar2;

            throw new ArgumentException("Unsupported type");
        }

        private T ConvertOracleValue<T>(object value)
        {
            if (typeof(T) == typeof(bool))
            {
                return (T)(object)((OracleBoolean)value).IsTrue;
            }
            else if (typeof(T) == typeof(int))
            {
                return (T)(object)Convert.ToInt32(((OracleDecimal)value).ToInt32());
            }
            else if (typeof(T) == typeof(decimal))
            {
                return (T)(object)((OracleDecimal)value).Value;
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)value.ToString();
            }

            throw new InvalidCastException("Unsupported type conversion");
        }

        private T ConvertSystemValue<T>(object value)
        {

            if (value == null)
            {
                if (typeof(T).IsValueType)
                {
                    return default(T);
                }
                return (T)(object)null;
            }

            if (typeof(T) == typeof(bool))
            {
                return (T)(object)((bool)value);
            }
            else if (typeof(T) == typeof(int))
            {
                return (T)(object)((int)value);
            }
            else if (typeof(T) == typeof(decimal))
            {
                return (T)(object)((decimal)value);
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)value.ToString();
            }

            throw new InvalidCastException("Unsupported type conversion");
        }

        private object ConvertOracleValue(object oracleValue, OracleDbType dbType)
        {
            if (oracleValue == DBNull.Value)
            {
                return null;
            }

            return dbType switch
            {
                OracleDbType.Varchar2 => oracleValue.ToString(),
                OracleDbType.Int32 => (oracleValue).ToString(),
                OracleDbType.Decimal => Convert.ToDecimal(oracleValue),
                OracleDbType.Date => Convert.ToDateTime(oracleValue),
                OracleDbType.Double => Convert.ToDouble(oracleValue),
                OracleDbType.Single => Convert.ToSingle(oracleValue),
                OracleDbType.Boolean => Convert.ToBoolean(oracleValue),
                _ => oracleValue
            };
        }

        public void Dispose()
        {
            // 各テストの後に実行される共通のクリーンアップ処理
            Console.WriteLine("StoredUnitFixture: クリーンアップ処理");

            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
            }
        }

    }
}
