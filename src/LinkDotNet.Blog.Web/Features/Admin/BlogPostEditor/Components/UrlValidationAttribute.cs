using System;
using System.ComponentModel.DataAnnotations;

namespace LinkDotNet.Blog.Web.Features.Admin.BlogPostEditor.Components;

[AttributeUsage(AttributeTargets.Property)]
public sealed class UrlValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string url || string.IsNullOrEmpty(url))
        {
            return ValidationResult.Success;
        }

        var isAbsolute = Uri.TryCreate(url, UriKind.Absolute, out var absolute)
            && (absolute.Scheme == Uri.UriSchemeHttps || absolute.Scheme == Uri.UriSchemeHttp);
        var isRootRelative = url.StartsWith('/');

        return isAbsolute || isRootRelative
            ? ValidationResult.Success
            : new ValidationResult("Please enter a valid URL (e.g. https://example.com/image.webp) or a root-relative path (e.g. /assets/image.webp).");
    }
}
