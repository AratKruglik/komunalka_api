using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KomunalkaAPI.Extensions;

public static class ModelStateExtensions
{
    public static List<string> ToErrorList(this ModelStateDictionary modelState)
    {
        var errors = new List<string>();
        foreach (var kvp in modelState)
        {
            foreach (var error in kvp.Value.Errors)
            {
                var message = string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Invalid value" : error.ErrorMessage;
                if (!string.IsNullOrWhiteSpace(kvp.Key))
                {
                    message = $"{kvp.Key}: {message}";
                }
                errors.Add(message);
            }
        }
        return errors;
    }
}