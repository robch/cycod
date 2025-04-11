---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Memory Patterns & Examples

This page provides practical patterns and real-world examples for using ChatX's memory system in different scenarios. Each pattern demonstrates a specific way to leverage memories for enhanced productivity.

## Pattern: Context Management for Large Projects

### Problem
When working on large projects, it's difficult to keep track of all the components, dependencies, and architectural decisions.

### Solution
Create a comprehensive memory system organized by component, with architecture diagrams, API documentation, and decision records.

### Implementation

1. Create a structured memory directory:

   ```
   .memories/
   ├── architecture/
   │   ├── overview.md
   │   ├── frontend.md
   │   └── backend.md
   ├── components/
   │   ├── auth-service.md
   │   ├── user-service.md
   │   └── payment-service.md
   ├── api/
   │   ├── rest-conventions.md
   │   ├── user-endpoints.md
   │   └── payment-endpoints.md
   └── decisions/
       ├── 2023-01-15-database-selection.md
       ├── 2023-02-28-authentication-approach.md
       └── 2023-03-12-deployment-strategy.md
   ```

2. Create targeted aliases for different contexts:

   ```bash
   # Component-specific alias
   chatx --input "/files .memories/components/auth-service.md" \
         --input "/files .memories/api/*auth*.md" \
         --input "/files .memories/decisions/*auth*.md" \
         --add-system-prompt "You are focusing on the authentication service. Consider the relevant memory documents about this component." \
         --save-local-alias auth-context

   # Architecture overview alias
   chatx --input "/files .memories/architecture/*.md" \
         --add-system-prompt "You are examining the system architecture. Consider the architecture documentation in memory." \
         --save-local-alias arch-context
   ```

3. Use these contexts for targeted discussions:

   ```bash
   chatx --auth-context --question "Explain how our authentication flow works with third-party providers."
   
   chatx --arch-context --question "How does the auth service communicate with the user service?"
   ```

## Pattern: Code Style Enforcement

### Problem
Maintaining consistent code style across a team or multiple projects.

### Solution
Create memory files that document code style rules, then reference them during code reviews and development.

### Implementation

1. Create style guide memory files:

   ```
   .memories/
   ├── style/
   │   ├── javascript.md
   │   ├── python.md
   │   ├── css.md
   │   └── naming-conventions.md
   ```

2. In `javascript.md`, include detailed style rules:

   ```markdown
   # JavaScript Style Guide

   ## Formatting
   - Use 2 spaces for indentation
   - Use semicolons at the end of statements
   - Maximum line length of 100 characters
   - Use single quotes for strings

   ## Functions
   - Use arrow functions for anonymous functions
   - Use named functions for methods
   - Avoid functions longer than 30 lines

   ## Variables
   - Use camelCase for variables and functions
   - Use PascalCase for classes and components
   - Use UPPER_CASE for constants
   - Avoid global variables

   ## ES Features
   - Prefer const over let, avoid var
   - Use destructuring where appropriate
   - Use template literals instead of string concatenation
   - Use spread operators for array/object manipulation
   ```

3. Create a code review alias:

   ```bash
   chatx --input "/files .memories/style/*.md" \
         --add-system-prompt "You are reviewing code for style compliance. Check for adherence to the style guidelines in memory. When reviewing code, point out any style violations and suggest corrections. Be thorough but prioritize substantive issues over minor formatting." \
         --save-local-alias code-review
   ```

4. Use it during code reviews:

   ```bash
   chatx --code-review --instruction "Review this JavaScript code for style issues:
   
   function getData(userID) {
     var results = []
     for(var i=0;i<users.length;i++) {
       if(users[i].id == userID) {
         results.push({ userName: users[i].name, userAge: users[i].age })
       }
     }
     return results;
   }"
   ```

## Pattern: Knowledge Base for Support

### Problem
Support teams need quick access to product information, common issues, and solutions.

### Solution
Build a memory-based knowledge base that support team members can query conversationally.

### Implementation

1. Create a categorized knowledge base:

   ```
   .memories/
   ├── product/
   │   ├── features.md
   │   ├── limitations.md
   │   └── pricing.md
   ├── troubleshooting/
   │   ├── common-errors.md
   │   ├── account-issues.md
   │   └── performance-problems.md
   └── procedures/
       ├── account-recovery.md
       ├── data-migration.md
       └── refunds.md
   ```

2. In `common-errors.md`, document issues and solutions:

   ```markdown
   # Common Errors

   ## "Unable to connect to database" Error
   
   ### Symptoms
   - App displays "Unable to connect to database" error message
   - Account data doesn't load
   - Changes don't save
   
   ### Possible Causes
   1. Network connectivity issues
   2. Server maintenance
   3. Corrupt local cache
   
   ### Solutions
   1. Check internet connection and retry
   2. Clear application cache (Settings > Storage > Clear Cache)
   3. Wait 10 minutes and try again (if server maintenance)
   4. If problem persists, check status.example.com for system outages
   
   ## "Invalid credentials" Error
   
   ### Symptoms
   - User cannot log in despite correct password
   - Receives "Invalid credentials" message
   
   ### Possible Causes
   1. Account locked after too many attempts
   2. Password expired
   3. Account deactivated
   
   ### Solutions
   1. Reset password via forgot password flow
   2. Check email for account lock notification
   3. Contact admin if account is deactivated
   ```

3. Create a support agent alias:

   ```bash
   chatx --input "/files .memories/**/*.md" \
         --add-system-prompt "You are a support agent assisting customers with product issues. Use the knowledge base in memory to provide accurate information and troubleshooting steps. If the exact issue isn't covered in memory, use the most relevant information to provide appropriate guidance. Always be polite, clear, and thorough." \
         --save-local-alias support-agent
   ```

4. Use in support conversations:

   ```bash
   chatx --support-agent --question "A customer is getting 'Unable to connect to database' errors. What should I tell them?"
   ```

## Pattern: Project Onboarding Accelerator

### Problem
Getting new team members up to speed on a project takes time and often involves repeated explanations.

### Solution
Create a comprehensive onboarding memory system that new developers can query conversationally.

### Implementation

1. Create onboarding memory files:

   ```
   .memories/
   ├── onboarding/
   │   ├── getting-started.md
   │   ├── development-environment.md
   │   ├── architecture-overview.md
   │   ├── workflow.md
   │   └── team-norms.md
   ```

2. In `development-environment.md`, include detailed setup instructions:

   ```markdown
   # Development Environment Setup

   ## Required Tools
   - Node.js 16+
   - Docker Desktop
   - Visual Studio Code
   - Git
   
   ## Setup Steps
   
   1. Clone the repository:
      ```
      git clone https://github.com/company/project.git
      cd project
      ```
   
   2. Install dependencies:
      ```
      npm install
      ```
   
   3. Set up environment variables:
      ```
      cp .env.example .env
      ```
      Edit .env to add your personal development API keys.
   
   4. Start the development environment:
      ```
      npm run dev
      ```
   
   5. Access the application at http://localhost:3000
   
   ## Common Setup Issues
   
   ### Port conflicts
   If port 3000 is already in use, edit the .env file to change PORT=3000 to an available port.
   
   ### Docker container errors
   If Docker containers fail to start, try:
   ```
   docker compose down -v
   docker compose up -d
   ```
   ```

3. Create an onboarding alias:

   ```bash
   chatx --input "/files .memories/onboarding/*.md" \
         --add-system-prompt "You are helping a new team member get onboarded to the project. Reference relevant information from memory to answer their questions clearly and helpfully. Provide specific commands, paths, and procedures when relevant." \
         --save-local-alias onboarding
   ```

4. New team members can use this for self-service learning:

   ```bash
   chatx --onboarding --question "How do I set up my local development environment?"
   chatx --onboarding --question "What's our git workflow for submitting changes?"
   ```

## Pattern: Personal Workflow Enhancement

### Problem
Individual developers have unique preferences and workflows that they need to remember across projects.

### Solution
Create personal memory files that store your preferences, snippets, and workflows.

### Implementation

1. Create personal memory files in your user directory:

   ```bash
   mkdir -p ~/.chatx/.memories/workflow
   ```

2. Create a file for command snippets:

   ```bash
   echo "# Useful Commands

   ## Git Workflows
   
   ### Cleaning up local branches
   ```
   # Delete all local branches that have been merged to main
   git branch --merged main | grep -v main | xargs git branch -d
   ```
   
   ### Organizing commits before PR
   ```
   # Squash last 3 commits with interactive rebase
   git rebase -i HEAD~3
   ```
   
   ## Docker Helpers
   
   ### Clean up unused resources
   ```
   # Remove all stopped containers, unused networks, dangling images, and build cache
   docker system prune -a
   ```
   
   ## Deployment Commands
   
   ### Production deployment
   ```
   npm run build
   npm run deploy:prod
   ```
   
   ### Staging deployment
   ```
   npm run build
   npm run deploy:staging
   ```" > ~/.chatx/.memories/workflow/commands.md
   ```

3. Create a preferences file:

   ```bash
   echo "# Personal Preferences

   ## Communication Style
   - Prefer concise, direct communication
   - Include code examples when possible
   - Use bullet points for lists rather than paragraphs
   - Highlight key insights or decisions
   
   ## Code Style
   - Prefer functional programming patterns
   - Use descriptive variable names
   - Include JSDoc comments for functions
   - Write unit tests for all new functions
   
   ## Learning Style
   - Start with concrete examples
   - Explain concepts with analogies
   - Show patterns and anti-patterns
   - Connect new information to existing knowledge" > ~/.chatx/.memories/workflow/preferences.md
   ```

4. Create a personal assistant alias:

   ```bash
   chatx --input "/files ~/.chatx/.memories/**/*.md" \
         --add-system-prompt "You are my personal assistant. Adapt your responses to match my personal preferences in memory. When discussing code or commands, reference my stored snippets and patterns where relevant." \
         --save-user-alias me
   ```

5. Use your personalized assistant:

   ```bash
   chatx --me --question "What's the command for cleaning up Docker resources?"
   chatx --me --question "Help me refactor this function to match my preferred style."
   ```

## Pattern: Living Documentation

### Problem
Project documentation becomes outdated quickly and is often separated from the code.

### Solution
Create memory-backed documentation that can be easily updated and queried.

### Implementation

1. Create documentation memory structure:

   ```
   .memories/
   ├── docs/
   │   ├── overview.md
   │   ├── architecture.md
   │   ├── api-reference.md
   │   └── deployment.md
   ```

2. Set up a documentation alias:

   ```bash
   chatx --input "/files .memories/docs/*.md" \
         --add-system-prompt "You are providing documentation assistance. Use the documentation in memory to answer questions accurately. If documentation seems incomplete or outdated, note this in your response." \
         --save-local-alias docs
   ```

3. Add self-updating capability:

   ```bash
   chatx --input "/files .memories/docs/*.md" \
         --add-system-prompt "You are providing documentation assistance. Use the documentation in memory to answer questions accurately. If you notice outdated or incorrect information, suggest updates using the format:
         MEMORY UPDATE [.memories/docs/filename.md]:
         Section: [section name]
         Current: [existing text]
         Updated: [corrected text]" \
         --save-local-alias docs-update
   ```

4. Use for documentation queries and updates:

   ```bash
   chatx --docs --question "How do we deploy to production?"
   
   # When documentation needs updating
   chatx --docs-update --question "How do we deploy using the new CI pipeline?"
   ```

## Combining Patterns for Powerful Workflows

The real power of ChatX's memory system comes from combining these patterns:

```bash
# Create a comprehensive development context
chatx --input "/files .memories/architecture/*.md" \
      --input "/files .memories/components/auth-service.md" \
      --input "/files ~/.chatx/.memories/workflow/preferences.md" \
      --input "/files ~/.chatx/.memories/workflow/commands.md" \
      --add-system-prompt "You are helping me work on the authentication service. Consider both the project documentation and my personal preferences when responding. Format code according to my preferences and reference useful commands when relevant." \
      --save-local-alias auth-dev
```

This creates a personalized, context-aware assistant that combines project knowledge with your individual preferences and workflows.

## Next Steps

Now that you've explored these practical patterns, you're ready to build your own memory system tailored to your specific needs. Experiment with different memory organizations, access patterns, and system prompts to create a workflow that maximizes your productivity with ChatX.