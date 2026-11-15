# Prompt: Find One Easy, Valid Issue in TalebElm

You are acting as a careful junior-friendly software engineering contributor.

## Goal

Inspect the **TalebElm** project and propose **exactly one small, beginner-friendly GitHub issue** that would be useful to the project.

The issue should be genuinely easy to understand and implement. Prefer a small bug fix, a missing validation, a minor error-handling improvement, a small test addition, or a focused documentation correction.

## Step 1: Understand the project first

Before suggesting anything:

1. Read the repository structure and the main project documentation.
2. Read the relevant architecture and contribution guidelines, especially `ARCHITECTURE.md` and any technical architecture reference.
3. Inspect the relevant source code and tests. Follow the existing implementation patterns.
4. Understand how the affected feature currently works. Do not suggest work that is already implemented.
5. Check the repository's existing GitHub issues and pull requests, if GitHub access is available. Avoid anything that already has an issue or an open/merged PR.

Do not start changing code. This task is only to investigate and write an issue proposal.

## Step 2: Choose a suitable issue

Choose only one issue that meets all these requirements:

- Small and realistically achievable by a beginner.
- Has a clear, limited scope.
- Can be explained in a few sentences.
- Does not require a major refactor, new architecture, or a new feature spanning multiple layers.
- Fits the project's existing architecture and coding conventions.
- Is not already implemented.
- Is not already covered by an existing issue or pull request, based on the checks available to you.
- Has a clear way to verify the solution, preferably with a focused test.

Prefer an issue that can be completed in approximately **1–3 hours** by someone familiar with the codebase.

Do not choose an issue just because it is easy to describe. Confirm the problem exists in the actual code.

## Step 3: Verify the finding

Before writing the issue:

- Identify the exact file(s) and relevant class, method, or endpoint.
- Explain the current behavior and why it is a problem.
- Describe the smallest reasonable expected change.
- Mention any relevant existing pattern that the implementation should follow.
- If you cannot verify that the problem exists, discard it and investigate another candidate.
- If you cannot check GitHub issues or pull requests, clearly state that limitation. Do not claim that the issue is definitely unreported.

## Step 4: Output

Create **one Markdown file** named:

`easy-issue-proposal.md`

Use this exact structure:

# [Short, clear issue title]

## Summary
Explain the problem and why it matters in 2–4 sentences.

## Current Behavior
Describe what the code currently does. Include the relevant file path and class/method name.

## Expected Behavior
Describe the desired behavior clearly.

## Proposed Scope
List the minimal changes needed. Do not provide a full implementation.

## Acceptance Criteria
- [ ] A specific, observable requirement is satisfied.
- [ ] Relevant tests are added or updated, where appropriate.
- [ ] Existing behavior outside the issue's scope remains unchanged.

## Verification
Explain how a contributor can verify the fix, including the relevant test command if known.

## Repository Evidence
- Relevant files:
- Relevant classes/methods:
- Existing project convention to follow:

## Difficulty
Beginner

## Estimated Effort
Give a realistic estimate.

## Duplicate Check
State what issues and pull requests you checked and whether you found a possible duplicate. If GitHub access was unavailable, say so explicitly.

## Important rules

- Output exactly **one** issue proposal, not a list of ideas.
- Do not create the issue on GitHub.
- Do not modify source code, tests, project files, or configuration.
- Do not invent files, endpoints, classes, bugs, or repository behavior.
- Do not suggest a broad feature or vague improvement.
- Keep the language simple, direct, and suitable for a beginner contributor.
- If you cannot find a valid issue that meets these requirements, do not fabricate one. Instead, write that no suitable issue could be verified and explain what prevented verification.
