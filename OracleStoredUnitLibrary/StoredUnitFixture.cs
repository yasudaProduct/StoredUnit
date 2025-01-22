using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using Microsoft.Extensions.Configuration;
namespace OracleStoredUnitLibrary
{
    public class StoredUnitFixture : IDisposable
    {
        private readonly string _connectionString;
        public OracleConnection Connection { get; private set; }
        private readonly bool _insertMockData = false;

        public StoredUnitFixture()
        {
            Console.WriteLine("StoredUnitFixture: 初期化処理");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "AppSettings.json")
                .Build();

            if(String.IsNullOrEmpty(configuration.GetConnectionString("DefaultConnection"))) throw new ArgumentNullException("DefaultConnectionを設定してください。");

            _connectionString = configuration.GetConnectionString("DefaultConnection");
            Connection = new OracleConnection(_connectionString);
            Connection.Open();

        }

        /// <summary>
        /// Oracle関数を実行し、結果を取得します。
        /// </summary>
        /// <typeparam name="T">戻り値の型</typeparam>
        /// <param name="functionName">実行する関数の名前</param>
        /// <param name="parameters">関数に渡すパラメータ</param>
        /// <returns>関数の実行結果</returns>
        public T ExecuteFunction<T>(string functionName, OracleParameter[]? parameters = null)
        {
            if(String.IsNullOrEmpty(functionName)) throw new ArgumentNullException(nameof(functionName));

            using (var command = new OracleCommand($"BEGIN :result := {functionName}; END;", Connection))
            {

                var resultParam = new OracleParameter("result", GetOracleDbType(typeof(T)),300)
                {
                    Direction = ParameterDirection.ReturnValue,
                };

                command.Parameters.Add(resultParam);
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                command.ExecuteNonQuery();

                return ConvertOracleValue<T>(resultParam.Value);
            }
        }

        /// <summary>
        /// Oracleストアドプロシージャを実行します。
        /// </summary>
        /// <param name="procedureName">実行するプロシージャの名前</param>
        /// <param name="parameters">プロシージャに渡すパラメータ</param>
        public void ExecuteProcedure(string procedureName, OracleParameter[]? parameters = null)
        {
            if(String.IsNullOrEmpty(procedureName)) throw new ArgumentNullException(nameof(procedureName));

            using (var command = new OracleCommand($"BEGIN {procedureName}; END;", Connection))
            {

                if(parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Oracleストアドプロシージャを実行し、OUTパラメータの結果を取得します。
        /// </summary>
        /// <param name="procedureName">実行するプロシージャの名前</param>
        /// <param name="parameters">プロシージャに渡すパラメータ</param>
        /// <returns>OUTパラメータの結果</returns>
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

        /// <summary>
        /// クエリを実行し、スカラー値を取得します。
        /// </summary>
        /// <typeparam name="T">戻り値の型</typeparam>
        /// <param name="query">実行するクエリ</param>
        /// <param name="parameters">クエリに渡すパラメータ</param>
        /// <returns>クエリの実行結果</returns>
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

        /// <summary>
        /// クエリを実行し、結果をリストとして取得します。
        /// </summary>
        /// <param name="query">実行するクエリ</param>
        /// <param name="parameters">クエリに渡すパラメータ</param>
        /// <returns>クエリの実行結果</returns>
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

        /// <summary>
        /// クエリを実行し、結果を返さずに処理を行います。
        /// </summary>
        /// <param name="query">実行するクエリ</param>
        /// <param name="parameters">クエリに渡すパラメータ</param>
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

        /// <summary>
        /// 型に応じたOracleDbTypeを取得します。
        /// </summary>
        /// <param name="type">型情報</param>
        /// <returns>対応するOracleDbType</returns>
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

        /// <summary>
        /// Oracleの値を指定された型に変換します。
        /// </summary>
        /// <typeparam name="T">変換後の型</typeparam>
        /// <param name="value">変換する値</param>
        /// <returns>変換後の値</returns>
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

            throw new InvalidCastException($"{typeof(T)}はキャスト出来ませんでした。if文を追加し実装してください。");
        }

        /// <summary>
        /// システムの値を指定された型に変換します。
        /// </summary>
        /// <typeparam name="T">変換後の型</typeparam>
        /// <param name="value">変換する値</param>
        /// <returns>変換後の値</returns>
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

        /// <summary>
        /// Oracleの値を指定されたOracleDbTypeに変換します。
        /// </summary>
        /// <param name="oracleValue">変換する値</param>
        /// <param name="dbType">変換後のOracleDbType</param>
        /// <returns>変換後の値</returns>
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
            Console.WriteLine("StoredUnitFixture: Dispose");

            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
            }
        }

    }
}
