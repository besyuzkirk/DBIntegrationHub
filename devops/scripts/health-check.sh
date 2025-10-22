#!/bin/bash
# Health check script for all services

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🔍 DBIntegrationHub Health Check${NC}"
echo ""

# Function to check service
check_service() {
    local name=$1
    local url=$2
    
    echo -n "Checking $name... "
    
    if curl -sf "$url" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ OK${NC}"
        return 0
    else
        echo -e "${RED}✗ FAIL${NC}"
        return 1
    fi
}

# Function to check container
check_container() {
    local name=$1
    
    echo -n "Checking $name container... "
    
    if docker-compose ps | grep -q "$name.*Up"; then
        echo -e "${GREEN}✓ Running${NC}"
        return 0
    else
        echo -e "${RED}✗ Not Running${NC}"
        return 1
    fi
}

# Check containers
echo -e "${BLUE}📦 Container Status:${NC}"
check_container "postgres"
check_container "api"
check_container "ui"
echo ""

# Check services
echo -e "${BLUE}🌐 Service Health:${NC}"
check_service "PostgreSQL" "localhost:5432" || true
check_service "API" "http://localhost:5149/health"
check_service "UI" "http://localhost:3000"
echo ""

# Show resource usage
echo -e "${BLUE}💻 Resource Usage:${NC}"
docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}" \
  dbintegrationhub-postgres dbintegrationhub-api dbintegrationhub-ui 2>/dev/null || echo "Container stats not available"
echo ""

echo -e "${GREEN}✅ Health check complete!${NC}"

