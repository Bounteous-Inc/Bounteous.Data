#!/bin/bash

# Bounteous.Data Pre-Commit Validation Script
# This script runs build, tests, and sample application to validate changes before committing

set -e  # Exit on error

echo "========================================="
echo "Bounteous.Data Pre-Commit Validation"
echo "========================================="
echo ""

echo "========================================="
echo "Step 1: Building Solution"
echo "========================================="
dotnet build --configuration Release

echo ""
echo "========================================="
echo "Step 2: Running Automated Tests"
echo "========================================="
dotnet test --no-build --configuration Release --verbosity normal

echo ""
echo "========================================="
echo "Step 3: Running Sample Application"
echo "========================================="
dotnet run --project src/Bounteous.Data.Sample/Bounteous.Data.Sample.csproj --no-build --configuration Release

echo ""
echo "========================================="
echo "✓ All Validations Passed Successfully!"
echo "========================================="
echo ""
echo "Your changes are ready to commit and push."
echo ""
