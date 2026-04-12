# Tambouille

Application mobile-first de planning des repas de la semaine.

## Stack

- **Backend** : ASP.NET Core (.NET 8) — Minimal API, EF Core, SQLite
- **Frontend** : Vue 3 + TypeScript + Vite + Tailwind CSS
- **Déploiement** : Docker + Nginx Proxy Manager sur VPS OVH

## Lancer en local

```bash
docker compose up
```

L'app est accessible sur `http://localhost:8080`.

## Configuration

Les variables à surcharger via Docker Compose ou `.env` :

| Variable | Description |
|---|---|
| `Auth__Password` | Mot de passe global de l'app |
| `Auth__JwtSecret` | Clé secrète pour signer les JWT |

## Licence

MIT
