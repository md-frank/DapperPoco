using System.Text.RegularExpressions;

namespace Mondol.DapperPoco.Utils
{
    public class PagingUtil
    {
        private static readonly Regex RegexColumns = new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex RegexDistinct = new Regex(@"\ADISTINCT\s",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex RegexOrderBy = new Regex(
                @"\bORDER\s+BY\s+(?!.*?(?:\)|\s+)AS\s)(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\[\]`""\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\[\]`""\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*",
                RegexOptions.RightToLeft | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        ///     Splits the given <paramref name="sql" /> into <paramref name="parts" />;
        /// </summary>
        /// <param name="sql">The SQL to split.</param>
        /// <param name="parts">The SQL parts.</param>
        /// <returns><c>True</c> if the SQL could be split; else, <c>False</c>.</returns>
        public static bool SplitSql(string sql, out PartedSql parts)
        {
            parts = new PartedSql
            {
                SelectRemovedSql = null,
                CountSql = null,
                OrderBySql = null
            };

            // Extract the columns from "SELECT <whatever> FROM"
            var m = RegexColumns.Match(sql);
            if (!m.Success)
                return false;

            // Save column list and replace with COUNT(*)
            var g = m.Groups[1];
            parts.SelectRemovedSql = sql.Substring(g.Index);

            if (RegexDistinct.IsMatch(parts.SelectRemovedSql))
                parts.CountSql = sql.Substring(0, g.Index) + "COUNT(" + m.Groups[1].ToString().Trim() + ") " + sql.Substring(g.Index + g.Length);
            else
                parts.CountSql = sql.Substring(0, g.Index) + "COUNT(*) " + sql.Substring(g.Index + g.Length);

            // Look for the last "ORDER BY <whatever>" clause not part of a ROW_NUMBER expression
            m = RegexOrderBy.Match(parts.CountSql);
            if (m.Success)
            {
                g = m.Groups[0];
                parts.OrderBySql = g.ToString();
                parts.CountSql = parts.CountSql.Substring(0, g.Index) + parts.CountSql.Substring(g.Index + g.Length);
            }

            return true;
        }

        public static string ReplaceOrderBy(string str1, string str2)
        {
            return RegexOrderBy.Replace(str1, str2, 1);
        }

        public static bool ContainsDistinct(string str)
        {
            return RegexDistinct.IsMatch(str);
        }
    }
}
