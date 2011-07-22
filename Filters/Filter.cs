using System.Collections.Generic;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Filters {
    public class Filter {

        public string Name { get; set; }
        public FilterActions Operation { get; set; }
        public IList<FilterValue> Values = new List<FilterValue>();

        protected internal GroupFilterTerm GetFilter(IAssetType type) {
            var terms = new List<IFilterTerm>();
            foreach (var value in Values) {
                var term = new FilterTerm(type.GetAttributeDefinition(Name));

                switch(value.Action) {
                    case FilterValuesActions.Equal:
                        term.Equal(value.Value);
                        break;
                    case FilterValuesActions.NotEqual:
                        term.NotEqual(value.Value);
                        break;
                }

                terms.Add(term);
            }

            return Operation == FilterActions.And
                ? (GroupFilterTerm) new AndFilterTerm(terms.ToArray())
                : new OrFilterTerm(terms.ToArray());
        }
    }
}
