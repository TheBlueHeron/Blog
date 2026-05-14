using System.Collections.Generic;

namespace LinkDotNet.Blog.Domain;

public class SimilarBlogPost : Entity
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "IList does not expose AddRange")]
    public IList<string> SimilarBlogPostIds { get; set; } = [];
}
