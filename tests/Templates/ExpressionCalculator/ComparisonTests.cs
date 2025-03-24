namespace Templates.ExpressionCalculator;

[TestClass]
public class ComparisonTests : ExpressionCalculatorTestBase
{
    [TestMethod]
    public void TestEqualComparison()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("5 == 5");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestNotEqualComparison()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("5 != 3");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestLessThanComparison()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("3 < 5");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestLessThanOrEqualComparison()
    {
        // Arrange & Act
        dynamic resultLess = _calculator.Evaluate("3 <= 5");
        dynamic resultEqual = _calculator.Evaluate("5 <= 5");

        // Assert
        Assert.IsTrue(resultLess);
        Assert.IsTrue(resultEqual);
    }

    [TestMethod]
    public void TestGreaterThanComparison()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("5 > 3");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestGreaterThanOrEqualComparison()
    {
        // Arrange & Act
        dynamic resultGreater = _calculator.Evaluate("5 >= 3");
        dynamic resultEqual = _calculator.Evaluate("5 >= 5");

        // Assert
        Assert.IsTrue(resultGreater);
        Assert.IsTrue(resultEqual);
    }

    [TestMethod]
    public void TestComparisonWithExpressions()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("(2 + 3) > (1 + 3)");

        // Assert
        Assert.IsTrue(result);
    }
}