using System;
using System.Data.SqlClient;

namespace NhCodeFirst.NhCodeFirst
{
    public class DatabaseDropperAndRecreator
    {

        private const string SqlScript =
            @"
DECLARE @DatabaseName nvarchar(50)
SET @DatabaseName = N'{0}'

DECLARE @SQL varchar(max)

SELECT @SQL = COALESCE(@SQL,'') + 'Kill ' + Convert(varchar, SPId) + ';'
FROM MASTER..SysProcesses
WHERE DBId = DB_ID(@DatabaseName) AND SPId <> @@SPId and loginame <> 'sa'

EXEC(@SQL);
GO
IF EXISTS(select * from sys.databases where name = '{0}')
BEGIN
    DROP DATABASE {0};
END

GO
CREATE DATABASE {0};
";
        private readonly string _masterConnectionString;
        private readonly string _targetDatabase;

        public DatabaseDropperAndRecreator(string connectionString)
        {
            this._masterConnectionString = connectionString.Replace("=" + _targetDatabase, "database=master");
        }
        public void Execute()
        {
            using (var conn = new SqlConnection(_masterConnectionString))
            {
                conn.Open();
                foreach (var sql in SqlScript.Split(new[] {"\r\nGO\r\n"}, StringSplitOptions.RemoveEmptyEntries))
                {
                    using (var cmd = new SqlCommand(string.Format(sql, _targetDatabase), conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}