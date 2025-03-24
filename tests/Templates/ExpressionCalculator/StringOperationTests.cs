namespace Templates.ExpressionCalculator;

[TestClass]
public class StringOperationTests : ExpressionCalculatorTestBase
{
    [TestMethod]
    public void TestStringLiteral()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("\"Hello World\"");

        // Assert
        Assert.AreEqual("Hello World", result);
    }

    [TestMethod]
    public void TestStringComparisonEquality()
    {
        // Arrange & Act
        dynamic equal = _calculator.Evaluate("\"Hello\" == \"Hello\"");
        dynamic notEqual = _calculator.Evaluate("\"Hello\" == \"World\"");

        // Assert
        Assert.IsTrue(equal);
        Assert.IsFalse(notEqual);
    }

    [TestMethod]
    public void TestStringComparisonInequality()
    {
        // Arrange & Act
        dynamic notEqual = _calculator.Evaluate("\"Hello\" != \"World\"");
        dynamic equal = _calculator.Evaluate("\"Hello\" != \"Hello\"");

        // Assert
        Assert.IsTrue(notEqual);
        Assert.IsFalse(equal);
    }

    [TestMethod]
    public void TestStringComparisonLessThan()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("\"Apple\" < \"Banana\"");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestStringComparisonGreaterThan()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("\"Zebra\" > \"Apple\"");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TestStringFunctionToLower()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("TOLOWER(\"Hello WORLD\")");

        // Assert
        Assert.AreEqual("hello world", result);
    }

    [TestMethod]
    public void TestStringFunctionToUpper()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("TOUPPER(\"Hello world\")");

        // Assert
        Assert.AreEqual("HELLO WORLD", result);
    }

    [TestMethod]
    public void TestStringFunctionEquals()
    {
        // Arrange & Act
        dynamic equal = _calculator.Evaluate("EQUALS(\"Hello\", \"Hello\")");
        dynamic notEqual = _calculator.Evaluate("EQUALS(\"Hello\", \"World\")");

        // Assert
        Assert.IsTrue(equal);
        Assert.IsFalse(notEqual);
    }

    [TestMethod]
    public void TestStringFunctionContains()
    {
        // Arrange & Act
        dynamic contains = _calculator.Evaluate("CONTAINS(\"Hello World\", \"World\")");
        dynamic notContains = _calculator.Evaluate("CONTAINS(\"Hello World\", \"Universe\")");

        // Assert
        Assert.IsTrue(contains);
        Assert.IsFalse(notContains);
    }

    [TestMethod]
    public void TestStringFunctionStartsWith()
    {
        // Arrange & Act
        dynamic startsWith = _calculator.Evaluate("STARTSWITH(\"Hello World\", \"Hello\")");
        dynamic notStartsWith = _calculator.Evaluate("STARTSWITH(\"Hello World\", \"World\")");

        // Assert
        Assert.IsTrue(startsWith);
        Assert.IsFalse(notStartsWith);
    }

    [TestMethod]
    public void TestStringFunctionEndsWith()
    {
        // Arrange & Act
        dynamic endsWith = _calculator.Evaluate("ENDSWITH(\"Hello World\", \"World\")");
        dynamic notEndsWith = _calculator.Evaluate("ENDSWITH(\"Hello World\", \"Hello\")");

        // Assert
        Assert.IsTrue(endsWith);
        Assert.IsFalse(notEndsWith);
    }

    [TestMethod]
    public void TestStringFunctionIsEmpty()
    {
        // Arrange & Act
        dynamic empty = _calculator.Evaluate("ISEMPTY(\"\")");
        dynamic notEmpty = _calculator.Evaluate("ISEMPTY(\"Hello\")");

        // Assert
        Assert.IsTrue(empty);
        Assert.IsFalse(notEmpty);
    }
}