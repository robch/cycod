import { FileHelpers } from './FileHelpers';
import { ConsoleHelpers } from './ConsoleHelpers';

/**
 * Custom exception class for calculation errors.
 */
export class CalcException extends Error {
    public Position: number;
    public Expression: string;

    constructor(message: string, position: number, expression: string, cause?: Error) {
        super(message);
        this.name = 'CalcException';
        this.Position = position;
        this.Expression = expression;
        if (cause) {
            this.cause = cause;
        }
    }
}

/**
 * Helper class for handling and displaying exceptions.
 */
export class ExceptionHelpers {
    /**
     * Saves an exception to a log file and displays it to the console.
     * @param ex The error to save and display
     */
    public static SaveAndDisplayException(ex: Error): void {
        let innerExceptionNumber = 0;
        let currentError: Error | undefined = ex;
        const outterMostExceptionFileName = FileHelpers.GetFileNameFromTemplate("exception.log", "{filebase}-{fileext}-{time}.{fileext}") ?? "exception.log";

        while (currentError) {
            const exceptionType = currentError.constructor.name || 'Error';
            const stackTrace = currentError.stack?.replace(/\n/g, "\n  ") || 'No stack trace available';

            const message1 = `EXCEPTION: ${currentError.message}`;
            const message2 = `  ${exceptionType}\n\n  ${stackTrace}`;

            const fileName = innerExceptionNumber > 0
                ? FileHelpers.GetFileNameFromTemplate(outterMostExceptionFileName, `{filebase}${innerExceptionNumber}.{fileext}`) ?? `exception${innerExceptionNumber}.log`
                : outterMostExceptionFileName;

            // Try to save to exceptions folder, fallback to current directory
            const saveToFolderOnAccessDenied = './exceptions'; // Simplified for Node.js
            const savedFileName = FileHelpers.WriteAllText(fileName, message1 + "\n\n" + message2, saveToFolderOnAccessDenied) ?? fileName;

            ConsoleHelpers.WriteLine('', true);
            ConsoleHelpers.WriteWarning(`SAVED: ${savedFileName}`);

            ConsoleHelpers.Write("\n\n", true);
            ConsoleHelpers.WriteError(message1);
            ConsoleHelpers.WriteLine(`\n\n${message2}`, 'cyan', true);

            // Check for nested errors (similar to InnerException)
            currentError = (currentError as any).cause || (currentError as any).innerException;
            if (!currentError) break;

            innerExceptionNumber++;
        }
    }

    /**
     * Saves and displays a CalcException with special formatting.
     * @param ex The CalcException to save and display
     */
    public static SaveAndDisplayCalcException(ex: CalcException): void {
        ConsoleHelpers.WriteWarning(`${ex.message} (${ex.Position})`);
        ConsoleHelpers.WriteLine('', true);
        ConsoleHelpers.WriteWarning(ex.Expression);
        
        if (ex.Position < 50) {
            ConsoleHelpers.WriteLine('', true);
            ConsoleHelpers.WriteWarning(' '.repeat(ex.Position) + '^');
        }
        
        ConsoleHelpers.WriteLine('', true);
        ExceptionHelpers.SaveAndDisplayException(ex);
    }
}
