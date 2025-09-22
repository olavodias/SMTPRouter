#pragma warning disable IDE0270 // Use coalesce expression

using SMTPRouter.ConfigurationSchema;
using SMTPRouter.RoutingRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Router;

/// <summary>
/// The Object to Create Rules
/// </summary>
public static class RuleFactory
{
    private readonly static Dictionary<string, Type> _registeredTypes = new()
    {
        { "MailFromDomainRoutingRule", typeof(MailFromDomainRoutingRule) },
        { "MailFromRegexMatchRoutingRule", typeof(MailFromRegexMatchRoutingRule) },
        { "RelayRoutingRule", typeof(RelayRoutingRule) },
    };
    public static IReadOnlyDictionary<string, Type> RegisteredTypes
    {
        get
        {
            return _registeredTypes;
        }
    }

    /// <summary>
    /// Register all types inside the assembly as long as they implement the <see cref="IRoutingRule"/> interface
    /// </summary>
    /// <param name="assemblyWithTypes">The assembly to search upon</param>
    public static void RegisterTypes(Assembly assemblyWithTypes)
    {
        var types = assemblyWithTypes.GetTypes()
                                     .Where(t => typeof(IRoutingRule).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                                     .ToArray();

        if (types is null) return;

        foreach (var type in types)
        {
            if (!string.IsNullOrWhiteSpace(type.FullName))
                _registeredTypes.TryAdd(type.FullName, type);
        }
    }

    /// <summary>
    /// Creates a <see cref="IRoutingRule"/> based on the <paramref name="ruleDefinition"/>
    /// </summary>
    /// <param name="ruleDefinition">The definition to create the rule</param>
    /// <returns></returns>
    /// <exception cref="TypeLoadException">Thrown when there are issues loading the type, or the type is invalid</exception>
    /// <exception cref="MissingMemberException">Thrown when unable to locate a property</exception>
    /// <exception cref="MemberAccessException">Thrown when a property defined in the parameters is read-only</exception>
    /// <exception cref="ArgumentException">Thrown when unable to set the value of a property</exception>
    public static IRoutingRule Create(Rule ruleDefinition)
    {
        // Get the type based on the string description
        if (string.IsNullOrWhiteSpace(ruleDefinition.Type))
            throw new TypeLoadException($"The type specified on \"{ruleDefinition.ConnectionKey}\" is null or empty.");
        
        if (!RuleFactory.RegisteredTypes.TryGetValue(ruleDefinition.Type, out var routingRuleType))
            throw new TypeLoadException($"The type \"{ruleDefinition.Type}\", specified on \"{ruleDefinition.ConnectionKey}\", could not be located.");

        if (!routingRuleType.GetInterfaces().Contains(typeof(IRoutingRule)))
            throw new TypeLoadException($"The type \"{ruleDefinition.Type}\", specified on \"{ruleDefinition.ConnectionKey}\", cannot be used. A Routing Rule must implement the interface \"{nameof(IRoutingRule)}\".");

        if (!typeof(IRoutingRule).IsAssignableFrom(routingRuleType))
            throw new TypeLoadException($"The type \"{ruleDefinition.Type}\", specified on \"{ruleDefinition.ConnectionKey}\", cannot be used. A Routing Rule must implement the interface \"{nameof(IRoutingRule)}\".");

        // Create the Routing Rule Object
        if (Activator.CreateInstance(routingRuleType) is not IRoutingRule routingRule)
            throw new TypeLoadException($"An object of type \"{ruleDefinition.Type}\", specified on \"{ruleDefinition.ConnectionKey}\", could not be created.");

        // Set Parameters
        if (ruleDefinition.Parameters is not null)
        {
            foreach (var p in ruleDefinition.Parameters)
            {
                var propertyInfo = routingRule.GetType().GetProperty(p.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (propertyInfo is null)
                    throw new MissingMemberException($"The property \"{p.Key}\" does not exist on type \"{routingRuleType.Name}\"");

                if (!propertyInfo.CanWrite)
                    throw new MemberAccessException($"The property \"{p.Key}\" is read-only on type \"{routingRuleType.Name}\"");

                try
                {
                    propertyInfo.SetValue(routingRule, Convert.ChangeType(p.Value, propertyInfo.PropertyType));
                }
                catch (Exception e)
                {
                    throw new ArgumentException($"Unable to set property \"{p.Key}\" to \"{p.Value}\" on type \"{routingRuleType.Name}\"", e);
                }
            }
        }

        return routingRule;
    }

}

#pragma warning restore IDE0270 // Use coalesce expression
