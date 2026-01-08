# Generic User ID Tests Documentation

## Overview

This document describes the comprehensive xUnit test suite for the generic user ID support in `DbContextBase<TUserId>`.

## Test Coverage

### Test File: `GenericUserIdTests.cs`

This test suite validates that the generic `DbContextBase<TUserId>` correctly handles audit operations for different user ID types (`long`, `int`, etc.).

## Test Cases

### 1. **DbContextBase_With_Long_UserId_Should_Set_Audit_Fields_On_Create**
- **Purpose**: Validates that audit fields are correctly set when creating a new entity with `long` user ID
- **User ID Type**: `long`
- **Verifies**:
  - `CreatedBy` is set to the provided user ID
  - `ModifiedBy` is set to the provided user ID
  - `CreatedOn` and `ModifiedOn` timestamps are set
  - `Version` is incremented to 1

### 2. **DbContextBase_With_Long_UserId_Should_Update_Audit_Fields_On_Modify**
- **Purpose**: Validates that audit fields are correctly updated when modifying an existing entity
- **User ID Type**: `long`
- **Verifies**:
  - `CreatedBy` remains unchanged (original user)
  - `ModifiedBy` is updated to the new user ID
  - `ModifiedOn` is later than `CreatedOn`
  - `Version` is incremented to 2

### 3. **DbContextBase_With_Long_UserId_Should_Handle_Null_UserId**
- **Purpose**: Validates behavior when no user ID is provided
- **User ID Type**: `long`
- **Verifies**:
  - `CreatedBy` and `ModifiedBy` are null
  - Timestamps are still set correctly
  - `Version` remains 0 when no user ID is provided

### 4. **DbContextBase_With_Int_UserId_Should_Set_Audit_Fields_On_Create**
- **Purpose**: Validates that audit fields work correctly with `int` user ID type
- **User ID Type**: `int`
- **Verifies**:
  - All audit fields are set correctly with integer user IDs
  - Type safety is maintained

### 5. **DbContextBase_With_Int_UserId_Should_Update_Audit_Fields_On_Modify**
- **Purpose**: Validates update operations with `int` user ID type
- **User ID Type**: `int`
- **Verifies**:
  - Update audit logic works correctly with integer user IDs
  - Version tracking works as expected

### 6. **DbContextBase_With_Long_UserId_Should_Support_Multiple_Entities**
- **Purpose**: Validates that multiple entity types can be audited in the same context
- **User ID Type**: `long`
- **Verifies**:
  - Multiple entities (Product and Customer) are audited correctly
  - Same user ID is applied to all entities in the transaction

### 7. **DbContextBase_With_Long_UserId_Should_Handle_Soft_Delete**
- **Purpose**: Validates soft delete functionality with generic user IDs
- **User ID Type**: `long`
- **Verifies**:
  - `IsDeleted` flag is set to true
  - `ModifiedBy` is updated with the deleting user's ID
  - Entity is not physically removed from database

## Test Infrastructure

### Test Entities

1. **`ProductWithLongUserId`**
   - Inherits from `AuditBase<Guid, long>`
   - Entity ID: `Guid`
   - User ID: `long`
   - Properties: Name, Description, Price

2. **`CustomerWithLongUserId`**
   - Inherits from `AuditBase<Guid, long>`
   - Entity ID: `Guid`
   - User ID: `long`
   - Properties: Name, Email

3. **`ProductWithIntUserId`**
   - Inherits from `AuditBase<Guid, int>`
   - Entity ID: `Guid`
   - User ID: `int`
   - Properties: Name, Description, Price

### Test Contexts

1. **`TestDbContextLong`**
   - Inherits from `DbContextBase<long>`
   - Supports entities with `long` user IDs
   - DbSets: Products, Customers

2. **`TestDbContextInt`**
   - Inherits from `DbContextBase<int>`
   - Supports entities with `int` user IDs
   - DbSets: Products

## Running the Tests

### Command Line
```bash
cd /Users/greg/Projects/bounteous/github/Bounteous.Data
dotnet test src/Bounteous.Data.Tests/Bounteous.Data.Tests.csproj
```

### Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~GenericUserIdTests"
```

### Single Test
```bash
dotnet test --filter "FullyQualifiedName~DbContextBase_With_Long_UserId_Should_Set_Audit_Fields_On_Create"
```

## Backward Compatibility

The existing tests in `DbContextObserverTests.cs` continue to work without modification because:
- The original `DbContextBase` now inherits from `DbContextBase<Guid>`
- All existing entities using `AuditBase` continue to use `Guid` for user IDs
- No breaking changes to the public API

## NuGet Package Validation

These tests ensure that when published as a NuGet package, consumers can:

1. **Use the default Guid-based implementation** (backward compatible)
   ```csharp
   public class MyContext : DbContextBase { }
   ```

2. **Use long-based user IDs** (new functionality)
   ```csharp
   public class MyContext : DbContextBase<long> { }
   public class MyEntity : AuditBase<int, long> { }
   ```

3. **Use int-based user IDs** (new functionality)
   ```csharp
   public class MyContext : DbContextBase<int> { }
   public class MyEntity : AuditBase<Guid, int> { }
   ```

4. **Mix entity ID types and user ID types** (flexible)
   ```csharp
   // Entity ID: int, User ID: long
   public class Product : AuditBase<int, long> { }
   
   // Entity ID: Guid, User ID: int
   public class Order : AuditBase<Guid, int> { }
   ```

## Expected Results

All tests should pass, demonstrating:
- ✅ Type-safe user ID handling
- ✅ Correct audit field population
- ✅ Version tracking
- ✅ Soft delete support
- ✅ Multiple entity support
- ✅ Null user ID handling
- ✅ Backward compatibility with existing Guid-based code

## Notes

- All tests use in-memory databases with unique names to avoid conflicts
- Mock observers are used to verify the observer pattern still works
- Tests verify both creation and modification scenarios
- Timestamp assertions use a 5-second tolerance to account for test execution time
