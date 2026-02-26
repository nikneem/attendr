# Specs

This folder contains implementation-ready specifications for incremental product improvements.

Guiding constraints for these specs:
- Follow the repo architecture (CQRS recommendation, Minimal APIs, vertical slices where it pays off).
- Keep endpoints thin and framework concerns at the edge.
- Align observability with OpenTelemetry (see ADR 0008).
- Avoid hardcoded styling tokens in Angular; prefer PrimeNG/theme variables (see ADR 0006).

Start here: [specs/index.md](index.md)
