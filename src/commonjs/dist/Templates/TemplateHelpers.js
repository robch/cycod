"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.TemplateHelpers = void 0;
class TemplateHelpers {
    static processTemplate(template, values) {
        const lines = template.split(/\r?\n/);
        const result = [];
        let skipNextLines = false;
        let conditionStack = [];
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
    static interpolateVariables(line, values) {
        // Replace {variableName} patterns
        return line.replace(/\{(\w+)\}/g, (match, variableName) => {
            const value = values.getValue(variableName);
            return value !== undefined ? value : match;
        });
    }
    static evaluateCondition(condition, values) {
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
    static replaceValues(template, values) {
        return this.interpolateVariables(template, values);
    }
}
exports.TemplateHelpers = TemplateHelpers;
//# sourceMappingURL=TemplateHelpers.js.map