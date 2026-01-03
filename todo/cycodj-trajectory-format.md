# Investigate Trajectory Format Support for cycodj export

## Context
User noticed that `cycodj export` creates a different format than "trajectory format" and questioned why we did that.

## Questions to Investigate
1. What is "trajectory format"? 
   - Is this a format used by cycod already?
   - Is it a format used for AI training/fine-tuning?
   - Where is it documented/implemented?

2. Should `cycodj export` support trajectory format?
   - As default format?
   - As an option like `--format trajectory` or `--format markdown`?

3. What are the differences between the current export format and trajectory format?
   - Current export: Markdown with emojis, blockquotes, table of contents
   - Trajectory format: ???

## Related Code
- `src/cycodj/CommandLineCommands/ExportCommand.cs`

## Next Steps
1. Find/document trajectory format specification
2. Compare with current export format
3. Decide if we should:
   - Replace current format
   - Add as an option
   - Keep both
4. Implement if needed

## Notes
- Current export format is very readable markdown with nice formatting
- If trajectory format is for machine consumption (training data), we might want both
