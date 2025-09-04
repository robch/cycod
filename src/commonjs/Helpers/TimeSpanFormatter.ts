export class TimeSpanFormatter {
    public static FormatMsOrSeconds(milliseconds: number): string {
        const secs = milliseconds / 1000;
        const duration = milliseconds >= 1000
            ? secs.toFixed(2) + " seconds"
            : Math.round(milliseconds).toString() + " ms";
        return duration;
    }
}
