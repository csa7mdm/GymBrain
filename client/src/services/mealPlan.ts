type Meal = { type: string; name: string; description: string; calories?: number; protein_g?: number; carbs_g?: number; fat_g?: number;
  servings?: number; prep_minutes?: number; cook_minutes?: number; ingredients: { name: string; quantity: string }[]; steps: string[]; day: number };
export interface MealPlan { message: string; meals: Meal[]; }
export function parseMealPlan(raw: string): MealPlan {
  const start = raw.indexOf('{');
  const end = raw.lastIndexOf('}');
  if (start < 0 || end <= start) throw new Error('The model did not return a complete meal plan. Try another model in Vault.');
  let data: Record<string, unknown>;
  try { data = JSON.parse(raw.slice(start, end + 1)); }
  catch { throw new Error('The model returned incomplete meal-plan JSON. Try another model in Vault.'); }
  const object = (value: unknown): Record<string, unknown> | null =>
    value !== null && typeof value === 'object' && !Array.isArray(value) ? value as Record<string, unknown> : null;
  if (!object(data)) throw new Error('The model returned no readable meals. Try another model in Vault.');
  const text = (value: unknown) => typeof value === 'string' ? value.trim() : '';
  const number = (value: unknown) => typeof value === 'number' && Number.isFinite(value) && value >= 0 ? value : undefined;
  const plan = object(data.meal_plan) || object(data.plan) || data;
  const groups = Array.isArray(plan.days) ? plan.days : [{ day_number: 1, meals: plan.meals }];
  const meals: Meal[] = [];
  for (const value of groups) {
    const day = object(value);
    if (!Array.isArray(day?.meals)) continue;
    for (const value of day.meals) {
      const meal = object(value);
      if (!meal) continue;
      const name = text(meal.name) || text(meal.title);
      if (!name) continue;
      const ingredients = Array.isArray(meal.ingredients) ? meal.ingredients.flatMap(value => {
        const ingredient = object(value);
        if (ingredient && text(ingredient.name)) return [{ name: text(ingredient.name), quantity: text(ingredient.quantity) || text(ingredient.amount) }];
        if (text(value)) return [{ name: text(value), quantity: '' }];
        return [];
      }) : [];
      const instructions = meal.steps ?? meal.instructions ?? meal.directions;
      const steps = Array.isArray(instructions) ? instructions.map(text).filter(Boolean)
        : text(instructions).split(/\n+/).map(s => s.replace(/^\d+[.)]\s*/, '').trim()).filter(Boolean);
      meals.push({ day: number(day.day_number) || number(day.day) || 1, name, type: text(meal.type) || text(meal.meal_type), description: text(meal.description),
        calories: number(meal.calories), protein_g: number(meal.protein_g), carbs_g: number(meal.carbs_g), fat_g: number(meal.fat_g),
        servings: number(meal.servings), prep_minutes: number(meal.prep_minutes), cook_minutes: number(meal.cook_minutes), ingredients, steps });
    }
  }
  if (!meals.length) throw new Error('The model returned no readable meals. Try another model in Vault.');
  return { message: text(plan.message_from_coach) || text(plan.message), meals };
}

