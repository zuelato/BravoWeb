using System.Text.RegularExpressions;

namespace BravoWeb.Services;

// scopes css rules to a container → prefixes selectors with e.g. "#fragment-42"
public static partial class CssScopers
{
    // css + scope selector → scoped css
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
            if (char.IsWhiteSpace(css[i])) { result.Append(css[i++]); continue; }

            // skip comments
            if (i + 1 < css.Length && css[i] == '/' && css[i + 1] == '*')
            {
                int end = css.IndexOf("*/", i + 2, StringComparison.Ordinal);
                if (end == -1) end = css.Length - 2;
                result.Append(css, i, end + 2 - i);
                i = end + 2;
                continue;
            }

            // at-rules → recurse into block
            if (css[i] == '@')
            {
                int braceStart = css.IndexOf('{', i);
                if (braceStart == -1) { result.Append(css[i..]); break; }

                string atRule = css[i..braceStart].Trim();

                // @keyframes / @font-face → pass through
                if (atRule.StartsWith("@keyframes", StringComparison.OrdinalIgnoreCase) ||
                    atRule.StartsWith("@font-face", StringComparison.OrdinalIgnoreCase))
                {
                    int blockEnd = FindClosingBrace(css, braceStart);
                    result.Append(css, i, blockEnd + 1 - i);
                    i = blockEnd + 1;
                    continue;
                }

                // @media, @supports etc → recurse
                int innerEnd = FindClosingBrace(css, braceStart);
                string innerCss = css[(braceStart + 1)..innerEnd];
                result.Append(atRule);
                result.Append(" {\n");
                result.Append(ScopeRules(innerCss, scope));
                result.Append("}\n");
                i = innerEnd + 1;
                continue;
            }

            // normal rule → scope selectors
            int ruleBodyStart = css.IndexOf('{', i);
            if (ruleBodyStart == -1) { result.Append(css[i..]); break; }

            string selectorPart = css[i..ruleBodyStart].Trim();
            int ruleBodyEnd = FindClosingBrace(css, ruleBodyStart);
            string body = css[(ruleBodyStart + 1)..ruleBodyEnd];

            var selectors = selectorPart.Split(',');
            for (int s = 0; s < selectors.Length; s++)
            {
                var sel = selectors[s].Trim();
                if (string.IsNullOrEmpty(sel)) continue;

                if (s > 0) result.Append(",\n");

                // :root / html / body → replace with scope
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
