[TestClass]
public class RadixConversionTests : ExpressionCalculatorTestBase
{
    [TestMethod]
    public void TestRadixConversionDecToHex()
    {
        // Arrange & Act
        string result = _calculator.StrFromDRadix(255.0, global::ExpressionCalculator.Radix.Hex);

        // Assert
        Assert.AreEqual("0FFh", result);
    }

    [TestMethod]
    public void TestRadixConversionDecToBin()
    {
        // Arrange & Act
        string result = _calculator.StrFromDRadix(7.0, global::ExpressionCalculator.Radix.Bin);

        // Assert
        Assert.AreEqual("111b", result);
    }

    [TestMethod]
    public void TestRadixConversionDecToOct()
    {
        // Arrange & Act
        string result = _calculator.StrFromDRadix(8.0, global::ExpressionCalculator.Radix.Oct);

        // Assert
        Assert.AreEqual("10o", result);
    }

    [TestMethod]
    public void TestRadixConversionDecToDec()
    {
        // Arrange & Act
        string result = _calculator.StrFromDRadix(42.0, global::ExpressionCalculator.Radix.Dec);

        // Assert
        Assert.AreEqual("42", result);
    }

    [TestMethod]
    [ExpectedException(typeof(CalcException))]
    public void TestRadixConversionWithTooLargeValue()
    {
        // Arrange & Act
        _calculator.StrFromDRadix(double.MaxValue, global::ExpressionCalculator.Radix.Hex);
    }
}