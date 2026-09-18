import { useState } from 'react';
import './MealCards.css';

import type { MealPlan } from '../services/mealPlan';

export default function MealCards({ plan }: { plan: MealPlan }) {
  const [index, setIndex] = useState(0);
  const meal = plan.meals[index];
  return <section className="meal-plan" aria-label="Your meal plan">
    {plan.message && <p className="md-body-md">{plan.message}</p>}
    <div className="meal-nav"><button type="button" className="m3-btn m3-btn--outlined" disabled={index === 0} onClick={() => setIndex(index - 1)}>Previous meal</button>
      <span role="status">{index + 1} / {plan.meals.length}</span>
      <button type="button" className="m3-btn m3-btn--outlined" disabled={index === plan.meals.length - 1} onClick={() => setIndex(index + 1)}>Next meal</button></div>
    <article className="meal-card" aria-labelledby="meal-title" aria-live="polite">
      <div className="meal-card__eyebrow">Day {meal.day} · {meal.type || 'Meal'}</div>
      <h3 id="meal-title">{meal.name}</h3><p>{meal.description}</p>
      <div className="meal-facts"><span>{meal.servings ?? '—'} servings</span><span>Prep {meal.prep_minutes ?? '—'} min</span><span>Cook {meal.cook_minutes ?? '—'} min</span></div>
      <div className="meal-facts"><span>{meal.calories ?? '—'} kcal</span><span>Protein {meal.protein_g ?? '—'} g</span><span>Carbs {meal.carbs_g ?? '—'} g</span><span>Fat {meal.fat_g ?? '—'} g</span></div>
      <p className="md-body-sm text-muted">Nutrition estimates per serving.</p>
      <h4>Ingredients</h4>{meal.ingredients.length ? <ul className="meal-ingredients">{meal.ingredients.map((item, i) => <li key={i}><span>{item.name}</span><strong>{item.quantity || 'Quantity not supplied'}</strong></li>)}</ul> : <p>Ingredients were not included in this plan. Generate a new plan for recipe details.</p>}
      <h4>How to cook</h4>{meal.steps.length ? <ol className="meal-steps">{meal.steps.map((step, i) => <li key={i}>{step}</li>)}</ol> : <p>Cooking instructions were not included in this plan.</p>}
    </article>
    <p className="md-body-sm text-muted">Check ingredients for your allergies and dietary needs. This generated plan stays on this screen; leaving it clears the plan.</p>
  </section>;
}
