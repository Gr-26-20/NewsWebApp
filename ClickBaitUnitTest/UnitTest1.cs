using Xunit;

namespace NewsWebApp.Tests;

public class SetupTests
{
    [Fact]
    public void TestProject_CanRunTests()
    {
        Assert.Equal(4, 2 + 2);
    }
}