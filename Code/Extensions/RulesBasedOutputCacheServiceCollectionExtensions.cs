﻿using IL.RulesBasedOutputCache.Persistence.Rules;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using IL.RulesBasedOutputCache.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IL.RulesBasedOutputCache.Extensions;

public static class RulesBasedOutputCacheServiceCollectionExtensions
{
    public static IServiceCollection AddRulesBasedOutputCache(this IServiceCollection services, IConfiguration config, Action<RulesBasedOutputCacheConfiguration>? setupOptions = null)
    {
        services.AddOutputCache();
        services.AddScoped<IRulesRepository, InMemoryRulesRepository>();
        var section = config.GetSection(Constants.Constants.ConfigurationSection);
        if (section.Exists())
        {
            services.Configure<RulesBasedOutputCacheConfiguration>(section);
            var connectionStringName = section.Get<RulesBasedOutputCacheConfiguration>()?.SqlConnectionStringName;
            if (!string.IsNullOrEmpty(connectionStringName))
            {
                services.AddDbContext<RulesSqlRepository>(options => options.UseSqlServer(config.GetConnectionString(connectionStringName)));
                services.AddScoped<IRulesRepository, RulesSqlRepository>();
            }
        }
        else
        {
            services.Configure(setupOptions ?? RulesBasedOutputCacheConfiguration.Default);
        }

        return services;
    }
}