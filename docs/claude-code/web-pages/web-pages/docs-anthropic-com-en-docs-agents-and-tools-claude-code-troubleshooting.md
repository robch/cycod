## Claude Code troubleshooting - Anthropic

url: https://docs.anthropic.com/en/docs/agents-and-tools/claude-code/troubleshooting

Claude Code troubleshooting - AnthropicAnthropic home pageEnglishSearch...Ctrl KResearchNewsGo to claude.aiGo to claude.aiSearch...NavigationClaude CodeClaude Code troubleshootingWelcomeUser GuidesAPI ReferencePrompt LibraryRelease NotesDeveloper ConsoleDeveloper DiscordSupportGet startedOverviewInitial setupIntro to ClaudeLearn about ClaudeUse casesModels & pricingSecurity and complianceBuild with ClaudeDefine success criteriaDevelop test casesContext windowsVisionPrompt engineeringExtended thinkingMultilingual supportTool use (function calling)Prompt cachingPDF supportCitationsToken countingBatch processingEmbeddingsAgents and toolsClaude CodeOverviewClaude Code tutorialsTroubleshootingComputer use (beta)Model Context Protocol (MCP)Google Sheets add-onTest and evaluateStrengthen guardrailsUsing the Evaluation ToolAdministrationAdmin APIResourcesGlossaryModel deprecationsSystem statusClaude 3 model cardClaude 3.7 system cardAnthropic CookbookAnthropic CoursesAPI featuresLegal centerAnthropic Privacy PolicyClaude CodeClaude Code troubleshootingSolutions for common issues with Claude Code installation and usage.​Common installation issues  
​Linux permission issues  
When installing Claude Code with npm, you may encounter permission errors if your npm global prefix is not user writable (eg. /usr, or /use/local).  
​Recommended solution: Create a user-writable npm prefix  
The safest approach is to configure npm to use a directory within your home folder:  

Copy  
# First, save a list of your existing global packages for later migration  
npm list -g --depth=0 > ~/npm-global-packages.txt  

# Create a directory for your global packages  
mkdir -p ~/.npm-global  

# Configure npm to use the new directory path  
npm config set prefix ~/.npm-global  

# Note: Replace ~/.bashrc with ~/.zshrc, ~/.profile, or other appropriate file for your shell  
echo 'export PATH=~/.npm-global/bin:$PATH' >> ~/.bashrc  

# Apply the new PATH setting  
source ~/.bashrc  

# Now reinstall Claude Code in the new location  
npm install -g @anthropic-ai/claude-code  

# Optional: Reinstall your previous global packages in the new location  
# Look at ~/npm-global-packages.txt and install packages you want to keep  

This solution is recommended because it:  
Avoids modifying system directory permissions  
Creates a clean, dedicated location for your global npm packages  
Follows security best practices  

​System Recovery: If you have run commands that change ownership and permissions of system files or similar  
If you’ve already run a command that changed system directory permissions (such as sudo chown -R $USER:$(id -gn) /usr && sudo chmod -R u+w /usr) and your system is now broken (for example, if you see sudo: /usr/bin/sudo must be owned by uid 0 and have the setuid bit set), you’ll need to perform recovery steps.  

Ubuntu/Debian Recovery Method:  
While rebooting, hold SHIFT to access the GRUB menu  
Select “Advanced options for Ubuntu/Debian”  
Choose the recovery mode option  
Select “Drop to root shell prompt”  
Remount the filesystem as writable:  

Copy  
mount -o remount,rw /  

Fix permissions:  

Copy  
# Restore root ownership  
chown -R root:root /usr  
chmod -R 755 /usr  

# Ensure /usr/local is owned by your user for npm packages  
chown -R YOUR_USERNAME:YOUR_USERNAME /usr/local  

# Set setuid bit for critical binaries  
chmod u+s /usr/bin/sudo  
chmod 4755 /usr/bin/sudo  
chmod u+s /usr/bin/su  
chmod u+s /usr/bin/passwd  
chmod u+s /usr/bin/newgrp  
chmod u+s /usr/bin/gpasswd  
chmod u+s /usr/bin/chsh  
chmod u+s /usr/bin/chfn  

# Fix sudo configuration  
chown root:root /usr/libexec/sudo/sudoers.so  
chmod 4755 /usr/libexec/sudo/sudoers.so  
chown root:root /etc/sudo.conf  
chmod 644 /etc/sudo.conf  

Reinstall affected packages (optional but recommended):  

Copy  
# Save list of installed packages  
dpkg --get-selections > /tmp/installed_packages.txt  

# Reinstall them  
awk '{print $1}' /tmp/installed_packages.txt | xargs -r apt-get install --reinstall -y  

Reboot:  

Copy  
reboot  

Alternative Live USB Recovery Method:  
If the recovery mode doesn’t work, you can use a live USB:  
Boot from a live USB (Ubuntu, Debian, or any Linux distribution)  
Find your system partition:  

Copy  
lsblk  

Mount your system partition:  

Copy  
sudo mount /dev/sdXY /mnt  # replace sdXY with your actual system partition  

If you have a separate boot partition, mount it too:  

Copy  
sudo mount /dev/sdXZ /mnt/boot  # if needed  

Chroot into your system:  

Copy  
# For Ubuntu/Debian:  
sudo chroot /mnt  

# For Arch-based systems:  
sudo arch-chroot /mnt  

Follow steps 6-8 from the Ubuntu/Debian recovery method above  

After restoring your system, follow the recommended solution above to set up a user-writable npm prefix.  

​Auto-updater issues  
If Claude Code can’t update automatically, it may be due to permission issues with your npm global prefix directory. Follow the recommended solution above to fix this.  
If you prefer to disable the auto-updater instead, you can use:  

Copy  
claude config set -g autoUpdaterStatus disabled  

​Permissions and authentication  
​Repeated permission prompts  
If you find yourself repeatedly approving the same commands, you can allow specific tools to run without approval:  

Copy  
# Let npm test run without approval  
claude config add allowedTools "Bash(npm test)"  

# Let npm test and any of its sub-commands run without approval  
claude config add allowedTools "Bash(npm test:*)"  

​Authentication issues  
If you’re experiencing authentication problems:  
Run /logout to sign out completely  
Close Claude Code  
Restart with claude and complete the authentication process again  
If problems persist, try:  

Copy  
rm -rf ~/.config/claude-code/auth.json  
claude  

This removes your stored authentication information and forces a clean login.  

​Performance and stability  
​High CPU or memory usage  
Claude Code is designed to work with most development environments, but may consume significant resources when processing large codebases. If you’re experiencing performance issues:  
Use /compact regularly to reduce context size  
Close and restart Claude Code between major tasks  
Consider adding large build directories to your .gitignore and .claudeignore files  

​Command hangs or freezes  
If Claude Code seems unresponsive:  
Press Ctrl+C to attempt to cancel the current operation  
If unresponsive, you may need to close the terminal and restart  
For persistent issues, run Claude with verbose logging: claude --verbose  

​Getting more help  
If you’re experiencing issues not covered here:  
Use the /bug command within Claude Code to report problems directly to Anthropic  
Check the GitHub repository for known issues  
Run /doctor to check the health of your Claude Code installation  

Was this page helpful?YesNoClaude Code tutorialsComputer use (beta)xlinkedinOn this pageCommon installation issuesLinux permission issuesRecommended solution: Create a user-writable npm prefixSystem Recovery: If you have run commands that change ownership and permissions of system files or similarAuto-updater issuesPermissions and authenticationRepeated permission promptsAuthentication issuesPerformance and stabilityHigh CPU or memory usageCommand hangs or freezesGetting more help

