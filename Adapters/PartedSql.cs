namespace Mondol.DapperPoco
{
    /// <summary>
    ///     Presents the SQL parts.
    /// </summary>
    public struct PartedSql
    {
        /// <summary>
        /// Raw Sql
        /// </summary>
        public string RawSql;

        /// <summary>
        ///     The SQL count.
        /// </summary>
        public string CountSql;

        /// <summary>
        ///     The SQL Select
        /// </summary>
        public string SelectRemovedSql;

        /// <summary>
        ///     The SQL Order By
        /// </summary>
        public string OrderBySql;
    }
}
