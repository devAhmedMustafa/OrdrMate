using Xunit;

public class TestBase
{
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(5, 7, 12)]
    [InlineData(-1, 1, 0)]
    [InlineData(0, 0, 0)]
    public void TestAddition(int a, int b, int expected)
    {
        int result = a + b;
        Assert.Equal(expected, result);
    }
}