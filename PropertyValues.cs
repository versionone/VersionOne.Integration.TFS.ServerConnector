using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector {
    public class PropertyValues : IEnumerable<ValueId> {
        private readonly IDictionary<Oid, ValueId> dictionary = new Dictionary<Oid, ValueId>();

        public PropertyValues(IEnumerable<ValueId> valueIds) {
            foreach (var id in valueIds) {
                Add(id);
            }
        }

        public PropertyValues() { }

        public IEnumerator<ValueId> GetEnumerator() {
            return dictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public override string ToString() {
            return string.Join(", ", this.Select(item => item.ToString()).ToArray());
        }

        public ValueId Find(string token) {
            return dictionary.Where(id => token.Equals(id.Key.Momentless.Token)).Select(id => id.Value).FirstOrDefault();
        }

        internal void Add(ValueId value) {
            dictionary.Add(value.Oid, value);
        }
    }
}