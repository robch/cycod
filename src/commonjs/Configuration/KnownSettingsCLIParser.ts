import { KnownSettings } from './KnownSettings';

/**
 * Parser for command line options related to known settings.
 */
export class KnownSettingsCLIParser {
    /**
     * Attempts to parse a command line argument as a known setting.
     * @param arg The command line argument to parse.
     * @returns An object with settingName, value, and success flag, or null if parsing failed.
     */
    public static TryParseCLIOption(arg: string): { settingName: string; value: string | null; success: boolean } | null {
        // If it doesn't start with --, it's not a CLI option
        if (!arg.startsWith('--')) {
            return null;
        }
        
        // Check for option with attached value (--option=value)
        const equalsPos = arg.indexOf('=');
        let optionName: string;
        let value: string | null;
        
        if (equalsPos >= 0) {
            optionName = arg.substring(0, equalsPos);
            value = arg.substring(equalsPos + 1);
        } else {
            optionName = arg;
            value = null;
        }
        
        const settingName = KnownSettings.ToDotNotation(optionName);
        const isKnown = KnownSettings.IsKnown(settingName);
        
        if (isKnown) {
            return { settingName, value, success: true };
        }
        
        return null;
    }
}