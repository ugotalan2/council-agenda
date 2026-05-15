# Council Agenda Generator

Multi-tenant meeting agenda generator. Portfolio project targeting Kansas City employers (Garmin, H&R Block, Oracle Health).

## Tech Stack
- .NET 8 Web API (`/api`)
- React + TypeScript (`/client`)
- PostgreSQL via Npgsql + EF Core
- Clerk (auth, RBAC, invitations)
- OpenAI API (discussion question generation, meeting prep)
- Google Drive API (agenda export)
- Docker containerized
- Azure App Service + Azure Container Registry (API)
- Vercel (client)
- Supabase (hosted Postgres)
- GitHub Actions CI/CD

## Repo
Monorepo: `council-agenda` on GitHub
Local dev DB: `council_agenda_dev` on local Postgres 16