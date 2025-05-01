
/// <summary>
/// Exception thrown when GitHub token has expired and needs re-authentication
/// </summary>
public class GitHubTokenExpiredException : InvalidOperationException
{
    public GitHubTokenExpiredException(string message) : base(message) { }
    public GitHubTokenExpiredException(string message, Exception innerException) : base(message, innerException) { }
}