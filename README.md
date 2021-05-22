# About this Image
This is a utility version of mssql server on linux, that will take any backups present in the backups directory and create database from them that do not already exist.
Currently supports full (bak’s) and transactional (trn’s) backups.
Ola’s [maintenance solution](https://github.com/olahallengren/sql-server-maintenance-solution) has been added for convenience.

# How to use this Image
See the [offical sql server page](https://hub.docker.com/_/microsoft-mssql-server) for detailed documetation on how to use the base image.

Basic example:
`docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=yourStrong(!)Password' -p 1433:1433 -v /folder/seeding/backups:/var/backups --name mysqlserver -d aamcatamney/mssql-seeded`