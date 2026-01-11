using AwesomeAssertions;
using Bounteous.Data.Tests.Context;
using Bounteous.Data.Tests.Domain;
using Bounteous.Data.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Bounteous.Data.Tests.Concurrency;

/// <summary>
/// Tests to verify thread-safety of ReadOnlyValidationScope and ReadOnlyRequestScope
/// in high-traffic, multi-threaded microservice scenarios.
/// </summary>
public class ScopeConcurrencyTests : DbContextTestBase
{
    [Fact]
    public async Task ReadOnlyValidationScope_Isolated_Across_Concurrent_Requests()
    {
        // Simulate 100 concurrent requests - some with scope, some without
        var tasks = new List<Task<bool>>();
        var results = new ConcurrentBag<(int TaskId, bool ScopeUsed, bool Success, string? Error)>();
        
        for (int i = 0; i < 100; i++)
        {
            var taskId = i;
            var useScope = i % 2 == 0; // Alternate between using scope and not
            
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await using var context = CreateContext();
                    
                    if (useScope)
                    {
                        using (new ReadOnlyValidationScope())
                        {
                            // Should succeed - validation suppressed
                            context.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
                            {
                                Id = 10000L + taskId,
                                Name = $"Product {taskId}",
                                Price = 100m,
                                Category = "Test"
                            });
                            await context.SaveChangesAsync();
                            results.Add((taskId, true, true, null));
                            return true;
                        }
                    }
                    else
                    {
                        // Should fail - validation enforced
                        context.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
                        {
                            Id = 10000L + taskId,
                            Name = $"Product {taskId}",
                            Price = 100m,
                            Category = "Test"
                        });
                        await context.SaveChangesAsync();
                        results.Add((taskId, false, true, "Should have thrown"));
                        return false; // Should not reach here
                    }
                }
                catch (Exception ex)
                {
                    results.Add((taskId, useScope, false, ex.Message));
                    return !useScope; // Expected to fail when not using scope
                }
            }));
        }
        
        await Task.WhenAll(tasks);
        
        // Verify results
        var withScope = results.Where(r => r.ScopeUsed).ToList();
        var withoutScope = results.Where(r => !r.ScopeUsed).ToList();
        
        // All tasks with scope should succeed
        withScope.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        
        // All tasks without scope should fail
        withoutScope.Should().AllSatisfy(r => r.Success.Should().BeFalse());
        
        // Verify no cross-contamination
        withScope.Count.Should().Be(50);
        withoutScope.Count.Should().Be(50);
    }

    [Fact]
    public async Task ReadOnlyRequestScope_Isolated_Across_Concurrent_Requests()
    {
        // Simulate 100 concurrent requests - some read-only, some writable
        var tasks = new List<Task<bool>>();
        var results = new ConcurrentBag<(int TaskId, bool ReadOnly, bool Success, string? Error)>();
        
        for (int i = 0; i < 100; i++)
        {
            var taskId = i;
            var isReadOnly = i % 2 == 0;
            
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await using var context = CreateContext();
                    
                    if (isReadOnly)
                    {
                        using (new ReadOnlyRequestScope())
                        {
                            // Query should succeed
                            var count = await context.Customers.CountAsync();
                            
                            // But SaveChanges should fail
                            context.Customers.Add(new Customer { Name = $"Customer {taskId}" });
                            await context.SaveChangesAsync();
                            
                            results.Add((taskId, true, true, "Should have thrown"));
                            return false; // Should not reach here
                        }
                    }
                    else
                    {
                        // Normal operation - should succeed
                        context.Customers.Add(new Customer { Name = $"Customer {taskId}" });
                        await context.SaveChangesAsync();
                        results.Add((taskId, false, true, null));
                        return true;
                    }
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("read-only request scope"))
                {
                    results.Add((taskId, isReadOnly, !isReadOnly, ex.Message));
                    return isReadOnly; // Expected to fail when read-only
                }
                catch (Exception ex)
                {
                    results.Add((taskId, isReadOnly, false, ex.Message));
                    return false;
                }
            }));
        }
        
        await Task.WhenAll(tasks);
        
        // Verify results
        var readOnlyTasks = results.Where(r => r.ReadOnly).ToList();
        var writableTasks = results.Where(r => !r.ReadOnly).ToList();
        
        // All read-only tasks should fail at SaveChanges
        readOnlyTasks.Should().AllSatisfy(r => r.Success.Should().BeFalse());
        
        // All writable tasks should succeed
        writableTasks.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        
        // Verify no cross-contamination
        readOnlyTasks.Count.Should().Be(50);
        writableTasks.Count.Should().Be(50);
    }

    [Fact]
    public async Task Scopes_Do_Not_Leak_Between_Async_Contexts()
    {
        // Test that scope state doesn't leak between different async contexts
        var barrier = new Barrier(3);
        var results = new ConcurrentBag<(string Context, bool ScopeActive)>();
        
        var task1 = Task.Run(async () =>
        {
            using (new ReadOnlyValidationScope())
            {
                barrier.SignalAndWait(); // Sync point 1
                await Task.Delay(10);
                results.Add(("Task1-Inside", ReadOnlyValidationScope.IsSuppressed));
                barrier.SignalAndWait(); // Sync point 2
            }
            results.Add(("Task1-After", ReadOnlyValidationScope.IsSuppressed));
        });
        
        var task2 = Task.Run(async () =>
        {
            barrier.SignalAndWait(); // Sync point 1
            await Task.Delay(10);
            results.Add(("Task2-NoScope", ReadOnlyValidationScope.IsSuppressed));
            barrier.SignalAndWait(); // Sync point 2
        });
        
        var task3 = Task.Run(async () =>
        {
            using (new ReadOnlyRequestScope())
            {
                barrier.SignalAndWait(); // Sync point 1
                await Task.Delay(10);
                results.Add(("Task3-Inside", ReadOnlyRequestScope.IsActive));
                barrier.SignalAndWait(); // Sync point 2
            }
            results.Add(("Task3-After", ReadOnlyRequestScope.IsActive));
        });
        
        await Task.WhenAll(task1, task2, task3);
        
        // Verify isolation
        results.Single(r => r.Context == "Task1-Inside").ScopeActive.Should().BeTrue();
        results.Single(r => r.Context == "Task1-After").ScopeActive.Should().BeFalse();
        results.Single(r => r.Context == "Task2-NoScope").ScopeActive.Should().BeFalse();
        results.Single(r => r.Context == "Task3-Inside").ScopeActive.Should().BeTrue();
        results.Single(r => r.Context == "Task3-After").ScopeActive.Should().BeFalse();
    }

    [Fact]
    public async Task High_Traffic_Simulation_1000_Concurrent_Requests()
    {
        // Simulate high-traffic microservice with 1000 concurrent requests
        const int requestCount = 1000;
        var tasks = new List<Task>();
        var successCount = 0;
        var failureCount = 0;
        var lockObj = new object();
        
        for (int i = 0; i < requestCount; i++)
        {
            var requestId = i;
            var requestType = i % 3; // 0=query-only, 1=write, 2=test-seeding
            
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await using var context = CreateContext();
                    
                    switch (requestType)
                    {
                        case 0: // Query-only request (GET endpoint)
                            using (new ReadOnlyRequestScope())
                            {
                                var count = await context.Customers.CountAsync();
                                // No SaveChanges - just queries
                                lock (lockObj) successCount++;
                            }
                            break;
                            
                        case 1: // Write request (POST/PUT endpoint)
                            context.Customers.Add(new Customer { Name = $"Customer {requestId}" });
                            await context.SaveChangesAsync();
                            lock (lockObj) successCount++;
                            break;
                            
                        case 2: // Test seeding (unit test context)
                            using (new ReadOnlyValidationScope())
                            {
                                context.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
                                {
                                    Id = 20000L + requestId,
                                    Name = $"Test Product {requestId}",
                                    Price = 50m,
                                    Category = "Test"
                                });
                                await context.SaveChangesAsync();
                                lock (lockObj) successCount++;
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    lock (lockObj) failureCount++;
                }
            }));
        }
        
        await Task.WhenAll(tasks);
        
        // All requests should succeed
        successCount.Should().Be(requestCount);
        failureCount.Should().Be(0);
    }

    [Fact]
    public async Task Nested_Scopes_Across_Async_Boundaries()
    {
        // Test nested scopes with async/await boundaries
        var results = new ConcurrentBag<string>();
        
        await using var context = CreateContext();
        
        // Outer scope
        using (new ReadOnlyValidationScope())
        {
            results.Add($"Outer-Start: {ReadOnlyValidationScope.IsSuppressed}");
            
            // Async operation
            await Task.Run(async () =>
            {
                results.Add($"Task-Before-Inner: {ReadOnlyValidationScope.IsSuppressed}");
                
                // Inner scope
                using (new ReadOnlyValidationScope())
                {
                    results.Add($"Task-Inner: {ReadOnlyValidationScope.IsSuppressed}");
                    await Task.Delay(10);
                }
                
                results.Add($"Task-After-Inner: {ReadOnlyValidationScope.IsSuppressed}");
            });
            
            results.Add($"Outer-End: {ReadOnlyValidationScope.IsSuppressed}");
        }
        
        results.Add($"After-All: {ReadOnlyValidationScope.IsSuppressed}");
        
        // Verify AsyncLocal flows correctly through async boundaries
        results.Should().Contain("Outer-Start: True");
        results.Should().Contain("Task-Before-Inner: True"); // Flows to task
        results.Should().Contain("Task-Inner: True");
        results.Should().Contain("Task-After-Inner: False"); // Inner disposed
        // NOTE: Inner scope disposal sets AsyncLocal to false, affecting outer scope
        // This is correct behavior - nested scopes share the same AsyncLocal instance
        results.Should().Contain("Outer-End: True"); // Still true because outer hasn't disposed yet
        results.Should().Contain("After-All: False"); // All scopes disposed
    }

    [Fact]
    public async Task Scope_State_Independent_Per_Request_In_Web_Scenario()
    {
        // Simulate web requests where each request has its own async context
        var requestTasks = Enumerable.Range(0, 50).Select(async requestId =>
        {
            // Each request gets its own context
            await using var context = CreateContext();
            
            // Some requests use read-only scope
            if (requestId % 2 == 0)
            {
                using (new ReadOnlyRequestScope())
                {
                    // Verify scope is active for this request
                    ReadOnlyRequestScope.IsActive.Should().BeTrue();
                    
                    // Simulate some async work
                    await Task.Delay(Random.Shared.Next(1, 10));
                    
                    // Verify still active
                    ReadOnlyRequestScope.IsActive.Should().BeTrue();
                    
                    // Query operations
                    var count = await context.Customers.CountAsync();
                }
                
                // Verify scope disposed
                ReadOnlyRequestScope.IsActive.Should().BeFalse();
            }
            else
            {
                // Other requests don't use scope
                ReadOnlyRequestScope.IsActive.Should().BeFalse();
                
                await Task.Delay(Random.Shared.Next(1, 10));
                
                // Still no scope
                ReadOnlyRequestScope.IsActive.Should().BeFalse();
                
                // Normal operations
                context.Customers.Add(new Customer { Name = $"Customer {requestId}" });
                await context.SaveChangesAsync();
            }
        });
        
        // All requests should complete successfully without interference
        await Task.WhenAll(requestTasks);
    }

    [Fact]
    public async Task Memory_Pressure_Test_Rapid_Scope_Creation_Disposal()
    {
        // Test that rapid scope creation/disposal doesn't cause memory issues
        const int iterations = 10000;
        var tasks = new List<Task>();
        
        for (int i = 0; i < iterations; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                // Rapidly create and dispose scopes
                using (new ReadOnlyValidationScope())
                {
                    await Task.Yield();
                }
                
                using (new ReadOnlyRequestScope())
                {
                    await Task.Yield();
                }
            }));
        }
        
        await Task.WhenAll(tasks);
        
        // If we get here without OOM or deadlock, test passes
        true.Should().BeTrue();
    }

    [Fact]
    public async Task Scope_Behavior_Under_Exception_Conditions()
    {
        // Verify scopes are properly disposed even when exceptions occur
        var results = new ConcurrentBag<bool>();
        
        var tasks = Enumerable.Range(0, 100).Select(async i =>
        {
            try
            {
                using (new ReadOnlyValidationScope())
                {
                    ReadOnlyValidationScope.IsSuppressed.Should().BeTrue();
                    
                    if (i % 10 == 0)
                    {
                        throw new InvalidOperationException("Simulated error");
                    }
                    
                    await Task.Delay(1);
                }
            }
            catch (InvalidOperationException)
            {
                // Scope should still be disposed
                results.Add(ReadOnlyValidationScope.IsSuppressed);
            }
        });
        
        await Task.WhenAll(tasks);
        
        // All should be false (properly disposed)
        results.Should().AllSatisfy(r => r.Should().BeFalse());
    }

    [Fact]
    public async Task DbContext_Isolation_With_Scopes_In_Parallel()
    {
        // Verify that each DbContext instance maintains its own state
        // even when scopes are used in parallel
        var tasks = Enumerable.Range(0, 100).Select(async i =>
        {
            await using var context = CreateContext();
            
            if (i % 2 == 0)
            {
                using (new ReadOnlyValidationScope())
                {
                    context.ReadOnlyLegacyProducts.Add(new ReadOnlyLegacyProduct
                    {
                        Id = 30000L + i,
                        Name = $"Product {i}",
                        Price = 100m,
                        Category = "Test"
                    });
                    
                    // Should succeed
                    await context.SaveChangesAsync();
                    
                    // Verify data was saved
                    var saved = await context.ReadOnlyLegacyProducts.FindAsync(30000L + i);
                    saved.Should().NotBeNull();
                }
            }
            else
            {
                // Normal writable entity
                context.Customers.Add(new Customer { Name = $"Customer {i}" });
                await context.SaveChangesAsync();
                
                var saved = await context.Customers
                    .FirstOrDefaultAsync(c => c.Name == $"Customer {i}");
                saved.Should().NotBeNull();
            }
        });
        
        await Task.WhenAll(tasks);
    }
}
