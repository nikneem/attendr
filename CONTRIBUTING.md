# Contributing to Attendr

Thank you for your interest in contributing to Attendr! We welcome contributions from the community and are excited to have you join us in making conference experiences better for everyone.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [How to Contribute](#how-to-contribute)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Commit Guidelines](#commit-guidelines)
- [Pull Request Process](#pull-request-process)
- [Testing](#testing)
- [Documentation](#documentation)

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment. Please:

- Be respectful and considerate in your communication
- Welcome newcomers and help them get started
- Accept constructive criticism gracefully
- Focus on what is best for the community and the project

## Getting Started

### Prerequisites

Before you begin, ensure you have:

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- [Node.js 20+](https://nodejs.org/) and npm
- [Angular CLI 21+](https://angular.io/cli)
- Git
- A code editor (VS Code recommended)
- PostgreSQL (for backend development)
- MongoDB (for Presence service development)

### Setting Up Your Development Environment

1. **Fork the repository** on GitHub

2. **Clone your fork:**
   ```bash
   git clone https://github.com/YOUR-USERNAME/attendr.git
   cd attendr
   ```

3. **Add the upstream repository:**
   ```bash
   git remote add upstream https://github.com/nikneem/attendr.git
   ```

4. **Install backend dependencies:**
   ```bash
   cd src
   dotnet restore
   dotnet build
   ```

5. **Install frontend dependencies:**
   ```bash
   cd src/App
   npm install
   ```

6. **Run tests to verify setup:**
   ```bash
   # Backend tests
   dotnet test src/Attendr.slnx
   
   # Frontend tests
   cd src/App
   npm test
   ```

## How to Contribute

### Reporting Bugs

If you find a bug, please create an issue using the bug report template. Include:

- A clear, descriptive title
- Steps to reproduce the issue
- Expected vs actual behavior
- Screenshots (if applicable)
- Your environment details

### Suggesting Features

Feature requests are welcome! Use the feature request template and include:

- A clear description of the problem you're trying to solve
- Your proposed solution
- Alternative solutions you've considered
- Which component/service would be affected

### Improving Documentation

Documentation improvements are always appreciated! This includes:

- README updates
- Code comments
- API documentation
- Architecture documentation
- Setup guides

## Development Workflow

### Creating a New Branch

Always create a new branch for your work:

```bash
# Update your local main branch
git checkout main
git pull upstream main

# Create a new feature branch
git checkout -b feature/your-feature-name

# Or for bug fixes
git checkout -b fix/bug-description
```

### Branch Naming Conventions

- `feature/feature-name` - New features
- `fix/bug-description` - Bug fixes
- `docs/documentation-update` - Documentation changes
- `refactor/component-name` - Code refactoring
- `test/test-description` - Test additions or improvements

## Coding Standards

### Backend (.NET)

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and single-purpose
- Use dependency injection
- Follow SOLID principles
- Write unit tests for new features

**Example:**
```csharp
/// <summary>
/// Retrieves a group by its unique identifier.
/// </summary>
/// <param name="groupId">The unique identifier of the group.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The group if found, null otherwise.</returns>
public async Task<Group?> GetByIdAsync(Guid groupId, CancellationToken cancellationToken)
{
    // Implementation
}
```

### Frontend (Angular)

- Follow [Angular Style Guide](https://angular.io/guide/styleguide)
- Use standalone components
- Use Angular Signals for state management
- Keep components focused and reusable
- Use proper TypeScript typing (avoid `any`)
- Follow PrimeNG patterns for UI components
- Write unit tests for components and services

**Example:**
```typescript
export class GroupListComponent {
  private readonly groupsService = inject(GroupsService);
  private readonly groupsStore = inject(GroupsStore);
  
  groups = this.groupsStore.groups;
  loading = this.groupsStore.loading;
  
  ngOnInit(): void {
    this.loadGroups();
  }
  
  private loadGroups(): void {
    this.groupsService.getGroups().subscribe();
  }
}
```

### General Guidelines

- Write self-documenting code
- Keep functions small and focused
- Avoid deep nesting
- Handle errors appropriately
- Don't commit commented-out code
- Remove console.log statements before committing
- Use meaningful commit messages

## Commit Guidelines

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, missing semicolons, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

### Examples

```bash
feat(groups): add search functionality to groups list

fix(presence): correct session check-in timing issue

docs(readme): update installation instructions

test(conferences): add unit tests for conference service
```

## Pull Request Process

1. **Ensure your code builds and tests pass:**
   ```bash
   # Backend
   dotnet build src/Attendr.slnx
   dotnet test src/Attendr.slnx
   
   # Frontend
   cd src/App
   npm run build
   npm test
   ```

2. **Update documentation** if needed

3. **Push your changes:**
   ```bash
   git push origin feature/your-feature-name
   ```

4. **Create a Pull Request** on GitHub:
   - Use a clear, descriptive title
   - Reference any related issues
   - Describe your changes and why they're needed
   - Add screenshots for UI changes
   - Ensure CI/CD checks pass

5. **Code Review:**
   - Address reviewer feedback promptly
   - Make requested changes in new commits
   - Keep the conversation respectful and constructive

6. **Merge:**
   - Once approved, a maintainer will merge your PR
   - Your branch will be deleted after merge

### Pull Request Checklist

- [ ] Code builds successfully
- [ ] All tests pass
- [ ] New tests added for new features
- [ ] Documentation updated
- [ ] Commit messages follow conventions
- [ ] No merge conflicts
- [ ] Code follows project style guidelines

## Testing

### Backend Testing

We use xUnit for backend testing. Tests should be:

- Fast and isolated
- Focused on one thing
- Readable and maintainable
- Use proper arrange-act-assert pattern

```csharp
[Fact]
public async Task GetByIdAsync_ExistingGroup_ReturnsGroup()
{
    // Arrange
    var groupId = Guid.NewGuid();
    var expectedGroup = new Group { Id = groupId, Name = "Test Group" };
    
    // Act
    var result = await _repository.GetByIdAsync(groupId, CancellationToken.None);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(expectedGroup.Name, result.Name);
}
```

### Frontend Testing

We use Jasmine and Karma for frontend testing:

```typescript
it('should load groups on initialization', () => {
  component.ngOnInit();
  
  expect(groupsService.getGroups).toHaveBeenCalled();
  expect(component.loading()).toBe(true);
});
```

### Running Tests

```bash
# Run all backend tests
dotnet test src/Attendr.slnx

# Run tests with coverage
dotnet test src/Attendr.slnx --collect:"XPlat Code Coverage"

# Run specific project tests
dotnet test src/Groups/HexMaster.Attendr.Groups.Tests

# Run frontend tests
cd src/App
npm test

# Run frontend tests in headless mode
npm test -- --browsers=ChromeHeadless --watch=false
```

## Documentation

### Code Documentation

- Add XML comments to public APIs (C#)
- Add JSDoc comments for complex TypeScript functions
- Document non-obvious behavior
- Keep comments up-to-date with code changes

### Architecture Documentation

When making architectural changes:

- Update architecture diagrams if needed
- Document design decisions in `/docs/architecture`
- Update API documentation
- Add examples for new patterns

## Questions or Need Help?

- Open an issue with the question label
- Check existing issues and discussions
- Review the documentation in `/docs`

## Recognition

Contributors will be recognized in:

- The project's README
- Release notes
- GitHub contributors page

Thank you for contributing to Attendr! Your efforts help make conference experiences better for everyone. 🎯
