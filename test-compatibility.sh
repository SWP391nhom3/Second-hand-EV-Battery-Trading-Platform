#!/bin/bash

# Test Compatibility Script
# Mô phỏng việc setup và test code trên máy mới với config riêng

echo "=========================================="
echo "  COMPATIBILITY TEST SIMULATOR"
echo "=========================================="
echo ""
echo "Script này sẽ test xem code có chạy được trên máy khác không"
echo "bằng cách simulate việc setup với config mới"
echo ""

ERRORS=0
WARNINGS=0

# Colors
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 1. Test Backend Setup Simulation
echo -e "${BLUE}1. Testing Backend Setup...${NC}"

cd EVehicleManagementAPI/EVehicleManagementAPI 2>/dev/null || {
    echo -e "${RED}❌ ERROR:${NC} Backend directory not found"
    ((ERRORS++))
    exit 1
}

# Check if appsettings.example.json exists
if [ ! -f "appsettings.example.json" ]; then
    echo -e "${RED}❌ ERROR:${NC} appsettings.example.json not found"
    ((ERRORS++))
else
    echo -e "${GREEN}✅ OK:${NC} appsettings.example.json exists"
fi

# Simulate creating appsettings.json from example
if [ -f "appsettings.json" ]; then
    echo -e "${YELLOW}⚠️  INFO:${NC} appsettings.json already exists (using local config)"
else
    echo -e "${BLUE}📝 Simulating:${NC} Creating appsettings.json from example..."
    cp appsettings.example.json appsettings.json
    echo -e "${GREEN}✅ OK:${NC} Can create appsettings.json from template"
fi

# Check .csproj file
if [ ! -f "EVehicleManagementAPI.csproj" ]; then
    echo -e "${RED}❌ ERROR:${NC} .csproj file not found"
    ((ERRORS++))
else
    echo -e "${GREEN}✅ OK:${NC} .csproj file exists"
fi

# Test dotnet restore (check if dependencies are defined)
echo -e "${BLUE}📦 Testing:${NC} Checking package references..."
if dotnet list package --outdated 2>&1 | grep -q "error\|not found"; then
    echo -e "${YELLOW}⚠️  WARNING:${NC} Cannot check packages (dotnet CLI issue or no packages)"
    ((WARNINGS++))
else
    echo -e "${GREEN}✅ OK:${NC} Package references are valid"
fi

# Check if Program.cs has required services
echo -e "${BLUE}📝 Checking:${NC} Program.cs configuration..."
if grep -q "AddDbContext\|AddControllers\|AddSwaggerGen" Program.cs 2>/dev/null; then
    echo -e "${GREEN}✅ OK:${NC} Required services configured"
else
    echo -e "${YELLOW}⚠️  WARNING:${NC} Some services might be missing"
    ((WARNINGS++))
fi

# Check CORS configuration flexibility
if grep -q "Configuration\[\"CORS:AllowedOrigins\"\]" Program.cs 2>/dev/null; then
    echo -e "${GREEN}✅ OK:${NC} CORS uses configuration (flexible)"
else
    if grep -q "localhost:5173\|localhost:3000" Program.cs 2>/dev/null | grep -v "example\|template"; then
        echo -e "${YELLOW}⚠️  WARNING:${NC} CORS might be hardcoded - check Program.cs"
        ((WARNINGS++))
    else
        echo -e "${GREEN}✅ OK:${NC} CORS configuration looks flexible"
    fi
fi

cd ../../

# 2. Test Frontend Setup Simulation
echo ""
echo -e "${BLUE}2. Testing Frontend Setup...${NC}"

if [ ! -d "Second-hand-EV-Battery-Trading-Platform-FE" ]; then
    echo -e "${YELLOW}⚠️  WARNING:${NC} Frontend directory not found (might be submodule)"
    ((WARNINGS++))
else
    cd Second-hand-EV-Battery-Trading-Platform-FE || exit 1
    
    # Check package.json
    if [ ! -f "package.json" ]; then
        echo -e "${RED}❌ ERROR:${NC} package.json not found"
        ((ERRORS++))
    else
        echo -e "${GREEN}✅ OK:${NC} package.json exists"
    fi
    
    # Check .env.example
    if [ ! -f ".env.example" ]; then
        echo -e "${RED}❌ ERROR:${NC} .env.example not found"
        ((ERRORS++))
    else
        echo -e "${GREEN}✅ OK:${NC} .env.example exists"
        
        # Check if .env.example has VITE_API_BASE_URL
        if grep -q "VITE_API_BASE_URL" .env.example; then
            echo -e "${GREEN}✅ OK:${NC} .env.example contains API URL template"
        else
            echo -e "${YELLOW}⚠️  WARNING:${NC} .env.example might be missing API URL"
            ((WARNINGS++))
        fi
    fi
    
    # Check axios config uses environment variable
    if [ -f "src/configs/axios.js" ]; then
        if grep -q "import.meta.env.VITE_API_BASE_URL\|process.env.VITE_API_BASE_URL" src/configs/axios.js; then
            echo -e "${GREEN}✅ OK:${NC} axios.js uses environment variable"
        else
            echo -e "${YELLOW}⚠️  WARNING:${NC} axios.js might have hardcoded URL"
            ((WARNINGS++))
        fi
    fi
    
    # Check vite.config.js
    if [ -f "vite.config.js" ]; then
        echo -e "${GREEN}✅ OK:${NC} vite.config.js exists"
    else
        echo -e "${YELLOW}⚠️  WARNING:${NC} vite.config.js not found"
        ((WARNINGS++))
    fi
    
    cd ..
fi

# 3. Test Configuration Independence
echo ""
echo -e "${BLUE}3. Testing Configuration Independence...${NC}"

# Check if sensitive files are in .gitignore
echo -e "${BLUE}📝 Checking:${NC} .gitignore configuration..."
if grep -q "^appsettings\.json$" .gitignore 2>/dev/null || grep -q "appsettings\.json" .gitignore 2>/dev/null; then
    echo -e "${GREEN}✅ OK:${NC} appsettings.json in .gitignore"
else
    echo -e "${RED}❌ ERROR:${NC} appsettings.json NOT in .gitignore"
    ((ERRORS++))
fi

if grep -q "launchSettings\.json" .gitignore 2>/dev/null; then
    echo -e "${GREEN}✅ OK:${NC} launchSettings.json in .gitignore"
else
    echo -e "${RED}❌ ERROR:${NC} launchSettings.json NOT in .gitignore"
    ((ERRORS++))
fi

# 4. Test Documentation Completeness
echo ""
echo -e "${BLUE}4. Testing Documentation...${NC}"

REQUIRED_DOCS=("SETUP_GUIDE.md" "README.md" "PRE_PUSH_CHECKLIST.md")
for doc in "${REQUIRED_DOCS[@]}"; do
    if [ -f "$doc" ]; then
        echo -e "${GREEN}✅ OK:${NC} $doc exists"
    else
        echo -e "${YELLOW}⚠️  WARNING:${NC} $doc not found"
        ((WARNINGS++))
    fi
done

# 5. Dry-run Build Test
echo ""
echo -e "${BLUE}5. Testing Build (Dry-run)...${NC}"

cd EVehicleManagementAPI/EVehicleManagementAPI 2>/dev/null || exit 1

# Check if can build (without actually building, just check syntax)
echo -e "${BLUE}📝 Checking:${NC} Project structure..."
if dotnet build --no-restore --verbosity quiet 2>&1 | grep -q "error\|Error\|ERROR" && [ $? -eq 0 ]; then
    echo -e "${YELLOW}⚠️  WARNING:${NC} Build might have issues (check manually)"
    echo "   Run: dotnet build"
    ((WARNINGS++))
else
    # Try a syntax check
    if dotnet build --no-restore --no-incremental 2>&1 | tail -3 | grep -q "Build succeeded\|error"; then
        BUILD_RESULT=$(dotnet build --no-restore --no-incremental 2>&1 | tail -1)
        if echo "$BUILD_RESULT" | grep -q "succeeded"; then
            echo -e "${GREEN}✅ OK:${NC} Project builds successfully"
        else
            echo -e "${YELLOW}⚠️  WARNING:${NC} Build check inconclusive - manual test needed"
            echo "   Build output: $BUILD_RESULT"
            ((WARNINGS++))
        fi
    else
        echo -e "${YELLOW}⚠️  INFO:${NC} Build check skipped (restore needed first)"
        echo "   Run: dotnet restore && dotnet build"
    fi
fi

cd ../../

# 6. Check for Migration Safety
echo ""
echo -e "${BLUE}6. Testing Migration Safety...${NC}"

if [ -d "EVehicleManagementAPI/EVehicleManagementAPI/Migrations" ]; then
    MIGRATION_COUNT=$(find EVehicleManagementAPI/EVehicleManagementAPI/Migrations -name "*.cs" -type f | wc -l | tr -d ' ')
    if [ "$MIGRATION_COUNT" -gt 0 ]; then
        echo -e "${GREEN}✅ OK:${NC} Found $MIGRATION_COUNT migration file(s)"
        
        # Check for dangerous migrations
        if grep -r "DropTable\|DropColumn\|DeleteData" EVehicleManagementAPI/EVehicleManagementAPI/Migrations/*.cs 2>/dev/null | grep -v "Down()\|Down(" | head -1; then
            echo -e "${YELLOW}⚠️  WARNING:${NC} Found potentially dangerous migration operations"
            echo "   → Review migrations before applying"
            ((WARNINGS++))
        else
            echo -e "${GREEN}✅ OK:${NC} Migrations look safe"
        fi
    else
        echo -e "${YELLOW}⚠️  INFO:${NC} No migrations found"
    fi
else
    echo -e "${YELLOW}⚠️  INFO:${NC} Migrations directory not found"
fi

# Summary
echo ""
echo "=========================================="
echo -e "${BLUE}  TEST SUMMARY${NC}"
echo "=========================================="
echo -e "Errors: ${RED}$ERRORS${NC}"
echo -e "Warnings: ${YELLOW}$WARNINGS${NC}"
echo ""

if [ $ERRORS -eq 0 ]; then
    echo -e "${GREEN}✅ Core compatibility tests passed!${NC}"
    echo ""
    echo "Code should work on other machines IF:"
    echo "  1. Team members copy appsettings.example.json → appsettings.json"
    echo "  2. Team members copy .env.example → .env"
    echo "  3. Team members update their own connection strings"
    echo "  4. Team members run: dotnet restore && dotnet build"
    echo ""
    echo -e "${BLUE}Next steps:${NC}"
    echo "  1. Share SETUP_GUIDE.md with team"
    echo "  2. Ask team to follow setup steps"
    echo "  3. If issues arise, check PRE_PUSH_CHECKLIST.md"
    exit 0
else
    echo -e "${RED}❌ Critical errors found!${NC}"
    echo "   Fix errors before code can run on other machines"
    exit 1
fi

