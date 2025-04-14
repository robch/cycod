[TestClass]
public class ArithmeticTests : ExpressionCalculatorTestBase
{
    [TestMethod]
    public void TestAddition()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("2 + 3");

        // Assert
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestSubtraction()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("5 - 3");

        // Assert
        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestMultiplication()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("4 * 5");

        // Assert
        Assert.AreEqual(20.0, result);
    }

    [TestMethod]
    public void TestDivision()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("20 / 4");

        // Assert
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestModulo()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("10 % 3");

        // Assert
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void TestModKeyword()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("10 MOD 3");

        // Assert
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void TestIntegerDivision()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("10 DIV 3");

        // Assert
        Assert.AreEqual(3.0, result);
    }

    [TestMethod]
    public void TestPower()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("2 ^ 3");

        // Assert
        Assert.AreEqual(8.0, result);
    }

    [TestMethod]
    public void TestPowerWithDoubleAsterisk()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("2 ** 3");

        // Assert
        Assert.AreEqual(8.0, result);
    }

    [TestMethod]
    public void TestNegation()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("-5");

        // Assert
        Assert.AreEqual(-5.0, result);
    }

    [TestMethod]
    public void TestPositiveSign()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("+5");

        // Assert
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestComplexExpression()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("2 + 3 * 4");

        // Assert
        Assert.AreEqual(14.0, result);
    }

    [TestMethod]
    public void TestComplexExpressionWithParentheses()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("(2 + 3) * 4");

        // Assert
        Assert.AreEqual(20.0, result);
    }

    [TestMethod]
    public void TestComplexNestedExpression()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("2 * (3 + (4 - 1))");

        // Assert
        Assert.AreEqual(12.0, result);
    }
}