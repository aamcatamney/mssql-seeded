using System;
using System.IO;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Collections.Generic;
using System.Linq;

namespace dbrestore
{
  class Program
  {
    static void Main(string[] args)
    {
      try
      {
        // Get Environment Variables
        var requiredDbs = Environment.GetEnvironmentVariable("MSSS_REQUIRED_DBS") ?? "";
        var backupsPath = Environment.GetEnvironmentVariable("MSSQL_BACKUP_DIR") ?? "/var/backups";
        var password = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD") ?? Environment.GetEnvironmentVariable("SA_PASSWORD");

        var ownerName = Environment.GetEnvironmentVariable("MSSS_USER_OWNER_NAME");
        var ownerPassword = Environment.GetEnvironmentVariable("MSSS_USER_OWNER_PASSWORD");
        var readerName = Environment.GetEnvironmentVariable("MSSS_USER_READER_NAME");
        var readerPassword = Environment.GetEnvironmentVariable("MSSS_USER_READER_PASSWORD");
        var haveOwner = !string.IsNullOrEmpty(ownerName) && !string.IsNullOrEmpty(ownerPassword);
        var haveReader = !string.IsNullOrEmpty(readerName) && !string.IsNullOrEmpty(readerPassword);

        // Construct connection string
        var csb = new SqlConnectionStringBuilder
        {
          DataSource = "localhost",
          Password = password,
          UserID = "sa"
        };

        // Connect to the sql server
        using var conn = new SqlConnection(csb.ToString());
        conn.Open();
        var sc = new ServerConnection(conn);
        var server = new Server(sc);

        // Get names of all existing dbs
        var dbs = conn.Query<string>("select name from sys.databases").ToList();

        // Get all current logins
        var sqlLogins = conn.Query<string>("select name from sys.sql_logins");

        // Add owner login if missing
        if (haveOwner && !sqlLogins.Any(sl => sl.Equals(ownerName, StringComparison.OrdinalIgnoreCase)))
        {
          conn.Execute($"CREATE LOGIN [{ownerName}] WITH PASSWORD = '{ownerPassword}', CHECK_POLICY = OFF;");
        }

        // Add reader login if missing
        if (haveReader && !sqlLogins.Any(sl => sl.Equals(readerName, StringComparison.OrdinalIgnoreCase)))
        {
          conn.Execute($"CREATE LOGIN [{readerName}] WITH PASSWORD = '{readerPassword}', CHECK_POLICY = OFF;");
        }

        // Get all the backup files
        var exts = new[] { ".bak", ".trn" }.ToList();
        var headers = new List<RestoreHeader>();
        foreach (var file in Directory.GetFiles(backupsPath, "*", SearchOption.AllDirectories))
        {
          // Only try and load bak's or trn's
          if (exts.Contains(Path.GetExtension(file)))
          {
            foreach (var header in conn.Query<RestoreHeader>("RESTORE HEADERONLY FROM DISK = @FilePath", new { FilePath = file }))
            {
              header.FilePath = file;
              headers.Add(header);
            }
          }
        }

        // Get only full backups and group them by name
        var latestBaks = headers.Where(b => b.BackupType == 1).GroupBy(b => b.DatabaseName);
        foreach (var fullBak in latestBaks)
        {
          // Only attempt to restore dbs that done exist
          if (!dbs.Any(d => d.Equals(fullBak.Key, StringComparison.OrdinalIgnoreCase)))
          {
            // Find the most recent backup
            var newest = fullBak.OrderByDescending(b => b.BackupStartDate).FirstOrDefault();

            // Find any transcation backups that where created after the full backup
            var logs = headers.Where(b => b.BackupType == 2 && b.DatabaseName == fullBak.Key && b.BackupStartDate > newest.BackupStartDate).OrderBy(b => b.BackupStartDate).ToList();

            // Get the file details from the full backup
            var files = conn.Query<RestoreFile>("RESTORE FILELISTONLY FROM DISK = @FilePath", new { FilePath = newest.FilePath });

            // Setup the full backup
            var res = new Restore
            {
              Database = fullBak.Key,
              NoRecovery = logs.Any(),
              ReplaceDatabase = true
            };
            res.Devices.AddDevice(newest.FilePath, DeviceType.File);

            // Move the files to the server default locations
            foreach (var f in files)
            {
              if (f.Type == "D")
              {
                res.RelocateFiles.Add(new RelocateFile(f.LogicalName, $"{server.DefaultFile}/{f.LogicalName}.mdf"));
              }
              else if (f.Type == "L")
              {
                res.RelocateFiles.Add(new RelocateFile(f.LogicalName, $"{server.DefaultLog}/{f.LogicalName}.ldf"));
              }
            }

            // Do the restore
            res.SqlRestore(server);

            // If we have any transcation logs
            if (logs.Any())
            {
              // Restore the transcation log
              foreach (var l in logs)
              {
                var isLast = logs.LastOrDefault() == l;
                res = new Restore
                {
                  Database = fullBak.Key,
                  NoRecovery = !isLast
                };
                res.Devices.AddDevice(l.FilePath, DeviceType.File);
                res.SqlRestore(server);
              }
            }

            // Add the owner user
            if (haveOwner)
            {
              CreateUser(server, fullBak.Key, ownerName, "db_owner");
            }

            // Add the reader user
            if (haveReader)
            {
              CreateUser(server, fullBak.Key, readerName, "db_datareader");
            }

            // Add the dbs list
            dbs.Add(fullBak.Key);
          }
        }

        // Add any missing required dbs
        foreach (var db in requiredDbs.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
          // Only attempt to create dbs that done exist
          if (!dbs.Any(d => d.Equals(db, StringComparison.OrdinalIgnoreCase)))
          {
            var ndb = new Database(server, db);
            ndb.Collation = "Latin1_General_CI_AS";
            ndb.Create();

            // Add the owner user
            if (haveOwner)
            {
              CreateUser(server, db, ownerName, "db_owner");
            }

            // Add the reader user
            if (haveReader)
            {
              CreateUser(server, db, readerName, "db_datareader");
            }
          }
        }
      }
      catch (Exception er)
      {
        Console.Error.WriteLine(er.Message);
      }
    }

    static void CreateUser(Server server, string dbName, string username, string role)
    {
      if (server.Databases[dbName].Users.Contains(username))
      {
        server.Databases[dbName].Users[username].DropIfExists();
      }
      var reader = new User(server.Databases[dbName], username);
      reader.Login = username;
      reader.DropIfExists();
      reader.Create();
      reader.AddToRole(role);
    }
  }
}
