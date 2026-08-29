using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DatingApp.API.Shared
{
    public static class ModelStateValidator
    {
        public static IActionResult ValidModelState(ActionContext context)
        {
            var entryPair = context.ModelState.First(x => x.Value?.Errors.Count > 0);
            var fieldName = entryPair.Key;
            var entry = entryPair.Value;

            var path = NormaliseField(fieldName);
            var error = Error.Deserialize(entry.Errors[0].ErrorMessage);

            if (error is null)
            {
                return new BadRequestObjectResult(Error.Errors.General.InvalidFieldDataType(path));
            }

            // The Error carries a flat name ("username"), but ModelState knows where the
            // failure actually lives. A client form can only locate a nested control from
            // the full path, so prefer it and keep the Error's code and message.
            return new BadRequestObjectResult(new Error(error.Code, error.Message, path));
        }

        private static string NormaliseField(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return "request";
            }

            var name = fieldName.StartsWith("$.", StringComparison.Ordinal)
                ? fieldName.Substring(2)
                : fieldName;

            // Lines[0].Sku -> Lines.0.Sku
            name = name.Replace("[", ".").Replace("]", string.Empty);

            // Lines.0.Sku -> lines.0.sku, leaving the index alone
            return string.Join(".",
                name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Select(CamelCase));
        }

        private static string CamelCase(string segment) =>
            segment.Length == 0 || !char.IsUpper(segment[0])
                ? segment
                : char.ToLowerInvariant(segment[0]) + segment.Substring(1);
    }
}
