// Disable warning CS8618 (Non-nullable field is uninitialized)
// _calculator is initialized in the Setup method
#pragma warning disable CS8618

/// <summary>
/// Base class for all ExpressionCalculator tests that provides common setup and utilities
/// </summary>
public abstract class ExpressionCalculatorTestBase
{
    protected global::ExpressionCalculator _calculator;

    [TestInitialize]
    public void Setup()
    {
        _calculator = new global::ExpressionCalculator();
    }
}