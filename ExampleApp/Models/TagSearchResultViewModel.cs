using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExampleApp.Models {
    /// <summary>
    /// Describes the results of a tag search.
    /// </summary>
    public class TagSearchResultViewModel {

        /// <summary>
        /// Gets or sets the data source name.
        /// </summary>
        public string DataSourceName { get; set; }

        /// <summary>
        /// Gets or sets the search filter that was used.
        /// </summary>
        public DataCore.Client.Model.TagSearchFilter Filter { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        public IEnumerable<DataCore.Client.Model.TagSearchResult> Tags { get; set; }

    }
}