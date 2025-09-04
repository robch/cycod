export class EnvVarSettingException extends Error {
    public constructor();
    public constructor(message: string);
    public constructor(message?: string) {
        super(message);
        this.name = 'EnvVarSettingException';
    }
}
