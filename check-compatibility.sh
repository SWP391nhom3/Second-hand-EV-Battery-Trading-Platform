#!/bin/bash

# Compatibility Checker Script
# Kiểm tra tính tương thích của code với các máy khác

echo "=========================================="
echo "  COMPATIBILITY CHECKER"
echo "=========================================="
echo ""

ERRORS=0
WARNINGS=0

# Colors
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

# Function to check for hardcoded values
check_hardcoded() {
    local pattern=$1
    local file=$2
    local description=$3
    
    if grep -r "$pattern" "$file" 2>/dev/null | grep -v "example" | grep -v ".git" | grep -v "node_modules" > /dev/null; then
        echo -e "${RED}❌ ERROR:${NC} $description found in $file"
        grep -r "$pattern" "$file" 2>/dev/null | grep -v "example" | grep -v ".git" | head -3
        ((ERRORS++))
        return 1
    else
        echo -e "${GREEN}✅ OK:${NC} No $description"
        return 0
    fi
}

# Function to check if file exists
check_file_exists() {
    local file=$1
    local description=$2
    
    if [ -f "$file" ]; then
        echo -e "${GREEN}✅ OK:${NC} $description exists"
        return 0
    else
        echo -e "${YELLOW}⚠️  WARNING:${NC} $description not found"
        ((WARNINGS++))
        return 1
    fi
}

# 1. Check appsettings.example.json
echo "1. Checking configuration files..."
check_file_exists "EVehicleManagementAPI/EVehicleManagementAPI/appsettings.example.json" "appsettings.example.json"

# 2. Check if appsettings.json is in .gitignore
echo ""
echo "2. Checking .gitignore..."
if grep -q "appsettings.json" .gitignore 2>/dev/null; then
    echo -e "${GREEN}✅ OK:${NC} appsettings.json is in .gitignore"
else
    echo -e "${RED}❌ ERROR:${NC} appsettings.json NOT in .gitignore - will cause conflicts!"
    ((ERRORS++))
fi

if grep -q "launchSettings.json" .gitignore 2>/dev/null; then
    echo -e "${GREEN}✅ OK:${NC} launchSettings.json is in .gitignore"
else
    echo -e "${RED}❌ ERROR:${NC} launchSettings.json NOT in .gitignore"
    ((ERRORS++))
fi

# 3. Check for hardcoded connection strings
echo ""
echo "3. Checking for hardcoded values..."
if [ -f "EVehicleManagementAPI/EVehicleManagementAPI/appsettings.json" ]; then
    if grep -q "StrongPass123!" "EVehicleManagementAPI/EVehicleManagementAPI/appsettings.json" 2>/dev/null; then
        echo -e "${RED}❌ ERROR:${NC} Hardcoded password found in appsettings.json"
        echo "   → Should use environment variables or appsettings.Development.json"
        ((ERRORS++))
    fi
    
    if grep -q "localhost,1433" "EVehicleManagementAPI/EVehicleManagementAPI/appsettings.json" 2>/dev/null; then
        echo -e "${YELLOW}⚠️  WARNING:${NC} Hardcoded localhost in connection string"
        echo "   → Consider using environment variables"
        ((WARNINGS++))
    fi
fi

# 4. Check CORS configuration
echo ""
echo "4. Checking CORS configuration..."
if grep -q "localhost:5173" "EVehicleManagementAPI/EVehicleManagementAPI/Program.cs" 2>/dev/null; then
    if grep -q "WithOrigins\|AllowedOrigins" "EVehicleManagementAPI/EVehicleManagementAPI/Program.cs" 2>/dev/null | grep -q "Configuration\|Environment"; then
        echo -e "${GREEN}✅ OK:${NC} CORS uses configuration"
    else
        echo -e "${YELLOW}⚠️  WARNING:${NC} CORS hardcoded to localhost:5173"
        echo "   → May not work on other machines or production"
        ((WARNINGS++))
    fi
fi

# 5. Check package versions
echo ""
echo "5. Checking package versions..."
if [ -f "EVehicleManagementAPI/EVehicleManagementAPI/EVehicleManagementAPI.csproj" ]; then
    if grep -q "Version=\"9.0.9\"" "EVehicleManagementAPI/EVehicleManagementAPI/EVehicleManagementAPI.csproj"; then
        echo -e "${YELLOW}⚠️  WARNING:${NC} Specific package version (9.0.9) may cause issues"
        echo "   → Consider using version ranges or latest stable"
        ((WARNINGS++))
    fi
fi

# 6. Check for node_modules (should be in .gitignore)
echo ""
echo "6. Checking frontend..."
if [ -d "Second-hand-EV-Battery-Trading-Platform-FE" ]; then
    # Check if node_modules is tracked in git (not just if folder exists locally)
    if git ls-files | grep -q "Second-hand-EV-Battery-Trading-Platform-FE/node_modules" 2>/dev/null; then
        echo -e "${RED}❌ ERROR:${NC} node_modules is tracked in git"
        echo "   → Should be in .gitignore and removed from git"
        ((ERRORS++))
    else
        echo -e "${GREEN}✅ OK:${NC} node_modules not tracked in git"
    fi
    
    # Check if .env is tracked (should not be)
    if git ls-files | grep -q "Second-hand-EV-Battery-Trading-Platform-FE/.env$" 2>/dev/null; then
        echo -e "${RED}❌ ERROR:${NC} .env file is tracked in git"
        echo "   → Should use .env.example instead"
        ((ERRORS++))
    else
        echo -e "${GREEN}✅ OK:${NC} .env not tracked in git"
    fi
    
    check_file_exists "Second-hand-EV-Battery-Trading-Platform-FE/.env.example" ".env.example"
fi

# 7. Check README and setup documentation
echo ""
echo "7. Checking documentation..."
check_file_exists "README.md" "README.md"
check_file_exists "EVehicleManagementAPI/EVehicleManagementAPI/MIGRATION_GUIDE.md" "MIGRATION_GUIDE.md"

# 8. Check for environment variable usage
echo ""
echo "8. Checking environment variable usage..."
if grep -q "Environment.GetEnvironmentVariable\|Configuration\[" "EVehicleManagementAPI/EVehicleManagementAPI/Program.cs" 2>/dev/null; then
    echo -e "${GREEN}✅ OK:${NC} Using environment variables for configuration"
else
    echo -e "${YELLOW}⚠️  WARNING:${NC} Not using environment variables"
    echo "   → Consider using environment variables for sensitive data"
    ((WARNINGS++))
fi

# Summary
echo ""
echo "=========================================="
echo "  SUMMARY"
echo "=========================================="
echo -e "Errors: ${RED}$ERRORS${NC}"
echo -e "Warnings: ${YELLOW}$WARNINGS${NC}"
echo ""

if [ $ERRORS -eq 0 ] && [ $WARNINGS -eq 0 ]; then
    echo -e "${GREEN}✅ All checks passed!${NC}"
    exit 0
elif [ $ERRORS -eq 0 ]; then
    echo -e "${YELLOW}⚠️  Some warnings found - please review${NC}"
    exit 0
else
    echo -e "${RED}❌ Errors found - please fix before pushing${NC}"
    exit 1
fi

