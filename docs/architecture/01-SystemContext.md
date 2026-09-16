# C4 Model - System Context Diagram

## System Context: GymBrain AI-Powered Fitness Coaching Platform

### Users
- **End User (Trainee)**: Individuals seeking personalized fitness coaching, workout plans, nutrition guidance, and progress tracking
- **Administrator**: System administrators managing platform configuration and monitoring

### System Boundary: GymBrain Platform
The core GymBrain system providing AI-powered fitness coaching capabilities.

### External Systems
- **PostgreSQL Database (Neon)**: Primary data storage for user profiles, workout plans, nutrition data, telemetry
- **Redis Cache (Upstash)**: Distributed caching layer for improved performance
- **Authentication Providers**: 
  - Email/Password authentication via JWT
  - Potential social logins (Google, Apple) - future enhancement
- **LLM Providers**:
  - OpenAI GPT models via direct API
  - Groq API for fast inference
  - OpenRouter for model routing and fallback
- **Exercise Metadata Service**: External API for exercise database and metadata
- **Frontend Hosting**: Firebase Hosting serving React/TypeScript SPA
- **API Hosting**: Railway hosting .NET 9 minimal API
- **Payment Processor** (Future): Stripe/PayPal for premium subscriptions
- **Email Service** (Future): SendGrid/SMTP for notifications and newsletters
- **Analytics & Monitoring**: 
  - Application Insights / Serilog for logging
  - Potential integration with Google Analytics or Mixpanel
  - Error tracking with Sentry (future)

### User Goals
1. Get personalized workout plans based on goals, equipment, experience level
2. Receive nutrition guidance tailored to fitness goals and dietary preferences
3. Track workout completion and progress over time
4. Adapt plans based on feedback and progress
5. Access exercise library with proper form guidance
6. Stay motivated through milestone tracking and achievement system

### System Responsibilities
1. **User Management**: Registration, authentication, profile management
2. **Workout Planning**: Generate adaptive workout plans based on user data
3. **Nutrition Guidance**: Provide meal plans and dietary recommendations
4. **Progress Tracking**: Record and analyze workout completion and metrics
5. **Exercise Library**: Maintain comprehensive exercise database with metadata
6. **AI Coaching**: Use LLMs to provide personalized recommendations and form feedback
7. **Notifications**: Send reminders, motivational messages, and progress updates
8. **Data Security**: Protect user data with encryption and secure authentication

### Key Integration Points
- **Database**: PostgreSQL via Entity Framework Core
- **Cache**: Redis via StackExchange.Redis for performance optimization
- **LLM Services**: Multiple provider abstraction with fallback capabilities
- **External APIs**: Exercise metadata service for exercise library
- **Authentication**: JWT-based stateless authentication
- **Frontend**: RESTful API consumed by React/TypeScript SPA