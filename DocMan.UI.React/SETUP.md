# DocMan React UI Setup

## Features

- ✅ React 18 with Vite
- ✅ Chakra UI for responsive, themable components
- ✅ Dark/Light mode support
- ✅ Mobile-friendly responsive design
- ✅ React Router for navigation
- ✅ Axios for API calls
- ✅ JWT authentication with localStorage
- ✅ Protected routes

## Installation

```bash
cd DocMan.UI.React
npm install
```

## Configuration

1. Copy `.env.example` to `.env`:
```bash
cp .env.example .env
```

2. Update `.env` with your API URL:
```
VITE_API_URL=https://localhost:7221
```

## Development

Start the development server:
```bash
npm run dev
```

The app will be available at `http://localhost:5173`

## Build

Build for production:
```bash
npm run build
```

## Project Structure

```
src/
├── pages/           # Page components (Login, Chat, Documents)
├── components/      # Reusable components (Sidebar, ProtectedRoute)
├── services/        # API and auth services
├── theme.js         # Chakra UI theme configuration
├── App.jsx          # Main app with routing
└── main.jsx         # Entry point
```

## Demo Credentials

- **Username:** john_doe
- **Password:** Password123!

## Features

### Authentication
- Login with username/password
- JWT token stored in localStorage
- Auto-logout on 401 response
- Protected routes

### UI/UX
- Responsive design (mobile, tablet, desktop)
- Dark/Light theme toggle
- Chakra UI components
- Mobile-friendly navigation with drawer menu

### Chat
- Real-time message display
- Query search with metrics
- Auto-scroll to latest message
- Loading states

## API Integration

The app connects to the .NET API at `https://localhost:7221`

Endpoints used:
- `POST /api/auth/login` - User login
- `POST /api/search/chat` - Chat search
- `GET /api/documents` - List documents
- `GET /api/categories` - List categories
- `GET /api/users` - List users

