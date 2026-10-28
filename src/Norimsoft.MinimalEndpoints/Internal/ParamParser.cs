using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace Norimsoft.MinimalEndpoints.Internal;

internal enum ParamParseStatus
{
    Success,
    Missing,
    Invalid,
}

/// <summary>
/// Converts raw route and query values. Supports <see cref="string"/>, enums and any type implementing
/// <see cref="IParsable{TSelf}"/>, and their nullable variants. Always parses with the invariant culture.
/// </summary>
internal static class ParamParser
{
    private delegate bool Parser(string value, out object? result);

    // null means the type is not supported
    private static readonly ConcurrentDictionary<Type, Parser?> Parsers = new();

    public static ParamParseStatus Parse<T>(string? raw, out T? value)
    {
        var parser = GetParser(typeof(T)) ?? throw new NotSupportedException(
            $"Parameter type '{typeof(T)}' is not supported. Use string, an enum or a type implementing IParsable<T>, " +
            "e.g. int, decimal, bool, Guid, DateTime, DateOnly or TimeSpan, or their nullable variants.");

        value = default;

        // An empty value, e.g. "?page=", counts as missing
        if (string.IsNullOrEmpty(raw))
        {
            return ParamParseStatus.Missing;
        }

        if (!parser(raw, out var result))
        {
            return ParamParseStatus.Invalid;
        }

        value = (T)result!;
        return ParamParseStatus.Success;
    }

    private static Parser? GetParser(Type type) =>
        Parsers.GetOrAdd(Nullable.GetUnderlyingType(type) ?? type, CreateParser);

    private static Parser? CreateParser(Type type)
    {
        if (type == typeof(string))
        {
            return (string value, out object? result) =>
            {
                result = value;
                return true;
            };
        }

        if (type.IsEnum)
        {
            var isFlags = type.IsDefined(typeof(FlagsAttribute), inherit: false);
            return (string value, out object? result) =>
                Enum.TryParse(type, value, ignoreCase: true, out result) &&
                // Enum.TryParse accepts any number, only allow defined values (or combinations for flags)
                (isFlags || Enum.IsDefined(type, result!));
        }

        // Checked through the interfaces, as MakeGenericType throws for types that don't satisfy IParsable's constraint
        var isParsable = type.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IParsable<>) && i.GenericTypeArguments[0] == type);
        if (isParsable)
        {
            return (Parser)typeof(ParamParser)
                .GetMethod(nameof(CreateParsableParser), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(type)
                .Invoke(null, null)!;
        }

        return null;
    }

    private static Parser CreateParsableParser<T>() where T : IParsable<T> =>
        (string value, out object? result) =>
        {
            var success = T.TryParse(value, CultureInfo.InvariantCulture, out var parsed);
            result = parsed;
            return success;
        };
}
