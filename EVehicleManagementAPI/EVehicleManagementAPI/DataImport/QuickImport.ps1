# PowerShell Script để Import CSV vào SQL Server
# Cách dùng: .\QuickImport.ps1 -CsvFile "VehicleModels.csv" -TableName "VehicleModels"

param(
    [Parameter(Mandatory=$true)]
    [string]$CsvFile,
    
    [Parameter(Mandatory=$true)]
    [ValidateSet("VehicleModels", "BatteryModels")]
    [string]$TableName,
    
    [string]$ServerName = "localhost",
    [string]$Database = "EVehicleDB",
    [string]$Username = "sa",
    [string]$Password = "StrongPass123!"
)

Write-Host "Importing $CsvFile to $TableName..." -ForegroundColor Green

# Đọc CSV
$csv = Import-Csv $CsvFile

# Tạo SQL INSERT statements
$sqlStatements = @()
foreach ($row in $csv) {
    if ($TableName -eq "VehicleModels") {
        $sql = "INSERT INTO VehicleModels (Name, Brand, Year, Type, MotorPower, BatteryType, Voltage, MaxSpeed, ``
                Range, Weight, Seats, Description, IsCustom, IsApproved, CreatedAt)
                VALUES ('$($row.Name)', '$($row.Brand)', $($row.Year), '$($row.Type)', $($row.MotorPower), 
                '$($row.BatteryType)', $($row.Voltage), $($row.MaxSpeed), $($row.Range), $($row.Weight), 
                $($row.Seats), '$($row.Description)', 0, 1, GETDATE());"
    }
    else {
        $sql = "INSERT INTO BatteryModels (Name, Brand, Chemistry, Voltage, CapacityKWh, Amperage, Weight, 
                FormFactor, Cycles, Description, IsCustom, IsApproved, CreatedAt)
                VALUES ('$($row.Name)', '$($row.Brand)', '$($row.Chemistry)', $($row.Voltage), 
                $($row.CapacityKWh), $($row.Amperage), $($row.Weight), '$($row.FormFactor)', 
                $($row.Cycles), '$($row.Description)', 0, 1, GETDATE());"
    }
    $sqlStatements += $sql
}

# Join statements
$finalSql = $sqlStatements -join "`n`n"

# Save to file
$outputFile = "Import_$TableName_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
$finalSql | Out-File $outputFile -Encoding UTF8

Write-Host "SQL script generated: $outputFile" -ForegroundColor Green
Write-Host "Now run this file in SQL Server Management Studio" -ForegroundColor Yellow

