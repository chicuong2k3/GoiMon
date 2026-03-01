using System.Collections;
using StrawberryShake;

namespace GoiMon.Client.Shared;

public static class GraphQLErrorFormatter
{
    public static string Format(IReadOnlyList<IClientError> errors, string fallback = "Có lỗi xảy ra.")
    {
        if (errors.Count == 0)
            return fallback;

        var baseMessages = new List<string>();
        var detailMessages = new List<string>();

        foreach (var error in errors)
        {
            if (error.Extensions is { Count: > 0 })
            {
                foreach (var extensionMessage in ExtractMessages(error.Extensions))
                {
                    if (!string.IsNullOrWhiteSpace(extensionMessage))
                        detailMessages.Add(extensionMessage);
                }
            }

            var normalizedMessage = Normalize(error.Message);
            var isValidationWrapper = string.Equals(error.Code, "VALIDATION_ERROR", StringComparison.OrdinalIgnoreCase)
                                      && string.Equals(normalizedMessage, "Validation failed", StringComparison.OrdinalIgnoreCase);
            if (isValidationWrapper && detailMessages.Count > 0)
                continue;

            var message = string.IsNullOrWhiteSpace(error.Code)
                ? error.Message
                : $"[{error.Code}] {error.Message}";

            if (!string.IsNullOrWhiteSpace(message))
                baseMessages.Add(message);
        }

        var distinctDetails = detailMessages
            .Select(Normalize)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (distinctDetails.Count > 0)
            return string.Join(" | ", distinctDetails);

        var distinctBase = baseMessages
            .Select(m => m.Trim())
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct()
            .ToList();

        return distinctBase.Count > 0 ? string.Join(" | ", distinctBase) : fallback;
    }

    private static IEnumerable<string> ExtractMessages(IReadOnlyDictionary<string, object?> extensions)
    {
        if (extensions.TryGetValue("errors", out var errorsNode))
        {
            foreach (var message in Flatten(errorsNode))
                yield return message;
        }

        if (extensions.TryGetValue("message", out var messageNode))
        {
            foreach (var message in Flatten(messageNode))
                yield return message;
        }
    }

    private static IEnumerable<string> Flatten(object? node)
    {
        if (node is null)
            yield break;

        if (node is string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
                yield return text;
            yield break;
        }

        if (node is IReadOnlyDictionary<string, object?> readOnlyDict)
        {
            if (readOnlyDict.TryGetValue("message", out var nestedMessage))
            {
                foreach (var message in Flatten(nestedMessage))
                    yield return message;
                yield break;
            }

            foreach (var value in readOnlyDict.Values)
            {
                foreach (var message in Flatten(value))
                    yield return message;
            }
            yield break;
        }

        if (node is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                foreach (var message in Flatten(entry.Value))
                    yield return message;
            }
            yield break;
        }

        if (node is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                foreach (var message in Flatten(item))
                    yield return message;
            }
            yield break;
        }

        var raw = node.ToString();
        if (!string.IsNullOrWhiteSpace(raw))
            yield return raw;
    }

    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return text.Trim().Replace("\n", " ").Replace("\r", " ");
    }
}
