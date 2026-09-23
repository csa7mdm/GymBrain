# GymBrain pilot UX revamp

Updated 2026-09-23. The goal is a clear mobile experience that preserves work and makes the next action obvious. These are usability and cognitive-accessibility choices; no neurological benefit is claimed without user research.

## Screen map

| Destination | Primary job | What belongs there |
|---|---|---|
| Home | Resume | Start training, open the latest saved meal plan, recent activity |
| Train | Complete a session | Current exercise and set, progress, rest, substitutions on request |
| Meals | Plan and cook | Latest saved plan, one meal card at a time, generate another plan |
| History | Review | Saved workout results and previous sessions |
| Profile | Change settings | Personal details, training and dietary preferences, AI connection |

The bottom navigation uses the five frequent destinations. AI connection moves from the persistent bar into Profile because setup is occasional; generated meals get the prominent destination users need to return to.

## Core journeys and states

1. Generate a meal plan. The app shows progress, validates the response, stores the latest valid plan under the signed-in account, and confirms where to find it. Meals and Home restore it after navigation or reload. Empty, loading, provider failure and storage failure are distinct states. A plan from another account never appears.
2. Start a workout. The screen shows session progress and a focused exercise with readable sets. Warm-up and upcoming exercises remain scannable. Equipment substitution is available only when needed. Completion results remain editable and save through the existing server flow.
3. Change settings. Profile holds persistent preferences and clear links to Meals and AI connection. Leaving either destination does not silently erase saved work.

## Design rules

- One visually dominant action per view; secondary actions stay available without repeating on every card. This applies W3C guidance on reducing information overload and making important tasks easy to find: https://www.w3.org/WAI/people-use-web/tools-techniques/presentation/ and https://www.w3.org/WAI/WCAG2/supplemental/objectives/o2-find/.
- Use plain labels, visible saved state, and a predictable location for plans. W3C's cognitive patterns call for clear page purpose and preventing data loss: https://www.w3.org/WAI/WCAG2/supplemental/patterns/o1p01-clear-purpose/ and https://www.w3.org/WAI/WCAG2/supplemental/patterns/o4p09-data-loss/.
- Make controls at least 44-48 CSS pixels high as a practical touch target. WCAG 2.2 AA sets a 24 CSS pixel minimum with exceptions; Android recommends 48 dp: https://www.w3.org/WAI/WCAG22/Understanding/target-size-minimum and https://developer.android.com/develop/ui/compose/accessibility/api-defaults.
- Keep normal text legible at narrow widths and 200% text scaling, preserve keyboard focus and status announcements, and respect reduced motion: https://www.w3.org/WAI/WCAG21/Understanding/resize-text, https://www.w3.org/WAI/WCAG21/Understanding/reflow, https://www.w3.org/WAI/WCAG21/Understanding/status-messages, and https://www.w3.org/WAI/WCAG22/Techniques/css/C39.

## Release acceptance

- A generated meal plan survives tab change, page reload and fresh sign-in; the latest plan appears from Home and Meals. Saving a new plan replaces the previous latest plan only after the new one validates and saves.
- A different signed-in account receives no plan from the first account; an unauthenticated request is rejected.
- Workout summary, set inputs and substitutions remain usable at 320 px, 390 px and desktop widths. No clipped text or horizontal scroll at 200% zoom. Primary touch controls meet the practical target above.
- Complete generation, substitution, set logging, save retry, and history flows still pass automated browser tests. Backend ownership and malformed-output tests pass. CI and deployed revision are recorded in the launch checklist.
- A real signed-in pilot user confirms meal restoration and workout completion in production before the release is called verified end to end.
