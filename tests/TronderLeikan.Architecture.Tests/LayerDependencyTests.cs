using System.Reflection;
using AwesomeAssertions;
using NetArchTest.Rules;
using TronderLeikan.API.Common;
using TronderLeikan.Application.Common.Interfaces;
using TronderLeikan.Domain.Common;

namespace TronderLeikan.Architecture.Tests;

// Håndhever lagdelingen i Clean Architecture: Domain ← Application ← Infrastructure ← API.
// Feiler en test her, er det koden som skal endres — ikke regelen.
public sealed class LayerDependencyTests
{
    private const string Application = "TronderLeikan.Application";
    private const string Infrastructure = "TronderLeikan.Infrastructure";
    private const string Api = "TronderLeikan.API";
    private const string EfCore = "Microsoft.EntityFrameworkCore";

    private static readonly Assembly DomainAssembly = typeof(Entity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(IAppDbContext).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly ApiAssembly = typeof(ApiControllerBase).Assembly;

    [Fact]
    public void Domain_AvhengerIkkeAvAndreLagEllerEfCore()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot().HaveDependencyOnAny(Application, Infrastructure, Api, EfCore)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Brudd(result));
    }

    [Fact]
    public void Application_AvhengerIkkeAvInfrastructureEllerApi()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot().HaveDependencyOnAny(Infrastructure, Api)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Brudd(result));
    }

    [Fact]
    public void Infrastructure_AvhengerIkkeAvApi()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot().HaveDependencyOn(Api)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Brudd(result));
    }

    [Fact]
    public void Controllers_GaarViaSender_IkkeDirekteTilDatabasen()
    {
        // Controllers skal bare bruke ISender — aldri IAppDbContext, EF Core eller Infrastructure
        var result = Types.InAssembly(ApiAssembly)
            .That().Inherit(typeof(ApiControllerBase))
            .ShouldNot().HaveDependencyOnAny(Infrastructure, EfCore, typeof(IAppDbContext).FullName!)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Brudd(result));
    }

    [Fact]
    public void Controllers_ArverApiControllerBase()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That().HaveNameEndingWith("Controller")
            .Should().Inherit(typeof(ApiControllerBase))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Brudd(result));
    }

    [Fact]
    public void Handlers_ErSealedOgHeterHandler()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That().ImplementInterface(typeof(ICommandHandler<>))
            .Or().ImplementInterface(typeof(ICommandHandler<,>))
            .Or().ImplementInterface(typeof(IQueryHandler<,>))
            .Should().BeSealed()
            .And().HaveNameEndingWith("Handler")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Brudd(result));
    }

    private static string Brudd(TestResult result) =>
        "disse typene bryter regelen: " + string.Join(", ", result.FailingTypeNames ?? []);
}
