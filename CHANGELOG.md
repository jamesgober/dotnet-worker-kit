# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

- _No changes yet._

## [1.0.0] - 2026-03-04

### Added
- Initial release of JG.WorkerKit
- IJobQueue interface for enqueuing background work items with typed job handlers
- IScheduler interface for recurring jobs with cron expressions and interval-based scheduling
- Job retry with configurable policies: fixed, linear, exponential backoff with jitter
- Per-job timeout via CancellationToken
- Dead letter handling for jobs that exhaust all retries
- Job priorities (critical, normal, low) with separate channels
- Graceful shutdown with configurable timeout
- Scoped DI per job execution
- Configurable max concurrent workers
- IJobMiddleware pipeline for cross-cutting concerns
- Source-generated LoggerMessage for all log points
- CronExpression parser for 5-field cron expressions
- Thread-safe, async-native design with ConfigureAwait(false)
- 
[Unreleased]: https://github.com/jamesgober/dotnet-worker-kit/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/jamesgober/dotnet-worker-kit/releases/tag/v1.0.0
