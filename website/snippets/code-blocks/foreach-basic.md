```bash
# Basic foreach with multiple values
cycod --foreach var name in Alice Bob Charlie --input "Hello, {name}!"

# Numeric range
cycod --foreach var day in 1..7 --input "Day {day} of the week is..."

# Multiple foreach variables (creates combinations)
cycod --foreach var language in Python JavaScript Go --foreach var topic in functions loops --input "How to use {topic} in {language}?"

# Parallel processing with threads
cycod --threads 4 --foreach var city in London Paris Rome Berlin Tokyo --input "What's the weather in {city}?"

# Values from a file
cycod --foreach var customer in @customers.txt --input "Dear {customer}, thank you for your purchase."
```