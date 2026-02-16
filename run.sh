#!/bin/bash

# ============================================
# DirectPay Gateway - Development Runner
# ============================================

set -e

PROJECT_ROOT="$(cd "$(dirname "$0")" && pwd)"
API_DIR="$PROJECT_ROOT/src/API"
WEB_DIR="$PROJECT_ROOT/src/Web"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}============================================${NC}"
echo -e "${BLUE}   DirectPay Gateway - Development Runner   ${NC}"
echo -e "${BLUE}============================================${NC}"
echo ""

# ---------------------
# Cleanup on exit
# ---------------------
cleanup() {
    echo ""
    echo -e "${YELLOW}Shutting down...${NC}"
    if [ ! -z "$API_PID" ]; then
        kill $API_PID 2>/dev/null && echo -e "${GREEN}Backend stopped${NC}"
    fi
    if [ ! -z "$WEB_PID" ]; then
        kill $WEB_PID 2>/dev/null && echo -e "${GREEN}Frontend stopped${NC}"
    fi
    # Kill any remaining processes on ports
    lsof -ti:5000 | xargs kill -9 2>/dev/null
    lsof -ti:3000 | xargs kill -9 2>/dev/null
    echo -e "${GREEN}Done.${NC}"
    exit 0
}
trap cleanup SIGINT SIGTERM

# ---------------------
# Check prerequisites
# ---------------------
echo -e "${YELLOW}[1/5] Checking prerequisites...${NC}"

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}ERROR: .NET SDK not found. Install from https://dotnet.microsoft.com/download${NC}"
    exit 1
fi
DOTNET_VERSION=$(dotnet --version)
echo -e "  .NET SDK: ${GREEN}$DOTNET_VERSION${NC}"

if ! command -v node &> /dev/null; then
    echo -e "${RED}ERROR: Node.js not found. Install from https://nodejs.org${NC}"
    exit 1
fi
NODE_VERSION=$(node --version)
NPM_VERSION=$(npm --version)
echo -e "  Node.js:  ${GREEN}$NODE_VERSION${NC}"
echo -e "  npm:      ${GREEN}$NPM_VERSION${NC}"
echo ""

# ---------------------
# Kill existing processes on ports
# ---------------------
echo -e "${YELLOW}[2/5] Freeing ports 5000 and 3000...${NC}"
lsof -ti:5000 | xargs kill -9 2>/dev/null && echo -e "  Port 5000: ${GREEN}freed${NC}" || echo -e "  Port 5000: ${GREEN}available${NC}"
lsof -ti:3000 | xargs kill -9 2>/dev/null && echo -e "  Port 3000: ${GREEN}freed${NC}" || echo -e "  Port 3000: ${GREEN}available${NC}"
sleep 1
echo ""

# ---------------------
# Copy seed file if needed
# ---------------------
if [ -f "$PROJECT_ROOT/campaigns-seed.json" ]; then
    cp "$PROJECT_ROOT/campaigns-seed.json" "$API_DIR/campaigns-seed.json"
    echo -e "  Campaigns seed file: ${GREEN}copied${NC}"
fi

# ---------------------
# Restore & Build Backend
# ---------------------
echo -e "${YELLOW}[3/5] Building backend...${NC}"
cd "$PROJECT_ROOT"
dotnet restore --verbosity quiet 2>/dev/null
dotnet build --verbosity quiet --no-restore 2>/dev/null
echo -e "  Backend: ${GREEN}built successfully${NC}"
echo ""

# ---------------------
# Install Frontend Dependencies
# ---------------------
echo -e "${YELLOW}[4/5] Installing frontend dependencies...${NC}"
cd "$WEB_DIR"
if [ ! -d "node_modules" ]; then
    npm install --silent 2>/dev/null
    echo -e "  Dependencies: ${GREEN}installed${NC}"
else
    echo -e "  Dependencies: ${GREEN}already installed${NC}"
fi
echo ""

# ---------------------
# Start Both Servers
# ---------------------
echo -e "${YELLOW}[5/5] Starting servers...${NC}"
echo ""

# Start Backend
cd "$API_DIR"
ASPNETCORE_ENVIRONMENT=Development dotnet run --no-build &
API_PID=$!
echo -e "  Backend PID:  ${GREEN}$API_PID${NC}"

# Start Frontend
cd "$WEB_DIR"
npm start &
WEB_PID=$!
echo -e "  Frontend PID: ${GREEN}$WEB_PID${NC}"

# Wait for servers to start
sleep 5

echo ""
echo -e "${BLUE}============================================${NC}"
echo -e "${GREEN}  Servers are running!${NC}"
echo -e "${BLUE}============================================${NC}"
echo ""
echo -e "  Frontend:  ${GREEN}http://localhost:3000${NC}"
echo -e "  Backend:   ${GREEN}http://localhost:5000${NC}"
echo -e "  Swagger:   ${GREEN}http://localhost:5000/swagger${NC}"
echo ""
echo -e "  Admin:     ${YELLOW}admin@admin.com / Admin123!${NC}"
echo ""
echo -e "  Press ${RED}Ctrl+C${NC} to stop all servers"
echo -e "${BLUE}============================================${NC}"

# Wait for both processes
wait
