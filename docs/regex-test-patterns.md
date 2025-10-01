# Regex Pattern Test File

This file contains content specifically designed to match the 5 regex patterns that didn't find matches in our previous search.

## Pattern: `\w+\.md`
Here are some references to markdown files:
- README.md is the main documentation file
- function-calling.md contains information about functions
- installation.md has setup instructions
- troubleshooting.md helps solve common problems

## Pattern: `\{\{.*\}\}`
Template expressions:
- {{variable}}
- {{set theme = "dark"}}
- {{for item in items}}
- {{if condition}}{{else}}{{endif}}
- {{calculate sqrt(16)}}

## Pattern: `\[\^.*?\]`
Footnote references in the text[^1] are useful for adding additional context[^note] without interrupting the flow.

[^1]: This is a numbered footnote.
[^note]: This is a named footnote.

## Pattern: `\[[a-zA-Z\s]+\]`
Square bracketed text:
- [See more information]
- [External Link]
- [Click here]
- [Learn about regex patterns]
- [Optional section]

## Pattern: `\w+='.*?'`
Single-quoted assignments:
- config='default'
- theme='dark'
- font='Consolas'
- path='/usr/local/bin'
- mode='production'