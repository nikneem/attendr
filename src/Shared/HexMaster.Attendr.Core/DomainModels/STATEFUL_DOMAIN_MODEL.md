# Stateful Domain Model

This document explains the `StatefulDomainModel<TId>` base class and its state machine implementation.

## Overview

The `StatefulDomainModel<TId>` provides a base class for domain models that tracks their lifecycle state. This is useful for determining what database operations should be performed (insert, update, or delete) and for auditing purposes.

## State Machine

### States

- **Created**: The domain model was just created (e.g., via a POST request). This is a terminal state - objects in Created state cannot change state.
- **Pristine**: The domain model was loaded from the database and has not been modified.
- **Touched**: A property setter was called, but the value did not actually change.
- **Modified**: The domain model has been modified (actual values changed).
- **Deleted**: The domain model has been marked for deletion. This is a terminal state.

### State Transition Rules

```
Created ──┐ (terminal state - no transitions allowed)
          │
Pristine ──┬──> Touched ──┬──> Modified ──> Deleted
           │              │
           ├──────────────┤
           │              │
           └──────────────┴──> Deleted
```

- **Created**: Cannot change state (terminal state for new objects)
- **Objects can never transition TO Created** (except during construction)
- **Pristine**: Can change to Touched, Modified, or Deleted
- **Touched**: Can change to Modified or Deleted
- **Modified**: Can only change to Deleted
- **Deleted**: Cannot change state (terminal state)

Invalid state transitions are silently ignored.

## Usage Examples

### Creating a New Domain Model

```csharp
public sealed class Product : StatefulDomainModel<Guid>
{
    private string _name;
    private decimal _price;

    public string Name => _name;
    public decimal Price => _price;

    // Constructor for new entities (Created state)
    private Product(Guid id, string name, decimal price)
        : base(id, DomainModelState.Created)
    {
        _name = name;
        _price = price;
    }

    // Constructor for loading from database (Pristine state)
    private Product(Guid id, string name, decimal price, DateTimeOffset createdOn, DateTimeOffset? modifiedOn)
        : base(id, DomainModelState.Pristine)
    {
        _name = name;
        _price = price;
        SetCreatedOn(createdOn);
        SetModifiedOn(modifiedOn);
    }

    // Factory method for new entities
    public static Product Create(Guid id, string name, decimal price)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        return new Product(id, name, price);
    }

    // Factory method for loading from database
    public static Product Load(Guid id, string name, decimal price, DateTimeOffset createdOn, DateTimeOffset? modifiedOn)
    {
        return new Product(id, name, price, createdOn, modifiedOn);
    }

    // Property setter using ShouldUpdateProperty helper (recommended)
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (ShouldUpdateProperty(_name, name))
        {
            _name = name;
            UpdateModifiedOn();
        }
    }

    // Property setter using manual state tracking (alternative)
    public void UpdatePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        TrackPropertyChange(); // Marks as Touched

        if (!Equals(_price, price))
        {
            _price = price;
            SetState(DomainModelState.Modified); // Only Modified if value changed
            UpdateModifiedOn();
        }
    }

    public void MarkAsDeleted()
    {
        SetState(DomainModelState.Deleted);
    }
}
```

### Using the Domain Model

```csharp
// Creating a new product (POST request scenario)
var newProduct = Product.Create(Guid.NewGuid(), "Laptop", 999.99m);
Console.WriteLine(newProduct.State); // Output: Created

// Trying to modify a Created entity
newProduct.UpdateName("Gaming Laptop"); // State remains Created
Console.WriteLine(newProduct.State); // Output: Created (no change)

// Loading from database
var existingProduct = Product.Load(
    Guid.NewGuid(),
    "Mouse",
    29.99m,
    DateTimeOffset.UtcNow.AddDays(-10),
    null);
Console.WriteLine(existingProduct.State); // Output: Pristine

// Calling setter with same value
existingProduct.UpdateName("Mouse"); // Value doesn't change
Console.WriteLine(existingProduct.State); // Output: Touched

// Calling setter with different value
existingProduct.UpdatePrice(34.99m); // Value changes
Console.WriteLine(existingProduct.State); // Output: Modified

// Marking for deletion
existingProduct.MarkAsDeleted();
Console.WriteLine(existingProduct.State); // Output: Deleted

// Trying to modify after deletion
existingProduct.UpdateName("New Name"); // Silently ignored
Console.WriteLine(existingProduct.State); // Output: Deleted (no change)
```

## Repository Pattern Integration

The state can be used in repositories to determine which operation to perform:

```csharp
public async Task SaveAsync(Product product, CancellationToken cancellationToken)
{
    switch (product.State)
    {
        case DomainModelState.Created:
            await InsertAsync(product, cancellationToken);
            break;

        case DomainModelState.Modified:
            await UpdateAsync(product, cancellationToken);
            break;

        case DomainModelState.Deleted:
            await DeleteAsync(product, cancellationToken);
            break;

        case DomainModelState.Pristine:
        case DomainModelState.Touched:
            // No database operation needed
            break;
    }
}
```

## Best Practices

1. **Use factory methods** instead of public constructors to create instances with the correct state.

2. **Choose the appropriate helper method**:
   - Use `ShouldUpdateProperty()` for cleaner code (recommended)
   - Use `TrackPropertyChange()` + manual check when you need more control

3. **Always validate** before setting values, not just in setters.

4. **Update timestamps** when making actual changes (`UpdateModifiedOn()`).

5. **State is immutable** once set to Created or Deleted (terminal states).

6. **Test state transitions** to ensure your domain model behaves correctly.

## State Machine Guarantees

- Objects created with `DomainModelState.Created` will never change state, ensuring you always know they need to be inserted into the database.
- Objects loaded from the database start as `Pristine`, allowing you to track if any changes were made.
- The `Touched` state allows you to distinguish between "property setter called" and "value actually changed".
- Once marked as `Deleted`, objects cannot be modified, preventing accidental updates after deletion.
- Invalid state transitions are silently ignored, making the domain model resilient to incorrect usage.
