using Dapper;
using Pgvector;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Pgvector.Dapper;

public class HalfvecTypeHandler : SqlMapper.TypeHandler<HalfVector?>
{
    public override HalfVector? Parse(object value)
        => value switch
        {
            null or DBNull => null,
            HalfVector vec => vec,
            _ => value.ToString() is string s ? new HalfVector(s) : null
        };

    public override void SetValue(IDbDataParameter parameter, HalfVector? value)
    {
        parameter.Value = value is null ? DBNull.Value : value;

        if (parameter is SqlParameter sqlParameter)
        {
            sqlParameter.UdtTypeName = "halfvec";
        }
    }
}
