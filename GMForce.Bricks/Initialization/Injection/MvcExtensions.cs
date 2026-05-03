using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using GMForce.Bricks.Initialization.Filters;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace GMForce.Bricks.Initialization.Injection;

[ExcludeFromCodeCoverage]
public static class MvcExtensions
{
    private static readonly IEnumerable<Func<FilterCollection, IFilterMetadata>> DefaultFilters = [
        filters => filters.Add<RollbackTransactionFilter>(),
        filters => filters.Add<TransactionFilter>()
    ];

    public static IMvcBuilder AddMvc(this IServiceCollection services, IEnumerable<Func<FilterCollection, IFilterMetadata>> filters,
        IEnumerable<JsonConverter> converters, params Assembly[] assemblies)
    {
        var result = services.AddControllers(options =>
        {
            foreach (var filter in filters.Union(DefaultFilters))
            {
                _ = filter(options.Filters);
            }
        })
        .AddJsonOptions(opt =>
        {
            foreach (var converter in converters)
            {
                opt.JsonSerializerOptions.Converters.Add(converter);
            }
        })
        .ConfigureApiBehaviorOptions(options => options.SuppressMapClientErrors = true);
        _ = services.AddFluentValidationAutoValidation()
            .AddFluentValidationClientsideAdapters()
            .AddValidatorsFromAssemblies(assemblies);
        return result;
    }
}
