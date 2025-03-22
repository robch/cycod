public class ExceptionHelpers
{
    public static void SaveAndDisplayException(Exception ex)
    {
        var innerExceptionNumber = 0;
        var outterMostExceptionFileName = FileHelpers.GetFileNameFromTemplate("exception.log", "{filebase}-{fileext}-{time}.{fileext}")!;

        while (true)
        {
            var exceptionType = ex.GetType().FullName;
            var stackTrace = ex.StackTrace?.Replace("\n", "\n  ");

            var message1 = $"EXCEPTION: {ex.Message}";
            var message2 = $"  {exceptionType}\n\n  {stackTrace}";

            var fileName = innerExceptionNumber > 0
                ? FileHelpers.GetFileNameFromTemplate(outterMostExceptionFileName, "{filebase}" + innerExceptionNumber + ".{fileext}")!
                : outterMostExceptionFileName;
            FileHelpers.WriteAllText(fileName, message1 + "\n\n" + message2);

            ConsoleHelpers.WriteLine(overrideQuiet: true);
            ConsoleHelpers.WriteWarning($"SAVED: {fileName}");

            ConsoleHelpers.Write("\n\n", overrideQuiet: true);
            ConsoleHelpers.WriteError($"{message1}");
            ConsoleHelpers.WriteLine($"\n\n{message2}", ConsoleColor.Cyan, overrideQuiet: true);

            if (ex.InnerException == null) break;

            innerExceptionNumber++;
            ex = ex.InnerException;
        }
    }
}
