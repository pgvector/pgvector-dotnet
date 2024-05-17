using Dapper;
using Pgvector;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Pgvector.Dapper;

public class SparsevecTypeHandler : SqlMapper.TypeHandler<SparseVector?>
{
    public override SparseVector? Parse(object value)
        => value switch
        {
            null or DBNull => null,
            SparseVector vec => vec,
            _ => value.ToString() is string s ? new SparseVector(s) : null
        };

    public override void SetValue(IDbDataParameter parameter, SparseVector? value)
    {
        parameter.Value = value is null ? DBNull.Value : value;

        if (parameter is SqlParameter sqlParameter)
        {
            sqlParameter.UdtTypeName = "sparsevec";
        }
    }
}
