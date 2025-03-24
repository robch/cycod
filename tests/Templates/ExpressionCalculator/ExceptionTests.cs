namespace Templates.ExpressionCalculator;

[TestClass]
public class ExceptionTests : ExpressionCalculatorTestBase
{
    [TestMethod]
    [ExpectedException(typeof(CalcException))]
    public void TestMissingClosingParenthesis()
    {
        // Act
        _calculator.Evaluate("5 * (2 + 3");
    }

    [TestMethod]
    [ExpectedException(typeof(CalcException))]
    public void TestUnexpectedCharacter()
    {
        // Act
        _calculator.Evaluate("5 # 3");
    }

    [TestMethod]
    [ExpectedException(typeof(CalcException))]
    public void TestUndefinedVariable()
    {
        // Act
        _calculator.Evaluate("undefinedVar");
    }

    [TestMethod]
    [ExpectedException(typeof(CalcException))]
    public void TestMissingOpenParenthesisInFunction()
    {
        // Act
        _calculator.Evaluate("SIN 0");
    }

    [TestMethod]
    [ExpectedException(typeof(CalcException))]
    public void TestMissingClosingParenthesisInFunction()
    {
        // Act
        _calculator.Evaluate("SIN(0");
    }

    [TestMethod]
    [ExpectedException(typeof(CalcException))]
    public void TestMissingCommaInTwoParameterFunction()
    {
        // Act
        _calculator.Evaluate("MAX(5 6)");
    }

    [TestMethod]
    [ExpectedException(typeof(CalcException))]
    public void TestNonStringArgumentInStringFunction()
    {
        // Act
        _calculator.Evaluate("TOLOWER(123)");
    }

    [TestMethod]
    [ExpectedException(typeof(CalcException))]
    public void TestLogicalOperationWithNonBooleanValues()
    {
        // Act
        _calculator.Evaluate("5 && 3");
    }

    [TestMethod]
    [ExpectedException(typeof(CalcException))]
    public void TestInvalidStringComparison()
    {
        // Act
        _calculator.Evaluate("\"hello\" && \"world\"");
    }

    [TestMethod]
    [ExpectedException(typeof(CalcException))]
    public void TestMissingQuoteInStringLiteral()
    {
        // Act
        _calculator.Evaluate("\"hello");
    }
}