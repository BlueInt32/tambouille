# Tambouille — Brief projet

## Présentation

Application web de gestion des menus de la semaine, pensée pour un usage mobile.  
Nom : **Tambouille**  
Stack : **ASP.NET Core (API REST) + Vue.js (Composition API + Vite) + SQLite**  
Déploiement : **Docker + Portainer + Nginx Proxy Manager** sur VPS OVH

---

## Fonctionnalités

### Authentification
- Mot de passe global unique (pas de système de compte)
- Mot de passe configuré côté serveur (variable d'environnement ou `appsettings.json`)
- Session maintenue via un token JWT stocké en localStorage

### Planning de la semaine
- Vue par défaut : semaine en cours (lundi → dimanche)
- Navigation semaine précédente / semaine suivante (flèches)
- Chaque jour affiche ses repas (0, 1 ou 2 repas)

### Gestion des repas
- Zoom sur un jour : vue détaillée avec formulaire d'ajout
- Chaque repas contient :
  - **Nom** (texte libre)
  - **Nombre de personnes** (entier)
- Maximum 2 repas par jour (ex : midi et soir)
- Modification et suppression d'un repas

---

## Modèle de données (SQLite)

```sql
CREATE TABLE Meal (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Date TEXT NOT NULL,         -- format ISO 8601 : "2025-04-14"
    Slot INTEGER NOT NULL,      -- 0 = midi, 1 = soir
    Name TEXT NOT NULL,
    Persons INTEGER NOT NULL DEFAULT 2
);
```

---

## API REST (ASP.NET Core)

### Auth
| Méthode | Route | Description |
|--------|-------|-------------|
| POST | `/api/auth/login` | Vérifie le mot de passe, retourne un JWT |

### Repas
| Méthode | Route | Description |
|--------|-------|-------------|
| GET | `/api/meals?weekStart=2025-04-14` | Repas de la semaine (lundi de la semaine) |
| POST | `/api/meals` | Créer un repas |
| PUT | `/api/meals/{id}` | Modifier un repas |
| DELETE | `/api/meals/{id}` | Supprimer un repas |

Toutes les routes `/api/meals` sont protégées par JWT.

---

## Frontend Vue.js

### Structure des composants
```
src/
├── views/
│   ├── LoginView.vue          # Saisie du mot de passe
│   └── PlanningView.vue       # Vue principale semaine
├── components/
│   ├── WeekGrid.vue           # Grille 7 jours
│   ├── DayCard.vue            # Carte d'un jour (résumé)
│   ├── DayDetail.vue          # Vue zoomée sur un jour
│   └── MealForm.vue           # Formulaire ajout/modif repas
├── stores/
│   └── planning.js            # Pinia store (semaine courante, repas)
└── services/
    └── api.js                 # Appels HTTP (fetch ou axios)
```

### Navigation
- Vue semaine : liste verticale adaptée mobile
- Flèches `<` et `>` pour changer de semaine
- Tap sur un jour → vue détaillée du jour
- Bouton retour pour revenir à la semaine

---

## Configuration

### `appsettings.json`
```json
{
  "Auth": {
    "Password": "motdepasse",
    "JwtSecret": "une-clé-secrète-longue"
  },
  "ConnectionStrings": {
    "Default": "Data Source=/data/gastroplanning.db"
  }
}
```

Les valeurs sont surchargeables via variables d'environnement Docker (`Auth__Password`, `Auth__JwtSecret`).

---

## Docker

```yaml
# docker-compose.yml
services:
  gastroplanning:
    image: gastroplanning
    restart: unless-stopped
    volumes:
      - ./data:/data
    environment:
      - Auth__Password=motdepasse
      - Auth__JwtSecret=une-clé-secrète-longue
    ports:
      - "8080:8080"
```

Le volume `/data` persiste la base SQLite.

---

## Conventions de code

### Backend — ASP.NET Core Minimal API

- **Architecture** : Minimal API (.NET 8), pas de Controllers
- **Organisation** : endpoints regroupés par feature dans des fichiers d'extension (`MealEndpoints.cs`, `AuthEndpoints.cs`) appelés depuis `Program.cs` via `app.MapMealEndpoints()`
- **Validation** : FluentValidation (NuGet) pour valider les request bodies
- **ORM** : Entity Framework Core avec provider SQLite (`Microsoft.EntityFrameworkCore.Sqlite`)
- **Migrations** : EF Core Migrations (`dotnet ef migrations add`, `dotnet ef database update`)
- **DTOs** : classes dédiées pour les entrées/sorties (ne jamais exposer les entités EF directement)
- **Gestion des erreurs** : `Results.Problem()` / `Results.NotFound()` / `Results.ValidationProblem()` — pas de try/catch partout
- **Logging** : `ILogger<T>` injecté via DI, pas de `Console.WriteLine`
- **JWT** : `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Nommage** : PascalCase pour classes/méthodes, camelCase pour variables locales, snake_case interdit
- **Structure des dossiers** :
  ```
  Tambouille/
  ├── Program.cs
  ├── appsettings.json
  ├── Data/
  │   └── AppDbContext.cs
  ├── Models/
  │   └── Meal.cs
  ├── DTOs/
  │   ├── MealDto.cs
  │   └── CreateMealRequest.cs
  ├── Endpoints/
  │   ├── AuthEndpoints.cs
  │   └── MealEndpoints.cs
  └── Services/
      └── AuthService.cs
  ```

### Frontend — Vue.js

- **Vue 3** Composition API exclusivement (`<script setup lang="ts">`)
- **TypeScript** activé (`lang="ts"` sur tous les SFCs)
- **Pinia** pour le state management (un store par domaine : `usePlanningStore`)
- **Vue Router 4** pour la navigation (`/login`, `/`)
- **Tailwind CSS v3** (config via `tailwind.config.js`, `@tailwind` directives dans `main.css`)
- **Axios** pour les appels HTTP, avec un intercepteur pour injecter le JWT automatiquement
- **Gestion des erreurs** : toast ou message inline, jamais de `alert()`
- **Nommage** : composants en PascalCase, composables en `useXxx`, stores en `useXxxStore`
- **Types** : interfaces TypeScript définies dans `src/types/index.ts`
- **Structure des dossiers** :
  ```
  src/
  ├── main.ts
  ├── App.vue
  ├── types/
  │   └── index.ts          # interfaces Meal, Week, etc.
  ├── services/
  │   └── api.ts            # instance axios + intercepteurs
  ├── stores/
  │   └── planning.ts       # usePlanningStore (Pinia)
  ├── router/
  │   └── index.ts          # Vue Router
  ├── views/
  │   ├── LoginView.vue
  │   └── PlanningView.vue
  └── components/
      ├── WeekGrid.vue
      ├── DayCard.vue
      ├── DayDetail.vue
      └── MealForm.vue
  ```

### Général
- **Dates** : ISO 8601 côté API (`"2025-04-14"`), jamais de timestamp Unix
- **Semaine** : commence le lundi (`DayOfWeek.Monday`)
- **CORS** : configuré côté .NET pour autoriser le dev Vite (`localhost:5173`)
- **Variables d'environnement** : surcharge via Docker Compose pour prod

### Style visuel — Tailwind CSS

Ambiance **chaleureuse, cuisine maison** — oranges, ambrés, verts herbe.

**Palette de couleurs** (à définir dans `tailwind.config.js`) :
```js
colors: {
  primary:  { DEFAULT: '#ea7c1e', light: '#f5a84e', dark: '#c45e0a' },  // orange chaud
  accent:   { DEFAULT: '#4a7c59', light: '#6aac7a' },                   // vert herbe
  surface:  { DEFAULT: '#fdf6ee', card: '#fff8f0' },                    // blanc cassé chaud
  text:     { DEFAULT: '#2d1f0e', muted: '#8a6a4a' },                   // brun foncé
}
```

**Principes UI** :
- Fond général `bg-surface`, cartes `bg-surface-card` avec `shadow-sm`
- Boutons primaires : `bg-primary text-white rounded-xl` + hover `bg-primary-dark`
- Coins arrondis généreux (`rounded-xl`, `rounded-2xl`) — look mobile doux
- Espacement aéré (`p-4`, `gap-4`) pour une lecture facile sur mobile
- Icônes : [Heroicons](https://heroicons.com/) (cohérentes avec l'écosystème Tailwind)

**Composants types** :
- `DayCard` : carte blanche, bordure gauche `border-l-4 border-primary`
- Jour sélectionné : fond `bg-primary text-white`
- `MealForm` : inputs `border border-amber-200 rounded-lg focus:ring-2 focus:ring-primary`
- Navigation semaine : flèches `<` `>` en `text-primary`, date centrée en bold
