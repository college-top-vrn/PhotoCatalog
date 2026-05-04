namespace PhotoCatalog.Test.Architectural;

using ArchUnitNET.xUnit;



using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

public class NamingConventionsTests
{
    private static readonly Architecture Architecture =
        new ArchLoader()
            .LoadAssembly(System.Reflection.Assembly.Load("PhotoCatalog"))
            .Build();

    [Fact]
    public void AllInterfaces_Should_StartWith_I()
    {
        var rule = Interfaces()
            .Should()
            .HaveNameStartingWith("I");

        rule.Check(Architecture);
    }
}