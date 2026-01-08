#!/bin/bash

# Bounteous.Data Sample Application Runner
# This script builds the solution and runs the sample application

set -e  # Exit on error

echo "========================================="
echo "Building Bounteous.Data Solution"
echo "========================================="
dotnet build --configuration Release --output ./temp-build

echo ""
echo "========================================="
echo "Running Bounteous.Data.Sample Application"
echo "========================================="
dotnet run --project src/Bounteous.Data.Sample/Bounteous.Data.Sample.csproj

echo ""
echo "========================================="
echo "Sample application completed successfully!"
echo "========================================="
