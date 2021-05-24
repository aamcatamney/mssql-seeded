#!/bin/bash
  
# turn on bash's job control
set -m
  
# Start SQL Server and put it in the background
/opt/mssql/bin/sqlservr &

sleep 10s &&

# Install MaintenanceSolution
# https://github.com/olahallengren/sql-server-maintenance-solution
/opt/mssql-tools/bin/sqlcmd -S localhost -i /usr/src/app/MaintenanceSolution.sql -U sa -P $SA_PASSWORD &&
  
# Start the restore script
/usr/src/app/dbrestore
  
# now we bring the sql process back into the foreground
# and leave it there
fg %1