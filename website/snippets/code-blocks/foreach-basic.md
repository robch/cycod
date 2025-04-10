```bash
# Basic foreach with multiple values
chatx --foreach var name in Alice Bob Charlie --input "Hello, {name}!"

# Numeric range
chatx --foreach var day in 1..7 --input "Day {day} of the week is..."

# Multiple foreach variables (creates combinations)
chatx --foreach var language in Python JavaScript Go --foreach var topic in functions loops --input "How to use {topic} in {language}?"

# Parallel processing with threads
chatx --threads 4 --foreach var city in London Paris Rome Berlin Tokyo --input "What's the weather in {city}?"

# Values from a file
chatx --foreach var customer in @customers.txt --input "Dear {customer}, thank you for your purchase."
```