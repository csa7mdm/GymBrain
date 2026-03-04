import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';

interface UserProfile {
  name: string;
  age: number | '';
  height: number | '';
  weight: number | '';
  bodyFat: number | '';
  fitnessLevel: string;
  goal: string;
  daysPerWeek: number;
  equipment: string[];
  focusAreas: string[];
  dietaryRestrictions: string[];
  calorieGoal: number | '';
  proteinGoal: number | '';
}

const EQUIPMENT = ['Barbell', 'Dumbbells', 'Cables', 'Machines', 'Bodyweight', 'Kettlebell', 'Bands', 'Pull-up Bar'];
const FOCUS = ['Chest', 'Back', 'Shoulders', 'Arms', 'Legs', 'Core', 'Glutes', 'Full Body'];
const DIETS = ['None', 'Vegetarian', 'Vegan', 'Keto', 'Paleo', 'Gluten-Free', 'Dairy-Free', 'Halal'];

export default function ProfilePage() {
  const { user, logout } = useAuth();
  const [profile, setProfile] = useState<UserProfile>({
    name: '', age: '', height: '', weight: '', bodyFat: '',
    fitnessLevel: 'intermediate', goal: 'muscle_gain',
    daysPerWeek: 4, equipment: ['Barbell', 'Dumbbells'],
    focusAreas: ['Full Body'], dietaryRestrictions: ['None'],
    calorieGoal: '', proteinGoal: '',
  });
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    const stored = localStorage.getItem('gymbrain_profile');
    if (stored) { try { setProfile(JSON.parse(stored)); } catch {} }
  }, []);

  const save = () => {
    localStorage.setItem('gymbrain_profile', JSON.stringify(profile));
    setSaved(true);
    setTimeout(() => setSaved(false), 2000);
  };

  const toggleChip = (field: 'equipment' | 'focusAreas' | 'dietaryRestrictions', val: string) => {
    setProfile(p => {
      const arr = p[field];
      return { ...p, [field]: arr.includes(val) ? arr.filter(v => v !== val) : [...arr, val] };
    });
  };

  const upd = (field: keyof UserProfile, val: string | number) => setProfile(p => ({ ...p, [field]: val }));

  return (
    <div className="app-content">
      <div style={{ textAlign: 'center', marginBottom: 20 }}>
        <div style={{ fontSize: '2.5rem', marginBottom: 4 }}>👤</div>
        <h2 className="md-headline-sm" style={{ color: 'var(--md-primary)' }}>Your Profile</h2>
        <p className="md-body-sm text-muted">{user?.email}</p>
      </div>

      <div className="profile-section">
        <h3 className="profile-section__title">📋 Personal Info</h3>
        <div className="m3-field">
          <label className="m3-field__label">Display Name</label>
          <input className="m3-input" placeholder="Your name" value={profile.name} onChange={e => upd('name', e.target.value)} />
        </div>
        <div className="profile-row">
          <div className="m3-field">
            <label className="m3-field__label">Age</label>
            <input className="m3-input" type="number" placeholder="25" value={profile.age} onChange={e => upd('age', +e.target.value || '')} />
          </div>
          <div className="m3-field">
            <label className="m3-field__label">Height (cm)</label>
            <input className="m3-input" type="number" placeholder="175" value={profile.height} onChange={e => upd('height', +e.target.value || '')} />
          </div>
        </div>
        <div className="profile-row">
          <div className="m3-field">
            <label className="m3-field__label">Weight (kg)</label>
            <input className="m3-input" type="number" placeholder="75" value={profile.weight} onChange={e => upd('weight', +e.target.value || '')} />
          </div>
          <div className="m3-field">
            <label className="m3-field__label">Body Fat %</label>
            <input className="m3-input" type="number" placeholder="15" value={profile.bodyFat} onChange={e => upd('bodyFat', +e.target.value || '')} />
          </div>
        </div>
      </div>

      <div className="m3-divider" />

      <div className="profile-section">
        <h3 className="profile-section__title">🎯 Fitness Goals</h3>
        <div className="m3-field">
          <label className="m3-field__label">Primary Goal</label>
          <select className="m3-select" value={profile.goal} onChange={e => upd('goal', e.target.value)}>
            <option value="muscle_gain">💪 Muscle Gain</option>
            <option value="fat_loss">🔥 Fat Loss</option>
            <option value="strength">🏋️ Strength</option>
            <option value="endurance">🏃 Endurance</option>
            <option value="flexibility">🧘 Flexibility</option>
            <option value="general_fitness">⚡ General Fitness</option>
          </select>
        </div>
        <div className="m3-field">
          <label className="m3-field__label">Fitness Level</label>
          <select className="m3-select" value={profile.fitnessLevel} onChange={e => upd('fitnessLevel', e.target.value)}>
            <option value="beginner">🌱 Beginner (0-6 months)</option>
            <option value="intermediate">💪 Intermediate (6-24 months)</option>
            <option value="advanced">🔥 Advanced (2+ years)</option>
            <option value="athlete">🏆 Athlete (5+ years)</option>
          </select>
        </div>
        <div className="m3-field">
          <label className="m3-field__label">Training Days / Week</label>
          <div style={{ display: 'flex', gap: 8, justifyContent: 'center' }}>
            {[2,3,4,5,6].map(d => (
              <button key={d} className={`m3-chip ${profile.daysPerWeek === d ? 'm3-chip--selected' : ''}`}
                onClick={() => upd('daysPerWeek', d)}>{d}</button>
            ))}
          </div>
        </div>
      </div>

      <div className="m3-divider" />

      <div className="profile-section">
        <h3 className="profile-section__title">🏋️ Equipment Available</h3>
        <div className="goals-wrap">
          {EQUIPMENT.map(eq => (
            <button key={eq} className={`m3-chip ${profile.equipment.includes(eq) ? 'm3-chip--selected' : ''}`}
              onClick={() => toggleChip('equipment', eq)}>{eq}</button>
          ))}
        </div>
      </div>

      <div className="profile-section">
        <h3 className="profile-section__title">🎯 Focus Areas</h3>
        <div className="goals-wrap">
          {FOCUS.map(f => (
            <button key={f} className={`m3-chip ${profile.focusAreas.includes(f) ? 'm3-chip--selected' : ''}`}
              onClick={() => toggleChip('focusAreas', f)}>{f}</button>
          ))}
        </div>
      </div>

      <div className="m3-divider" />

      <div className="profile-section">
        <h3 className="profile-section__title">🍎 Nutrition Preferences</h3>
        <div className="goals-wrap" style={{ marginBottom: 12 }}>
          {DIETS.map(d => (
            <button key={d} className={`m3-chip ${profile.dietaryRestrictions.includes(d) ? 'm3-chip--selected' : ''}`}
              onClick={() => toggleChip('dietaryRestrictions', d)}>{d}</button>
          ))}
        </div>
        <div className="profile-row">
          <div className="m3-field">
            <label className="m3-field__label">Daily Calories</label>
            <input className="m3-input" type="number" placeholder="2500" value={profile.calorieGoal} onChange={e => upd('calorieGoal', +e.target.value || '')} />
          </div>
          <div className="m3-field">
            <label className="m3-field__label">Protein (g)</label>
            <input className="m3-input" type="number" placeholder="150" value={profile.proteinGoal} onChange={e => upd('proteinGoal', +e.target.value || '')} />
          </div>
        </div>
      </div>

      <button className="m3-btn m3-btn--filled m3-btn--full m3-btn--lg" onClick={save}>
        {saved ? '✅ Profile Saved!' : '💾 Save Profile'}
      </button>

      {profile.weight && profile.height && (
        <div className="m3-card m3-card--outlined" style={{ marginTop: 16 }}>
          <p className="md-label-lg text-muted" style={{ marginBottom: 8 }}>📊 Your Stats</p>
          <div className="profile-row">
            <div className="profile-stat">
              <span className="profile-stat__val">{(Number(profile.weight) / Math.pow(Number(profile.height)/100, 2)).toFixed(1)}</span>
              <span className="profile-stat__label">BMI</span>
            </div>
            <div className="profile-stat">
              <span className="profile-stat__val">{profile.bodyFat || '—'}<span className="profile-stat__unit">%</span></span>
              <span className="profile-stat__label">Body Fat</span>
            </div>
          </div>
        </div>
      )}

      <div className="m3-divider" />
      <button className="m3-btn m3-btn--outlined m3-btn--full" onClick={logout}>Sign Out</button>
    </div>
  );
}