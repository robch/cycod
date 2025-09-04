/**
 * Interface for named values storage and retrieval
 */
export interface INamedValues {
    /**
     * Gets a value by name
     * @param name The name/key of the value to retrieve
     * @returns The value as a string, or null if not found
     */
    get(name: string): string | null;

    /**
     * Checks if a value with the given name exists
     * @param name The name/key to check for
     * @returns True if the name exists, false otherwise
     */
    contains(name: string): boolean;
}