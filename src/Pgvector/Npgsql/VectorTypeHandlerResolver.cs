using Npgsql.Internal;
using Npgsql.Internal.TypeHandling;
using Npgsql.PostgresTypes;
using NpgsqlTypes;
using System;

namespace Pgvector.Npgsql
{
	public class VectorTypeHandlerResolver : TypeHandlerResolver
	{
		readonly NpgsqlDatabaseInfo _databaseInfo;
		readonly VectorHandler _vectorHandler;

		internal VectorTypeHandlerResolver(NpgsqlConnector connector)
		{
			_databaseInfo = connector.DatabaseInfo;

			var pgVectorType = PgType("vector");

			if (!(pgVectorType is null))
			{
				_vectorHandler = new VectorHandler(pgVectorType);
			}
		}

		public override NpgsqlTypeHandler ResolveByDataTypeName(string typeName)
		{
			switch (typeName)
			{
				case "vector":
					return _vectorHandler;
				default:
					return null;
			};
		}

		public override NpgsqlTypeHandler ResolveByClrType(Type type)
		{
			var dataTypeName = ClrTypeToDataTypeName(type);

			if (dataTypeName != null)
			{
				var handler = ResolveByDataTypeName(dataTypeName);

				if (handler != null)
				{
					return handler;
				}
			}

			return null;
		}

		public override TypeMappingInfo GetMappingByDataTypeName(string dataTypeName)
		{
			return DoGetMappingByDataTypeName(dataTypeName);
		}

		internal static string ClrTypeToDataTypeName(Type type)
		{
			if (type == typeof(Vector))
			{
				return "vector";
			}

			return null;
		}

		internal static TypeMappingInfo DoGetMappingByDataTypeName(string dataTypeName)
		{ 
			switch(dataTypeName)
			{
				case "vector":
					return new TypeMappingInfo(NpgsqlDbType.Unknown, "vector");
				default:
					return null;
			}
		}

		PostgresType PgType(string pgTypeName) => _databaseInfo.TryGetPostgresTypeByName(pgTypeName, out var pgType) ? pgType : null;
	}
}