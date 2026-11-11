# Security policy

## Reporting a vulnerability

Please do not open a public issue for a security problem, and do not post the
details in Discussions. A public report tells everyone about the problem before
there is a fix.

1. Open the [Security tab](https://github.com/Tawjjeh/TalebElm/security) and
   use **Report a vulnerability**. If that form is available it is private, and
   only the repository owners can read it.
2. If the form is not available, open a short issue that says you have a
   security report and nothing else — no steps, no affected file, no exploit.
   One of the owners will reply with a private way to collect the details.

Once you have a private channel, include what you can:

- the affected part (route, service, workflow, or dependency),
- the commit or branch you tested,
- the steps to reproduce it,
- what an attacker could do with it,
- and a suggested fix if you have one.

## What happens next

We read every report. We will tell you whether we could reproduce it, what we
plan to do, and when a fix is on `main`. If you want credit, say so and we will
name you when the fix ships. We do not promise dates.

Please give us a reasonable window to fix the problem before you write about it
in public.

## Scope

In scope:

- the API and its middleware (`src/TalebElm.Api`),
- persistence and migrations (`src/TalebElm.Infrastructure`),
- authentication, password hashing, tokens, and authorization,
- the Codeforces and GitHub verification clients,
- and the workflows under `.github/workflows`.

Out of scope: vulnerabilities in Codeforces, GitHub, or other third-party
services. Report those to the service that owns them.

## Supported versions

The project has no tagged releases yet; `main` is the only supported line and
fixes land there.
