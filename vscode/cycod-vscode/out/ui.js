"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const ui_styles_1 = __importDefault(require("./ui-styles"));
const getHtml = () => `<!DOCTYPE html>
<html lang="en">
<head>
	<meta charset="UTF-8">
	<meta name="viewport" content="width=device-width, initial-scale=1.0">
	<title>Cycod Chat</title>
	${ui_styles_1.default}
</head>
<body>
	<div class="header">
		<div style="display: flex; align-items: center;">
			<h2>Cycod Chat</h2>
		</div>
	</div>

	<div class="chat-container" id="chatContainer">
		<div class="messages" id="messages"></div>
		
		<div class="input-container" id="inputContainer">
			<div class="textarea-container">
				<div class="textarea-wrapper">
					<textarea class="input-field" id="messageInput" placeholder="Type your message to Cycod..." rows="1"></textarea>
				</div>
				<button class="send-btn" id="sendBtn" onclick="sendMessage()">Send</button>
				<button class="stop-btn" id="stopBtn" onclick="stopRequest()" style="display: none;">Stop</button>
			</div>
		</div>
	</div>

	<div class="status ready" id="status">
		<div class="status-indicator"></div>
		<div class="status-text" id="statusText">Ready</div>
	</div>

	<!-- Loading indicator (hidden by default) -->
	<div class="loading" id="loading" style="display: none;">
		<div class="loading-spinner">
			<div class="loading-ball"></div>
			<div class="loading-ball"></div>
			<div class="loading-ball"></div>
		</div>
		<div class="loading-text" id="loadingText">Cycod is working...</div>
	</div>

	<script>
		const vscode = acquireVsCodeApi();
		let isProcessing = false;

		// Auto-resize textarea
		const textarea = document.getElementById('messageInput');
		textarea.addEventListener('input', function() {
			this.style.height = 'auto';
			this.style.height = Math.min(this.scrollHeight, 200) + 'px';
		});

		// Send message on Enter (but not Shift+Enter)
		textarea.addEventListener('keydown', function(e) {
			if (e.key === 'Enter' && !e.shiftKey) {
				e.preventDefault();
				sendMessage();
			}
		});

		function sendMessage() {
			const input = document.getElementById('messageInput');
			const message = input.value.trim();
			
			if (!message || isProcessing) {
				return;
			}

			vscode.postMessage({
				type: 'sendMessage',
				text: message
			});

			input.value = '';
			input.style.height = 'auto';
		}

		function stopRequest() {
			vscode.postMessage({
				type: 'stopRequest'
			});
		}

		function addMessage(type, content) {
			const messagesDiv = document.getElementById('messages');
			const messageDiv = document.createElement('div');
			messageDiv.className = \`message \${type}\`;

			const headerDiv = document.createElement('div');
			headerDiv.className = 'message-header';

			const iconDiv = document.createElement('div');
			iconDiv.className = \`message-icon \${type}\`;
			iconDiv.textContent = type === 'user' ? 'U' : type === 'cycod' ? 'C' : 'E';

			const labelDiv = document.createElement('div');
			labelDiv.className = 'message-label';
			labelDiv.textContent = type === 'user' ? 'You' : type === 'cycod' ? 'Cycod' : 'Error';

			const contentDiv = document.createElement('div');
			contentDiv.className = 'message-content';
			contentDiv.textContent = content;

			headerDiv.appendChild(iconDiv);
			headerDiv.appendChild(labelDiv);
			messageDiv.appendChild(headerDiv);
			messageDiv.appendChild(contentDiv);

			messagesDiv.appendChild(messageDiv);
			messagesDiv.scrollTop = messagesDiv.scrollHeight;
		}

		function updateStatus(status, text) {
			const statusDiv = document.getElementById('status');
			const statusText = document.getElementById('statusText');
			
			statusDiv.className = \`status \${status}\`;
			statusText.textContent = text;
		}

		function showLoading(show = true, text = 'Cycod is working...') {
			const loadingDiv = document.getElementById('loading');
			const loadingText = document.getElementById('loadingText');
			
			loadingDiv.style.display = show ? 'flex' : 'none';
			loadingText.textContent = text;
		}

		function updateButtons(processing) {
			const sendBtn = document.getElementById('sendBtn');
			const stopBtn = document.getElementById('stopBtn');
			
			sendBtn.style.display = processing ? 'none' : 'flex';
			stopBtn.style.display = processing ? 'flex' : 'none';
			sendBtn.disabled = processing;
		}

		// Handle messages from the extension
		window.addEventListener('message', event => {
			const message = event.data;
			
			switch (message.type) {
				case 'ready':
					updateStatus('ready', message.data);
					break;
					
				case 'userInput':
					addMessage('user', message.data);
					break;
					
				case 'output':
					addMessage('cycod', message.data);
					break;
					
				case 'error':
					addMessage('error', message.data);
					break;
					
				case 'loading':
					showLoading(true, message.data);
					break;
					
				case 'clearLoading':
					showLoading(false);
					break;
					
				case 'setProcessing':
					isProcessing = message.data.isProcessing;
					updateButtons(isProcessing);
					
					if (isProcessing) {
						updateStatus('processing', 'Processing...');
					} else {
						updateStatus('ready', 'Ready');
					}
					break;
			}
		});

		// Focus on input field when loaded
		window.addEventListener('load', () => {
			document.getElementById('messageInput').focus();
		});
	</script>
</body>
</html>`;
exports.default = getHtml;
//# sourceMappingURL=ui.js.map