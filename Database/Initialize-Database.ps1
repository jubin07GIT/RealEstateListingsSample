# Run in Windows PowerShell 5.1 x64. Requires Microsoft ACE OLE DB provider (Microsoft Access or Database Engine x64).
# Creates a genuine Access .accdb (Access 2007+ file format), then loads sample_listings.json.
param([switch]$Force)
$ErrorActionPreference = 'Stop'
$dbPath = Join-Path $PSScriptRoot 'Listings.accdb'
$jsonPath = Join-Path $PSScriptRoot 'sample_listings.json'
if ((Test-Path $dbPath) -and !$Force) { throw "Database already exists at $dbPath. Use -Force to replace it." }
if (Test-Path $dbPath) { Remove-Item -LiteralPath $dbPath -Force }
$catalog = $null; $connection = $null
try {
    $catalog = New-Object -ComObject ADOX.Catalog
    $connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$dbPath;"
    $catalog.Create($connectionString)
    [void][System.Runtime.InteropServices.Marshal]::FinalReleaseComObject($catalog)
    $catalog = $null
    $connection = New-Object System.Data.OleDb.OleDbConnection($connectionString)
    $connection.Open()
    $ddl = @'
CREATE TABLE Listings (
 [Id] TEXT(32) CONSTRAINT PK_Listings PRIMARY KEY,
 [Source] TEXT(30),
 [Address] TEXT(255),
 [City] TEXT(100),
 [State] TEXT(10),
 [Zip] TEXT(12),
 [Price] CURRENCY,
 [Bedrooms] INTEGER,
 [Bathrooms] DOUBLE,
 [Sqft] INTEGER,
 [Latitude] DOUBLE,
 [Longitude] DOUBLE,
 [ListedDate] DATETIME,
 [Status] TEXT(30),
 [Description] MEMO
)
'@
    $command = $connection.CreateCommand()
    $command.CommandText = $ddl
    [void]$command.ExecuteNonQuery()
    $rows = @(Get-Content -Raw $jsonPath | ConvertFrom-Json)
    foreach ($row in $rows) {
        $cmd = $connection.CreateCommand()
        $cmd.CommandText = 'INSERT INTO Listings ([Id],[Source],[Address],[City],[State],[Zip],[Price],[Bedrooms],[Bathrooms],[Sqft],[Latitude],[Longitude],[ListedDate],[Status],[Description]) VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)'
        $values = @([string]$row.id,[string]$row.source,[string]$row.address,[string]$row.city,[string]$row.state,[string]$row.zip,[decimal]$row.price,[int]$row.bedrooms,[double]$row.bathrooms,[int]$row.sqft,[double]$row.latitude,[double]$row.longitude,[datetime]::ParseExact([string]$row.listedDate,'yyyy-MM-dd',[Globalization.CultureInfo]::InvariantCulture),[string]$row.status,[string]$row.description)
        $types = @([System.Data.OleDb.OleDbType]::VarWChar,[System.Data.OleDb.OleDbType]::VarWChar,[System.Data.OleDb.OleDbType]::VarWChar,[System.Data.OleDb.OleDbType]::VarWChar,[System.Data.OleDb.OleDbType]::VarWChar,[System.Data.OleDb.OleDbType]::VarWChar,[System.Data.OleDb.OleDbType]::Currency,[System.Data.OleDb.OleDbType]::Integer,[System.Data.OleDb.OleDbType]::Double,[System.Data.OleDb.OleDbType]::Integer,[System.Data.OleDb.OleDbType]::Double,[System.Data.OleDb.OleDbType]::Double,[System.Data.OleDb.OleDbType]::Date,[System.Data.OleDb.OleDbType]::VarWChar,[System.Data.OleDb.OleDbType]::LongVarWChar)
        for ($i=0; $i -lt $values.Count; $i++) {
            $parameter = $cmd.Parameters.Add("p$i",$types[$i]); $parameter.Value = $values[$i]
        }
        [void]$cmd.ExecuteNonQuery()
        $cmd.Dispose()
    }
    Write-Host "Created $dbPath with $($rows.Count) records." -ForegroundColor Green
}
finally {
    if ($connection) { $connection.Dispose() }
    if ($catalog) { [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($catalog) }
}
