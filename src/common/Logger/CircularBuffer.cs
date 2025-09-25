using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class CircularBuffer<T>
{
    private readonly T[] _buffer;
    private readonly int _capacity;
    private long _writePosition = 0;
    private long _totalWritten = 0;
    
    public CircularBuffer(int capacity)
    {
        _capacity = capacity;
        _buffer = new T[capacity];
    }
    
    public void Write(T item)
    {
        var position = Interlocked.Increment(ref _writePosition) - 1;
        var index = (int)(position % _capacity);
        
        _buffer[index] = item;
        Interlocked.Increment(ref _totalWritten);
    }
    
    public IEnumerable<T> ReadAll()
    {
        var totalWritten = Volatile.Read(ref _totalWritten);
        var startIndex = totalWritten > _capacity ? (int)((totalWritten - _capacity) % _capacity) : 0;
        var count = (int)Math.Min(totalWritten, _capacity);
        
        for (int i = 0; i < count; i++)
        {
            var index = (startIndex + i) % _capacity;
            yield return _buffer[index];
        }
    }
    
    public void Clear()
    {
        // Reset the buffer state
        Interlocked.Exchange(ref _writePosition, 0);
        Interlocked.Exchange(ref _totalWritten, 0);
        
        // Clear the buffer contents
        Array.Clear(_buffer, 0, _capacity);
    }
    
    public long GetOldestLineNumber() => Math.Max(0, _totalWritten - _capacity);
    public long GetNewestLineNumber() => _totalWritten;
}