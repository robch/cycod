# Evidence Directory

This directory contains evidence and analysis from CI failures and investigations.

## Directory Structure

Each subdirectory represents a specific CI run or investigation:

### `ci-failure-20046335559/`

**CI Run**: https://github.com/robch/cycod/actions/runs/20046335559  
**Date**: December 8, 2025  
**PR**: #73  
**Issue**: TRX file has 385 test results but only 384 test definitions

**Start here**: Read `ci-failure-20046335559/ANALYSIS.md` for the complete investigation.

**Key Finding**: The missing test definition is for a **regular test**, not a matrix test. This indicates the matrix bug fix in PR #74 is working, but there's a separate bug in TRX generation.

## Purpose

This evidence directory serves as a "save cube" - preserving the exact state of CI artifacts and detailed analysis so we can:

1. **Reproduce issues** locally with the same data that failed in CI
2. **Reference precise findings** when debugging and fixing issues
3. **Verify fixes** by comparing new TRX output against the broken baseline
4. **Document investigation process** for future reference

## Usage

When investigating a TRX-related issue:

1. Download the TRX files from CI artifacts
2. Create a new subdirectory named after the CI run ID
3. Copy the TRX files and create an ANALYSIS.md documenting findings
4. Commit the evidence to the branch for preservation

This approach ensures we never lose track of what we discovered and can always return to the exact failing state.
