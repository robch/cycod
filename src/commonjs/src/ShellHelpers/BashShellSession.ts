import { ShellSession, ShellType } from './ShellSession';

export class BashShellSession extends ShellSession {
    protected getShellType(): ShellType {
        return ShellType.Bash;
    }
}