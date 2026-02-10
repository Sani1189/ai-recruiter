using System.Reflection;

namespace Recruiter.Infrastructure.Extensions;

public static class DynamicQueryExtensions
{
    public static IQueryable<object> WhereDynamic(this IQueryable<object> query, List<PropertyInfo> props, string name, int version)
    {
        // naive filter (Name + Version)
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(object), "x");
        System.Linq.Expressions.Expression? body = null;

        foreach (var prop in props)
        {
            // Cast x to the actual type
            var casted = System.Linq.Expressions.Expression.Convert(parameter, prop.DeclaringType ?? typeof(object));
            var propertyExpr = System.Linq.Expressions.Expression.Property(casted, prop.Name);

            System.Linq.Expressions.Expression? comparison = null;
            if (prop.Name.EndsWith("Name"))
            {
                comparison = System.Linq.Expressions.Expression.Equal(
                    propertyExpr,
                    System.Linq.Expressions.Expression.Constant(name)
                );
            }
            else if (prop.Name.EndsWith("Version"))
            {
                comparison = System.Linq.Expressions.Expression.Equal(
                    propertyExpr,
                    System.Linq.Expressions.Expression.Constant(version)
                );
            }

            if (comparison != null)
            {
                body = body == null ? comparison : System.Linq.Expressions.Expression.AndAlso(body, comparison);
            }
        }

        if (body == null)
        {
            // No filter, return original query
            return query;
        }

        var lambda = System.Linq.Expressions.Expression.Lambda<Func<object, bool>>(body, parameter);
        return query.Where(lambda).AsQueryable();
    }
}