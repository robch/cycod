/**
 * Helper class for working with JSON data in TypeScript/Node.js.
 */
export class JsonHelpers {
    /**
     * Gets a property value from a JSON string.
     * @param json The JSON string to parse
     * @param propertyName The name of the property to retrieve
     * @param valueIfNotFound The value to return if the property is not found
     * @returns The property value as a string, or valueIfNotFound if not found
     */
    public static GetJsonPropertyValue(json: string, propertyName: string, valueIfNotFound: string | null = null): string | null {
        if (!json || json.trim() === '') return valueIfNotFound;

        try {
            const jsonObject = JSON.parse(json);
            
            // Check if the JSON is an object and has the specified property
            if (typeof jsonObject === 'object' && jsonObject !== null && propertyName in jsonObject) {
                const propertyValue = jsonObject[propertyName];
                // Return the string value or default if not a string
                return typeof propertyValue === 'string' ? propertyValue : valueIfNotFound;
            }
        } catch {
            // Silently catch any exceptions (invalid JSON, etc.)
        }

        return valueIfNotFound;
    }

    /**
     * Parses a JSON array string into an array of string dictionaries.
     * @param json The JSON array string to parse
     * @returns An array of string dictionaries
     */
    public static FromJsonArrayText(json: string): Map<string, string>[] {
        const list: Map<string, string>[] = [];
        
        try {
            const jsonArray = JSON.parse(json);
            
            if (Array.isArray(jsonArray)) {
                for (const item of jsonArray) {
                    const properties = new Map<string, string>();
                    
                    if (typeof item === 'object' && item !== null) {
                        for (const [key, value] of Object.entries(item)) {
                            const stringValue = typeof value === 'string' 
                                ? value 
                                : JSON.stringify(value);
                            properties.set(key, stringValue);
                        }
                    }
                    
                    list.push(properties);
                }
            }
        } catch {
            // Return empty array on parse error
        }
        
        return list;
    }

    /**
     * Converts an array of string dictionaries to JSON array text.
     * @param list The array of string dictionaries to convert
     * @returns The JSON array string
     */
    public static GetJsonArrayText(list: Map<string, string>[]): string {
        const arrayData: any[] = [];

        for (const item of list.filter(x => x !== null && x !== undefined)) {
            const objectData: any = {};
            
            for (const [key, value] of item) {
                if (key.endsWith('.json')) {
                    if (value && value.trim() !== '') {
                        try {
                            objectData[key] = JSON.parse(value);
                        } catch {
                            objectData[key] = value;
                        }
                    }
                } else {
                    objectData[key] = value;
                }
            }
            
            arrayData.push(objectData);
        }

        return JSON.stringify(arrayData);
    }

    /**
     * Converts an array of string dictionaries to JSON array text (alternative overload).
     * @param list The array of string dictionaries to convert
     * @returns The JSON array string
     */
    public static GetJsonArrayTextFromStringMaps(list: { [key: string]: string }[]): string {
        const arrayData: any[] = [];

        for (const item of list.filter(x => x !== null && x !== undefined)) {
            const objectData: any = {};
            
            for (const key of Object.keys(item)) {
                const value = item[key];
                if (key.endsWith('.json')) {
                    if (value && value.trim() !== '') {
                        try {
                            objectData[key] = JSON.parse(value);
                        } catch {
                            objectData[key] = value;
                        }
                    }
                } else {
                    objectData[key] = value;
                }
            }
            
            arrayData.push(objectData);
        }

        return JSON.stringify(arrayData);
    }

    /**
     * Converts an object with string array values to JSON text.
     * @param properties The object with string array values
     * @returns The JSON string
     */
    public static GetJsonObjectText(properties: { [key: string]: string[] }): string {
        const objectData: any = {};

        for (const key of Object.keys(properties)) {
            const values = properties[key].filter(x => x !== null && x !== undefined && x !== '');
            
            if (values.length === 1) {
                const value = values[0];
                if (key.endsWith('.json')) {
                    if (value && value.trim() !== '') {
                        try {
                            objectData[key] = JSON.parse(value);
                        } catch {
                            objectData[key] = value;
                        }
                    }
                } else {
                    objectData[key] = value;
                }
            } else if (values.length > 1) {
                const arrayData: any[] = [];
                for (const value of values) {
                    if (key.endsWith('.json')) {
                        if (value && value.trim() !== '') {
                            try {
                                arrayData.push(JSON.parse(value));
                            } catch {
                                arrayData.push(value);
                            }
                        }
                    } else {
                        arrayData.push(value);
                    }
                }
                objectData[key] = arrayData;
            }
        }

        return JSON.stringify(objectData);
    }
}