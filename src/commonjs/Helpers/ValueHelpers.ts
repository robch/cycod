//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

import { INamedValues } from './INamedValues';
import { AtFileHelpers } from './AtFileHelpers';

/**
 * Provides string interpolation and value replacement functionality
 */
export class ValueHelpers {
    /**
     * Replaces placeholders in text with values from the provided source
     * @param text The text containing placeholders like {name} or ${name}
     * @param values The source of named values
     * @param deleteUnresolved If true, unresolved placeholders are removed; if false, they're left as-is
     * @returns The text with placeholders replaced by values
     */
    public static replaceValues(text: string, values: INamedValues, deleteUnresolved: boolean = false): string {
        if (!text || text.length === 0 || !text.includes('{')) return text;

        let i = 0;
        const sb: string[] = Array.from(text);
        
        while (i < sb.length) {
            if (sb[i] === '{') {
                i += ValueHelpers.interpolate(1, sb, i, values, deleteUnresolved);
            } else if (sb[i] === '$' && i + 1 < sb.length && sb[i + 1] === '{') {
                i += ValueHelpers.interpolate(2, sb, i, values, deleteUnresolved);
            } else {
                i++;
            }
        }

        return sb.join('');
    }

    private static interpolate(cchPrefix: number, sb: string[], start: number, values: INamedValues, deleteUnresolved: boolean, level: number = 0): number {
        if (sb[start + cchPrefix - 1] !== '{') {
            throw new Error(`Interpolate() '{' not found; pos=${start}`);
        }

        let i = cchPrefix;

        while (start + i < sb.length && sb[start + i] !== '}') {
            if (sb[start + i] === '{') {
                i += ValueHelpers.interpolate(1, sb, start + i, values, deleteUnresolved, level + 1);
            } else if (sb[start + i] === '$' && start + i + 1 < sb.length && sb[start + i + 1] === '{') {
                i += ValueHelpers.interpolate(2, sb, start + i, values, deleteUnresolved, level + 1);
            } else {
                i++;
            }
        }

        if (start + i >= sb.length) return cchPrefix; // no closing '}' ... that's ok, we'll just leave it as is
        if (sb[start + i] !== '}') {
            throw new Error(`Interpolate() '}' not found; pos=${start + i}`);
        }
        i += 1; // skipping '}'

        const nameStartsAt = start + cchPrefix;
        let name = sb.slice(nameStartsAt, start + i - 1).join('');

        // check if name is a well known escape sequence, like \n, \t, etc.
        if (name.length === 2 && name[0] === '\\' && name[1] === 't') {
            sb.splice(start, name.length + cchPrefix + 1, '\t');
            return 1;
        } else if (name.length === 2 && name[0] === '\\' && name[1] === 'n') {
            sb.splice(start, name.length + cchPrefix + 1, '\n');
            return 1;
        } else if (name.length === 2 && name[0] === '\\' && name[1] === 'r') {
            sb.splice(start, name.length + cchPrefix + 1, '\r');
            return 1;
        }

        const expandAtFile = name.startsWith('@');
        if (expandAtFile) {
            name = name.substring(1);
        }

        // next, try to get it from the values
        let str = values.get(name);
        if (str === null && !expandAtFile) {
            str = deleteUnresolved
                ? ''
                : cchPrefix === 1
                    ? `{${name}}`
                    : `\${${name}}`;
            sb.splice(start, name.length + cchPrefix + 1, ...Array.from(str));
            return str.length;
        }

        if (str === null && expandAtFile) {
            str = name;
        }

        str = expandAtFile ? AtFileHelpers.expandAtFileValue(`@${str}`, values) : str!;
        sb.splice(start, name.length + cchPrefix + 1 + (expandAtFile ? 1 : 0), ...Array.from(str));
        return 0;
    }
}