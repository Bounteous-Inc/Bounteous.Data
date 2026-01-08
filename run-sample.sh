#!/bin/bash

# Bounteous.Data Sample Application Runner
# This script builds the solution, runs tests, and runs the sample application

set -e  # Exit on error

echo "========================================="
echo "Building Bounteous.Data Solution"
echo "========================================="
dotnet build --configuration Release

echo ""
echo "========================================="
echo "Running Automated Tests"
echo "========================================="
dotnet test --no-build --configuration Release --verbosity normal

echo ""
echo "========================================="
echo "Running Bounteous.Data.Sample Application"
echo "========================================="
dotnet run --project src/Bounteous.Data.Sample/Bounteous.Data.Sample.csproj --no-build --configuration Release

echo ""
echo "========================================="
echo "All validations completed successfully!"
echo "========================================="
