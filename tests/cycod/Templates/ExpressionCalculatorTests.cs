
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ExpressionCalculatorTests
{
    private ExpressionCalculator? _calculator;

    [TestInitialize]
    public void Setup()
    {
        _calculator = new ExpressionCalculator();
    }

    #region Arithmetic Operations Tests

    [TestMethod]
    public void Evaluate_BasicArithmetic_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = _calculator!.Evaluate("2 + 3 * 4");

        // Assert
        Assert.AreEqual(14.0, result);
    }

    [TestMethod]
    public void Evaluate_Parentheses_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = _calculator!.Evaluate("(2 + 3) * 4");

        // Assert
        Assert.AreEqual(20.0, result);
    }

    [TestMethod]
    public void Evaluate_Division_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = _calculator!.Evaluate("10 / 2");

        // Assert
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void Evaluate_Modulo_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = _calculator!.Evaluate("10 % 3");

        // Assert
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void Evaluate_Power_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = _calculator!.Evaluate("2 ^ 3");

        // Assert
        Assert.AreEqual(8.0, result);
    }

    [TestMethod]
    public void Evaluate_Negation_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = _calculator!.Evaluate("-5 + 3");

        // Assert
        Assert.AreEqual(-2.0, result);
    }

    [TestMethod]
    public void Evaluate_ComplexExpression_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = _calculator!.Evaluate("2 + 3 * 4 - (10 / 2) + 7");

        // Assert
        Assert.AreEqual(16.0, result);
    }

    #endregion

    #region String Operation Tests

    [TestMethod]
    public void Evaluate_StringComparison_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = _calculator!.Evaluate("\"apple\" < \"banana\"");

        // Assert
        Assert.IsTrue((bool)result);
    }

    [TestMethod]
    public void Evaluate_StringEquals_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = _calculator!.Evaluate("EQUALS(\"test\", \"test\")");

        // Assert
        Assert.IsTrue((bool)result);
    }

    [TestMethod]
    public void Evaluate_StringContains_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = _calculator!.Evaluate("CONTAINS(\"testing\", \"test\")");

        // Assert
        Assert.IsTrue((bool)result);
    }

    [TestMethod]
    public void Evaluate_ToLower_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = _calculator!.Evaluate("TOLOWER(\"HELLO\")");

        // Assert
        Assert.AreEqual("hello", result);
    }

    [TestMethod]
    public void Evaluate_ToUpper_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = _calculator!.Evaluate("TOUPPER(\"hello\")");

        // Assert
        Assert.AreEqual("HELLO", result);
    }

    #endregion

    #region Boolean Operation Tests

    [TestMethod]
    public void Evaluate_LogicalAnd_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = _calculator!.Evaluate("true && true");

        // Assert
        Assert.IsTrue((bool)result);
    }

    [TestMethod]
    public void Evaluate_LogicalOr_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = _calculator!.Evaluate("false || true");

        // Assert
        Assert.IsTrue((bool)result);
    }

    [TestMethod]
    public void Evaluate_LogicalNot_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = _calculator!.Evaluate("!false");

        // Assert
        Assert.IsTrue((bool)result);
    }

    [TestMethod]
    public void Evaluate_Comparison_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result1 = _calculator!.Evaluate("5 > 3");
        var result2 = _calculator!.Evaluate("5 >= 5");
        var result3 = _calculator!.Evaluate("3 < 5");
        var result4 = _calculator!.Evaluate("5 <= 5");
        var result5 = _calculator!.Evaluate("5 == 5");
        var result6 = _calculator!.Evaluate("5 != 3");

        // Assert
        Assert.IsTrue((bool)result1);
        Assert.IsTrue((bool)result2);
        Assert.IsTrue((bool)result3);
        Assert.IsTrue((bool)result4);
        Assert.IsTrue((bool)result5);
        Assert.IsTrue((bool)result6);
    }

    #endregion

    #region Math Function Tests

    [TestMethod]
    public void Evaluate_MathFunctions_ReturnsCorrectResults()
    {
        // Arrange & Act
        var absResult = _calculator!.Evaluate("ABS(-5)");
        var sinResult = _calculator!.Evaluate("SIN(0)");
        var cosResult = _calculator!.Evaluate("COS(0)");
        var maxResult = _calculator!.Evaluate("MAX(5, 10)");
        var minResult = _calculator!.Evaluate("MIN(5, 10)");
        var sqrtResult = _calculator!.Evaluate("SQRT(4)");

        // Assert
        Assert.AreEqual(5.0, absResult);
        Assert.AreEqual(0.0, sinResult);
        Assert.AreEqual(1.0, cosResult);
        Assert.AreEqual(10.0, maxResult);
        Assert.AreEqual(5.0, minResult);
        Assert.AreEqual(2.0, sqrtResult);
    }

    #endregion

    #region Error Handling Tests

    [TestMethod]
    [ExpectedException(typeof(CalcException))]
    public void Evaluate_UnbalancedParentheses_ThrowsException()
    {
        // Arrange & Act
        _calculator!.Evaluate("2 + (3 * 4");

        // Assert is handled by ExpectedException
    }

    [TestMethod]
    [ExpectedException(typeof(CalcException))]
    public void Evaluate_UnknownFunction_ThrowsException()
    {
        // Arrange & Act
        _calculator!.Evaluate("UNKNOWN_FUNC(5)");

        // Assert is handled by ExpectedException
    }

    [TestMethod]
    [ExpectedException(typeof(CalcException))]
    public void Evaluate_InvalidSyntax_ThrowsException()
    {
        // Arrange & Act
        _calculator!.Evaluate("5 + * 3");

        // Assert is handled by ExpectedException
    }

    #endregion

    #region Variable Tests

    [TestMethod]
    public void Evaluate_VariableAssignment_StoresAndRetrievesCorrectly()
    {
        // Arrange & Act
        _calculator!.Evaluate("x = 10");
        var result = _calculator!.Evaluate("x + 5");

        // Assert
        Assert.AreEqual(15.0, result);
    }

    [TestMethod]
    public void Evaluate_MultipleVariables_CalculatesCorrectly()
    {
        // Arrange & Act
        _calculator!.Evaluate("x = 10");
        _calculator!.Evaluate("y = 5");
        var result = _calculator!.Evaluate("x * y");

        // Assert
        Assert.AreEqual(50.0, result);
    }

    [TestMethod]
    public void Evaluate_VariableReassignment_UpdatesValue()
    {
        // Arrange & Act
        _calculator!.Evaluate("x = 10");
        _calculator!.Evaluate("x = 20");
        var result = _calculator!.Evaluate("x");

        // Assert
        Assert.AreEqual(20.0, result);
    }

    #endregion

    #region Constant Tests

    [TestMethod]
    public void Evaluate_BuiltInConstants_ReturnsCorrectValues()
    {
        // Arrange & Act
        var piResult = _calculator!.Evaluate("PI");
        var eResult = _calculator!.Evaluate("E");

        // Assert
        Assert.AreEqual(3.14159265358979, piResult);
        Assert.AreEqual(2.71828182845905, eResult);
    }

    [TestMethod]
    public void Evaluate_CustomConstant_ReturnsCorrectValue()
    {
        // Arrange
        _calculator!.AddConstant(new ExpressionCalculator.Constant("GOLDEN_RATIO", 1.61803398875));
        
        // Act
        var result = _calculator!.Evaluate("GOLDEN_RATIO");

        // Assert
        Assert.AreEqual(1.61803398875, result);
    }

    #endregion
}