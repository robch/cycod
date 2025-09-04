
/**
 * Exception thrown when GitHub token has expired and needs re-authentication
 */
export class GitHubTokenExpiredException extends Error {
    public constructor(message: string);
    public constructor(message: string, innerException: Error);
    public constructor(message: string, innerException?: Error) {
        super(message);
        this.name = 'GitHubTokenExpiredException';
        if (innerException) {
            this.cause = innerException;
        }
    }
}