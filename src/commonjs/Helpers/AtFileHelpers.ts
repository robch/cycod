import { INamedValues } from './INamedValues';
import { FileHelpers } from './FileHelpers';
import { ConsoleHelpers } from './ConsoleHelpers';
import { ConfigStore } from '../Configuration/ConfigStore';
import { EmbeddedFileHelpers } from './EmbeddedFileHelpers';

export class AtFileHelpers {
    public static expandAtFileValue(atFileValue: string, values?: INamedValues): string {
        if (atFileValue.startsWith('@') && FileHelpers.fileExists(atFileValue.slice(1))) {
            return FileHelpers.readAllText(atFileValue.slice(1));
        } else if (atFileValue.startsWith('@') && ConsoleHelpers.isStandardInputReference(atFileValue.slice(1))) {
            return ConsoleHelpers.getAllLinesFromStdin().join('\n');
        } else if (atFileValue.startsWith('@')) {
            const key = atFileValue.slice(1);

            // First check the provided values if any
            if (values && values.Contains(key)) {
                const result = values.Get(key);
                if (result !== null) {
                    return result;
                }
            }

            // Then check ConfigStore
            const configValue = ConfigStore.instance.getFromAnyScope(key);
            if (!configValue.isNotFoundNullOrEmpty()) {
                return configValue.asString() ?? '';
            }

            // Then check embedded files
            const embeddedFileExists = AtFileHelpers.embeddedFileExists(key);
            if (embeddedFileExists) {
                return AtFileHelpers.readEmbeddedStream(key)!;
            }
        }

        return atFileValue;
    }

    private static readEmbeddedStream(key: string): string | null {
        let value = EmbeddedFileHelpers.readEmbeddedStream(key);
        if (value !== null) return value;

        value = EmbeddedFileHelpers.readEmbeddedStream(`prompts.${key}`);
        if (value !== null) return value;

        value = EmbeddedFileHelpers.readEmbeddedStream(`prompts.${key}.md`);
        if (value !== null) return value;

        return null;
    }

    private static embeddedFileExists(key: string): boolean {
        return EmbeddedFileHelpers.embeddedStreamExists(key)
            || EmbeddedFileHelpers.embeddedStreamExists(`prompts.${key}`)
            || EmbeddedFileHelpers.embeddedStreamExists(`prompts.${key}.md`);
    }
}