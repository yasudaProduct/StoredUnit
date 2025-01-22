using OracleStoredUnitLibrary;

namespace UnitTestSample.ModuleA
{
    public class F_GET_CURRENT_YEAR : IClassFixture<StoredUnitFixture>
    {
        private readonly StoredUnitFixture _fixture;

        public F_GET_CURRENT_YEAR(StoredUnitFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName ="UT20001")]
        public void Test01()
        {
            // Arrange

            // Act
            var result = _fixture.ExecuteFunction<int>(FunctionName.F_GET_CURRENT_YEAR);

            // Assert
            Assert.Equal(2025, result);
        }
    }
}