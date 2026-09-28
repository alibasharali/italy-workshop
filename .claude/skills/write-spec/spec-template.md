---
issue: <N>
title: <issue title>
status: draft
branch: feat/<N>-<slug>
---

# <title>

## Problem

<Two to four sentences: who has the problem, and what is true after this story ships.>

## Domain decisions

- **D1 – <name>:** <the rule>. *Why:* <reason, including the alternatives the developer rejected>.

## Acceptance criteria

- **AC1 – <name>:** Given <state>, when <action>, then <observable result, with concrete values>.

## Slices

Build in this order. Each slice is vertical and ends with green tests.

1. **S1 – <name>:** <behaviour end to end>. Satisfies AC1, AC2.

## Test seams

| AC | Seam | Prior art |
|---|---|---|
| AC1 | <domain unit / handler / API / Playwright> | <existing test to copy the style of> |

## Proof plan

How each AC is demonstrated against the running app (Playwright screenshot or API call) before the PR.

| AC | Proof |
|---|---|
| AC1 | <page + steps, or request + expected response> |

## Data and migrations

<New entities, fields and migrations, and what happens to existing rows. "None" is a valid answer.>

## Out of scope

- <What this story deliberately does not do>

## Open questions

<Empty before approval, or each item marked "deferred by <name>".>
