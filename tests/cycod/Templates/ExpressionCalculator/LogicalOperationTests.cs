[TestClass]
public class LogicalOperationTests : ExpressionCalculatorTestBase
{
    [TestMethod]
    public void TestLogicalAnd()
    {
        // Arrange & Act
        dynamic trueAndTrue = _calculator.Evaluate("true && true");
        dynamic trueAndFalse = _calculator.Evaluate("true && false");
        dynamic falseAndTrue = _calculator.Evaluate("false && true");
        dynamic falseAndFalse = _calculator.Evaluate("false && false");

        // Assert
        Assert.IsTrue(trueAndTrue);
        Assert.IsFalse(trueAndFalse);
        Assert.IsFalse(falseAndTrue);
        Assert.IsFalse(falseAndFalse);
    }

    [TestMethod]
    public void TestLogicalOr()
    {
        // Arrange & Act
        dynamic trueOrTrue = _calculator.Evaluate("true || true");
        dynamic trueOrFalse = _calculator.Evaluate("true || false");
        dynamic falseOrTrue = _calculator.Evaluate("false || true");
        dynamic falseOrFalse = _calculator.Evaluate("false || false");

        // Assert
        Assert.IsTrue(trueOrTrue);
        Assert.IsTrue(trueOrFalse);
        Assert.IsTrue(falseOrTrue);
        Assert.IsFalse(falseOrFalse);
    }

    [TestMethod]
    public void TestLogicalNot()
    {
        // Arrange & Act
        dynamic notTrue = _calculator.Evaluate("!true");
        dynamic notFalse = _calculator.Evaluate("!false");

        // Assert
        Assert.IsFalse(notTrue);
        Assert.IsTrue(notFalse);
    }

    [TestMethod]
    public void TestComplexLogicalExpression()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("true && (false || !false)");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestLogicalWithComparisons()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("(5 > 3) && (10 != 5)");

        // Assert
        Assert.IsTrue(result);
    }
}