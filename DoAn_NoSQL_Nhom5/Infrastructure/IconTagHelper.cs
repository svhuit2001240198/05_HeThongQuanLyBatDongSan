using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DoAn_NoSQL_Nhom5.Infrastructure;

[HtmlTargetElement("icon")]
public sealed class IconTagHelper : TagHelper
{
    public string Name { get; set; } = "home";
    private static readonly Dictionary<string, string> Paths = new()
    {
        ["home"] = "M3 10 12 3l9 7M5 9v11h14V9M9 20v-7h6v7",
        ["user"] = "M20 21v-2a7 7 0 0 0-14 0v2M16 7a4 4 0 1 1-8 0 4 4 0 0 1 8 0",
        ["users"] = "M16 21v-2a6 6 0 0 0-12 0v2M14 7a4 4 0 1 1-8 0 4 4 0 0 1 8 0M17 4a4 4 0 0 1 0 8M20 21v-2a6 6 0 0 0-3-5",
        ["heart"] = "M20.8 4.6a5.5 5.5 0 0 0-7.8 0L12 5.7l-1.1-1.1a5.5 5.5 0 0 0-7.8 7.8L12 21l8.8-8.6a5.5 5.5 0 0 0 0-7.8",
        ["bell"] = "M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9M10 21h4",
        ["shield"] = "M12 3 3 7v5c0 5 9 9 9 9s9-4 9-9V7l-9-4M8 12l3 3 5-6",
        ["lock"] = "M6 10h12v11H6V10M8 10V7a4 4 0 0 1 8 0v3M12 14v3",
        ["arrow"] = "M4 12h16M14 6l6 6-6 6",
        ["chevron"] = "m9 5 7 7-7 7",
        ["search"] = "M17 10a7 7 0 1 1-14 0 7 7 0 0 1 14 0m-2 5 6 6",
        ["pin"] = "M20 10c0 6-8 11-8 11S4 16 4 10a8 8 0 1 1 16 0M15 10a3 3 0 1 1-6 0 3 3 0 0 1 6 0",
        ["clock"] = "M22 12a10 10 0 1 1-20 0 10 10 0 0 1 20 0M12 6v6l4 2",
        ["mail"] = "M3 5h18v14H3V5m0 0 9 8 9-8",
        ["phone"] = "M5 3H3v5c0 7 6 13 13 13h5v-5l-5-1-2 3a14 14 0 0 1-8-8l3-2-1-5H5",
        ["check"] = "m5 12 4 4L19 6",
        ["eye"] = "M2 12s4-7 10-7 10 7 10 7-4 7-10 7-10-7-10-7M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0",
        ["star"] = "m12 3 2.8 5.7 6.2.9-4.5 4.4 1.1 6.2L12 17.3 6.4 20.2l1.1-6.2L3 9.6l6.2-.9L12 3",
        ["logout"] = "M9 21H3V3h6M8 12h13m-5-5 5 5-5 5",
        ["edit"] = "m16 3 5 5-12 12-6 1 1-6L16 3M13 6l5 5",
        ["building"] = "M4 21V3h12v18M16 10h4v11M8 7h4M8 11h4M8 15h4M8 21v-3h4v3M2 21h20",
        ["grid"] = "M3 3h7v7H3V3m11 0h7v7h-7V3M3 14h7v7H3v-7m11 0h7v7h-7v-7",
        ["trash"] = "M3 6h18M9 6V3h6v3M5 6l1 15h12l1-15M10 10v7M14 10v7",
        ["menu"] = "M3 6h18M3 12h18M3 18h18",
        ["area"] = "M3 3h18v18H3V3M8 3v4M13 3v3M18 3v4M3 8h4M3 13h3M3 18h4",
        ["info"] = "M22 12a10 10 0 1 1-20 0 10 10 0 0 1 20 0M12 11v6M12 7v1"
    };
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "svg"; output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.RemoveAll("name"); output.Attributes.SetAttribute("viewBox", "0 0 24 24");
        output.Attributes.SetAttribute("class", "icon " + (output.Attributes["class"]?.Value?.ToString() ?? ""));
        output.Attributes.SetAttribute("aria-hidden", "true"); output.Attributes.SetAttribute("fill", "none");
        output.Attributes.SetAttribute("stroke", "currentColor"); output.Attributes.SetAttribute("stroke-width", "1.7");
        output.Attributes.SetAttribute("stroke-linecap", "round"); output.Attributes.SetAttribute("stroke-linejoin", "round");
        output.Content.SetHtmlContent("<path d=\"" + Paths.GetValueOrDefault(Name, Paths["home"]) + "\"></path>");
    }
}
