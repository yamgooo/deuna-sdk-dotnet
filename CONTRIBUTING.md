# Contributing

## Developing

1. Clone the repository.
2. The SDK targets .NET 10. Make sure you have the .NET 10 SDK installed.
3. Run `dotnet build` from the root directory.
4. Run `dotnet test` to execute all unit and integration tests.

## Publishing Releases

Releases are published automatically via GitHub Actions (`.github/workflows/release.yml`) whenever a new tag starting with `v` is pushed to the repository.

To publish a new version:

1. Update the `CHANGELOG.md` with the new version details.
2. Commit your changes.
3. Create a git tag for the new version:
   ```bash
   git tag v1.0.0
   ```
4. Push the tag to GitHub:
   ```bash
   git push origin v1.0.0
   ```

### Prerequisites for Maintainers

For the `release.yml` workflow to successfully push to NuGet.org, the repository requires a GitHub Repository Secret named `NUGET_API_KEY`. 

To set this up:
1. Log in to [NuGet.org](https://www.nuget.org/).
2. Generate an API Key with the "Push" scope for the `Deuna.Merchant.Sdk` package (or "Glob pattern" `Deuna.Merchant.Sdk` if it's the first time).
3. In this GitHub repository, go to **Settings > Secrets and variables > Actions**.
4. Create a new repository secret:
   - **Name**: `NUGET_API_KEY`
   - **Secret**: (Paste the generated API key)
