type Meal = { type: string; name: string; description: string; calories?: number; protein_g?: number; carbs_g?: number; fat_g?: number;
  servings?: number; prep_minutes?: number; cook_minutes?: number; ingredients: { name: string; quantity: string }[]; steps: string[]; day: number };
export interface MealPlan { message: string; meals: Meal[]; }
export function parseMealPlan(raw: string): MealPlan {
  const data = JSON.parse(raw.replace(/^```(?:json)?\s*/, '').replace(/\s*```$/, ''));
  const text = (value: unknown) => typeof value === 'string' ? value : '';
  const number = (value: unknown) => typeof value === 'number' && Number.isFinite(value) && value >= 0 ? value : undefined;
  const groups = Array.isArray(data?.days) ? data.days : [{ day_number: 1, meals: data?.meals }];
  const meals: Meal[] = [];
  for (const day of groups) {
    if (!Array.isArray(day?.meals)) continue;
    for (const meal of day.meals) {
      if (!meal || !text(meal.name)) continue;
      meals.push({ day: number(day.day_number) || 1, name: text(meal.name), type: text(meal.type), description: text(meal.description),
        calories: number(meal.calories), protein_g: number(meal.protein_g), carbs_g: number(meal.carbs_g), fat_g: number(meal.fat_g),
        servings: number(meal.servings), prep_minutes: number(meal.prep_minutes), cook_minutes: number(meal.cook_minutes),
        ingredients: Array.isArray(meal.ingredients) ? meal.ingredients.filter((i: unknown) => i && typeof i === 'object' && 'name' in i && typeof i.name === 'string').map((i: { name: string; quantity?: unknown }) => ({ name: i.name, quantity: text(i.quantity) })) : [],
        steps: Array.isArray(meal.steps) ? meal.steps.filter((s: unknown): s is string => typeof s === 'string' && !!s.trim()) : [],
      });
    }
  }
  if (!meals.length) throw new Error('The plan contained no readable meals. Please generate it again.');
  return { message: text(data.message_from_coach), meals };
}

