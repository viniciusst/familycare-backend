# Security Policy

## Reporting a Vulnerability

If you discover a security vulnerability in FamilyCare, please **do not** open a public issue. Instead, report it privately so we can address it before it is publicly disclosed.

To report a vulnerability:

1. Open a [private security advisory](../../security/advisories/new) on this repository, or
2. Open a regular GitHub issue with the subject `SECURITY:` and the maintainers will move the discussion to a private channel.

Please include:

- A description of the vulnerability and its impact.
- Steps to reproduce, if possible.
- The affected version (commit SHA or release tag).
- Any suggested mitigation, if you have one.

We aim to acknowledge reports within 7 days and provide a fix or mitigation timeline within 30 days, depending on severity.

## Scope

In scope:

- The FamilyCare backend (this repository).
- The Docker setup and CI/CD pipeline shipped in this repository.

Out of scope:

- Default credentials in `docker-compose.yml` (intentional, for local dev only — see comments in that file).
- The placeholder JWT key in `appsettings.json` (intentional — see [README](README.md#-security)).
- Third-party dependencies — please report those upstream.

## Supported Versions

This project is currently in early development. Security fixes are applied to the `main` branch only.

| Version  | Supported |
| -------- | --------- |
| `main`   | ✅        |
| Others   | ❌        |
