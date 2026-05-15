# Deployment

## Architecture
- Azure App Service — .NET API (container)
- Azure Container Registry (`councilagendaacr` in `council-agenda-rg`, Basic, Central US)
- Vercel — React client
- Supabase — hosted Postgres
- Docker for local dev (Dockerfile + docker-compose.yml + .env.docker)

## Current State
- ACR provisioned ✅
- App Service provisioned ✅
- Docker container verified working locally (Swagger confirmed)
- `deploy.yml` workflow scaffolded
- App Service still in Code mode — needs switch to Container

## Immediate Next Steps
1. Switch App Service from Code to Container
2. Push Docker image to ACR
3. Configure App Service to pull from ACR
4. Set env vars on App Service (Clerk, OpenAI, Supabase connection)
5. Update Vercel `VITE_API_URL` to App Service URL
6. Update GitHub secret `VITE_API_URL`
7. Set up Clerk webhook (post-launch) for invite auto-acceptance

## GitHub Secrets
- `VITE_CLERK_PUBLISHABLE_KEY`
- `VITE_API_URL`
- (Add ACR credentials when finalizing deploy.yml)