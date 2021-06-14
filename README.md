# About this Image
This is a utility version of mssql server on linux, that will take any backups present in the backups directory and create database from them that do not already exist.

Currently supports full (bak’s) and transactional (trn’s) backups.  Backups will be grouped by database name, then the most rescent full backup will be used to restore from, then any susequent tranactional backups will be applied. 

Ola’s [maintenance solution](https://github.com/olahallengren/sql-server-maintenance-solution) has been added for convenience.

# How to use this Image
See the [offical sql server page](https://hub.docker.com/_/microsoft-mssql-server) for detailed documetation on how to use the base image.

Basic example:

`docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=yourStrong(!)Password' -p 1433:1433 -v /folder/seeding/backups:/var/backups --name mysqlserver -d aamcatamney/mssql-seeded`


Detailed example:

`docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=yourStrong(!)Password' -e 'MSSQL_BACKUP_DIR=/var/backups' -e 'MSSS_USER_OWNER_NAME=my-owner' -e 'MSSS_USER_OWNER_PASSWORD=other(!)Password' -e 'MSSS_USER_READER_NAME=my-reader' -e 'MSSS_USER_READER_PASSWORD=other(!)Password' -p 1433:1433 -v /folder/seeding/backups:/var/backups --name mysqlserver -d aamcatamney/mssql-seeded`

Ola’s backup script can be used to take full backups:

`docker exec mysqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P yourStrong(!)Password! -q "EXECUTE dbo.DatabaseBackup @Databases = 'USER_DATABASES', @Directory = '/var/backups', @DirectoryStructure = NULL, @BackupType = 'FULL', @Verify = 'Y', @FileName = '{ServerName}${InstanceName}_{BackupType}_{DatabaseName}_{Partial}_{CopyOnly}_{Year}{Month}{Day}{Hour}{Minute}{Second}_{FileNumber}.{FileExtension}'"`

Ola’s backup script can be used to take log backups:

`docker exec mysqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P yourStrong(!)Password! -q "EXECUTE dbo.DatabaseBackup @Databases = 'USER_DATABASES', @Directory = '/var/backups', @DirectoryStructure = NULL, @BackupType = 'FULL', @Verify = 'Y', @FileName = '{ServerName}${InstanceName}_{BackupType}_{DatabaseName}_{Partial}_{CopyOnly}_{Year}{Month}{Day}{Hour}{Minute}{Second}_{FileNumber}.{FileExtension}'"`

# Environment Variables

`MSSS_USER_OWNER_NAME` is the login & user name with db_owner role, that will be created and applied to each restored database.  OPTIONAL

`MSSS_USER_OWNER_PASSWORD` is the password for the above user.  OPTIONAL

`MSSS_USER_READER_NAME` is the login & user name with db_datareader role that will be created and applied to each restored database.  OPTIONAL

`MSSS_USER_READER_PASSWORD` is the password for the above user.  OPTIONAL

`MSSS_REQUIRED_DBS` is a comma seperated list of databases that need to be created or restored.  OPTIONAL