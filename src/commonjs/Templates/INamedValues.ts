/**
 * Interface for classes that provide named values for template substitution
 */
export interface INamedValues {
    /**
     * Gets a value by name, or an empty string if not found
     * @param name The name of the value to retrieve
     * @returns The value, or null if not found
     */
    Get(name: string): string | null;

    /**
     * Sets a value by name
     * @param name The name of the value to set
     * @param value The value to set
     */
    Set(name: string, value: string): void;

    /**
     * Checks if a value exists
     * @param name The name of the value to check
     * @returns True if the value exists, false otherwise
     */
    Contains(name: string): boolean;
}