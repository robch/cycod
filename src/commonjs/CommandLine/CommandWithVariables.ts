import { Command } from './Command';
import { ForEachVariable } from './ForEachVariable';

/**
 * Abstract base class for commands that support template variables.
 * Extends the base Command class with variable substitution capabilities.
 */
export abstract class CommandWithVariables extends Command {
    /**
     * Dictionary of template variables and their values.
     * These variables can be used for template substitution in the command.
     */
    public Variables: Map<string, string> = new Map<string, string>();

    /**
     * List of foreach variables that will be expanded into multiple command instances.
     * Each ForEachVariable contains a name and a list of values to iterate over.
     */
    public ForEachVariables: ForEachVariable[] = [];

    /**
     * Creates a deep clone of this command with all its variables and configuration.
     * This is used during foreach variable expansion to create multiple command instances.
     * @returns A new CommandWithVariables instance that is a copy of this one.
     */
    public abstract Clone(): CommandWithVariables;
}
