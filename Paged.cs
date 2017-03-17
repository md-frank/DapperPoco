using System.Collections.Generic;

namespace Mondol.DapperPoco
{
    /// <summary>
    ///     Holds the results of a paged request.
    /// </summary>
    /// <typeparam name="T">The type of Poco in the returned result set</typeparam>
    public class Paged<T> where T : new()
    {
        /// <summary>
        ///     The current page number contained in this page of result set
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        ///     The total number of pages in the full result set
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        ///     The total number of records in the full result set
        /// </summary>
        public long TotalItems { get; set; }

        /// <summary>
        ///     The number of items per page
        /// </summary>
        public int ItemsPerPage { get; set; }

        /// <summary>
        ///     The actual records on this page
        /// </summary>
        public List<T> Items { get; set; }
    }
}
