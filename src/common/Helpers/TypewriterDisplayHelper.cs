using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Handles typewriter display of assistant responses with character-by-character timing effects.
/// Uses a producer-consumer pattern for immediate display.
/// </summary>
public class TypewriterDisplayHelper
{
    private bool _isEnabled = false;
    private int _typewriterDelayMs = 30;
    
    // Producer-Consumer components
    private readonly ConcurrentQueue<string> _textQueue = new ConcurrentQueue<string>();
    private Task? _consumerTask;
    private Task? _keyboardTask;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRunning = false;
    
    // Skip ahead functionality
    private volatile bool _skipToNextBreak = false;
    private volatile bool _isCurrentlyDisplaying = false;
    
    public TypewriterDisplayHelper(bool enabled = false, int typewriterDelayMs = 30)
    {
        _isEnabled = enabled;
        _typewriterDelayMs = typewriterDelayMs;
    }

    public bool IsEnabled => _isEnabled;

    /// <summary>
    /// Enqueues text for immediate display (Producer)
    /// </summary>
    public void EnqueueText(string text)
    {
        if (!_isEnabled || string.IsNullOrEmpty(text)) return;

        _textQueue.Enqueue(text);
    }

    /// <summary>
    /// Starts the background consumer that displays queued text and keyboard monitoring
    /// </summary>
    public void StartDisplaying()
    {
        if (!_isEnabled || _isRunning) return;

        _isRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();
        _consumerTask = Task.Run(() => ConsumerLoop(_cancellationTokenSource.Token));
        _keyboardTask = Task.Run(() => KeyboardMonitorLoop(_cancellationTokenSource.Token));
    }

    /// <summary>
    /// Stops the background consumer and keyboard monitor, waits for them to finish
    /// </summary>
    public async Task StopDisplayingAsync()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _cancellationTokenSource?.Cancel();
        
        // Wait for both tasks to complete
        var tasks = new List<Task>();
        if (_consumerTask != null) tasks.Add(_consumerTask);
        if (_keyboardTask != null) tasks.Add(_keyboardTask);
        
        if (tasks.Count > 0)
        {
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling
            }
            catch (Exception ex)
            {
                ConsoleHelpers.LogException(ex, "Error stopping typewriter tasks", showToUser: false);
            }
        }
        
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _consumerTask = null;
        _keyboardTask = null;
        _skipToNextBreak = false;
        _isCurrentlyDisplaying = false;
        
        // Clear any remaining keypresses from the input buffer to prevent bleeding
        ClearInputBuffer();
    }

    /// <summary>
    /// Enables or disables the typewriter effect
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        _isEnabled = enabled;
    }

    private async Task KeyboardMonitorLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Check for keyboard input (non-blocking)
                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);
                    if (keyInfo.KeyChar == 'A' || keyInfo.KeyChar == 'a')
                    {
                        // Only set skip flag if we can actually skip something
                        if (CanSkipAhead())
                        {
                            _skipToNextBreak = true;
                        }
                        // If we can't skip, just ignore the A press (consume it silently)
                    }
                }
                
                // Wait a short time before checking again
                await Task.Delay(50, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
        catch (Exception ex)
        {
            ConsoleHelpers.LogException(ex, "Error in keyboard monitor loop", showToUser: false);
        }
    }

    private bool CanSkipAhead()
    {
        // Can only skip if we're currently displaying text OR there's content in queue
        return _isCurrentlyDisplaying || !_textQueue.IsEmpty;
    }

    private async Task ConsumerLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_textQueue.TryDequeue(out string? text))
                {
                    // Display this text chunk with typewriter effect
                    await DisplayWithTypewriterEffect(text);
                }
                else
                {
                    // No text in queue, wait a short time before checking again
                    await Task.Delay(10, cancellationToken);
                }
            }
            
            // After cancellation, process any remaining items in queue
            while (_textQueue.TryDequeue(out string? remainingText))
            {
                await DisplayWithTypewriterEffect(remainingText);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
        catch (Exception ex)
        {
            ConsoleHelpers.LogException(ex, "Error in typewriter consumer loop", showToUser: false);
        }
    }

    private async Task DisplayWithTypewriterEffect(string text)
    {
        _isCurrentlyDisplaying = true;
        
        try
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                ConsoleHelpers.Write(c.ToString(), ConsoleColor.White, overrideQuiet: true);
                
                // Check if we should skip to the next breaking character
                if (_skipToNextBreak)
                {
                    // Skip ahead until we find a breaking character
                    if (IsBreakingCharacter(c))
                    {
                        _skipToNextBreak = false; // Reset skip flag when we reach a break
                    }
                    continue; // Skip the delay and continue to next character
                }
                
                // Calculate delay based on character type
                var delay = CalculateCharacterDelay(text, i);
                
                if (delay > 0)
                {
                    await Task.Delay(delay);
                }
            }
        }
        finally
        {
            _isCurrentlyDisplaying = false;
        }
    }

    private bool IsBreakingCharacter(char c)
    {
        // Breaking characters are sentence endings and major punctuation
        return c == '.' || c == '!' || c == '?' || c == ':' || c == ';' || c == ',' || c == '\n';
    }

    private void ClearInputBuffer()
    {
        try
        {
            // Clear any pending keypresses to prevent them from bleeding into chat input
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
        }
        catch (Exception)
        {
            // Ignore any errors clearing the buffer
        }
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
        if (c == ':' || c == ';' || c == ',')
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
}