namespace Templates.ExpressionCalculator;

[TestClass]
public class BitwiseOperationTests : ExpressionCalculatorTestBase
{
    [TestMethod]
    public void TestBitwiseAnd()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("5 & 3");

        // Assert
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void TestBitwiseOr()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("5 | 3");

        // Assert
        Assert.AreEqual(7.0, result);
    }

    [TestMethod]
    public void TestBitwiseNot()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("~5");

        // Assert
        Assert.AreEqual(-6.0, result);
    }

    [TestMethod]
    public void TestComplexBitwiseExpression()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("(5 & 3) | (8 & 12)");

        // Assert
        Assert.AreEqual(9.0, result);
    }
}