# PostgreSQL Setup Guide for BravoWeb

## Why PostgreSQL?
PostgreSQL is now configured as the default database for BravoWeb to enable:
- ✅ Cross-device data synchronization (PC ↔ Laptop for showcase)
- ✅ Shared database accessible from multiple machines
- ✅ Better performance and reliability for production
- ✅ Support for database replication and backups

## Step 1: Install PostgreSQL

### Option A: Windows Installer (Recommended)
1. Download from https://www.postgresql.org/download/windows/
2. Run the installer (select version 15 or higher)
3. During installation:
   - Accept default installation path
   - Choose a superuser password (remember this!)
   - Port: Keep default **5432**
   - Locale: Recommended to use default
4. Complete the installation

### Option B: Docker (Advanced)
If you have Docker installed, run:
```powershell
docker run --name bravoweb-postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:latest
```

## Step 2: Verify PostgreSQL is Running

### On Windows:
1. Open **Services** (Win + R → `services.msc`)
2. Look for **postgresql-x64-15** (or your version)
3. Ensure status is **Running**
4. If stopped, right-click and select **Start**

### Via Command Line:
```powershell
# Test connection (requires PostgreSQL installed)
psql -h localhost -U postgres -c "SELECT 1"

# Should return: 1
```

## Step 3: Create Database and User

Open **pgAdmin 4** (installed with PostgreSQL) or use command line:

### Using pgAdmin (GUI - Easier):
1. Open pgAdmin 4 (find in Start Menu)
2. Connect to local server
3. Right-click **Databases** → **Create** → **Database**
4. Name: `bravoweb_db`
5. Click **Save**

### Using Command Line:
```powershell
psql -U postgres

# Inside psql prompt:
CREATE DATABASE bravoweb_db;
\q  # Exit
```

## Step 4: Update Connection String (If Needed)

Default connection string in `appsettings.json`:
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=bravoweb_db;Username=postgres;Password=postgres;Timeout=30"
```

**Change if you used different credentials:**
- `Host`: Your PostgreSQL server address (localhost for local machine)
- `Port`: Default is 5432
- `Database`: Must match the database name created above
- `Username`: PostgreSQL user (default: postgres)
- `Password`: Password you set during installation

## Step 5: Test Connection from Visual Studio

1. Open **BravoWeb.sln** in Visual Studio
2. Build the project (Ctrl + Shift + B)
3. Press **F5** to run the application
4. If successful:
   - Application will start without connection errors
   - Database tables will be created automatically
5. If failed:
   - Check PostgreSQL service is running
   - Verify connection string in appsettings.json
   - Check firewall isn't blocking port 5432

## Step 6: Verify Database Tables

### Using pgAdmin:
1. Open pgAdmin 4
2. Navigate to **Databases** → **bravoweb_db** → **Schemas** → **public** → **Tables**
3. Should see tables like: `SitePages`, `ContentFragments`, `CustomTemplates`

### Using Command Line:
```powershell
psql -U postgres -d bravoweb_db

# Inside psql:
\dt  # List all tables
```

## Backup & Restore for Showcase

### Backup from Current Machine:
```powershell
pg_dump -U postgres -h localhost bravoweb_db > bravoweb_backup.sql
```

### Restore on Showcase Laptop:
1. Ensure PostgreSQL is installed on laptop
2. Create empty database:
   ```powershell
   psql -U postgres -c "CREATE DATABASE bravoweb_db"
   ```
3. Restore from backup:
   ```powershell
   psql -U postgres -d bravoweb_db < bravoweb_backup.sql
   ```

## Troubleshooting

### ❌ "Connection refused" error
- Check PostgreSQL service is running (Services.msc)
- Verify connection string has correct Host, Port, Database name

### ❌ "role 'postgres' does not exist"
- Reinstall PostgreSQL
- Or create new user and update connection string

### ❌ "Database does not exist"
- Create the database using pgAdmin or psql
- Ensure database name matches `appsettings.json`

### ❌ Port 5432 already in use
- Change port in connection string to unused port (e.g., 5433)
- Or stop the program using port 5432

## Next Steps
✅ PostgreSQL setup complete!  
→ Run the application and verify database is created  
→ Proceed with Step 2: Create User authentication model
