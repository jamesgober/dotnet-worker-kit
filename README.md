# dotnet-worker-kit

[![NuGet](https://img.shields.io/nuget/v/JG.WorkerKit?logo=nuget)](https://www.nuget.org/packages/JG.WorkerKit)
[![Downloads](https://img.shields.io/nuget/dt/JG.WorkerKit?color=%230099ff&logo=nuget)](https://www.nuget.org/packages/JG.WorkerKit)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](./LICENSE)
[![CI](https://github.com/jamesgober/dotnet-worker-kit/actions/workflows/ci.yml/badge.svg)](https://github.com/jamesgober/dotnet-worker-kit/actions)

---

A background job queue and scheduled task runner for .NET applications. Enqueue work, schedule recurring tasks with cron expressions, and process jobs with retry policies — all built on `IHostedService` and `Channel<T>` with zero external dependencies.

## Features

- **Job queue** — enqueue typed jobs with priority, timeout, and retry configuration
- **Scheduled tasks** — cron expressions and interval-based recurring execution
- **Retry policies** — fixed, exponential backoff, or custom strategies with dead letter capture
- **Configurable concurrency** — N worker threads pulling from a bounded channel
- **Scoped execution** — each job gets its own DI scope for clean resource management
- **Job middleware** — wrap execution with logging, timing, metrics, or custom concerns
- **Graceful shutdown** — in-flight jobs complete before the host stops
- **Zero external dependencies** — no Redis, no SQL, just in-process `Channel<T>`

## Installation

```bash
dotnet add package JG.WorkerKit
```

## Quick Start

```csharp
builder.Services.AddWorkerKit(options =>
{
    options.WorkerCount = 2;
    options.DefaultRetryPolicy = RetryPolicy.Exponential(maxRetries: 3);
});

builder.Services.AddJobHandler<SendEmailData, SendEmailHandler>();
builder.Services.AddScheduledTask<CleanupTask>("0 */6 * * *");
```

## Documentation

- **[API Reference](./docs/API.md)** — Full API documentation and examples

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

Licensed under the Apache License 2.0. See [LICENSE](./LICENSE) for details.

---

**Ready to get started?** Install via NuGet and check out the [API reference](./docs/API.md).
