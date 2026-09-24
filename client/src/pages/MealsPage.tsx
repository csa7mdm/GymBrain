import { useEffect, useRef, useState } from 'react';
import MealCards from '../components/MealCards';
import { generateNutritionPlan, getLatestMealPlan, getProfile } from '../services/api';
import { parseMealPlan, type MealPlan } from '../services/mealPlan';
import './MealsPage.css';

export default function MealsPage() {
  const pageView = useRef<HTMLElement>(null);
  const [plan, setPlan] = useState<MealPlan | null>(null);
  const [generatedAt, setGeneratedAt] = useState<string | null>(null);
  const [diet, setDiet] = useState('Standard');
  const [calories, setCalories] = useState(2500);
  const [goal, setGoal] = useState('health');
  const [days, setDays] = useState('');
  const [dailyBudget, setDailyBudget] = useState('');
  const [currency, setCurrency] = useState('EGP');
  const [preferredItems, setPreferredItems] = useState('');
  const [restrictions, setRestrictions] = useState('');
  const [loading, setLoading] = useState(true);
  const [generating, setGenerating] = useState(false);
  const [error, setError] = useState('');
  const [loadFailed, setLoadFailed] = useState(false);
  const [preferencesReady, setPreferencesReady] = useState(false);
  const [retry, setRetry] = useState(0);

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      setLoading(true);
      setError('');
      setLoadFailed(false);
      const [saved, profile] = await Promise.all([getLatestMealPlan(), getProfile()]);
      if (cancelled) return;
      if (profile.data) {
        setDiet(profile.data.dietaryPreference || 'Standard');
        setCalories(profile.data.dailyCalories || 2500);
        setGoal(profile.data.goal || 'health');
        setPreferencesReady(true);
      }
      if (saved.error) { setLoadFailed(true); setError(`Could not load your saved meal plan. ${saved.error}`); }
      else if (saved.data) {
        try {
          const restored = parseMealPlan(saved.data.payloadJson);
          setPlan(restored);
          if (restored.options) {
            setDays(String(restored.options.durationDays ?? ''));
            setDailyBudget(String(restored.options.dailyBudget ?? ''));
            setCurrency(restored.options.currencyCode || 'EGP');
            setPreferredItems(restored.options.preferredItems?.join(', ') || '');
            setRestrictions(restored.options.restrictions || '');
          }
          setGeneratedAt(saved.data.generatedAtUtc);
        } catch {
          setLoadFailed(true);
          setError('Your saved meal plan could not be displayed. Please generate a new one.');
        }
      }
      if (profile.error || !profile.data) {
        setPreferencesReady(false);
        setLoadFailed(true);
        setError('Could not load your meal preferences. Retry loading before generating a plan.');
      }
      setLoading(false);
    };
    void load();
    return () => { cancelled = true; };
  }, [retry]);

  const generate = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const items = preferredItems.split(/[,،\n]+/).map(item => item.trim()).filter(Boolean);
    if (items.length > 20 || items.some(item => item.length > 80)) {
      setError('Use up to 20 preferred items, each at most 80 characters.');
      pageView.current?.scrollTo({ top: 0, behavior: 'instant' });
      return;
    }
    setGenerating(true);
    setError('');
    setLoadFailed(false);
    try {
      const result = await generateNutritionPlan(diet, calories, goal, {
        ...(days ? { durationDays: Number(days) } : {}),
        ...(dailyBudget ? { dailyBudget: Number(dailyBudget), currencyCode: currency } : {}),
        ...(items.length ? { preferredItems: items } : {}),
        ...(restrictions.trim() ? { restrictions: restrictions.trim() } : {}),
      });
      if (result.error) setError(result.error);
      else if (result.data?.payloadJson) {
        setPlan(parseMealPlan(result.data.payloadJson));
        setGeneratedAt(new Date().toISOString());
      } else setError('The model returned an empty meal plan. Try another model in Vault.');
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'Could not display your meal plan.');
    } finally {
      setGenerating(false);
      requestAnimationFrame(() => pageView.current?.scrollTo({ top: 0, behavior: 'instant' }));
    }
  };

  return <main className="app-content fade-in meals-page" ref={pageView}>
    <header className="meals-page__header">
      <span className="meals-page__eyebrow">YOUR NUTRITION</span>
      <h1>Meals</h1>
      <p>Find your latest plan here whenever you return.</p>
    </header>

    {loading ? <div className="m3-card meals-page__status" role="status">Loading your saved meal plan…</div> : <>
      {error && <div className="m3-error-banner" role="alert">{error} {loadFailed && <button type="button" className="m3-btn m3-btn--text" onClick={() => setRetry(value => value + 1)}>Retry loading</button>}</div>}
      {plan ? <section className="meals-page__saved" aria-label="Saved meal plan">
        <div className="meals-page__saved-header">
          <div><span className="meals-page__eyebrow">SAVED TO YOUR ACCOUNT</span><h2>Your latest plan</h2></div>
          {generatedAt && <time dateTime={generatedAt}>Created {new Date(generatedAt).toLocaleDateString()}</time>}
        </div>
        <MealCards key={generatedAt ?? 'saved'} plan={plan} />
      </section> : !loadFailed && <div className="m3-card meals-page__status"><h2>No meal plan yet</h2><p>Choose your preferences below to make your first plan. It will appear here when you return.</p></div>}

      <form className="m3-card meals-page__generate" aria-labelledby="meal-settings-title" onSubmit={generate} onInvalidCapture={event => event.currentTarget.querySelector('details')?.setAttribute('open', '')}>
        <h2 id="meal-settings-title">{plan ? 'Make a new plan' : 'Plan your meals'}</h2>
        <p className="text-muted">A new plan becomes your latest saved plan. These choices apply to this plan; change your defaults in Profile.</p>
        <label className="meals-page__field">Dietary preference
          <select value={diet} disabled={generating || !preferencesReady} onChange={event => setDiet(event.target.value)}>
            {['Standard', 'Keto', 'Vegan', 'Vegetarian', 'Paleo'].map(value => <option key={value}>{value}</option>)}
          </select>
        </label>
        <label className="meals-page__field">Daily calorie target <strong>{calories} kcal</strong>
          <input type="range" min={1200} max={4000} step={50} value={calories} disabled={generating || !preferencesReady} onChange={event => setCalories(Number(event.target.value))} />
        </label>
        <details className="meals-page__options">
          <summary>More options <span>All optional</span></summary>
          <label className="meals-page__field">Number of days
            <select aria-label="Number of days" value={days} disabled={generating || !preferencesReady} onChange={event => setDays(event.target.value)}>
              <option value="">Default · 1 day</option>
              {[1, 2, 3, 4, 5, 6, 7].map(day => <option key={day} value={day}>{day} {day === 1 ? 'day' : 'days'}</option>)}
            </select>
          </label>
          <div className="meals-page__budget">
            <label className="meals-page__field">Budget per day
              <input type="number" inputMode="decimal" min="0.01" max="1000000" step="0.01" placeholder="No budget set" value={dailyBudget}
                disabled={generating || !preferencesReady} onChange={event => setDailyBudget(event.target.value)} aria-describedby="budget-help" />
            </label>
            <label className="meals-page__field">Currency
              <select aria-label="Currency" value={currency} disabled={generating || !preferencesReady} onChange={event => setCurrency(event.target.value)}>
                {['EGP', 'USD', 'EUR', 'GBP', 'SAR', 'AED'].map(code => <option key={code}>{code}</option>)}
              </select>
            </label>
          </div>
          <p id="budget-help" className="meals-page__help">For one person, across all meals that day. A planning target; prices are not checked live.</p>
          <label className="meals-page__field">Preferred items
            <textarea aria-label="Preferred items" rows={3} maxLength={1600} placeholder="e.g. chicken, lentils, rice" value={preferredItems}
              disabled={generating || !preferencesReady} onChange={event => setPreferredItems(event.target.value)} aria-describedby="items-help" />
          </label>
          <p id="items-help" className="meals-page__help">Separate up to 20 foods or ingredients with commas or new lines.</p>
          <label className="meals-page__field">Restrictions
            <textarea aria-label="Restrictions" rows={3} maxLength={500} placeholder="e.g. no peanuts, no pork, dairy-free" value={restrictions}
              disabled={generating || !preferencesReady} onChange={event => setRestrictions(event.target.value)} aria-describedby="restrictions-help" />
          </label>
          <p id="restrictions-help" className="meals-page__help">Foods to exclude or other dietary limits. Restrictions take priority over preferred items. Check generated ingredients for your needs.</p>
          {days && Number(days) > 1 && <p className="meals-page__help">Longer plans use more tokens and may take longer to create.</p>}
        </details>
        <button type="submit" className="m3-btn m3-btn--filled m3-btn--full" disabled={generating || !preferencesReady}>
          {generating ? 'Creating your plan…' : plan ? 'Generate new meal plan' : 'Generate meal plan'}
        </button>
        {generating && <p role="status" className="text-muted">Your current plan remains available while the new one is created.</p>}
      </form>
    </>}
  </main>;
}
