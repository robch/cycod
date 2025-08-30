"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ConfigValue = void 0;
const ConfigSource_1 = require("./ConfigSource");
class ConfigValue {
    constructor(value, source, isSecret) {
        this._value = value ?? null;
        this.source = source ?? ConfigSource_1.ConfigSource.Default;
        this.isSecret = isSecret ?? false;
    }
    get value() {
        return this._value;
    }
    asString() {
        return this._value?.toString();
    }
    asObfuscated() {
        if (!this.isSecret || this._value == null) {
            return this.asString();
        }
        const valueStr = this._value.toString() || '';
        if (valueStr.length <= 4) {
            return "****";
        }
        // Show first 2 and last 2 characters, obfuscate the rest
        const obfuscatedLength = Math.min(60, valueStr.length - 4);
        return valueStr.substring(0, 2) + '*'.repeat(obfuscatedLength) + valueStr.substring(valueStr.length - 2);
    }
    asInt(defaultValue = 0) {
        if (typeof this._value === 'number') {
            return this._value;
        }
        if (typeof this._value === 'string') {
            const parsed = parseInt(this._value, 10);
            if (!isNaN(parsed)) {
                return parsed;
            }
        }
        return defaultValue;
    }
    asBool(defaultValue = false) {
        if (typeof this._value === 'boolean') {
            return this._value;
        }
        if (typeof this._value === 'string') {
            const lower = this._value.toLowerCase();
            return lower === 'true' || this._value === '1';
        }
        if (typeof this._value === 'number') {
            return this._value !== 0;
        }
        return defaultValue;
    }
    asList() {
        if (this._value == null) {
            return [];
        }
        if (Array.isArray(this._value)) {
            return this._value.map(item => item?.toString() || '');
        }
        if (typeof this._value === 'string') {
            if (this._value === '[]') {
                return [];
            }
            return [this._value];
        }
        // Handle other iterable types
        if (typeof this._value[Symbol.iterator] === 'function' && typeof this._value !== 'string') {
            const result = [];
            for (const item of this._value) {
                result.push(item?.toString() || '');
            }
            return result;
        }
        return [];
    }
    isNotFound() {
        return this.source === ConfigSource_1.ConfigSource.NotFound;
    }
    isNotFoundNullOrEmpty() {
        if (this.source === ConfigSource_1.ConfigSource.NotFound) {
            return true;
        }
        if (this._value == null) {
            return true;
        }
        if (typeof this._value === 'string') {
            return this._value.length === 0;
        }
        if (Array.isArray(this._value)) {
            return this._value.length === 0;
        }
        return false;
    }
    addToList(value) {
        const list = this.asList();
        if (!list.includes(value)) {
            list.push(value);
            this._value = list;
            return true;
        }
        return false;
    }
    removeFromList(value) {
        const list = this.asList();
        const index = list.indexOf(value);
        if (index !== -1) {
            list.splice(index, 1);
            this._value = list;
            return true;
        }
        return false;
    }
    clear() {
        this._value = null;
    }
    set(value) {
        this._value = value;
    }
    toString() {
        if (this._value == null) {
            return '';
        }
        if (Array.isArray(this._value)) {
            return this._value.map(item => item?.toString() || '').join(', ');
        }
        return this._value.toString() || '';
    }
}
exports.ConfigValue = ConfigValue;
//# sourceMappingURL=ConfigValue.js.map