import { INamedValues } from './INamedValues';
import { ExpressionCalculator } from './ExpressionCalculator';
import { TryCatchHelpers } from '../Helpers/TryCatchHelpers';
import { ConsoleHelpers } from '../Helpers/ConsoleHelpers';
import { ValueHelpers } from './ValueHelpers';

// Import the extension methods
import './ValueHelpers';

/**
 * Template processing utilities for conditional logic and variable substitution
 */
export class TemplateHelpers {
    /**
     * Processes a template with the provided values, supporting conditional logic and variable assignment
     * @param template The template string to process
     * @param values The named values for substitution
     * @returns The processed template string
     */
    public static processTemplate(template: string, values: INamedValues): string {
        const calculator = new ExpressionCalculator();

        const interpolate = (line: string): string => line.replaceValues(values);
        const evaluateCondition = (condition: string): boolean => {
            condition = condition.replaceValues(values).trim();
            const evaluated = (() => {
                switch (condition) {
                    case '':
                        return false;
                    case 'true':
                        return true;
                    case 'false':
                        return false;
                    default:
                        return calculator.evaluate(condition) as boolean;
                }
            })();

            ConsoleHelpers.writeDebugLine(`Evaluating condition: ${condition} => ${evaluated}`);
            return evaluated;
        };
        const setVariable = (s: string): void => {
            const result = calculator.evaluate(s);
            const name = s.split('=')[0].trim();
            values.Set(name, result.toString());
        };

        return TemplateHelpers.processTemplateWithCallbacks(template, evaluateCondition, interpolate, setVariable);
    }

    /**
     * Processes a template with custom callback functions
     * @param template The template string to process
     * @param evaluateCondition Function to evaluate boolean conditions
     * @param interpolate Function to interpolate values in strings
     * @param setVariable Function to set variable values
     * @returns The processed template string
     */
    public static processTemplateWithCallbacks(
        template: string,
        evaluateCondition: (condition: string) => boolean,
        interpolate: (line: string) => string,
        setVariable: (assignment: string) => void
    ): string {
        const safeEvaluateCondition = TryCatchHelpers.NoThrowWrapExceptionT(evaluateCondition, false);

        const lines = template.split('\n');
        const output: string[] = [];

        const inTrueBranchNow: boolean[] = [true];
        const skipElseBranches: boolean[] = [true];

        for (const line of lines) {
            const trimmedLine = line.trim();

            if (trimmedLine.startsWith('{{if ') && trimmedLine.endsWith('}}') && !trimmedLine.endsWith('{{endif}}')) {
                const condition = trimmedLine.slice(5, -2).trim();
                const evaluated = safeEvaluateCondition(condition);
                if (evaluated.ex === null) {
                    inTrueBranchNow.push(evaluated.value);
                    skipElseBranches.push(evaluated.value);
                    continue;
                }
            } else if (trimmedLine.startsWith('{{else if ') && trimmedLine.endsWith('}}')) {
                if (inTrueBranchNow[inTrueBranchNow.length - 1]) {
                    inTrueBranchNow.pop();
                    inTrueBranchNow.push(false);
                    // skipElseBranches.peek() should already be true
                    continue;
                } else if (skipElseBranches[skipElseBranches.length - 1]) {
                    continue;
                }

                const condition = trimmedLine.slice(10, -2).trim();
                const evaluated = safeEvaluateCondition(condition);
                if (evaluated.ex === null) {
                    inTrueBranchNow.pop();
                    inTrueBranchNow.push(evaluated.value);
                    skipElseBranches.pop();
                    skipElseBranches.push(evaluated.value);
                    continue;
                }
            } else if (trimmedLine.startsWith('{{else}}')) {
                if (inTrueBranchNow[inTrueBranchNow.length - 1]) {
                    inTrueBranchNow.pop();
                    inTrueBranchNow.push(false);
                    // skipElseBranches.peek() should already be true
                    continue;
                } else if (skipElseBranches[skipElseBranches.length - 1]) {
                    continue;
                }

                inTrueBranchNow.pop();
                inTrueBranchNow.push(true);
                skipElseBranches.pop();
                skipElseBranches.push(true);
                continue;
            } else if (trimmedLine.startsWith('{{endif}}')) {
                inTrueBranchNow.pop();
                skipElseBranches.pop();
                continue;
            } else if (trimmedLine.startsWith('{{set ') && trimmedLine.endsWith('}}')) {
                if (inTrueBranchNow.every(b => b)) {
                    const assignment = trimmedLine.slice(6, -2).trim();
                    setVariable(assignment);
                }
                continue;
            }

            if (inTrueBranchNow.every(b => b)) {
                let updated = line.replace(/\r?\n$/, '');

                const firstLine = output.length === 0;
                if (!firstLine) output.push('');

                const inlineIfPos = updated.indexOf('{{if ');
                const inlineEndIfPos = updated.indexOf('{{endif}}');
                const inlineIfEndIf = inlineIfPos >= 0 && inlineEndIfPos >= 0 && inlineIfPos < inlineEndIfPos;
                if (inlineIfEndIf) {
                    updated = TemplateHelpers.handleInlineIf(updated, evaluateCondition, interpolate);
                }

                output[output.length - 1] = (output[output.length - 1] || '') + interpolate(updated);
            }
        }

        return output.join('\n');
    }

    private static handleInlineIf(
        line: string,
        evaluateCondition: (condition: string) => boolean,
        interpolate: (line: string) => string
    ): string {
        const safeEvaluateCondition = TryCatchHelpers.NoThrowWrapExceptionT(evaluateCondition, false);

        const output: string[] = [];

        const inTrueBranchNow: boolean[] = [true];
        const skipElseBranches: boolean[] = [true];

        const chars = [...line];
        let position = 0;
        while (position < chars.length) {
            const cch = TemplateHelpers.countCharsToCheck(chars, position);
            if (cch === 1) {
                if (inTrueBranchNow.every(b => b)) {
                    output.push(chars[position]);
                }
                position++;
                continue;
            }

            const check = chars.slice(position, position + cch).join('');
            position += cch;

            if (check.startsWith('{{if ') && check.endsWith('}}')) {
                const condition = check.slice(5, -2).trim();
                const evaluated = safeEvaluateCondition(condition);
                if (evaluated.ex === null) {
                    inTrueBranchNow.push(evaluated.value);
                    skipElseBranches.push(evaluated.value);
                    continue;
                }
            } else if (check.startsWith('{{else if ') && check.endsWith('}}')) {
                if (inTrueBranchNow[inTrueBranchNow.length - 1]) {
                    inTrueBranchNow.pop();
                    inTrueBranchNow.push(false);
                    // skipElseBranches.peek() should already be true
                    continue;
                } else if (skipElseBranches[skipElseBranches.length - 1]) {
                    continue;
                }

                const condition = check.slice(10, -2).trim();
                const evaluated = safeEvaluateCondition(condition);
                if (evaluated.ex === null) {
                    inTrueBranchNow.pop();
                    inTrueBranchNow.push(evaluated.value);
                    skipElseBranches.pop();
                    skipElseBranches.push(evaluated.value);
                    continue;
                }
            } else if (check.startsWith('{{else}}') && check.endsWith('}}')) {
                if (inTrueBranchNow[inTrueBranchNow.length - 1]) {
                    inTrueBranchNow.pop();
                    inTrueBranchNow.push(false);
                    // skipElseBranches.peek() should already be true
                    continue;
                } else if (skipElseBranches[skipElseBranches.length - 1]) {
                    continue;
                }

                inTrueBranchNow.pop();
                inTrueBranchNow.push(true);
                skipElseBranches.pop();
                skipElseBranches.push(true);
                continue;
            } else if (check.startsWith('{{endif}}') && check.endsWith('}}')) {
                inTrueBranchNow.pop();
                skipElseBranches.pop();
                continue;
            } else if (inTrueBranchNow.every(b => b)) {
                output.push(interpolate(check));
            }
        }

        return output.join('');
    }

    private static countCharsToCheck(chars: string[], position: number): number {
        if (chars[position] !== '{') return 1;

        let cch = 1;
        let braces = 1;
        for (let i = position + 1; i < chars.length; i++) {
            if (chars[i] === '{') {
                braces++;
            } else if (chars[i] === '}') {
                braces--;
                if (braces === 0) {
                    cch = i - position + 1;
                    break;
                }
            }
        }

        return cch;
    }
}