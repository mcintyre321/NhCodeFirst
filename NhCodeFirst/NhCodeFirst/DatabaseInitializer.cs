using System;
using System.Data.SqlClient;

namespace NhCodeFirst.NhCodeFirst
{
    public class DatabaseInitializer
    {

        private const string DropScript =
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
";
        private const string CreateScript =
    @"
CREATE DATABASE {0};
";
        private readonly string _masterConnectionString;
        private readonly string _targetDatabase;

        public DatabaseInitializer(string connectionString)
        {
            var csb = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            _targetDatabase = csb.InitialCatalog;
            csb.InitialCatalog = "master";
            this._masterConnectionString = csb.ToString();
        }
        public void Drop()
        {
            Execute(DropScript);
        }

        public void Create()
        {
            Execute(CreateScript);
        }

        private int Execute(string script)
        {
            using (var conn = new SqlConnection(_masterConnectionString))
            {
                conn.Open();
                int retVal = 0;
                foreach (var sql in script.Split(new[] { "\r\nGO\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    using (var cmd = new SqlCommand(string.Format(sql, _targetDatabase), conn))
                    {
                        retVal = cmd.ExecuteNonQuery();
                    }
                }
                return retVal;
            }
        }

        public bool Exists()
        {
            return Execute("select * from sys.databases where name = '{0}'") > 0;
        }
    }
}