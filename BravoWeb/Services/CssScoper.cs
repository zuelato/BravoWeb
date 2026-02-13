using System.Text.RegularExpressions;

namespace BravoWeb.Services;

/// <summary>
/// Scopes CSS rules so they only apply within a specific container element.
/// Prefixes every selector with the given scope selector (e.g. "#fragment-42").
/// </summary>
public static partial class CssScoper
{
    /// <summary>
    /// Prefix every CSS rule's selector(s) with <paramref name="scopeSelector"/>.
    /// Handles nested selectors, media queries, and keyframes.
    /// </summary>
    public static string Scope(string css, string scopeSelector)
    {
        if (string.IsNullOrWhiteSpace(css) || string.IsNullOrWhiteSpace(scopeSelector))
            return css;

        return ScopeRules(css, scopeSelector);
    }

    private static string ScopeRules(string css, string scope)
    {
        var result = new System.Text.StringBuilder();
        int i = 0;

        while (i < css.Length)
        {
            // Skip whitespace
            if (char.IsWhiteSpace(css[i])) { result.Append(css[i++]); continue; }

            // Skip comments
            if (i + 1 < css.Length && css[i] == '/' && css[i + 1] == '*')
            {
                int end = css.IndexOf("*/", i + 2, StringComparison.Ordinal);
                if (end == -1) end = css.Length - 2;
                result.Append(css, i, end + 2 - i);
                i = end + 2;
                continue;
            }

            // At-rules: @media, @supports → recurse into their block
            if (css[i] == '@')
            {
                int braceStart = css.IndexOf('{', i);
                if (braceStart == -1) { result.Append(css[i..]); break; }

                string atRule = css[i..braceStart].Trim();

                // @keyframes / @font-face — pass through without scoping
                if (atRule.StartsWith("@keyframes", StringComparison.OrdinalIgnoreCase) ||
                    atRule.StartsWith("@font-face", StringComparison.OrdinalIgnoreCase))
                {
                    int blockEnd = FindClosingBrace(css, braceStart);
                    result.Append(css, i, blockEnd + 1 - i);
                    i = blockEnd + 1;
                    continue;
                }

                // @media, @supports, @layer etc — recurse into the block content
                int innerEnd = FindClosingBrace(css, braceStart);
                string innerCss = css[(braceStart + 1)..innerEnd];
                result.Append(atRule);
                result.Append(" {\n");
                result.Append(ScopeRules(innerCss, scope));
                result.Append("}\n");
                i = innerEnd + 1;
                continue;
            }

            // Normal rule: selector { ... }
            int ruleBodyStart = css.IndexOf('{', i);
            if (ruleBodyStart == -1) { result.Append(css[i..]); break; }

            string selectorPart = css[i..ruleBodyStart].Trim();
            int ruleBodyEnd = FindClosingBrace(css, ruleBodyStart);
            string body = css[(ruleBodyStart + 1)..ruleBodyEnd];

            // Scope each comma-separated selector
            var selectors = selectorPart.Split(',');
            for (int s = 0; s < selectors.Length; s++)
            {
                var sel = selectors[s].Trim();
                if (string.IsNullOrEmpty(sel)) continue;

                if (s > 0) result.Append(",\n");

                // Don't scope :root or html/body — replace them with the scope
                if (sel == ":root" || sel.Equals("html", StringComparison.OrdinalIgnoreCase) ||
                    sel.Equals("body", StringComparison.OrdinalIgnoreCase))
                {
                    result.Append(scope);
                }
                else
                {
                    result.Append(scope).Append(' ').Append(sel);
                }
            }
            result.Append(" {").Append(body).Append("}\n");
            i = ruleBodyEnd + 1;
        }

        return result.ToString();
    }

    private static int FindClosingBrace(string css, int openBrace)
    {
        int depth = 1;
        int i = openBrace + 1;
        while (i < css.Length && depth > 0)
        {
            if (css[i] == '{') depth++;
            else if (css[i] == '}') depth--;
            if (depth > 0) i++;
        }
        return i < css.Length ? i : css.Length - 1;
    }
}
