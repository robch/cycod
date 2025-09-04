/**
 * Represents a foreach variable used in command line expansion.
 * The variable has a name and a list of values that will be used
 * to create multiple CommandWithVariables instances during expansion.
 */
export class ForEachVariable {
    /**
     * The name of the variable, which will be used in templates as {name}
     */
    public readonly Name: string;
    
    /**
     * The list of values this variable can take
     */
    public readonly Values: string[];

    /**
     * Creates a new ForEachVariable with the specified name and values
     * @param name The variable name
     * @param values The list of values
     */
    public constructor(name: string, values: string[]) {
        this.Name = name;
        this.Values = values;
    }
}