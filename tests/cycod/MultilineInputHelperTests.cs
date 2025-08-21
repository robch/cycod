using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class MultilineInputHelperTests
{
    [TestMethod]
    public void StartsWithBackticks_ValidBackticks_ReturnsTrue()
    {
        // Arrange
        string input = "```code block";
        
        // Act
        bool result = MultilineInputHelper.StartsWithBackticks(input);
        
        // Assert
        Assert.IsTrue(result);
    }
    
    [TestMethod]
    public void StartsWithBackticks_NotEnoughBackticks_ReturnsFalse()
    {
        // Arrange
        string input = "``code block";
        
        // Act
        bool result = MultilineInputHelper.StartsWithBackticks(input);
        
        // Assert
        Assert.IsFalse(result);
    }
    
    [TestMethod]
    public void StartsWithBackticks_NoBackticks_ReturnsFalse()
    {
        // Arrange
        string input = "code block";
        
        // Act
        bool result = MultilineInputHelper.StartsWithBackticks(input);
        
        // Assert
        Assert.IsFalse(result);
    }
    
    [TestMethod]
    public void StartsWithBackticks_NullInput_ReturnsFalse()
    {
        // Arrange
        string? input = null;
        
        // Act
        bool result = MultilineInputHelper.StartsWithBackticks(input);
        
        // Assert
        Assert.IsFalse(result);
    }
    
    // Note: Testing ReadMultilineInput is more complex as it involves Console.ReadLine
    // and would require mocking the console input
}