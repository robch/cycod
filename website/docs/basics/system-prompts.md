---
hide:
- toc
---

# System Prompts

System prompts guide how AI models behave when responding to your requests.

Use `--add-system-prompt` or `--system-prompt` to add to or replace the system prompt.

``` { .bash .cli-command title="Replace the default system prompt" }
chatx --system-prompt "Provide only code, no explanations." --question "Show me hello world in Python"
```

```` { .plaintext .cli-output }
```python
print("Hello, world!")
```
````

``` { .bash .cli-command title="Add to the default system prompt" }
chatx --add-system-prompt "Always include emojis" --question "Your fav 5 programming languages"
```

``` { .plaintext .cli-output }
# My 5 Favorite Programming Languages 🌟

Sure, I'd be happy to share my favorite programming languages with you! Here are my top 5:

1. **Python** 🐍 - Known for its readability and versatility, Python is widely used in data science, web development, automation, and artificial intelligence.

2. **JavaScript** ⚡ - Powers the web and allows for both frontend and backend development with frameworks like React, Angular, and Node.js.

3. **Rust** 🦀 - A modern systems programming language focused on safety, speed, and concurrency without a garbage collector.        

4. **TypeScript** 📝 - Adds static typing to JavaScript, making large codebases more maintainable and catching errors before runtime.

5. **Go** 🐹 - Created by Google, Go (or Golang) offers simplicity, efficient concurrency, and great performance for server-side applications.

Each of these languages has unique strengths and different use cases where they excel! 😊
```

??? question "When should I use --system-prompt vs --add-system-prompt?"

    Use `--system-prompt` when you want to completely replace the default behavior.
    
    Use `--add-system-prompt` when you want to keep the default system prompt but add specific instructions.

    The `chatx` CLI's default system prompt has detailed instructions for how to behave as a programmer's assistant. If your question is very specific, or in a non-programming context, you may want to use `--system-prompt` to replace the default system prompt.

    Otherwise, `--add-system-prompt` is a good way to augment the default.

## Using `@` to read from files or stdin

You can use `@` to read system prompts from a file.

``` { .bash .cli-command title="Input prompt from a file" }
echo "You are a haiku generator. Always respond with haikus." > haiku.txt
chatx --system-prompt @haiku.txt --question "Tell me about programming"
```

``` { .plaintext .cli-output }
Languages converse
Machine and human bridged
Creation from thought
```

``` { .bash .cli-command title="Use stdin to read system prompts" }
echo "Only answer in haikus" | chatx --system-prompt @- --question "Tell me about C++"
```

``` { .plaintext .cli-output }
Language of power  
Objects dance in memory
C plus plus whispers
```

``` { .bash .cli-command title="Augment system prompt with multiple files" }
echo "You generate code snippets. Never explain." > code.txt
echo "Only use Python." > python.txt
chatx --add-system-prompt @code.txt @python.txt --question "Hello world"
```

```` { .plaintext .cli-output }
```python
print("Hello, world!")
```
````

## Use variables in system prompts

``` { .bash .cli-command title="Use variables in system prompts" }
echo "We only program in {language}." > explain.txt
echo "If not specified, language is Python" >> explain.txt
echo "If possible, answer question on single line of code." >> explain.txt
echo "Only speak in {language}." >> explain.txt
chatx --add-system-prompt @explain.txt --question "How to delete file by name?" --var language=C#
```

```` { .plaintext .cli-output }
In C#, you can delete a file by name using the `File.Delete()` method from the System.IO namespace. Here's the code:

```csharp
System.IO.File.Delete("path/to/your/file.txt");
```

Just replace "path/to/your/file.txt" with the actual path to the file you want to delete. This method will throw an exception if the file doesn't exist or if the process doesn't have sufficient permissions, so you might want to add exception handling in a real application.
````

``` { .bash .cli-command title="Create aliases for easier use" }
chatx --var language=C# --add-system-prompt @explain.txt --save-alias cs
chatx --var language=C++ --add-system-prompt @explain.txt --save-alias cpp
chatx --var language=Java --add-system-prompt @explain.txt --save-alias java
chatx --var language=JavaScript --add-system-prompt @explain.txt --save-alias js
chatx --var language=Go --add-system-prompt @explain.txt --save-alias go
chatx --var language=Python --add-system-prompt @explain.txt --save-alias py
chatx --var language=Ruby --add-system-prompt @explain.txt --save-alias rb
chatx --var language=Rust --add-system-prompt @explain.txt --save-alias rs
```

``` { .bash .cli-command title="Use the aliases" }
chatx --input "Show smallest hello world possible" --cs
chatx --input "Show smallest hello world possible" --cpp
chatx --input "Show smallest hello world possible" --java
chatx --input "Show smallest hello world possible" --js
chatx --input "Show smallest hello world possible" --go
chatx --input "Show smallest hello world possible" --py
chatx --input "Show smallest hello world possible" --rb
chatx --input "Show smallest hello world possible" --rs
```

## See Also

- [--system-prompt](../reference/cli/options/system-prompt.md): Reference for replacing the system prompt
- [--add-system-prompt](../reference/cli/options/add-system-prompt.md): Reference for adding to the system prompt
- [Creating User Aliases](../advanced/creating-user-aliases.md): How to save your system prompts for reuse
- [Configuration](configuration.md): How to configure ChatX's behavior