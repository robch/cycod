namespace Templates.ExpressionCalculator;

[TestClass]
public class ConstantsTests : ExpressionCalculatorTestBase
{
    [TestMethod]
    public void TestConstantPI()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("PI");

        // Assert
        Assert.AreEqual(3.14159265358979, result);
    }

    [TestMethod]
    public void TestConstantE()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("E");

        // Assert
        Assert.AreEqual(2.71828182845905, result);
    }

    [TestMethod]
    public void TestConstantCaseInsensitivity()
    {
        // Arrange & Act
        dynamic result1 = _calculator.Evaluate("PI");
        dynamic result2 = _calculator.Evaluate("pi");
        dynamic result3 = _calculator.Evaluate("Pi");

        // Assert
        Assert.AreEqual(result1, result2);
        Assert.AreEqual(result1, result3);
    }
}