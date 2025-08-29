export declare const createTempDir: () => Promise<string>;
export declare const cleanupTempDir: (tempDir: string) => Promise<void>;
export declare const mockConsole: () => {
    logs: string[];
    errors: string[];
    restore: () => void;
};
//# sourceMappingURL=test-setup.d.ts.map