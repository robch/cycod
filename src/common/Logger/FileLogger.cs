using System;
using System.IO;
using System.Text;
using System.Threading;

public class FileLogger : ILogger
{
    private static readonly Lazy<FileLogger> _instance = new(() => new FileLogger());
    public static FileLogger Instance => _instance.Value;
    
    private readonly LogFilter _filter = new();
    private readonly object _fileLock = new();
    
    private string _baseFileName = string.Empty;
    private string _currentFileName = string.Empty;
    private FileStream? _fileStream;
    private StreamWriter? _writer;
    private int _currentFileAppendix = 0;
    private DateTime _lastFileStartTime = DateTime.MinValue;
    private long _fileDataWritten = 0;
    
    // Configuration
    private uint _fileDurationSeconds = 0;
    private uint _fileSizeMB = 0;
    private bool _append = false;
    private bool _shouldFlush = true;
    
    public string Name => "File";
    public LogLevel Level { get; set; } = LogLevel.All;
    public bool IsLoggingEnabled => _fileStream != null;
    
    private FileLogger() { }
    
    public void SetFileOptions(string fileName, uint durationSeconds = 0, uint sizeMB = 0, bool append = false, bool autoFlush = true)
    {
        lock (_fileLock)
        {
            _fileDurationSeconds = durationSeconds;
            _fileSizeMB = sizeMB;
            _append = append;
            _shouldFlush = autoFlush;
            
            if (!string.Equals(_baseFileName, fileName, StringComparison.OrdinalIgnoreCase))
            {
                _currentFileAppendix = 0;
                _baseFileName = fileName;
                AssignFile();
            }
        }
    }
    
    private void AssignFile()
    {
        CloseCurrentFile();
        
        if (string.IsNullOrEmpty(_baseFileName))
            return;
            
        _currentFileName = BuildFileName(_baseFileName);
        
        try
        {
            var directory = Path.GetDirectoryName(_currentFileName);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            _fileStream = new FileStream(_currentFileName, _append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(_fileStream, Encoding.UTF8);
            _lastFileStartTime = DateTime.UtcNow;
            _fileDataWritten = 0;
        }
        catch (Exception ex)
        {
            // Log the error to console if possible, but don't crash the application
            try
            {
                Console.WriteLine($"WARNING: FileLogger failed to open '{_currentFileName}': {ex.Message}");
            }
            catch { /* Ignore console errors */ }
            
            // Disable this logger by setting stream to null
            _fileStream = null;
            _writer = null;
            return;
        }
    }
    
    private string BuildFileName(string fileName)
    {
        if (_currentFileAppendix == 0)
            return fileName;
            
        var directory = Path.GetDirectoryName(fileName);
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        
        var newName = $"{nameWithoutExt}-{_currentFileAppendix}{extension}";
        return string.IsNullOrEmpty(directory) ? newName : Path.Combine(directory, newName);
    }
    
    private void CheckRotation()
    {
        bool needsRotation = false;
        
        if (_fileDurationSeconds > 0)
        {
            var elapsed = DateTime.UtcNow - _lastFileStartTime;
            if (elapsed.TotalSeconds >= _fileDurationSeconds)
            {
                needsRotation = true;
            }
        }
        
        if (_fileSizeMB > 0 && _fileDataWritten > _fileSizeMB * 1024 * 1024)
        {
            needsRotation = true;
        }
        
        if (needsRotation)
        {
            _currentFileAppendix++;
            AssignFile();
        }
    }
    
    public bool IsLevelEnabled(LogLevel level) => (Level & level) != 0;
    
    public void SetFilter(string filterPattern) => _filter.SetFilter(filterPattern);
    
    public void LogMessage(LogLevel level, string title, string fileName, int lineNumber, string message)
    {
        if (!IsLevelEnabled(level) || !_filter.ShouldLog(message))
            return;
            
        lock (_fileLock)
        {
            if (_writer == null) return;
            
            CheckRotation();
            
            var formatted = LogFormatter.FormatMessage(level, title, fileName, lineNumber, message);
            
            _writer.Write(formatted);
            if (_shouldFlush)
                _writer.Flush();
                
            Interlocked.Add(ref _fileDataWritten, formatted.Length);
        }
    }
    
    private void CloseCurrentFile()
    {
        _writer?.Dispose();
        _fileStream?.Dispose();
        _writer = null;
        _fileStream = null;
    }
    
    public void Close()
    {
        lock (_fileLock)
        {
            CloseCurrentFile();
            _baseFileName = string.Empty;
            _currentFileName = string.Empty;
        }
    }
}