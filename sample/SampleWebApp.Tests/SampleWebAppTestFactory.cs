using Microsoft.AspNetCore.Mvc.Testing;

namespace SampleWebApp.Tests;

public class SampleWebAppTestFactory : WebApplicationFactory<Program>
{
}

[CollectionDefinition(Name)]
public class TestCollection : ICollectionFixture<SampleWebAppTestFactory>

{
    public const string Name = "SampleWebApp.Tests";
    
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
