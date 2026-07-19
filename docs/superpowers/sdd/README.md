# Nostrivia SDD scratch — surfaced artifacts

These files were the working artifacts of the subagent-driven build of the Nostrivia UI. They
originally lived in a hidden, untracked `.superpowers/sdd/` directory (invisible to git and most
file explorers); the reusable ones are surfaced here so the next person/agent can find them
alongside [the retrospective](../retrospective-nostrivia-ui.md).

| File | What it is |
|---|---|
| [`visual-build-playbook.md`](visual-build-playbook.md) | The brief every visual-build subagent read first — the "one true way" to build a prefab/screen via `Unity_RunCommand`, the RunCommand foot-guns, the capture→compare loop, and the don't-clobber-the-scene rules. Start here. |
| [`capture-helper.txt`](capture-helper.txt) | The portrait UI capture rig — a self-contained `Unity_RunCommand` C# snippet that transiently flips the Overlay canvas to a camera + RenderTexture, renders `Temp/uicap.png` (540×960), and restores Overlay. Paste verbatim. |
| [`progress.md`](progress.md) | The durable build ledger — per-task status, decisions, and gotchas as they were discovered. Survived an auth interruption and a context compaction; was the source of truth when memory wasn't. |

The distilled lessons are in [`../retrospective-nostrivia-ui.md`](../retrospective-nostrivia-ui.md).

**Not surfaced** (still local-only scratch, if it still exists on the build machine): the per-task
`task-*-brief.md` / `task-*-report.md` pairs and the review `*.diff` snapshots — one-off process
ephemera superseded by `progress.md` + `git log`.
