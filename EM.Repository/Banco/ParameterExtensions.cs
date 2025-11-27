using FirebirdSql.Data.FirebirdClient;
using System.Data;

namespace EM.Repository.Banco
{
    public static class ParameterExtensions
    {
        public static void CreateParameter(this IDbCommand command, string parameterName, object? value)
        {
            object dbValue = value ?? DBNull.Value;
            FbParameter parameter = new(parameterName, dbValue);
            command.Parameters.Add(parameter);
        }
    }
}
