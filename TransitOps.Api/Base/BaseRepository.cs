using System.Data;
using Microsoft.Data.SqlClient;

namespace TransitOps.Api.Base;

public abstract class BaseRepository : IBaseRepository
{
    private readonly string _connectionString;

    protected BaseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") 
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");
    }

    protected SqlConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<SpResult> ExecuteAsync(string sp, params SqlParameter[] parameters)
    {
        await using var connection = CreateConnection();
        await using var command = new SqlCommand(sp, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        foreach (var p in parameters)
        {
            command.Parameters.Add(p);
        }

        var pResultCode = new SqlParameter("@ResultCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var pMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 400) { Direction = ParameterDirection.Output };
        var pNewId = new SqlParameter("@NewId", SqlDbType.Int) { Direction = ParameterDirection.Output };

        if (!command.Parameters.Contains("@ResultCode")) command.Parameters.Add(pResultCode);
        else pResultCode = command.Parameters["@ResultCode"];

        if (!command.Parameters.Contains("@Message")) command.Parameters.Add(pMessage);
        else pMessage = command.Parameters["@Message"];

        if (!command.Parameters.Contains("@NewId")) command.Parameters.Add(pNewId);
        else pNewId = command.Parameters["@NewId"];

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();

        return new SpResult
        {
            ResultCode = pResultCode.Value != DBNull.Value ? (int)pResultCode.Value : 0,
            Message = pMessage.Value != DBNull.Value ? (string)pMessage.Value : string.Empty,
            NewId = pNewId.Value != DBNull.Value ? (int)pNewId.Value : 0
        };
    }

    public async Task<List<T>> QueryAsync<T>(string sp, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
    {
        var list = new List<T>();
        await using var connection = CreateConnection();
        await using var command = new SqlCommand(sp, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        foreach (var p in parameters)
        {
            command.Parameters.Add(p);
        }

        EnsureOutputParameters(command);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(map(reader));
        }

        return list;
    }

    public async Task<T?> QuerySingleAsync<T>(string sp, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
    {
        await using var connection = CreateConnection();
        await using var command = new SqlCommand(sp, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        foreach (var p in parameters)
        {
            command.Parameters.Add(p);
        }

        EnsureOutputParameters(command);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow);
        if (await reader.ReadAsync())
        {
            return map(reader);
        }

        return default;
    }

    public async Task<DataSet> QueryMultipleAsync(string sp, params SqlParameter[] parameters)
    {
        await using var connection = CreateConnection();
        await using var command = new SqlCommand(sp, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        foreach (var p in parameters)
        {
            command.Parameters.Add(p);
        }

        EnsureOutputParameters(command);

        await connection.OpenAsync();
        using var adapter = new SqlDataAdapter(command);
        var ds = new DataSet();
        adapter.Fill(ds);
        return ds;
    }

    private static void EnsureOutputParameters(SqlCommand command)
    {
        if (!command.Parameters.Contains("@ResultCode"))
            command.Parameters.Add(new SqlParameter("@ResultCode", SqlDbType.Int) { Direction = ParameterDirection.Output, Value = 0 });
        if (!command.Parameters.Contains("@Message"))
            command.Parameters.Add(new SqlParameter("@Message", SqlDbType.NVarChar, 400) { Direction = ParameterDirection.Output, Value = string.Empty });
        if (!command.Parameters.Contains("@NewId"))
            command.Parameters.Add(new SqlParameter("@NewId", SqlDbType.Int) { Direction = ParameterDirection.Output, Value = 0 });
    }

    // Helper reader extensions
    protected static T GetValue<T>(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
            return default!;
        return (T)reader.GetValue(ordinal);
    }

    protected static T? GetNullableValue<T>(SqlDataReader reader, string columnName) where T : struct
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
            return null;
        return (T)reader.GetValue(ordinal);
    }

    protected static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
            return null;
        return reader.GetString(ordinal);
    }
}

