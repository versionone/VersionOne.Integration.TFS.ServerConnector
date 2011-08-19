using System;
using System.Collections.Generic;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector.Filters {
    public class Filter {
        public readonly string Name;
        public readonly FilterActions Operation;

        private readonly IList<FilterValue> values;

        private Filter(string name, FilterActions operation) : this(name, operation, new List<FilterValue>()) { }

        private Filter(string name, FilterActions operation, IList<FilterValue> values) {
            Name = name;
            Operation = operation;
            this.values = values;
        }

        public static Filter Or(string fieldName) {
            return new Filter(fieldName, FilterActions.Or);
        }

        public static Filter And(string fieldName) {
            return new Filter(fieldName, FilterActions.And);
        }

        public static Filter Empty() {
            return new Filter(null, FilterActions.Or);
        }

        public Filter Equal(object value) {
            values.Add(new FilterValue(value, FilterValuesActions.Equal));
            return this;
        }

        public Filter NotEqual(object value) {
            values.Add(new FilterValue(value, FilterValuesActions.NotEqual));
            return this;
        }

        internal GroupFilterTerm GetFilter(IAssetType type) {
            var terms = new List<IFilterTerm>();

            foreach (var value in values) {
                var term = new FilterTerm(type.GetAttributeDefinition(Name));

                switch(value.Action) {
                    case FilterValuesActions.Equal:
                        term.Equal(value.Value);
                        break;
                    case FilterValuesActions.NotEqual:
                        term.NotEqual(value.Value);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                terms.Add(term);
            }

            return Operation == FilterActions.And ? (GroupFilterTerm) new AndFilterTerm(terms.ToArray()) : new OrFilterTerm(terms.ToArray());
        }
    }
}