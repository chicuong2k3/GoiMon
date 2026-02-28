using FluentValidation;
using HotChocolate.Resolvers;
using HotChocolate.Execution;

namespace GoiMon.Api.Infrastructure.Validation;

public sealed class FluentValidationMiddleware
{
    private readonly FieldDelegate _next;

    public FluentValidationMiddleware(FieldDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(IMiddlewareContext context)
    {
        // Validate each argument that has a registered FluentValidation.IValidator<T>
        var validationErrors = new List<object>();

        foreach (var arg in context.Selection.Field.Arguments)
        {
            var value = context.ArgumentValue<object?>(arg.Name);
            if (value is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(value.GetType());
            var validator = context.Services.GetService(validatorType) as IValidator;
            if (validator is null) continue;

            var validationContextType = typeof(ValidationContext<>).MakeGenericType(value.GetType());
            var validationContext = Activator.CreateInstance(validationContextType, value)!;

            var validateAsync = validator.GetType().GetMethod("ValidateAsync", new[] { validationContextType, typeof(CancellationToken) });
            if (validateAsync is null) continue;

            var task = (Task?)validateAsync.Invoke(validator, new object[] { validationContext, CancellationToken.None });
            if (task is null) continue;

            await task.ConfigureAwait(false);

            var result = task.GetType().GetProperty("Result")?.GetValue(task) as FluentValidation.Results.ValidationResult;
            if (result is not null && !result.IsValid)
            {
                foreach (var failure in result.Errors)
                {
                    validationErrors.Add(new Dictionary<string, object>
                    {
                        ["field"] = string.IsNullOrWhiteSpace(failure.PropertyName) ? arg.Name : $"{arg.Name}.{failure.PropertyName}",
                        ["message"] = failure.ErrorMessage,
                        ["code"] = string.IsNullOrWhiteSpace(failure.ErrorCode) ? "VALIDATION_ERROR" : failure.ErrorCode
                    });
                }
            }
        }

        if (validationErrors.Count > 0)
        {
            var error = ErrorBuilder.New()
                .SetMessage("Validation failed")
                .SetCode("VALIDATION_ERROR")
                .SetExtension("validationErrors", validationErrors)
                .Build();

            throw new QueryException(error);
        }

        await _next(context).ConfigureAwait(false);
    }
}
