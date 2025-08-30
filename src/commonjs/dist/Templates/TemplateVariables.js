"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.TemplateVariables = void 0;
class TemplateVariables {
    constructor(variables) {
        this._variables = new Map();
        if (variables) {
            for (const [key, value] of Object.entries(variables)) {
                this._variables.set(key, value);
            }
        }
    }
    getValue(name) {
        return this._variables.get(name);
    }
    hasValue(name) {
        return this._variables.has(name);
    }
    getKeys() {
        return Array.from(this._variables.keys());
    }
    setValue(name, value) {
        this._variables.set(name, value);
    }
    removeValue(name) {
        return this._variables.delete(name);
    }
    clear() {
        this._variables.clear();
    }
    static fromRecord(variables) {
        return new TemplateVariables(variables);
    }
}
exports.TemplateVariables = TemplateVariables;
//# sourceMappingURL=TemplateVariables.js.map