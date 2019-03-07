using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExampleApp.Models {
    /// <summary>
    /// Describes the result of a snapshot data query.
    /// </summary>
    public class TagSnapshotValuesViewModel {

        /// <summary>
        /// Gets or sets a lookup from qualified data source name to display name.
        /// </summary>
        public IDictionary<string, string> DataSourceDisplayNameLookup { get; set; }

        /// <summary>
        /// Gets or sets the snapshot tag values.
        /// </summary>
        public IDictionary<string, DataCore.Client.Model.SnapshotTagValueDictionary> Values { get; set; }

    }
}