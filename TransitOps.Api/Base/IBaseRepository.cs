using System.Data;
using Microsoft.Data.SqlClient;

namespace TransitOps.Api.Base;

public interface IBaseRepository
{
    Task<SpResult> ExecuteAsync(string sp, params SqlParameter[] parameters);
    Task<List<T>> QueryAsync<T>(string sp, Func<SqlDataReader, T> map, params SqlParameter[] parameters);
    Task<T?> QuerySingleAsync<T>(string sp, Func<SqlDataReader, T> map, params SqlParameter[] parameters);
    Task<DataSet> QueryMultipleAsync(string sp, params SqlParameter[] parameters);
}

