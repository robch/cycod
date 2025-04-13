---
hide:
- toc
icon: material/message-text
---

--8<-- "snippets/ai-generated.md"

# Custom Prompts

Custom prompts are reusable templates that help streamline common interactions with ChatX.

``` { .bash .cli-command title="Create a basic prompt template" }
chatx prompt create code-review "Please review this code and suggest improvements:
1. Identify any bugs or edge cases
2. Suggest performance optimizations
3. Comment on style and readability"
```

``` { .plaintext .cli-output }
Prompt 'code-review' saved to: C:\Users\username\.chatx\prompts\code-review.txt
```

## Creating Prompts

``` { .bash .cli-command title="Create a prompt for generating commit messages" }
chatx prompt create commit-msg "Based on the following git diff, generate a clear and concise commit message with:
- A short summary (50 chars or less)
- A more detailed explanation if needed
- Reference any relevant issue numbers

DIFF:
{diff}"
```

``` { .plaintext .cli-output }
Prompt 'commit-msg' saved to: C:\Users\username\.chatx\prompts\commit-msg.txt
```

``` { .bash .cli-command title="Create a prompt with complex formatting" }
chatx prompt create react-component "Create a React component with the following properties:

Component name: {name}
Props:
{props}

Include:
- TypeScript interfaces
- Error handling
- Basic styling with CSS modules
- Unit test template"
```

``` { .plaintext .cli-output }
Prompt 'react-component' saved to: C:\Users\username\.chatx\prompts\react-component.txt
```

## Using Prompts

``` { .bash .cli-command title="Use a prompt in CLI mode" }
chatx --prompt code-review --input "function add(a, b) { return a + b }"
```

``` { .plaintext .cli-output }
User: Please review this code and suggest improvements:
1. Identify any bugs or edge cases
2. Suggest performance optimizations
3. Comment on style and readability

function add(a, b) { return a + b }
```

``` { .bash .cli-command title="Use a prompt with variables" }
chatx --prompt react-component --var name=UserProfile --var "props=userId: string, showAvatar: boolean" --quiet
```

``` { .plaintext .cli-output }
Create a React component with the following properties:

Component name: UserProfile
Props:
userId: string, showAvatar: boolean

Include:
- TypeScript interfaces
- Error handling
- Basic styling with CSS modules
- Unit test template

Here's a React component with the specified properties:

```tsx
// UserProfile.tsx
import React, { useState, useEffect } from 'react';
import styles from './UserProfile.module.css';

interface UserProfileProps {
  userId: string;
  showAvatar: boolean;
}

const UserProfile: React.FC<UserProfileProps> = ({ userId, showAvatar }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  interface User {
    id: string;
    name: string;
    email: string;
    avatarUrl?: string;
  }

  useEffect(() => {
    const fetchUserData = async () => {
      try {
        setLoading(true);
        // Example API call
        const response = await fetch(`/api/users/${userId}`);
        
        if (!response.ok) {
          throw new Error(`Failed to fetch user data: ${response.status}`);
        }
        
        const userData = await response.json();
        setUser(userData);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'An unknown error occurred');
      } finally {
        setLoading(false);
      }
    };

    fetchUserData();
  }, [userId]);

  if (loading) {
    return <div className={styles.loading}>Loading user data...</div>;
  }

  if (error) {
    return <div className={styles.error}>Error: {error}</div>;
  }

  if (!user) {
    return <div className={styles.notFound}>User not found</div>;
  }

  return (
    <div className={styles.container}>
      {showAvatar && user.avatarUrl && (
        <img 
          src={user.avatarUrl} 
          alt={`${user.name}'s avatar`} 
          className={styles.avatar}
        />
      )}
      <div className={styles.userInfo}>
        <h2 className={styles.userName}>{user.name}</h2>
        <p className={styles.userEmail}>{user.email}</p>
      </div>
    </div>
  );
};

export default UserProfile;
```

```css
/* UserProfile.module.css */
.container {
  display: flex;
  align-items: center;
  padding: 1rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  background-color: #fff;
}

.avatar {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  object-fit: cover;
  margin-right: 1rem;
}

.userInfo {
  flex: 1;
}

.userName {
  margin: 0 0 0.5rem 0;
  font-size: 1.2rem;
  color: #333;
}

.userEmail {
  margin: 0;
  color: #666;
}

.loading {
  padding: 1rem;
  color: #666;
}

.error {
  padding: 1rem;
  color: #d32f2f;
  background-color: #ffebee;
  border-radius: 4px;
}

.notFound {
  padding: 1rem;
  color: #666;
  font-style: italic;
}
```

```tsx
// UserProfile.test.tsx
import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import UserProfile from './UserProfile';

// Mock fetch API
global.fetch = jest.fn();

describe('UserProfile Component', () => {
  const mockUser = {
    id: 'user123',
    name: 'Jane Doe',
    email: 'jane@example.com',
    avatarUrl: 'https://example.com/avatar.jpg'
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders loading state initially', () => {
    (global.fetch as jest.Mock).mockImplementation(() => new Promise(() => {}));
    
    render(<UserProfile userId="user123" showAvatar={true} />);
    
    expect(screen.getByText('Loading user data...')).toBeInTheDocument();
  });

  test('renders user data after successful fetch', async () => {
    (global.fetch as jest.Mock).mockImplementation(() => 
      Promise.resolve({
        ok: true,
        json: () => Promise.resolve(mockUser)
      })
    );
    
    render(<UserProfile userId="user123" showAvatar={true} />);
    
    await waitFor(() => {
      expect(screen.getByText('Jane Doe')).toBeInTheDocument();
      expect(screen.getByText('jane@example.com')).toBeInTheDocument();
      expect(screen.getByAltText(`Jane Doe's avatar`)).toBeInTheDocument();
    });
  });

  test('does not show avatar when showAvatar is false', async () => {
    (global.fetch as jest.Mock).mockImplementation(() => 
      Promise.resolve({
        ok: true,
        json: () => Promise.resolve(mockUser)
      })
    );
    
    render(<UserProfile userId="user123" showAvatar={false} />);
    
    await waitFor(() => {
      expect(screen.queryByAltText(`Jane Doe's avatar`)).not.toBeInTheDocument();
    });
  });

  test('renders error state on fetch error', async () => {
    (global.fetch as jest.Mock).mockImplementation(() => 
      Promise.resolve({
        ok: false,
        status: 404
      })
    );
    
    render(<UserProfile userId="invalid-id" showAvatar={true} />);
    
    await waitFor(() => {
      expect(screen.getByText(/Error:/)).toBeInTheDocument();
      expect(screen.getByText(/Failed to fetch user data: 404/)).toBeInTheDocument();
    });
  });
});
```
```

## Using Prompts Interactively

``` { .bash .cli-command title="Use a prompt in interactive mode" }
chatx
```

``` { .plaintext .cli-output }
User: ▌
```

``` { .plaintext .cli-command title="Use the code-review prompt interactively" }
/code-review
```

``` { .plaintext .cli-output }
Please paste or specify the code you'd like me to review. I'll provide feedback on:

1. Identify any bugs or edge cases
2. Suggest performance optimizations
3. Comment on style and readability

User: ▌
```

## Managing Prompts

``` { .bash .cli-command title="List all available prompts" }
chatx prompt list
```

``` { .plaintext .cli-output }
Available prompts:
  code-review - Review code for issues and improvements
  commit-msg - Generate a good commit message
  explain-code - Explain how code works
  react-component - Create a React component with specified properties
```

``` { .bash .cli-command title="Show a specific prompt" }
chatx prompt get code-review
```

``` { .plaintext .cli-output }
Prompt 'code-review':
Please review this code and suggest improvements:
1. Identify any bugs or edge cases
2. Suggest performance optimizations
3. Comment on style and readability
```

``` { .bash .cli-command title="Delete a prompt" }
chatx prompt delete react-component
```

``` { .plaintext .cli-output }
Prompt 'react-component' deleted.
```

## Prompt Storage

Prompts are stored as text files in your ChatX user directory:

```
Windows: %USERPROFILE%\.chatx\prompts\
macOS/Linux: ~/.chatx/prompts/
```

You can manually edit these files or create new ones by adding text files to this directory.