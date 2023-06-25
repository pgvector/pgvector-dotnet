using Dapper;
using System;
using System.Data;

namespace Pgvector.Dapper;

public class VectorTypeHandler : SqlMapper.TypeHandler<Vector>
{
    public override Vector Parse(object value)
        => value switch
        {
            Vector vec => vec,
            null or DBNull => null!,

            _ => value.ToString() is string s ? new Vector(s) : null!
        };

    public override void SetValue(IDbDataParameter parameter, Vector value)
        => parameter.Value = value == null ? DBNull.Value : value;
}
