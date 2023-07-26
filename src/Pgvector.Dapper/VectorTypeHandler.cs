using Dapper;
using Pgvector;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Pgvector.Dapper;

public class VectorTypeHandler : SqlMapper.TypeHandler<Vector?>
{
    public override Vector? Parse(object value)
        => value switch
        {
            null or DBNull => null,
            Vector vec => vec,
            _ => value.ToString() is string s ? new Vector(s) : null
        };

    public override void SetValue(IDbDataParameter parameter, Vector? value)
    {
        parameter.Value = value is null ? DBNull.Value : value;

        if (parameter is SqlParameter sqlParameter)
        {
            sqlParameter.UdtTypeName = "vector";
        }
    }
}
