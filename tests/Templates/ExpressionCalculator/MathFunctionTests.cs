[TestClass]
public class MathFunctionTests : ExpressionCalculatorTestBase
{
    [TestMethod]
    public void TestMathFunctionAbs()
    {
        // Arrange & Act
        dynamic positive = _calculator.Evaluate("ABS(5)");
        dynamic negative = _calculator.Evaluate("ABS(-5)");

        // Assert
        Assert.AreEqual(5.0, positive);
        Assert.AreEqual(5.0, negative);
    }

    [TestMethod]
    public void TestMathFunctionAcos()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("ACOS(1)");

        // Assert
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void TestMathFunctionAsin()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("ASIN(0)");

        // Assert
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void TestMathFunctionAtan()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("ATAN(0)");

        // Assert
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void TestMathFunctionAtan2()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("ATAN2(0, 1)");

        // Assert
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void TestMathFunctionCeil()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("CEIL(4.3)");

        // Assert
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void TestMathFunctionCos()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("COS(0)");

        // Assert
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void TestMathFunctionCosh()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("COSH(0)");

        // Assert
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void TestMathFunctionExp()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("EXP(0)");

        // Assert
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void TestMathFunctionFloor()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("FLOOR(4.7)");

        // Assert
        Assert.AreEqual(4.0, result);
    }

    [TestMethod]
    public void TestMathFunctionLog()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("LOG(1)");

        // Assert
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void TestMathFunctionLog10()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("LOG10(100)");

        // Assert
        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestMathFunctionSin()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("SIN(0)");

        // Assert
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void TestMathFunctionSinh()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("SINH(0)");

        // Assert
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void TestMathFunctionSqrt()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("SQRT(4)");

        // Assert
        Assert.AreEqual(2.0, result);
    }

    [TestMethod]
    public void TestMathFunctionTan()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("TAN(0)");

        // Assert
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void TestMathFunctionTanh()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("TANH(0)");

        // Assert
        Assert.AreEqual(0.0, result);
    }

    [TestMethod]
    public void TestMathFunctionTruncate()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("TRUNCATE(4.7)");

        // Assert
        Assert.AreEqual(4.0, result);
    }

    [TestMethod]
    public void TestMathFunctionMax()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("MAX(3, 7)");

        // Assert
        Assert.AreEqual(7.0, result);
    }

    [TestMethod]
    public void TestMathFunctionMin()
    {
        // Arrange & Act
        dynamic result = _calculator.Evaluate("MIN(3, 7)");

        // Assert
        Assert.AreEqual(3.0, result);
    }
}