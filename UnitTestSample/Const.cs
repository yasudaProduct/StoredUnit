using Newtonsoft.Json.Linq;

namespace UnitTestSample
{

    static public class FunctionName
    {
        public const string F_IS_NUMBER = "F_IS_NUMBER(:P_VALUE)";
        public const string F_GET_CURRENT_YEAR = "F_GET_CURRENT_YEAR()";
    }

    static public class ProcedureName
    {
        public const string P_INSERT_INTO_TABLE_A = "P_INSERT_INTO_TABLE_A(:P_ID,:P_NAME)";
        public const string P_RETURN_OUT = "P_RETURN_OUT(:P_NUMBER,:P_SQUARE)";
    }
}
