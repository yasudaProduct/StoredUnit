using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using OracleStoredUnitLibrary;

namespace UnitTestSample.ModuleB
{
    public class P_INSERT_INTO_TABLE_A : IClassFixture<StoredUnitFixture>
    {
        private readonly StoredUnitFixture _fixture;

        public P_INSERT_INTO_TABLE_A(StoredUnitFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "UT30001 登録値の確認 テスト前にデータを削除")]
        public void Test01()
        {
            // Arrange
            int id = 1;
            string name = "Test";
            var parameters = new OracleParameter[]
            {
                new OracleParameter("p_id", OracleDbType.Int32) { Value = id },
                new OracleParameter("p_name", OracleDbType.Varchar2) { Value = name }
            };

            // テストデータ削除
            DeleteMockData(id);

            // Act
            _fixture.ExecuteProcedure(ProcedureName.P_INSERT_INTO_TABLE_A, parameters);

            // 実行結果を取得
            var checkQuery = @"
                SELECT ID, NAME
                FROM TABLE_A
                WHERE ID = :ID";
            var checkQueryParams = new OracleParameter[]
                {
                    new OracleParameter("ID", OracleDbType.Int32) { Value = id },
                };
            var result = _fixture.ExecuteQuery(checkQuery, checkQueryParams);

            // Assert
            Assert.Equal(1, result.Count);                    // 1件のみ取得されること
            Assert.NotEqual(DBNull.Value, result[0]["ID"]);   // IDがNULLではないこと
            Assert.NotEqual(DBNull.Value, result[0]["NAME"]); // NAMEがNULLではないこと
            Assert.Equal(id, (Int64)result[0]["ID"]);         // 登録値が正しいこと
            Assert.Equal(name, result[0]["NAME"].ToString()); // 登録値が正しいこと
        }

        [Fact(DisplayName = "UT30002 Oracleエラーのテスト")]
        public void Test02()
        {
            // Arrange
            int id = 1;
            string name = "Test";
            var parameters = new OracleParameter[]
            {
                new OracleParameter("p_id", OracleDbType.Int32) { Value = id },
                new OracleParameter("p_name", OracleDbType.Varchar2) { Value = name }
            };

            // Act
            //var exception = Assert.Throws<OracleException>(() =>
            //_fixture.ExecuteProcedure(ProcedureName.P_INSERT_INTO_TABLE_A, parameters)
            //);
            var exception = Record.Exception(() =>
            {
                _fixture.ExecuteProcedure(ProcedureName.P_INSERT_INTO_TABLE_A, parameters);
            });

            // Assert
            Assert.NotNull(exception);
            Assert.Equal(typeof(OracleException), exception.GetType());
            Assert.Contains("ORA-00001", exception.Message);

        }

        private void DeleteMockData(int id)
        {
            string query = @"DELETE FROM TABLE_A WHERE ID = " + id;
            _fixture.ExecuteNonQuery(query, null);
        }
    }
}