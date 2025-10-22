using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Handles "click-through" display of assistant responses, showing text in chunks
/// with typewriter effect, requiring user interaction to advance.
/// </summary>
public class ClickThroughDisplayHelper
{
    private readonly StringBuilder _textBuffer = new StringBuilder();
    private readonly List<string> _chunks = new List<string>();
    private int _currentChunkIndex = 0;
    private bool _isEnabled = false;
    private int _typewriterDelayMs = 30;
    private int _maxSentencesPerChunk = 3;

    public ClickThroughDisplayHelper(bool enabled = false, int typewriterDelayMs = 30, int maxSentencesPerChunk = 3)
    {
        _isEnabled = enabled;
        _typewriterDelayMs = typewriterDelayMs;
        _maxSentencesPerChunk = maxSentencesPerChunk;
    }

    public bool IsEnabled => _isEnabled;

    /// <summary>
    /// Accumulates streaming text and processes it into displayable chunks
    /// </summary>
    public void AccumulateText(string newText)
    {
        if (!_isEnabled || string.IsNullOrEmpty(newText)) return;

        _textBuffer.Append(newText);
        ProcessBufferIntoChunks();
    }

    /// <summary>
    /// Processes and displays the next available chunk with typewriter effect
    /// </summary>
    public async Task<bool> ProcessNextChunk()
    {
        if (!_isEnabled || _currentChunkIndex >= _chunks.Count) return false;

        var chunk = _chunks[_currentChunkIndex];
        await DisplayChunkWithTypewriterEffect(chunk);
        
        _currentChunkIndex++;
        
        // If there are more chunks, wait for user input
        if (HasMoreContent())
        {
            DisplayContinuePrompt();
            WaitForUserInput();
            EraseContinuePrompt();
        }

        return true;
    }

    /// <summary>
    /// Processes any remaining buffered text and displays all remaining chunks
    /// </summary>
    public async Task ProcessRemainingContent()
    {
        if (!_isEnabled) return;

        // Process any final text in buffer
        FinalizeBuffer();

        // Display all remaining chunks
        while (HasMoreContent())
        {
            await ProcessNextChunk();
        }
    }

    /// <summary>
    /// Checks if there are more chunks to display
    /// </summary>
    public bool HasMoreContent()
    {
        return _isEnabled && _currentChunkIndex < _chunks.Count;
    }

    /// <summary>
    /// Resets the helper for a new assistant response
    /// </summary>
    public void Reset()
    {
        _textBuffer.Clear();
        _chunks.Clear();
        _currentChunkIndex = 0;
    }

    /// <summary>
    /// Enables or disables the click-through mode
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        _isEnabled = enabled;
        if (!enabled)
        {
            Reset();
        }
    }

    private void ProcessBufferIntoChunks()
    {
        var text = _textBuffer.ToString();
        var sentences = SplitIntoSentences(text);
        
        // Only process complete sentences (ending with . ! ? or similar)
        var completeSentences = new List<string>();
        var incompleteSentence = "";

        for (int i = 0; i < sentences.Count; i++)
        {
            var sentence = sentences[i].Trim();
            if (string.IsNullOrEmpty(sentence)) continue;

            // Check if this sentence looks complete
            if (IsCompleteSentence(sentence) || i < sentences.Count - 1)
            {
                completeSentences.Add(sentence);
            }
            else
            {
                // This is likely an incomplete sentence at the end
                incompleteSentence = sentence;
            }
        }

        // Create new chunks from complete sentences
        CreateChunksFromSentences(completeSentences);

        // Keep the incomplete sentence in the buffer
        _textBuffer.Clear();
        if (!string.IsNullOrEmpty(incompleteSentence))
        {
            _textBuffer.Append(incompleteSentence);
        }
    }

    private void FinalizeBuffer()
    {
        var remainingText = _textBuffer.ToString().Trim();
        if (!string.IsNullOrEmpty(remainingText))
        {
            // Add any remaining text as a final chunk
            _chunks.Add(remainingText);
            _textBuffer.Clear();
        }
    }

    private void CreateChunksFromText(string text)
    {
        var sentences = SplitIntoSentences(text);
        
        // Only process complete sentences (ending with . ! ? or similar)
        var completeSentences = new List<string>();
        var incompleteSentence = "";

        for (int i = 0; i < sentences.Count; i++)
        {
            var sentence = sentences[i].Trim();
            if (string.IsNullOrEmpty(sentence)) continue;

            // Check if this sentence looks complete
            if (IsCompleteSentence(sentence) || i < sentences.Count - 1)
            {
                completeSentences.Add(sentence);
            }
            else
            {
                // This is likely an incomplete sentence at the end
                incompleteSentence = sentence;
            }
        }

        // Create chunks from complete sentences using character length constraints
        CreateChunksFromSentences(completeSentences);

        // Keep the incomplete sentence in the buffer
        _textBuffer.Clear();
        if (!string.IsNullOrEmpty(incompleteSentence))
        {
            _textBuffer.Append(incompleteSentence);
        }
    }

    private List<string> SplitIntoSentences(string text)
    {
        // Split on sentence endings, but try to preserve the punctuation
        var pattern = @"(?<=[.!?])\s+(?=[A-Z])|(?<=[.!?])\s*$";
        var sentences = Regex.Split(text, pattern, RegexOptions.Multiline)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        return sentences;
    }

    private bool IsCompleteSentence(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence)) return false;
        
        var trimmed = sentence.Trim();
        return trimmed.EndsWith('.') || trimmed.EndsWith('!') || trimmed.EndsWith('?') ||
               trimmed.EndsWith(':') || trimmed.EndsWith(';');
    }

    private void CreateChunksFromSentences(List<string> sentences)
    {
        var currentChunk = new StringBuilder();
        var sentenceCount = 0;

        foreach (var sentence in sentences)
        {
            if (sentenceCount > 0 && sentenceCount >= _maxSentencesPerChunk)
            {
                // Start a new chunk
                var chunkText = currentChunk.ToString().Trim();
                if (!string.IsNullOrEmpty(chunkText))
                {
                    _chunks.Add(chunkText);
                }
                currentChunk.Clear();
                sentenceCount = 0;
            }

            if (currentChunk.Length > 0)
            {
                currentChunk.Append(' ');
            }
            currentChunk.Append(sentence.Trim());
            sentenceCount++;
        }

        // Add any remaining content as a chunk
        var remainingText = currentChunk.ToString().Trim();
        if (!string.IsNullOrEmpty(remainingText))
        {
            _chunks.Add(remainingText);
        }
    }



    private bool IsBreakablePunctuation(char c)
    {
        return c == ';' || c == ',' || c == '-' || c == '\n';
    }

    private async Task DisplayChunkWithTypewriterEffect(string chunk)
    {
        for (int i = 0; i < chunk.Length; i++)
        {
            char c = chunk[i];
            ConsoleHelpers.Write(c.ToString(), ConsoleColor.White, overrideQuiet: true);
            
            // Calculate delay based on character type
            var delay = CalculateCharacterDelay(chunk, i);
            
            if (delay > 0)
            {
                await Task.Delay(delay);
            }
        }
        
        // Ensure we're not at the beginning of a new line when showing the prompt
        // If the chunk ended with a newline, we need to handle the prompt differently
    }

    private int CalculateCharacterDelay(string text, int index)
    {
        char c = text[index];
        
        // Skip delay for whitespace
        if (char.IsWhiteSpace(c))
        {
            return 0;
        }

        // Check for ellipsis (3 dots in a row) - 3 characters per second = 333ms per character
        if (c == '.' && IsPartOfEllipsis(text, index))
        {
            return 333; // 3 characters per second
        }

        // Breaking punctuation gets a 1-second pause
        if (c == '.' || c == '!' || c == '?')
        {
            return 1000; // 1 second pause
        }

        // Other punctuation gets a shorter pause
        if (IsBreakablePunctuation(c))
        {
            return 200; // 200ms pause for commas, colons, etc.
        }

        // Regular characters use the standard typewriter delay
        return _typewriterDelayMs;
    }

    private bool IsPartOfEllipsis(string text, int index)
    {
        if (text[index] != '.') return false;

        // Count consecutive dots around this position
        int dotCount = 0;
        int start = index;
        
        // Go backwards to find start of dots
        while (start > 0 && text[start - 1] == '.')
        {
            start--;
        }
        
        // Count dots from start position
        int pos = start;
        while (pos < text.Length && text[pos] == '.')
        {
            dotCount++;
            pos++;
        }
        
        // It's an ellipsis if we have 3 or more dots
        return dotCount >= 3;
    }

    private void DisplayContinuePrompt()
    {
        ConsoleHelpers.Write("(*)", ConsoleColor.DarkGray, overrideQuiet: true);
    }

    private void EraseContinuePrompt()
    {
        // Replace the (*) with spaces, but preserve any spacing structure
        // Move back, write spaces to clear, move back to continue from the right position
        ConsoleHelpers.Write("\b\b\b", overrideQuiet: true);  // Move back 3 chars to start of (*)
        ConsoleHelpers.Write("   ", overrideQuiet: true);     // Clear (*) with 3 spaces
        ConsoleHelpers.Write("\b\b\b", overrideQuiet: true);  // Move back to where (*) was
    }

    private void WaitForUserInput()
    {
        ConsoleHelpers.ReadKey(true);
    }
}