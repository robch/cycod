/**
 * Types of shells supported by the process executors.
 */
export enum PersistentShellType {
    /**
     * Bash shell (default on Linux/macOS).
     */
    Bash = 'bash',
    
    /**
     * CMD shell (Windows command prompt).
     */
    Cmd = 'cmd',
    
    /**
     * PowerShell.
     */
    PowerShell = 'powershell'
}