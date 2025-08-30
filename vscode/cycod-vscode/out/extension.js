"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.activate = activate;
exports.deactivate = deactivate;
const vscode = __importStar(require("vscode"));
const cp = __importStar(require("child_process"));
const util = __importStar(require("util"));
const ui_1 = __importDefault(require("./ui"));
const exec = util.promisify(cp.exec);
function activate(context) {
    console.log('Cycod VS Code extension is being activated!');
    const provider = new CycodChatProvider(context.extensionUri, context);
    const disposable = vscode.commands.registerCommand('cycod-vscode.openChat', (column) => {
        console.log('Cycod Chat command executed!');
        provider.show(column);
    });
    // Register webview view provider for sidebar chat
    const webviewProvider = new CycodChatWebviewProvider(context.extensionUri, context, provider);
    vscode.window.registerWebviewViewProvider('cycod-vscode.chat', webviewProvider);
    // Create status bar item
    const statusBarItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Right, 100);
    statusBarItem.text = "Cycod";
    statusBarItem.tooltip = "Open Cycod Chat (Ctrl+Shift+Y)";
    statusBarItem.command = 'cycod-vscode.openChat';
    statusBarItem.show();
    context.subscriptions.push(disposable, statusBarItem);
    console.log('Cycod VS Code extension activation completed successfully!');
}
function deactivate() { }
class CycodChatWebviewProvider {
    _extensionUri;
    _context;
    _chatProvider;
    constructor(_extensionUri, _context, _chatProvider) {
        this._extensionUri = _extensionUri;
        this._context = _context;
        this._chatProvider = _chatProvider;
    }
    resolveWebviewView(webviewView, _context, _token) {
        webviewView.webview.options = {
            enableScripts: true,
            localResourceRoots: [this._extensionUri]
        };
        this._chatProvider.showInWebview(webviewView.webview, webviewView);
        webviewView.onDidChangeVisibility(() => {
            if (webviewView.visible) {
                if (this._chatProvider._panel) {
                    console.log('Closing main panel because sidebar became visible');
                    this._chatProvider._panel.dispose();
                    this._chatProvider._panel = undefined;
                }
                this._chatProvider.reinitializeWebview();
            }
        });
    }
}
class CycodChatProvider {
    _extensionUri;
    _context;
    _panel;
    _webview;
    _webviewView;
    _disposables = [];
    _messageHandlerDisposable;
    _currentCycodProcess;
    _isProcessing;
    constructor(_extensionUri, _context) {
        this._extensionUri = _extensionUri;
        this._context = _context;
    }
    show(column = vscode.ViewColumn.Two) {
        const actualColumn = column instanceof vscode.Uri ? vscode.ViewColumn.Two : column;
        this._closeSidebar();
        if (this._panel) {
            this._panel.reveal(actualColumn);
            return;
        }
        this._panel = vscode.window.createWebviewPanel('cycodChat', 'Cycod Chat', actualColumn, {
            enableScripts: true,
            retainContextWhenHidden: true,
            localResourceRoots: [this._extensionUri]
        });
        this._panel.webview.html = this._getHtmlForWebview();
        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);
        this._setupWebviewMessageHandler(this._panel.webview);
        // Send ready message
        setTimeout(() => {
            this._sendReadyMessage();
        }, 100);
    }
    _postMessage(message) {
        if (this._panel && this._panel.webview) {
            this._panel.webview.postMessage(message);
        }
        else if (this._webview) {
            this._webview.postMessage(message);
        }
    }
    _sendReadyMessage() {
        this._postMessage({
            type: 'ready',
            data: this._isProcessing ? 'Cycod is working...' : 'Ready to chat with Cycod! Type your message below.'
        });
    }
    _handleWebviewMessage(message) {
        switch (message.type) {
            case 'sendMessage':
                this._sendMessageToCycod(message.text);
                return;
            case 'stopRequest':
                this._stopCycodProcess();
                return;
        }
    }
    _setupWebviewMessageHandler(webview) {
        if (this._messageHandlerDisposable) {
            this._messageHandlerDisposable.dispose();
        }
        this._messageHandlerDisposable = webview.onDidReceiveMessage(message => this._handleWebviewMessage(message), null, this._disposables);
    }
    _closeSidebar() {
        if (this._webviewView) {
            vscode.commands.executeCommand('workbench.view.explorer');
        }
    }
    showInWebview(webview, webviewView) {
        if (this._panel) {
            console.log('Closing main panel because sidebar is opening');
            this._panel.dispose();
            this._panel = undefined;
        }
        this._webview = webview;
        this._webviewView = webviewView;
        this._webview.html = this._getHtmlForWebview();
        this._setupWebviewMessageHandler(this._webview);
        this._initializeWebview();
    }
    _initializeWebview() {
        setTimeout(() => {
            this._sendReadyMessage();
        }, 100);
    }
    reinitializeWebview() {
        if (this._webview) {
            this._initializeWebview();
            this._setupWebviewMessageHandler(this._webview);
        }
    }
    async _sendMessageToCycod(message) {
        const workspaceFolder = vscode.workspace.workspaceFolders?.[0];
        const cwd = workspaceFolder ? workspaceFolder.uri.fsPath : process.cwd();
        this._isProcessing = true;
        // Show user input in chat
        this._postMessage({
            type: 'userInput',
            data: message
        });
        // Set processing state
        this._postMessage({
            type: 'setProcessing',
            data: { isProcessing: true }
        });
        // Show loading indicator
        this._postMessage({
            type: 'loading',
            data: 'Cycod is working...'
        });
        // Get configuration
        const config = vscode.workspace.getConfiguration('cycodVscode');
        const cycodPath = config.get('cycod.path', '/opt/homebrew/bin/cycod');
        const autoApprove = config.get('cycod.autoApprove', '*');
        // Build command arguments
        const args = [
            'chat',
            '--auto-approve', autoApprove
        ];
        console.log('Cycod command:', cycodPath, args);
        const cycodProcess = cp.spawn(cycodPath, args, {
            shell: process.platform === 'win32',
            cwd: cwd,
            stdio: ['pipe', 'pipe', 'pipe'],
            env: {
                ...process.env,
                FORCE_COLOR: '0',
                NO_COLOR: '1'
            }
        });
        // Store process reference for potential termination
        this._currentCycodProcess = cycodProcess;
        // Send the message to Cycod's stdin
        if (cycodProcess.stdin) {
            cycodProcess.stdin.write(message + '\n');
            cycodProcess.stdin.end();
        }
        let rawOutput = '';
        let errorOutput = '';
        if (cycodProcess.stdout) {
            cycodProcess.stdout.on('data', (data) => {
                rawOutput += data.toString();
            });
        }
        if (cycodProcess.stderr) {
            cycodProcess.stderr.on('data', (data) => {
                errorOutput += data.toString();
            });
        }
        cycodProcess.on('close', (code) => {
            console.log('Cycod process closed with code:', code);
            if (!this._currentCycodProcess) {
                return;
            }
            // Clear process reference
            this._currentCycodProcess = undefined;
            // Clear loading indicator
            this._postMessage({
                type: 'clearLoading'
            });
            // Reset processing state
            this._isProcessing = false;
            this._postMessage({
                type: 'setProcessing',
                data: { isProcessing: false }
            });
            if (code === 0) {
                // Success - show output
                this._postMessage({
                    type: 'output',
                    data: rawOutput.trim() || 'Command completed successfully'
                });
            }
            else {
                // Error
                this._postMessage({
                    type: 'error',
                    data: errorOutput.trim() || `Command failed with exit code ${code}`
                });
            }
        });
        cycodProcess.on('error', (error) => {
            console.log('Cycod process error:', error.message);
            if (!this._currentCycodProcess) {
                return;
            }
            // Clear process reference
            this._currentCycodProcess = undefined;
            this._postMessage({
                type: 'clearLoading'
            });
            this._isProcessing = false;
            this._postMessage({
                type: 'setProcessing',
                data: { isProcessing: false }
            });
            // Check if cycod command is not found
            if (error.message.includes('ENOENT') || error.message.includes('command not found')) {
                this._postMessage({
                    type: 'error',
                    data: `Cycod not found at ${cycodPath}. Please check the path in settings.`
                });
            }
            else {
                this._postMessage({
                    type: 'error',
                    data: `Error running Cycod: ${error.message}`
                });
            }
        });
    }
    _stopCycodProcess() {
        console.log('Stop request received');
        this._isProcessing = false;
        // Update UI state
        this._postMessage({
            type: 'setProcessing',
            data: { isProcessing: false }
        });
        if (this._currentCycodProcess) {
            console.log('Terminating Cycod process...');
            // Try graceful termination first
            this._currentCycodProcess.kill('SIGTERM');
            // Force kill after 2 seconds if still running
            setTimeout(() => {
                if (this._currentCycodProcess && !this._currentCycodProcess.killed) {
                    console.log('Force killing Cycod process...');
                    this._currentCycodProcess.kill('SIGKILL');
                }
            }, 2000);
            // Clear process reference
            this._currentCycodProcess = undefined;
            this._postMessage({
                type: 'clearLoading'
            });
            // Send stop confirmation message
            this._postMessage({
                type: 'error',
                data: '⏹️ Cycod process was stopped.'
            });
            console.log('Cycod process termination initiated');
        }
        else {
            console.log('No Cycod process running to stop');
        }
    }
    _getHtmlForWebview() {
        return (0, ui_1.default)();
    }
    dispose() {
        if (this._panel) {
            this._panel.dispose();
            this._panel = undefined;
        }
        if (this._messageHandlerDisposable) {
            this._messageHandlerDisposable.dispose();
            this._messageHandlerDisposable = undefined;
        }
        while (this._disposables.length) {
            const disposable = this._disposables.pop();
            if (disposable) {
                disposable.dispose();
            }
        }
    }
}
//# sourceMappingURL=extension.js.map