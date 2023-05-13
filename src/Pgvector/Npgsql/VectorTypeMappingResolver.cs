using Npgsql.Internal;
using Npgsql.Internal.TypeHandling;
using Npgsql.Internal.TypeMapping;
using Npgsql.PostgresTypes;
using NpgsqlTypes;
using System;

namespace Pgvector.Npgsql
{
    public class VectorTypeMappingResolver : TypeMappingResolver
    {
        public override string GetDataTypeNameByClrType(Type type)
            => ClrTypeToDataTypeName(type);

        public override TypeMappingInfo GetMappingByDataTypeName(string dataTypeName)
            => DoGetMappingByDataTypeName(dataTypeName);

        public override TypeMappingInfo GetMappingByPostgresType(TypeMapper mapper, PostgresType type)
            => DoGetMappingByDataTypeName(type.Name);

        static TypeMappingInfo DoGetMappingByDataTypeName(string dataTypeName)
        {
            if (dataTypeName == "vector")
                return new TypeMappingInfo(NpgsqlDbType.Unknown, "vector");

            return null;
        }

        internal static string ClrTypeToDataTypeName(Type type)
        {
            if (type == typeof(Vector))
                return "vector";

            return null;
        }
    }
}
