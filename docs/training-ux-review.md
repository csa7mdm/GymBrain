# Training interaction review — 2026-09-25

## Decisions and checklist

- [x] Review exercise order, timer, set entry, swapping and saving in the current code.
- [x] Keep one exercise in focus; number the full routine and mark the current exercise explicitly.
- [x] Keep the selected exercise stable after its last set. Offer an explicit next exercise action.
- [x] Give the next incomplete set the main action treatment; completed sets have a clear Undo action.
- [x] Use an absolute deadline for rest so delayed browser ticks do not extend the timer. Persist deadline, paused remainder, origin set and selected exercise with the existing tab-local draft.
- [x] Add pause/resume, extend by 15 seconds, skip and a durable completed-rest state. Undo cancels the timer only for its originating set. Do not start rest after the final workout set.
- [x] Use a silent timer and screen-reader state announcements without announcing every second. No background alarm guarantee. Rest completion is information, not an instruction to exercise immediately.
- [x] Move animated demonstrations behind explicit form-tip disclosure; preserve concise prescriptions and optional instructions.
- [x] Verify replacement dialog keyboard behavior, error recovery, empty results and target size.
- [x] Verify countdown suspension, navigation/reload, undo and final-set behavior in desktop/mobile browser tests.
- [x] Inspect narrow layout, large text and visual hierarchy, then run lint/build/release checks.
- [ ] Deploy and verify the matching frontend artifact.
- [ ] Observe pilot users completing a session on actual phones and with assistive technology. Automation is not a full accessibility audit.

## Evidence and limits

- W3C [Help Users Focus](https://www.w3.org/WAI/WCAG2/supplemental/objectives/o5-user-focus/) supports reducing distractions and helping people resume interrupted work. Application here: stable exercise selection, no automatic page jump, retained timer state and optional animations.
- Nielsen Norman Group [Visibility of System Status](https://www.nngroup.com/articles/visibility-system-status/) supports immediate, understandable feedback. Application here: set count, timer states, explicit save feedback and separate request failures from empty alternatives.
- WCAG 2.2 [Target Size (Minimum)](https://www.w3.org/WAI/WCAG22/Understanding/target-size-minimum.html) specifies a 24 CSS pixel minimum with exceptions; this design uses at least 44px for primary training controls. This is a design target, not a claim that WCAG requires all controls to be 44px.
- Cai et al., 2025, [A scoping review of effects of acute exercise on executive function: evidence from event-related potentials](https://www.frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2025.1599861/full) synthesizes exercise/cognition studies. It does not compare workout application layouts or prove this interface improves cognitive or fitness outcomes. The concrete design decisions above follow usability and accessibility guidance and require user testing.

## Pilot tasks

Record a set one-handed; correct an accidental completion; pause and extend rest; navigate to Meals and return; lock/unlock the phone; finish an exercise without losing place; swap unavailable equipment with keyboard only; recover from failed save. Observe errors, hesitation, accidental taps, clarity of what is saved, and whether timer controls remain understandable at increased text size. Do not collect health data for this usability check.

## Local verification

2026-09-25: frontend lint/build passed. Full 46-test desktop/mobile suite passed, then six focused checks passed after the empty-workout and timer-focus review fixes (suite now contains 48 checks). Reviewed desktop and mobile screenshots, 320px layouts, 200% text reflow and modal keyboard controls. Fixtures intercept API calls; no live provider key or real account data was used. CI and deployment evidence is tracked in the main launch checklist.

