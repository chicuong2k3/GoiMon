namespace GoiMon.Api.Infrastructure.Validation;

public sealed class FluentValidationErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        var ex = error.Exception;
        if (ex is null && error.Extensions is not null && error.Extensions.TryGetValue("exception", out var _))
        {
            // nothing
        }

        if (ex is FluentValidation.ValidationException valEx)
        {
            var errors = valEx.Errors.Select(f => new
            {
                field = f.PropertyName,
                message = f.ErrorMessage,
                code = string.IsNullOrWhiteSpace(f.ErrorCode) ? "VALIDATION_ERROR" : f.ErrorCode
            }).ToList();

            var built = ErrorBuilder.New()
                .SetMessage("Validation failed")
                .SetCode("VALIDATION_ERROR")
                .SetExtension("validationErrors", errors)
                .Build();

            return built;
        }

        return error;
    }
}
