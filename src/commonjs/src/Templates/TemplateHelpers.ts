import { INamedValues } from './INamedValues';

export class TemplateHelpers {
    static processTemplate(template: string, values: INamedValues): string {
        const lines = template.split(/\r?\n/);
        const result: string[] = [];
        let skipNextLines = false;
        let conditionStack: boolean[] = [];

        for (let line of lines) {
            // Handle conditional blocks (simplified)
            if (line.trim().startsWith('{{if ')) {
                const condition = line.trim().slice(5, -2).trim();
                const conditionResult = this.evaluateCondition(condition, values);
                conditionStack.push(conditionResult);
                skipNextLines = !conditionResult;
                continue;
            }

            if (line.trim() === '{{endif}}') {
                conditionStack.pop();
                skipNextLines = conditionStack.some(c => !c);
                continue;
            }

            if (skipNextLines) {
                continue;
            }

            // Process variable substitution
            line = this.interpolateVariables(line, values);
            result.push(line);
        }

        return result.join('\n');
    }

    private static interpolateVariables(line: string, values: INamedValues): string {
        // Replace {variableName} patterns
        return line.replace(/\{(\w+)\}/g, (match, variableName) => {
            const value = values.getValue(variableName);
            return value !== undefined ? value : match;
        });
    }

    private static evaluateCondition(condition: string, values: INamedValues): boolean {
        // Simple condition evaluation
        condition = this.interpolateVariables(condition, values).trim();
        
        switch (condition.toLowerCase()) {
            case '':
            case 'false':
            case '0':
                return false;
            case 'true':
            case '1':
                return true;
            default:
                // For more complex expressions, you'd need a proper expression parser
                return condition.length > 0;
        }
    }

    static replaceValues(template: string, values: INamedValues): string {
        return this.interpolateVariables(template, values);
    }
}