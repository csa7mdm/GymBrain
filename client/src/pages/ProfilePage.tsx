import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { generateNutritionPlan, getProfile, saveProfile, type ProfileData } from '../services/api';

const GOALS = [
  { id: 'muscle', icon: '💪', label: 'Build Muscle' },
  { id: 'fat-loss', icon: '🔥', label: 'Lose Fat' },
  { id: 'strength', icon: '🏋️', label: 'Get Strong' },
  { id: 'endurance', icon: '🏃', label: 'Endurance' },
  { id: 'health', icon: '❤️', label: 'Stay Healthy' },
];

const LEVELS = ['Beginner', 'Intermediate', 'Advanced', 'Athlete'];
const EQUIPMENT = ['Barbell', 'Dumbbells', 'Cables', 'Machines', 'Kettlebells', 'Bands', 'Bodyweight', 'Pull-up Bar'];
const FOCUS_AREAS = ['Chest', 'Back', 'Shoulders', 'Arms', 'Legs', 'Core', 'Glutes', 'Full Body'];

export default function ProfilePage() {
  const { user, logout } = useAuth();

  const [name, setName] = useState('');
  const [age, setAge] = useState(28);
  const [height, setHeight] = useState(175);
  const [weight, setWeight] = useState(78);
  const [goal, setGoal] = useState('muscle');
  const [level, setLevel] = useState('Intermediate');
  const [daysPerWeek, setDaysPerWeek] = useState(4);
  const [equipment, setEquipment] = useState<string[]>(['Dumbbells', 'Bodyweight']);
  const [focusAreas, setFocusAreas] = useState<string[]>(['Full Body']);
  const [diet, setDiet] = useState('Standard');
  const [calories, setCalories] = useState(2500);
  const [saved, setSaved] = useState(false);
  const [loadingProfile, setLoadingProfile] = useState(true);

  useEffect(() => {
    const fetchProfile = async () => {
      const res = await getProfile();
      if (res.data) {
        setGoal(res.data.goal);
        setDaysPerWeek(res.data.daysPerWeek);
        setDiet(res.data.dietaryPreference);
        setCalories(res.data.dailyCalories);
        setInjuries(res.data.injuries || 'None');
        try {
          const eq = JSON.parse(res.data.equipmentJson || '[]');
          setEquipment(eq);
        } catch {
          setEquipment(['Bodyweight']);
        }
      }

      // Load other non-persisted fields from localstorage for now
      const p = JSON.parse(localStorage.getItem('gymbrain_profile') || '{}');
      if (p.name) setName(p.name);
      if (p.age) setAge(p.age);
      if (p.height) setHeight(p.height);
      if (p.weight) setWeight(p.weight);
      if (p.level) setLevel(p.level);
      if (p.focusAreas) setFocusAreas(p.focusAreas);

      setLoadingProfile(false);
    };
    fetchProfile();
  }, []);

  const [injuries, setInjuries] = useState('None');

  const handleSave = async () => {
    const profile: ProfileData = {
      goal,
      daysPerWeek,
      dietaryPreference: diet,
      dailyCalories: calories,
      injuries,
      equipmentJson: JSON.stringify(equipment)
    };

    const res = await saveProfile(profile);
    if (!res.error) {
      setSaved(true);
      setTimeout(() => setSaved(false), 2000);
    } else {
      alert('Failed to save profile to server: ' + res.error);
    }

    // Save metadata to localstorage
    const existing = JSON.parse(localStorage.getItem('gymbrain_profile') || '{}');
    const updated = { ...existing, name, age, height, weight, goal, level, daysPerWeek, equipment, focusAreas, diet, calories };
    localStorage.setItem('gymbrain_profile', JSON.stringify(updated));
  };

  const [generatingNutrition, setGeneratingNutrition] = useState(false);
  const [nutritionError, setNutritionError] = useState('');
  const [nutritionPlan, setNutritionPlan] = useState<any>(null);

  const handleGenerateNutrition = async () => {
    setGeneratingNutrition(true);
    setNutritionError('');
    setNutritionPlan(null);
    try {
      const res = await generateNutritionPlan(diet, calories, goal);
      if (res.error) {
        setNutritionError(res.error);
      } else if (res.data?.payloadJson) {
        let raw = res.data.payloadJson;
        if (raw.startsWith('```')) {
          raw = raw.replace(/^```(?:json)?\s*\n?/, '').replace(/\n?```\s*$/, '');
        }
        setNutritionPlan(JSON.parse(raw));
      }
    } catch (e) {
      setNutritionError('Failed to generate nutrition plan.');
    }
    setGeneratingNutrition(false);
  };

  const toggleChip = (list: string[], item: string, setter: (v: string[]) => void) => {
    setter(list.includes(item) ? list.filter(x => x !== item) : [...list, item]);
  };

  const bmi = height && weight ? (weight / ((height / 100) ** 2)).toFixed(1) : null;
  const initials = name ? name.charAt(0).toUpperCase() : (user?.email?.charAt(0).toUpperCase() || '?');

  if (loadingProfile) {
    return (
      <div className="app-content flex-center" style={{ height: '100vh' }}>
        <div className="m3-loader"></div>
        <p className="md-label-lg mt-md">Loading Profile...</p>
      </div>
    );
  }

  return (
    <div className="app-content fade-in">
      {/* Header */}
      <div className="profile-header">
        <div className="profile-avatar">{initials}</div>
        <div className="profile-header__info">
          <div className="profile-header__name">{name || 'Your Profile'}</div>
          <div className="profile-header__email">{user?.email}</div>
        </div>
      </div>

      {/* Body Data */}
      <div className="profile-section">
        <div className="profile-section__title">📏 Body Data</div>
        <div className="m3-card">
          <div className="m3-field">
            <label className="m3-field__label" htmlFor="p-name">Name</label>
            <input id="p-name" className="m3-input" value={name} onChange={e => setName(e.target.value)} />
          </div>
          <div className="m3-slider-group">
            <div className="m3-slider-group__header">
              <span className="m3-slider-group__label">Age</span>
              <span className="m3-slider-group__value">{age} years</span>
            </div>
            <input type="range" className="m3-slider" min={14} max={80} value={age} onChange={e => setAge(+e.target.value)} />
          </div>
          <div className="m3-slider-group">
            <div className="m3-slider-group__header">
              <span className="m3-slider-group__label">Height</span>
              <span className="m3-slider-group__value">{height} cm</span>
            </div>
            <input type="range" className="m3-slider" min={140} max={220} value={height} onChange={e => setHeight(+e.target.value)} />
          </div>
          <div className="m3-slider-group">
            <div className="m3-slider-group__header">
              <span className="m3-slider-group__label">Weight</span>
              <span className="m3-slider-group__value">{weight} kg</span>
            </div>
            <input type="range" className="m3-slider" min={35} max={200} value={weight} onChange={e => setWeight(+e.target.value)} />
          </div>
          {bmi && (
            <div style={{ display: 'flex', justifyContent: 'center', gap: 8, marginTop: 4 }}>
              <span className="chip chip-info">BMI: {bmi}</span>
              <span className="chip chip-success">
                {+bmi < 18.5 ? 'Underweight' : +bmi < 25 ? 'Normal' : +bmi < 30 ? 'Overweight' : 'Obese'}
              </span>
            </div>
          )}
        </div>
      </div>

      {/* Goals */}
      <div className="profile-section">
        <div className="profile-section__title">🎯 Goals & Level</div>
        <div className="m3-card">
          <div className="md-label-md text-muted mb-sm">Primary Goal</div>
          <div className="m3-chips mb-md">
            {GOALS.map(g => (
              <button key={g.id} className={`m3-chip ${goal === g.id ? 'm3-chip--selected' : ''}`}
                onClick={() => setGoal(g.id)}>
                {g.icon} {g.label}
              </button>
            ))}
          </div>
          <div className="md-label-md text-muted mb-sm">Fitness Level</div>
          <div className="m3-segmented">
            {LEVELS.map(l => (
              <button key={l} className={`m3-segmented__btn ${level === l ? 'm3-segmented__btn--active' : ''}`}
                onClick={() => setLevel(l)}>
                {l}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Training Preferences */}
      <div className="profile-section">
        <div className="profile-section__title">🏋️ Training Preferences</div>
        <div className="m3-card">
          <div className="md-label-md text-muted mb-sm">Days per Week</div>
          <div className="m3-segmented mb-md">
            {[2, 3, 4, 5, 6].map(d => (
              <button key={d} className={`m3-segmented__btn ${daysPerWeek === d ? 'm3-segmented__btn--active' : ''}`}
                onClick={() => setDaysPerWeek(d)}>
                {d}x
              </button>
            ))}
          </div>
          <div className="md-label-md text-muted mb-sm">Equipment</div>
          <div className="m3-chips mb-md">
            {EQUIPMENT.map(eq => (
              <button key={eq} className={`m3-chip ${equipment.includes(eq) ? 'm3-chip--selected' : ''}`}
                onClick={() => toggleChip(equipment, eq, setEquipment)}>
                {eq}
              </button>
            ))}
          </div>
          <div className="md-label-md text-muted mb-sm">Focus Areas</div>
          <div className="m3-chips">
            {FOCUS_AREAS.map(fa => (
              <button key={fa} className={`m3-chip ${focusAreas.includes(fa) ? 'm3-chip--selected' : ''}`}
                onClick={() => toggleChip(focusAreas, fa, setFocusAreas)}>
                {fa}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Nutrition */}
      <div className="profile-section">
        <div className="profile-section__title">🍎 Nutrition & Macros</div>
        <div className="m3-card">
          <div className="md-label-md text-muted mb-sm">Dietary Preference</div>
          <div className="m3-chips mb-md">
            {['Standard', 'Keto', 'Vegan', 'Vegetarian', 'Paleo'].map(d => (
              <button key={d} className={`m3-chip ${diet === d ? 'm3-chip--selected' : ''}`}
                onClick={() => setDiet(d)}>
                {d}
              </button>
            ))}
          </div>
          <div className="m3-slider-group mb-md">
            <div className="m3-slider-group__header">
              <span className="m3-slider-group__label">Daily Calories Target</span>
              <span className="m3-slider-group__value">{calories} kcal</span>
            </div>
            <input type="range" className="m3-slider" min={1200} max={4000} step={50} value={calories} onChange={e => setCalories(+e.target.value)} />
          </div>

          <button className="m3-btn m3-btn--outlined m3-btn--full" onClick={handleGenerateNutrition} disabled={generatingNutrition}>
            {generatingNutrition ? 'Generating...' : '✨ Generate AI Meal Plan'}
          </button>

          {nutritionError && <div className="m3-error-banner mt-sm">{nutritionError}</div>}

          {nutritionPlan && (
            <div className="mt-md p-md" style={{ background: 'var(--md-surface-variant)', borderRadius: 12 }}>
              <div className="md-body-md mb-md"><em>"{nutritionPlan.message_from_coach}"</em></div>
              {nutritionPlan.meals?.map((m: any, idx: number) => (
                <div key={idx} style={{ borderBottom: '1px solid rgba(255,255,255,0.1)', paddingBottom: 8, marginBottom: 8 }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <strong>{m.type}: {m.name}</strong>
                    <span className="text-muted">{m.calories} kcal</span>
                  </div>
                  <div className="md-body-sm text-muted">{m.description}</div>
                  <div style={{ display: 'flex', gap: 8, marginTop: 4 }}>
                    <span className="chip chip-info" style={{ fontSize: '0.7rem' }}>P: {m.protein_g}g</span>
                    <span className="chip chip-warning" style={{ fontSize: '0.7rem' }}>C: {m.carbs_g}g</span>
                    <span className="chip chip-success" style={{ fontSize: '0.7rem' }}>F: {m.fat_g}g</span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Save */}
      <button className="m3-btn m3-btn--filled m3-btn--full m3-btn--lg mt-md" onClick={handleSave}>
        {saved ? '✓ Saved!' : '💾 Save Profile'}
      </button>

      {/* Sign Out */}
      <hr className="m3-divider" />
      <button className="m3-btn m3-btn--text m3-btn--full" style={{ color: 'var(--md-error)' }} onClick={logout}>
        Sign Out
      </button>
    </div>
  );
}