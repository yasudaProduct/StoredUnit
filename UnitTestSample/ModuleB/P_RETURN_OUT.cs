using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using OracleStoredUnitLibrary;
using System.Data;

namespace UnitTestSample.ModuleB
{
    public class P_RETURN_OUT : IClassFixture<StoredUnitFixture>
    {
        private readonly StoredUnitFixture _fixture;

        public P_RETURN_OUT(StoredUnitFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "UT30001 登録値の確認 テスト前にデータを削除")]
        public void Test01()
        {
            // Arrange
            const int expected =4;
            int p_number = 2;
            int p_square = 0;
            var parameters = new Dictionary<string, (object value, OracleDbType dbType, ParameterDirection direction)>
            {
                { "P_NUMBER", (p_number, OracleDbType.Int32, ParameterDirection.Input) },
                { "P_SQUARE", (p_square, OracleDbType.Int32, ParameterDirection.Output) },
            };
            // Act
            var results = _fixture.ExecuteStoredProcedureOut(ProcedureName.P_RETURN_OUT, parameters);

            // OUT引数の値を取得
            object result = results["P_SQUARE"];

            // Assert
            Assert.Equal(expected.ToString(), result.ToString());
        }
    }
}