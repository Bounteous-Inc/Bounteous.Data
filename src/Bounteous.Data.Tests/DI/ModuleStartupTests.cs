using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Bounteous.Data.Tests;

public class ModuleStartupTests
{
    [Fact]
    public void RegisterServices_ShouldRegisterIDbContextObserver_AsSingleton()
    {
        var services = new ServiceCollection();
        var moduleStartup = new ModuleStartup();

        moduleStartup.RegisterServices(services);

        var serviceProvider = services.BuildServiceProvider();
        var observer = serviceProvider.GetService<IDbContextObserver>();

        observer.Should().NotBeNull();
        observer.Should().BeOfType<DefaultDbContextObserver>();
    }

    [Fact]
    public void RegisterServices_ShouldRegisterIDbContextObserver_WithSingletonLifetime()
    {
        var services = new ServiceCollection();
        var moduleStartup = new ModuleStartup();

        moduleStartup.RegisterServices(services);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDbContextObserver));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
        descriptor.ImplementationType.Should().Be(typeof(DefaultDbContextObserver));
    }

    [Fact]
    public void RegisterServices_ShouldRegisterIIdentityProvider_AsScoped()
    {
        var services = new ServiceCollection();
        var moduleStartup = new ModuleStartup();

        moduleStartup.RegisterServices(services);

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var identityProvider = scope.ServiceProvider.GetService<IIdentityProvider<int>>();

        identityProvider.Should().NotBeNull();
        identityProvider.Should().BeOfType<IdentityProvider<int>>();
    }

    [Fact]
    public void RegisterServices_ShouldRegisterIIdentityProvider_WithScopedLifetime()
    {
        var services = new ServiceCollection();
        var moduleStartup = new ModuleStartup();

        moduleStartup.RegisterServices(services);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IIdentityProvider<>));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
        descriptor.ImplementationType.Should().Be(typeof(IdentityProvider<>));
    }

    [Fact]
    public void RegisterServices_ShouldRegisterIIdentityProvider_ForDifferentGenericTypes()
    {
        var services = new ServiceCollection();
        var moduleStartup = new ModuleStartup();

        moduleStartup.RegisterServices(services);

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        
        var intProvider = scope.ServiceProvider.GetService<IIdentityProvider<int>>();
        var guidProvider = scope.ServiceProvider.GetService<IIdentityProvider<Guid>>();
        var longProvider = scope.ServiceProvider.GetService<IIdentityProvider<long>>();

        intProvider.Should().NotBeNull();
        intProvider.Should().BeOfType<IdentityProvider<int>>();
        
        guidProvider.Should().NotBeNull();
        guidProvider.Should().BeOfType<IdentityProvider<Guid>>();
        
        longProvider.Should().NotBeNull();
        longProvider.Should().BeOfType<IdentityProvider<long>>();
    }

    [Fact]
    public void RegisterServices_ShouldMaintainSingletonBehavior_ForIDbContextObserver()
    {
        var services = new ServiceCollection();
        var moduleStartup = new ModuleStartup();

        moduleStartup.RegisterServices(services);

        var serviceProvider = services.BuildServiceProvider();
        var observer1 = serviceProvider.GetService<IDbContextObserver>();
        var observer2 = serviceProvider.GetService<IDbContextObserver>();

        observer1.Should().NotBeNull();
        observer2.Should().NotBeNull();
        observer1.Should().BeSameAs(observer2);
    }

    [Fact]
    public void RegisterServices_ShouldMaintainScopedBehavior_ForIIdentityProvider()
    {
        var services = new ServiceCollection();
        var moduleStartup = new ModuleStartup();

        moduleStartup.RegisterServices(services);

        var serviceProvider = services.BuildServiceProvider();
        
        IIdentityProvider<int>? provider1Scope1;
        IIdentityProvider<int>? provider2Scope1;
        using (var scope1 = serviceProvider.CreateScope())
        {
            provider1Scope1 = scope1.ServiceProvider.GetService<IIdentityProvider<int>>();
            provider2Scope1 = scope1.ServiceProvider.GetService<IIdentityProvider<int>>();
            
            provider1Scope1.Should().NotBeNull();
            provider2Scope1.Should().NotBeNull();
            provider1Scope1.Should().BeSameAs(provider2Scope1);
        }

        IIdentityProvider<int>? providerScope2;
        using (var scope2 = serviceProvider.CreateScope())
        {
            providerScope2 = scope2.ServiceProvider.GetService<IIdentityProvider<int>>();
            providerScope2.Should().NotBeNull();
            provider1Scope1.Should().NotBeSameAs(providerScope2);
        }
    }

    [Fact]
    public void RegisterServices_ShouldNotOverrideExistingRegistration_ForIDbContextObserver()
    {
        var services = new ServiceCollection();
        var customObserver = new CustomDbContextObserver();
        services.AddSingleton<IDbContextObserver>(customObserver);
        
        var moduleStartup = new ModuleStartup();
        moduleStartup.RegisterServices(services);

        var serviceProvider = services.BuildServiceProvider();
        var observer = serviceProvider.GetService<IDbContextObserver>();

        observer.Should().NotBeNull();
        observer.Should().BeOfType<CustomDbContextObserver>();
        observer.Should().BeSameAs(customObserver);
    }

    [Fact]
    public void RegisterServices_ShouldNotOverrideExistingRegistration_ForIIdentityProvider()
    {
        var services = new ServiceCollection();
        services.AddScoped(typeof(IIdentityProvider<>), typeof(CustomIdentityProvider<>));
        
        var moduleStartup = new ModuleStartup();
        moduleStartup.RegisterServices(services);

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var provider = scope.ServiceProvider.GetService<IIdentityProvider<int>>();

        provider.Should().NotBeNull();
        provider.Should().BeOfType<CustomIdentityProvider<int>>();
    }

    [Fact]
    public void RegisterServices_ShouldRegisterAllServices_WhenCalledOnce()
    {
        var services = new ServiceCollection();
        var moduleStartup = new ModuleStartup();

        moduleStartup.RegisterServices(services);

        services.Count.Should().Be(2);
        services.Any(d => d.ServiceType == typeof(IDbContextObserver)).Should().BeTrue();
        services.Any(d => d.ServiceType == typeof(IIdentityProvider<>)).Should().BeTrue();
    }

    [Fact]
    public void Priority_ShouldReturnOne()
    {
        var moduleStartup = new ModuleStartup();

        moduleStartup.Priority.Should().Be(1);
    }

    private class CustomDbContextObserver : IDbContextObserver
    {
        public void Dispose() { }
        public void OnEntityTracked(object sender, Microsoft.EntityFrameworkCore.ChangeTracking.EntityTrackedEventArgs e) { }
        public void OnStateChanged(object? sender, Microsoft.EntityFrameworkCore.ChangeTracking.EntityStateChangedEventArgs e) { }
        public void OnSaved() { }
    }

    private class CustomIdentityProvider<TUserId> : IIdentityProvider<TUserId> where TUserId : struct
    {
        public TUserId GetCurrentUserId() => default;
    }
}
