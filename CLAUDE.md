# CLAUDE.md — Databento.CSharpApiClient

Guidelines for working on this repository with Claude Code or any AI coding assistant.

---

## Repo overview

Pure-managed C# (.NET 8) HTTP client for the [Databento Historical API](https://databento.com/docs/api-reference-historical).
No external runtime dependencies — only `System.Text.Json` (inbox on .NET 8+).

```
src/
  Databento.CSharpApiClient/        ← the library (packaged to NuGet)
tests/
  Databento.CSharpApiClient.UnitTests/          ← deterministic, no network
  Databento.CSharpApiClient.IntegrationTests/   ← live API, skipped without DATABENTO_API_KEY
.github/workflows/ci.yml            ← CI: build + unit tests on every push/PR; publish on v* tag
```

---

## Contribution model — PRs only, no direct commits

**No one pushes directly to `main`.** Every change — including maintainers — goes through a Pull Request that must be reviewed and approved before merging. Branch protection enforces this.

When working with Claude Code:
- Create a feature branch off `main` (`git checkout -b feat/my-change`)
- Make changes, build, test locally
- Push the branch and open a PR on GitHub
- Address review comments, then merge via the GitHub UI

---

## Building

```bash
dotnet build Databento.CSharpApiClient.slnx   # Debug by default
dotnet build Databento.CSharpApiClient.slnx -c Release
```

Zero warnings required — `GenerateDocumentationFile=true` is enabled and `CS1591` is not suppressed. Every public API surface needs an XML `<summary>` doc comment.

---

## Tests

```bash
# Unit tests only (fast, no network, always run)
dotnet test Databento.CSharpApiClient.slnx --filter "FullyQualifiedName!~IntegrationTests"

# Integration tests (requires a live Databento API key)
export DATABENTO_API_KEY=db-...
dotnet test tests/Databento.CSharpApiClient.IntegrationTests
```

Integration tests use `[SkippableFact]` from `Xunit.SkippableFact`. Without the env var they skip cleanly with an explanatory message — they never fail due to a missing key. CI runs all tests; integration tests are skipped there unless a `DATABENTO_API_KEY` secret is configured.

---

## Coding conventions

- **No external runtime dependencies.** If you're tempted to add a NuGet package, think twice. The whole value proposition is zero deps on .NET 8+.
- **`DateTimeOffset` for all API parameters**, not `DateTime`. Converts to `yyyy-MM-ddTHH:mm:ssZ` before sending.
- **`System.Text.Json` only** — no Newtonsoft, no other JSON lib.
- **`[JsonPropertyName("snake_case")]`** on every deserialized property — Databento sends snake_case; don't rely on case-insensitive matching alone.
- **Synchronous wrappers** (`GetCbbo1s`, `GetTrades`, …) call the `Async` counterpart via `.GetAwaiter().GetResult()`. Add both for every new endpoint.
- **XML doc on every public member** — required by `GenerateDocumentationFile=true`. No CS1591 warnings allowed.
- Allman brace style, 4-space indent.
- No `var` — explicit types only.

---

## Adding a new endpoint

1. Add the sync+async methods to the appropriate client (`DatabentoJsonClient` or `DatabentoClient`). Each client is split into partial files by kind: a timeseries `Get*Async` and its sync wrapper go in `*.Timeseries.cs`, its `Stream*Async`/`Stream*` pair in `*.Streaming.cs`, metadata and symbology endpoints in `DatabentoJsonClient.Metadata.cs`, batch endpoints in `DatabentoJsonClient.Batch.cs`, and private helpers in `*.Core.cs`. The main file holds only the fields, constructor and `Dispose`.
2. Add/update the data model class under `DataModel/` with `[JsonPropertyName]` attributes and XML docs.
3. Add a unit test in `UnitTests/` (mock transport via `IHttpTransport`).
4. Add an integration test in `IntegrationTests/` inheriting `IntegrationTestBase`, using `[SkippableFact]` + `SkipIfNoApiKey()`.
5. Build clean, run unit tests, open a PR.

---

## Releasing to NuGet.org

**Code changes and version bumps are separate PRs.** This is a public repo, so its history should show each release as its own step.

- **Feature or fix PRs** change code and tests, and add their entry under CHANGELOG `[Unreleased]`. They never touch `<Version>` or `<PackageReleaseNotes>`.
- **The version bump is a separate `chore:` PR**, opened once every feature PR of the release is merged. It holds only the version, the release notes and the CHANGELOG cut (step 2 below), plus the live-run record (step 1).
- When it isn't clear which kind of PR a change belongs in, ask the maintainer before opening it.

1. Decide whether the live integration suite must run before this release. Diff from the last `v*` tag to the tip of the preparation branch, which is the commit that will be tagged. A run is needed when anything touches the wire: request building, response parsing or decoding, models, retries or transport. A run is also needed when the last full live run predates such a change. Unit tests alone are not enough for such changes, because the fixtures share the decoder's assumptions about the format. When a run is needed:
   - Price it with `metadata.get_cost`, which is free; a full run cost about $1 as of 2.4.0.
   - Run the suite on the preparation branch's tip, and don't tag until it is green.
   - Record the outcome in the preparation PR's Testing section: the passed and skipped counts, or "not needed" and why. That record is how the next release knows when the last full run happened.
2. Bump `<Version>` in `src/Databento.CSharpApiClient/Databento.CSharpApiClient.csproj`, add the version's `<PackageReleaseNotes>` entry, and cut CHANGELOG `[Unreleased]` into a dated section with its compare link.
3. Commit the version bump on a branch and merge via PR.
4. After merge, on `main`:
   ```bash
   git tag v1.2.3
   git push origin v1.2.3
   ```
5. The `publish` job in CI fires automatically (triggered by the `v*` tag), builds Release, packs, and pushes to NuGet.org using the `NUGET_API_KEY` repository secret.
6. The package appears on nuget.org within a few minutes.

> **`--skip-duplicate` is set** — pushing a tag for a version that already exists on NuGet.org is a no-op, not an error.

---

## Repository secrets (GitHub → Settings → Secrets and variables → Actions)

| Secret | Purpose |
|--------|---------|
| `NUGET_API_KEY` | NuGet.org API key scoped to `Databento.CSharpApiClient`. Required for the `publish` CI job. |
| `DATABENTO_API_KEY` | Optional. If set, integration tests run in CI instead of being skipped. |
