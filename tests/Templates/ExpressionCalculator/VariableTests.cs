[TestClass]
public class VariableTests : ExpressionCalculatorTestBase
{
    [TestMethod]
    public void TestVariableAssignment()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("x = 5");

        // Assert
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestVariableUsage()
    {
        // Arrange
        _calculator.Evaluate("x = 5");

        // Act
        dynamic result = _calculator.Evaluate("x + 3");

        // Assert
        Assert.AreEqual(8.0, result);
    }

    [TestMethod]
    public void TestVariableReassignment()
    {
        // Arrange
        _calculator.Evaluate("x = 5");

        // Act
        dynamic reassign = _calculator.Evaluate("x = 10");
        dynamic result = _calculator.Evaluate("x");

        // Assert
        Assert.AreEqual(10.0, reassign);
        Assert.AreEqual(10.0, result);
    }

    [TestMethod]
    public void TestVariableCaseInsensitivity()
    {
        // Arrange
        _calculator.Evaluate("x = 5");

        // Act
        dynamic result1 = _calculator.Evaluate("x");
        dynamic result2 = _calculator.Evaluate("X");

        // Assert
        Assert.AreEqual(result1, result2);
    }

    [TestMethod]
    public void TestAddVariable()
    {
        // Arrange
        var variable = new global::ExpressionCalculator.Variable("testVar", 42.0);
        _calculator.AddVariable(variable);

        // Act
        dynamic result = _calculator.Evaluate("testVar");

        // Assert
        Assert.AreEqual(42.0, result);
    }

    [TestMethod]
    public void TestAssignToExpressionResult()
    {
        // Arrange & Act
        dynamic assignment = _calculator.Evaluate("x = 2 + 3 * 4");
        dynamic result = _calculator.Evaluate("x");

        // Assert
        Assert.AreEqual(14.0, assignment);
        Assert.AreEqual(14.0, result);
    }
}