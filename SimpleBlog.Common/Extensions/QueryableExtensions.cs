using SimpleBlog.Common.Specifications;

namespace SimpleBlog.Common.Extensions;

/// <summary>
/// Extension methods for IQueryable to work with specifications.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies a specification to the queryable.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The queryable to apply the specification to.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <returns>The modified queryable with the specification applied.</returns>
    public static IQueryable<T> ApplySpecification<T>(
        this IQueryable<T> query,
        ISpecification<T> specification) where T : class
    {
        return specification.Apply(query);
    }

    /// <summary>
    /// Applies multiple specifications to the queryable in sequence.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The queryable to apply the specifications to.</param>
    /// <param name="specifications">The specifications to apply.</param>
    /// <returns>The modified queryable with all specifications applied.</returns>
    public static IQueryable<T> ApplySpecifications<T>(
        this IQueryable<T> query,
        params ISpecification<T>[] specifications) where T : class
    {
        foreach (var specification in specifications)
        {
            query = specification.Apply(query);
        }
        
        return query;
    }
}
