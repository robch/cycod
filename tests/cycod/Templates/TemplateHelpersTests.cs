
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

[TestClass]
public class TemplateHelpersTests
{
    private TemplateVariables? _variables;

    [TestInitialize]
    public void Setup()
    {
        _variables = new TemplateVariables();
        _variables!.Set("name", "John");
        _variables!.Set("age", "30");
    }

    #region Basic Template Processing Tests

    [TestMethod]
    public void ProcessTemplate_SimpleVariableSubstitution_ReplacesVariables()
    {
        // Arrange
        var template = "Hello, {name}! You are {age} years old.";
        
        // Act
        var result = TemplateHelpers.ProcessTemplate(template, _variables!);
        
        // Assert
        Assert.AreEqual("Hello, John! You are 30 years old.", result);
    }

    [TestMethod]
    public void ProcessTemplate_VariableNotFound_LeavesPlaceholder()
    {
        // Arrange
        var template = "Hello, {name}! Your email is {email}.";
        
        // Act
        var result = TemplateHelpers.ProcessTemplate(template, _variables!);
        
        // Assert
        Assert.AreEqual("Hello, John! Your email is {email}.", result);
    }

    [TestMethod]
    public void ProcessTemplate_MultipleOccurrencesOfVariable_ReplacesAll()
    {
        // Arrange
        var template = "{name} is {age} years old. Hello, {name}!";
        
        // Act
        var result = TemplateHelpers.ProcessTemplate(template, _variables!);
        
        // Assert
        Assert.AreEqual("John is 30 years old. Hello, John!", result);
    }

    #endregion

    #region Conditional Logic Tests

    [TestMethod]
    public void ProcessTemplate_IfTrueCondition_IncludesContent()
    {
        // Arrange
        var template = "Hello!\n{{if {age} > 20}}\nYou are an adult.\n{{endif}}";
        
        // Act
        var result = TemplateHelpers.ProcessTemplate(template, _variables!);
        
        // Assert
        AssertNormalizedLFsAreEqual("Hello!\nYou are an adult.", result);
    }

    [TestMethod]
    public void ProcessTemplate_IfFalseCondition_ExcludesContent()
    {
        // Arrange
        var template = "Hello!\n{{if {age} < 20}}\nYou are a minor.\n{{endif}}";
        
        // Act
        var result = TemplateHelpers.ProcessTemplate(template, _variables!);
        
        // Assert
        Assert.AreEqual("Hello!", result);
    }

    [TestMethod]
    public void ProcessTemplate_IfElseCondition_IncludesCorrectBranch()
    {
        // Arrange
        var template = "{{if {age} < 20}}\nYou are a minor.\n{{else}}\nYou are an adult.\n{{endif}}";
        
        // Act
        var result = TemplateHelpers.ProcessTemplate(template, _variables!);
        
        // Assert
        Assert.AreEqual("You are an adult.", result);
    }

    [TestMethod]
    public void ProcessTemplate_IfElseIfElseCondition_IncludesCorrectBranch()
    {
        // Arrange
        var template = "{{if {age} < 20}}\nYou are a minor.\n{{else if {age} < 65}}\nYou are an adult.\n{{else}}\nYou are a senior.\n{{endif}}";
        
        // Act
        var result = TemplateHelpers.ProcessTemplate(template, _variables!);
        
        // Assert
        Assert.AreEqual("You are an adult.", result);
    }

    [TestMethod]
    public void ProcessTemplate_NestedIf_ProcessesCorrectly()
    {
        // Arrange
        var template = "{{if {age} > 20}}\nAdult\n{{if {age} < 40}}\nYoung adult\n{{endif}}\n{{endif}}";
        
        // Act
        var result = TemplateHelpers.ProcessTemplate(template, _variables!);
        
        // Assert
        AssertNormalizedLFsAreEqual("Adult\nYoung adult", result);
    }

    #endregion

    #region Inline Conditional Tests

    [TestMethod]
    public void ProcessTemplate_InlineIf_ProcessesCorrectly()
    {
        // Arrange
        var template = "You are {{if {age} < 20}}a minor{{else}}an adult{{endif}}.";
        
        // Act
        var result = TemplateHelpers.ProcessTemplate(template, _variables!);
        
        // Assert
        Assert.AreEqual("You are an adult.", result);
    }

    [TestMethod]
    public void ProcessTemplate_MultipleInlineIf_ProcessesCorrectly()
    {
        // Arrange
        var template = "{{if {age} < 20}}Minor{{endif}} {{if {age} >= 20}}Adult{{endif}} {{if {age} > 65}}Senior{{endif}}";
        
        // Act
        var result = TemplateHelpers.ProcessTemplate(template, _variables!);
        
        // Assert
        Assert.AreEqual(" Adult ", result);
    }

    #endregion

    #region Variable Assignment Tests

    [TestMethod]
    public void ProcessTemplate_SetVariable_UpdatesVariable()
    {
        // Arrange
        var template = "{{set x = 10}}\n{{set y = 20}}\n{{set z = x + y}}\nSum: {z}";
        
        // Act
        var result = TemplateHelpers.ProcessTemplate(template, _variables!);
        
        // Assert
        Assert.AreEqual("Sum: 30", result);
        Assert.AreEqual("30", _variables!.Get("z"));
    }

    [TestMethod]
    public void ProcessTemplate_SetVariableInConditional_UpdatesVariableOnlyIfConditionMet()
    {
        // Arrange
        var template = "{{if {age} > 20}}\n{{set status = \"adult\"}}\n{{else}}\n{{set status = \"minor\"}}\n{{endif}}\nStatus: {status}";
        
        // Act
        var result = TemplateHelpers.ProcessTemplate(template, _variables!);
        
        // Assert
        Assert.AreEqual("Status: adult", result);
        Assert.AreEqual("adult", _variables!.Get("status"));
    }

    #endregion

    #region Expressions Tests

    [TestMethod]
    public void ProcessTemplate_ArithmeticExpression_EvaluatesCorrectly()
    {
        // Arrange
        var template = "{{set x = 5 + 3 * 2}}\nResult: {x}";
        
        // Act
        var result = TemplateHelpers.ProcessTemplate(template, _variables!);
        
        // Assert
        Assert.AreEqual("Result: 11", result);
    }

    [TestMethod]
    public void ProcessTemplate_BooleanExpression_EvaluatesCorrectly()
    {
        // Arrange
        var template = "{{if {age} > 20 && {age} < 40}}\nYoung adult\n{{endif}}";
        
        // Act
        var result = TemplateHelpers.ProcessTemplate(template, _variables!);
        
        // Assert
        Assert.AreEqual("Young adult", result);
    }

    #endregion

    #region Helper Methods
 
    private static void AssertNormalizedLFsAreEqual(string actual, string expected)
    {
        actual = actual.Replace("\r\n", "\n").Replace("\r", "\n");
        expected = expected.Replace("\r\n", "\n").Replace("\r", "\n");
        var maxLength = Math.Max(expected.Length, actual.Length);
        for (int i = 0; i < maxLength; i++)
        {
            if (i >= expected.Length) Assert.Fail($"Actual string is longer than expected at index {i}. actual[i]: 0x{(int)actual[i]:X}\n{Environment.StackTrace}");
            if (i >= actual.Length) Assert.Fail($"Expected string is longer than actual at index {i}. expected[i]: 0x{(int)expected[i]:X}\n{Environment.StackTrace}");

            Assert.AreEqual((int)expected[i], (int)actual[i], $"Characters at index {i} do not match. Expected: (0x{(int)expected[i]:X}), Actual: (0x{(int)actual[i]:X})\n{Environment.StackTrace}");
        }
    }

    #endregion

}