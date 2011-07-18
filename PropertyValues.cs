using System.Collections;
using System.Collections.Generic;
using System.Text;
using VersionOne.SDK.APIClient;

namespace VersionOne.ServerConnector {
    public class PropertyValues : IEnumerable<ValueId> {

        private readonly Dictionary<Oid, ValueId> dictionary = new Dictionary<Oid, ValueId>();

        public PropertyValues(IEnumerable valueIds) {
            foreach (ValueId id in valueIds) {
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
            var dataBuilder = new StringBuilder();
            var isFirst = true;
            foreach (var value in this) {
                if (!isFirst) {
                    dataBuilder.Append(", ");
                } else {
                    isFirst = false;
                }
                dataBuilder.Append(value);
            }

            return dataBuilder.ToString();
        }

        internal ValueId Find(Oid oid) {
            return dictionary[oid.Momentless];
        }

        public ValueId Find(string token) {
            foreach(var id in dictionary) {
                if (token.Equals(id.Key.Momentless.Token)) {
                    return id.Value;
                }
            }
            return null;
        }

        public int Count {
            get { return dictionary.Count; }
        }

        internal bool ContainsOid(Oid value) {
            return dictionary.ContainsKey(value.Momentless);
        }

        public bool Contains(ValueId valueId) {
            return dictionary.ContainsValue(valueId);
        }

        public ValueId[] ToArray() {
            var values = new ValueId[Count];
            dictionary.Values.CopyTo(values, 0);
            return values;
        }

        internal void Add(ValueId value) {
            dictionary.Add(value.Oid, value);
        }

        internal PropertyValues Subset(IEnumerable oids) {
            var result = new PropertyValues();
            foreach (Oid oid in oids) {
                result.Add(Find(oid));
            }

            return result;
        }
    }
}
