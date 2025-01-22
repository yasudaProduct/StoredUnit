using Oracle.ManagedDataAccess.Client;
using OracleStoredUnitLibrary;

namespace UnitTestSample.ModuleA
{
    public class F_IS_NUMBER : IClassFixture<StoredUnitFixture>
    {
        private readonly StoredUnitFixture _fixture;

        public F_IS_NUMBER(StoredUnitFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName ="UT10001 入力値:\"12345\"")]
        public void Test01()
        {
            // Arrange
            var value = "12345";
            var expected = true;
            var parameters = new OracleParameter[]
            {
                new OracleParameter("p_value", OracleDbType.Varchar2) { Value = value }
            };

            // Act
            var result = _fixture.ExecuteFunction<bool>(FunctionName.F_IS_NUMBER, parameters);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact(DisplayName ="UT10002 入力値:\"abc123\"")]
        public void Test02()
        {
            // Arrange
            var value = "abc123";
            var expected = false;
            var parameters = new OracleParameter[]
            {
                new OracleParameter("p_value", OracleDbType.Varchar2) { Value = value }
            };

            // Act
            var result = _fixture.ExecuteFunction<bool>(FunctionName.F_IS_NUMBER, parameters);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact(DisplayName ="UT10003 入力値:\"abcde\"")]
        public void Test03()
        {
            // Arrange
            var value = "abcde";
            var expected = false;
            var parameters = new OracleParameter[]
            {
                new OracleParameter("p_value", OracleDbType.Varchar2) { Value = value }
            };

            // Act
            var result = _fixture.ExecuteFunction<bool>(FunctionName.F_IS_NUMBER, parameters);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact(DisplayName ="UT10004 入力値:null")]
        public void Test04()
        {
            // Arrange
            string value = null;
            var expected = false;
            var parameters = new OracleParameter[]
            {
                new OracleParameter("p_value", OracleDbType.Varchar2) { Value = value }
            };

            // Act
            var result = _fixture.ExecuteFunction<bool>(FunctionName.F_IS_NUMBER, parameters);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact(DisplayName ="UT10005 入力値:\"\"")]
        public void Test05()
        {
            // Arrange
            var value = "";
            var expected = false;
            var parameters = new OracleParameter[]
            {
                new OracleParameter("p_value", OracleDbType.Varchar2) { Value = value }
            };

            // Act
            var result = _fixture.ExecuteFunction<bool>(FunctionName.F_IS_NUMBER, parameters);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact(DisplayName ="UT10006 入力値:\"   \"")]
        public void Test06()
        {
            // Arrange
            var value = "   ";
            var expected = false;
            var parameters = new OracleParameter[]
            {
                new OracleParameter("p_value", OracleDbType.Varchar2) { Value = value }
            };

            // Act
            var result = _fixture.ExecuteFunction<bool>(FunctionName.F_IS_NUMBER, parameters);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact(DisplayName ="UT10007 入力値:\"　　\"")]
        public void Test07()
        {
            // Arrange
            var value = "　　";
            var expected = false;
            var parameters = new OracleParameter[]
            {
                new OracleParameter("p_value", OracleDbType.Varchar2) { Value = value }
            };

            // Act
            var result = _fixture.ExecuteFunction<bool>(FunctionName.F_IS_NUMBER, parameters);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact(DisplayName ="UT10008 入力値:\"01234\"")]
        public void Test08()
        {
            // Arrange
            var value = "01234";
            var expected = true;
            var parameters = new OracleParameter[]
            {
                new OracleParameter("p_value", OracleDbType.Varchar2) { Value = value }
            };

            // Act
            var result = _fixture.ExecuteFunction<bool>(FunctionName.F_IS_NUMBER, parameters);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact(DisplayName ="UT10009 入力値:\"１２３４\"")]
        public void Test09()
        {
            // Arrange
            var value = "１２３４";
            var expected = false;
            var parameters = new OracleParameter[]
            {
                new OracleParameter("p_value", OracleDbType.Varchar2) { Value = value }
            };

            // Act
            var result = _fixture.ExecuteFunction<bool>(FunctionName.F_IS_NUMBER, parameters);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
