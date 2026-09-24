# Optional meal planning settings

All new settings appear under More options. Leaving them blank requests one day with no daily budget, preferred items or extra restrictions. Users can select one to seven days, enter a daily budget with currency, list up to 20 preferred ingredients, and enter up to 500 characters of restrictions. The prompt gives restrictions priority over preferences and treats the budget as a per-person, per-day target. Prices are not checked live.

The server saves the supplied settings with the generated payload, independent of any model-reported settings. Meals restores them from the latest saved plan. Multi-day plans have a day selector. Incomplete or malformed output cannot replace the latest valid plan.

## Cooking-step decision

Cooking steps remain concise and included in the initial generation. Opening a saved recipe therefore needs no new provider call. Generating steps only on demand could reduce initial output for recipes never opened, but each first opening would add latency, failure risk, and prompt/context tokens. Hiding pre-generated steps alone would not reduce token usage. A later on-demand mode should save each successful recipe expansion, offer retry without losing the meal, and avoid charging repeated opens. Actual token/latency measurements should decide whether to introduce it.

The previous verbose multi-day schema has been replaced with the compact recipe schema. Longer plans get a bounded larger output allowance; they still depend on provider availability and output quality. Automated checks use controlled responses, not paid provider requests.
