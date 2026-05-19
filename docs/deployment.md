# Deployment

## Architecture
- Azure App Service — .NET API (container)
- Azure Container Registry (`councilagendaacr` in `council-agenda-rg`, Basic, Central US)
- Vercel — React client
- Supabase — hosted Postgres
- Docker for local dev (Dockerfile + docker-compose.yml + .env.docker)

## Current State
- ACR provisioned ✅
- App Service provisioned and running in Container mode ✅
- Docker image in ACR ✅
- App Service wired to ACR with admin credentials ✅
- Env vars set (Clerk, OpenAI, Supabase, Google OAuth) ✅
- Vercel deployed with correct VITE_API_URL and Clerk key ✅
- DB migrations applied to Supabase ✅
- GitHub Actions CD pipeline live (Azure + Vercel) ✅
- Clerk on dev keys intentionally (no custom domain) ✅
- Google OAuth2 per-org refresh token flow ✅

## Azure Environment Variables
- Clerk__SecretKey
- Clerk__Domain  
- OpenAI__ApiKey
- ConnectionStrings__DefaultConnection (Supabase)
- Google__OAuthClientId
- Google__OAuthClientSecret
- Google__OAuthRedirectUri
- Frontend__BaseUrl

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