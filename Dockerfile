FROM mcr.microsoft.com/mssql/server

# Switch to root user
USER root

# Create app directory
RUN mkdir -p /usr/src/app

# Download SQL Server Maintenance Solution
# https://github.com/olahallengren/sql-server-maintenance-solution
ADD https://raw.githubusercontent.com/olahallengren/sql-server-maintenance-solution/master/MaintenanceSolution.sql /usr/src/app 

# Copy the entrypoint
COPY entrypoint.sh /usr/src/app/entrypoint.sh

# Copy the dbrestore app
COPY bin/Release/net5.0/linux-x64/publish/dbrestore /usr/src/app/dbrestore

# Grant permissions
RUN chmod +x /usr/src/app/dbrestore
RUN chmod 777 /usr/src/app/MaintenanceSolution.sql

# Switch back to mssql user and run the entrypoint script
USER mssql

ENTRYPOINT /bin/bash /usr/src/app/entrypoint.sh