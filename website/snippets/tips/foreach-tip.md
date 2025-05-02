!!! tip "Power Tip: Combinations with --foreach"
    When you use multiple `--foreach` options, CycoD creates all possible combinations. For example:
    ```bash
    cycod --foreach var color in red blue --foreach var size in small large --input "{color} {size} shirt"
    ```
    This will run 4 commands with all combinations: red small, red large, blue small, blue large.