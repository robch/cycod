import { ConsoleColor, Colors } from './Colors';
import { OS } from './OS';

/**
 * Result of parsing color style text.
 */
export interface ColorStyleParseResult {
    parsedStyle: Colors | null;
    parsedText: string | null;
}

/**
 * Helper class for managing console colors and color schemes.
 */
export class ColorHelpers {
    private static colors: ConsoleColor[] = ColorHelpers.InitBestColors();
    private static colorMap: Map<ConsoleColor, ConsoleColor> = ColorHelpers.InitColorMap();

    /**
     * Resets console colors to default (placeholder for Node.js - colors are handled by ConsoleHelpers).
     */
    public static ResetColor(): void {
        // In Node.js, this would typically be handled by the terminal escape sequences
        // The actual color reset is handled by ConsoleHelpers
    }

    /**
     * Maps a console color to the appropriate color for the current terminal.
     * @param color The color to map
     * @returns The mapped color
     */
    public static MapColor(color: ConsoleColor): ConsoleColor {
        return ColorHelpers.colorMap.get(color) ?? color;
    }

    /**
     * Sets highlight colors (placeholder - actual implementation in ConsoleHelpers).
     */
    public static SetHighlightColors(): void {
        // Background: colors[0], Foreground: colors[1]
        // Actual implementation would be in ConsoleHelpers
    }

    /**
     * Gets the highlight colors configuration.
     * @returns Colors object with highlight foreground and background
     */
    public static GetHighlightColors(): Colors {
        return new Colors(ColorHelpers.colors[1], ColorHelpers.colors[0]);
    }

    /**
     * Gets highlight colors based on specific foreground and background colors.
     * @param fg Foreground color
     * @param bg Background color
     * @returns Colors object with highlight colors
     */
    public static GetHighlightColorsFor(fg: ConsoleColor, bg: ConsoleColor): Colors {
        const colors = ColorHelpers.GetBestColors(fg, bg);
        return new Colors(colors[1], colors[0]);
    }

    /**
     * Sets error colors (placeholder - actual implementation in ConsoleHelpers).
     */
    public static SetErrorColors(): void {
        // Background: colors[2], Foreground: colors[3]
        // Actual implementation would be in ConsoleHelpers
    }

    /**
     * Gets the error colors configuration.
     * @returns Colors object with error foreground and background
     */
    public static GetErrorColors(): Colors {
        return new Colors(ColorHelpers.colors[3], ColorHelpers.colors[2]);
    }

    /**
     * Gets error colors based on specific foreground and background colors.
     * @param fg Foreground color
     * @param bg Background color
     * @returns Colors object with error colors
     */
    public static GetErrorColorsFor(fg: ConsoleColor, bg: ConsoleColor): Colors {
        const colors = ColorHelpers.GetBestColors(fg, bg);
        return new Colors(colors[3], colors[2]);
    }

    /**
     * Parses color style text and extracts color information and text.
     * @param text The text to parse for color styling
     * @returns Object with parsed style and text, or null values if parsing failed
     */
    public static TryParseColorStyleText(text: string): ColorStyleParseResult {
        if (text.length > 2 && text[0] === '#') {
            if (text.startsWith('#example;')) {
                return {
                    parsedStyle: new Colors(ConsoleColor.Yellow, ConsoleColor.Black),
                    parsedText: text.substring(9)
                };
            } else if (text.startsWith('#error;')) {
                return {
                    parsedStyle: ColorHelpers.GetErrorColors(),
                    parsedText: text.substring(7)
                };
            } else if (text.length > 4 && text[3] === ';') {
                // Default colors for when '_' is used (simulating Console.ForegroundColor/BackgroundColor)
                const defaultFg = ConsoleColor.White;
                const defaultBg = ConsoleColor.Black;

                const fg = text[1] === '_' ? defaultFg : parseInt(text.substring(1, 2), 16) as ConsoleColor;
                const bg = text[2] === '_' ? defaultBg : parseInt(text.substring(2, 3), 16) as ConsoleColor;
                
                return {
                    parsedStyle: new Colors(fg, bg),
                    parsedText: text.substring(4)
                };
            }
        }

        return {
            parsedStyle: null,
            parsedText: null
        };
    }

    /**
     * Gets the best color scheme for current console settings.
     * @returns Array of colors: [highlightBg, highlightFg, errorBg, errorFg]
     */
    private static GetBestColors(): ConsoleColor[] {
        // Default colors (simulating Console.ForegroundColor/BackgroundColor)
        const bg = ConsoleColor.Black;
        const fg = ConsoleColor.White;
        return ColorHelpers.GetBestColors(fg, bg);
    }

    /**
     * Gets the best color scheme for specific foreground and background colors.
     * @param fg Foreground color
     * @param bg Background color
     * @returns Array of colors: [highlightBg, highlightFg, errorBg, errorFg]
     */
    private static GetBestColors(fg: ConsoleColor, bg: ConsoleColor): ConsoleColor[] {
        const fgIsWhite = fg === ConsoleColor.White || fg === ConsoleColor.Gray;
        const fgIsBlack = fg === ConsoleColor.Black;
        let colors: ConsoleColor[] = [ConsoleColor.White, ConsoleColor.Black, ConsoleColor.DarkRed, ConsoleColor.White];

        switch (bg) {
            case ConsoleColor.Black:
                colors[0] = ConsoleColor.DarkGray;
                colors[1] = ConsoleColor.White;
                break;

            case ConsoleColor.DarkBlue:
                colors[0] = ConsoleColor.Gray;
                colors[1] = ConsoleColor.Black;
                break;

            case ConsoleColor.DarkGreen:
                colors[0] = ConsoleColor.Gray;
                colors[1] = ConsoleColor.Black;
                colors[2] = ConsoleColor.Gray;
                colors[3] = ConsoleColor.DarkRed;
                break;

            case ConsoleColor.DarkCyan:
                colors[0] = ConsoleColor.Yellow;
                colors[1] = ConsoleColor.Black;
                colors[2] = ConsoleColor.White;
                colors[3] = ConsoleColor.DarkRed;
                break;

            case ConsoleColor.DarkRed:
                colors[0] = ConsoleColor.Yellow;
                colors[1] = ConsoleColor.Black;
                colors[2] = ConsoleColor.White;
                colors[3] = ConsoleColor.DarkRed;
                break;

            case ConsoleColor.DarkMagenta:
                colors[0] = ConsoleColor.Yellow;
                colors[1] = ConsoleColor.Black;
                colors[2] = ConsoleColor.White;
                colors[3] = ConsoleColor.DarkRed;
                break;

            case ConsoleColor.DarkYellow:
                colors[0] = ConsoleColor.Gray;
                colors[1] = ConsoleColor.Black;
                colors[2] = ConsoleColor.Gray;
                colors[3] = ConsoleColor.DarkRed;
                break;

            case ConsoleColor.Gray:
                colors[0] = ConsoleColor.White;
                colors[1] = ConsoleColor.Black;
                colors[2] = ConsoleColor.White;
                colors[3] = ConsoleColor.DarkRed;
                break;

            case ConsoleColor.DarkGray:
                colors[0] = ConsoleColor.White;
                colors[1] = ConsoleColor.Black;
                colors[2] = ConsoleColor.Gray;
                colors[3] = ConsoleColor.DarkRed;
                break;

            case ConsoleColor.Blue:
                colors[0] = fgIsBlack ? ConsoleColor.Gray : ConsoleColor.White;
                colors[1] = ConsoleColor.Black;
                break;

            case ConsoleColor.Green:
                colors[0] = ConsoleColor.Gray;
                colors[1] = ConsoleColor.Black;
                colors[2] = ConsoleColor.White;
                colors[3] = ConsoleColor.DarkRed;
                break;

            case ConsoleColor.Cyan:
                colors[0] = ConsoleColor.White;
                colors[1] = ConsoleColor.Black;
                break;

            case ConsoleColor.Red:
                colors[0] = ConsoleColor.White;
                colors[1] = ConsoleColor.Black;
                colors[2] = ConsoleColor.White;
                colors[3] = ConsoleColor.DarkRed;
                break;

            case ConsoleColor.Magenta:
                colors[0] = ConsoleColor.Yellow;
                colors[1] = ConsoleColor.Black;
                colors[2] = ConsoleColor.Gray;
                colors[3] = ConsoleColor.DarkRed;
                break;

            case ConsoleColor.Yellow:
                colors[0] = ConsoleColor.Gray;
                colors[1] = ConsoleColor.Black;
                colors[2] = ConsoleColor.DarkRed;
                colors[3] = ConsoleColor.White;
                break;

            case ConsoleColor.White:
                colors[0] = ConsoleColor.DarkGray;
                colors[1] = ConsoleColor.White;
                colors[2] = ConsoleColor.DarkRed;
                colors[3] = ConsoleColor.White;
                break;
        }

        return colors;
    }

    /**
     * Shows a color chart displaying all available console colors.
     */
    public static ShowColorChart(): void {
        console.log('\nConsole Color Chart:');
        console.log('Background colors with all foreground colors:');
        
        const colorNames = Object.keys(ConsoleColor).filter(key => isNaN(Number(key)));
        
        for (const bgName of colorNames) {
            const bgc = ConsoleColor[bgName as keyof typeof ConsoleColor];
            console.log(`\n${bgName.padEnd(15)}: ${bgc.toString().padStart(2)}: `);
            
            let line = '';
            for (const fgName of colorNames) {
                const fgc = ConsoleColor[fgName as keyof typeof ConsoleColor];
                line += `  ${fgc.toString().padStart(2)}  `;
            }
            console.log(line);
        }
        
        console.log('\nNote: Actual color rendering depends on terminal capabilities.');
    }

    /**
     * Initializes the best color scheme.
     * @returns Initial color array
     */
    private static InitBestColors(): ConsoleColor[] {
        return ColorHelpers.GetBestColors();
    }

    /**
     * Initializes the color mapping table.
     * @returns Color mapping
     */
    private static InitColorMap(): Map<ConsoleColor, ConsoleColor> {
        const map = new Map<ConsoleColor, ConsoleColor>();

        const xterm = process.env.TERM;
        const is256 = xterm != null && xterm.includes('256');
        const isWindows = OS.IsWindows();

        map.set(ConsoleColor.Black, ConsoleColor.Black);
        map.set(ConsoleColor.DarkBlue, ConsoleColor.DarkBlue);
        map.set(ConsoleColor.DarkGreen, ConsoleColor.DarkGreen);
        map.set(ConsoleColor.DarkCyan, ConsoleColor.DarkCyan);
        map.set(ConsoleColor.DarkRed, ConsoleColor.DarkRed);
        map.set(ConsoleColor.DarkMagenta, ConsoleColor.DarkMagenta);
        map.set(ConsoleColor.DarkYellow, ConsoleColor.DarkYellow);
        map.set(ConsoleColor.Gray, ConsoleColor.Gray);
        map.set(ConsoleColor.DarkGray, is256 || isWindows ? ConsoleColor.DarkGray : ConsoleColor.Gray);
        map.set(ConsoleColor.Blue, ConsoleColor.Blue);
        map.set(ConsoleColor.Green, ConsoleColor.Green);
        map.set(ConsoleColor.Cyan, ConsoleColor.Cyan);
        map.set(ConsoleColor.Red, ConsoleColor.Red);
        map.set(ConsoleColor.Magenta, ConsoleColor.Magenta);
        map.set(ConsoleColor.Yellow, ConsoleColor.Yellow);
        map.set(ConsoleColor.White, ConsoleColor.White);

        return map;
    }
}
