# Security Policy

## Supported Versions

Security updates are targeted at the main branch. If you are using a fork or older snapshot, please update to the latest version before reporting issues.

## Reporting a Vulnerability

Please do NOT open public issues for security vulnerabilities.

To report a vulnerability:
- Use GitHub's "Report a vulnerability" (Security Advisories) on this repository if available.
- Alternatively, open a private discussion with the maintainers.

Provide as much detail as possible:
- Affected component(s) and versions
- Steps to reproduce
- Impact assessment
- Suggested remediation (if known)

### Response Timelines
- Acknowledgement: within 72 hours
- Initial assessment: within 7 days
- Fix or mitigation plan: within 14 days (severity-dependent)

We appreciate responsible disclosure and will credit reporters in release notes if desired.

## Best Practices

When deploying or developing locally:
- Keep dependencies up to date
- Use environment variables for secrets; never commit secrets
- Rotate credentials regularly
- Enable HTTPS everywhere in production
- Follow principle of least privilege for databases and services
