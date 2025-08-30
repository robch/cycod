"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.StringHelpers = void 0;
class StringHelpers {
    static isNullOrEmpty(value) {
        return value == null || value.length === 0;
    }
    static isNullOrWhitespace(value) {
        return value == null || value.trim().length === 0;
    }
    static trimStart(value, chars) {
        if (!chars || chars.length === 0) {
            return value.trimStart();
        }
        let startIndex = 0;
        while (startIndex < value.length && chars.includes(value[startIndex])) {
            startIndex++;
        }
        return value.substring(startIndex);
    }
    static trimEnd(value, chars) {
        if (!chars || chars.length === 0) {
            return value.trimEnd();
        }
        let endIndex = value.length - 1;
        while (endIndex >= 0 && chars.includes(value[endIndex])) {
            endIndex--;
        }
        return value.substring(0, endIndex + 1);
    }
    static trim(value, chars) {
        return this.trimStart(this.trimEnd(value, chars), chars);
    }
    static split(value, separator, options) {
        const separators = Array.isArray(separator) ? separator : [separator];
        const regex = new RegExp(`[${separators.map(s => this.escapeRegex(s)).join('')}]`);
        const parts = value.split(regex);
        if (options?.removeEmptyEntries) {
            return parts.filter(part => part.length > 0);
        }
        return parts;
    }
    static join(separator, values) {
        return values.join(separator);
    }
    static format(template, ...args) {
        return template.replace(/{(\d+)}/g, (match, index) => {
            const argIndex = parseInt(index, 10);
            return args[argIndex] !== undefined ? String(args[argIndex]) : match;
        });
    }
    static padLeft(value, totalWidth, paddingChar = ' ') {
        if (value.length >= totalWidth) {
            return value;
        }
        const padding = paddingChar.repeat(totalWidth - value.length);
        return padding + value;
    }
    static padRight(value, totalWidth, paddingChar = ' ') {
        if (value.length >= totalWidth) {
            return value;
        }
        const padding = paddingChar.repeat(totalWidth - value.length);
        return value + padding;
    }
    static contains(value, substring, ignoreCase = false) {
        if (ignoreCase) {
            return value.toLowerCase().includes(substring.toLowerCase());
        }
        return value.includes(substring);
    }
    static startsWith(value, prefix, ignoreCase = false) {
        if (ignoreCase) {
            return value.toLowerCase().startsWith(prefix.toLowerCase());
        }
        return value.startsWith(prefix);
    }
    static endsWith(value, suffix, ignoreCase = false) {
        if (ignoreCase) {
            return value.toLowerCase().endsWith(suffix.toLowerCase());
        }
        return value.endsWith(suffix);
    }
    static escapeRegex(str) {
        return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }
}
exports.StringHelpers = StringHelpers;
//# sourceMappingURL=StringHelpers.js.map