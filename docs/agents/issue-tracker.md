# Issue tracker: GitHub

Issues and specs live in GitHub Issues for `lfarci/seances`.
Use the `gh` CLI from this clone, or pass `--repo lfarci/seances`.

## Operations

- Create: `gh issue create --title "..." --body-file <file>`.
- Read: `gh issue view <number> --json number,title,body,labels,comments`.
- List: `gh issue list --state open --json number,title,body,labels`.
  Add label and state filters as needed.
- Comment: `gh issue comment <number> --body-file <file>`.
- Label: `gh issue edit <number> --add-label "..."`.
- Remove label: `gh issue edit <number> --remove-label "..."`.
- Close: `gh issue close <number> --comment "..."`.

For parent/child tickets, use GitHub sub-issues when available.
Otherwise, put `Part of #<parent>` at the top of each child body
and link children in a task list in the parent.

For blockers, use native GitHub issue dependencies when available.
Otherwise, record `Blocked by: #<number>` in the blocked issue body.

## Pull requests as a triage surface

**PRs as a request surface: no.**

## Skill terminology

“Publish to the issue tracker” means create a GitHub issue.
“Fetch the relevant ticket” means read its GitHub issue.
