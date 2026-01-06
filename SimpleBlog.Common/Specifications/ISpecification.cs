namespace SimpleBlog.Common.Specifications;

/// <summary>
/// Represents a specification pattern for building reusable query logic.
/// </summary>
/// <typeparam name="T">The entity type to apply the specification to.</typeparam>
public interface ISpecification<T> where T : class
{
    /// <summary>
    /// Applies the specification logic to the given queryable.
    /// </summary>
    /// <param name="query">The queryable to apply the specification to.</param>
    /// <returns>The modified queryable with the specification applied.</returns>
    IQueryable<T> Apply(IQueryable<T> query);
}

/// <summary>
/// Base class for creating specifications with common functionality.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public abstract class Specification<T> : ISpecification<T> where T : class
{
    /// <summary>
    /// Applies the specification logic to the given queryable.
    /// </summary>
    public abstract IQueryable<T> Apply(IQueryable<T> query);
}
