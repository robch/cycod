import { Command } from './Command';
import { ForEachVariable } from './ForEachVariable';

export abstract class CommandWithVariables extends Command {
    variables: Map<string, string> = new Map();
    forEachVariables: ForEachVariable[] = [];

    abstract clone(): CommandWithVariables;
}