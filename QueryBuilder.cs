using System;
using System.Collections.Generic;
using VersionOne.SDK.APIClient;
using VersionOne.ServerConnector.Entities;

namespace VersionOne.ServerConnector {
    internal class QueryBuilder : IQueryBuilder {
        private IServices services;
        private IMetaModel metaModel;
        
        private readonly LinkedList<AttributeInfo> attributesToQuery = new LinkedList<AttributeInfo>();
        public IDictionary<string, PropertyValues> ListPropertyValues { get; private set; }

        public void Setup(IServices services, IMetaModel metaModel) {
            this.services = services;
            this.metaModel = metaModel;
            ListPropertyValues = GetListPropertyValues();
        }

        public void AddProperty(string attr, string prefix, bool isList) {
            attributesToQuery.AddLast(new AttributeInfo(attr, prefix, isList, false));
        }

        /// <summary>
        /// Add not list property which can be doesn't exist at start.
        /// </summary>
        /// <param name="attr">Attribute name</param>
        /// <param name="prefix">attribute type</param>
        public void AddOptionalProperty(string attr, string prefix) {
            attributesToQuery.AddLast(new AttributeInfo(attr, prefix, false, true));
        }

        public AssetList Query(string workitemTypeName, IFilterTerm filter) {
            try {
                var workitemType = metaModel.GetAssetType(workitemTypeName);
                var query = new Query(workitemType) { Filter = filter};

                AddSelection(query, workitemTypeName, workitemType);
                return services.Retrieve(query).Assets;
            } catch (Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }

        private void AddSelection(Query query, string typePrefix, IAssetType type) {
            foreach (var attrInfo in attributesToQuery) {
                if(attrInfo.Prefix != typePrefix) {
                    continue;
                }
                
                IAttributeDefinition def;
                // this was made for not to miss incorrect fields
                if (attrInfo.IsOptional) {
                    try {
                        def = type.GetAttributeDefinition(attrInfo.Attr);                        
                    } catch(MetaException) {
                        continue;
                    }
                } else {
                    def = type.GetAttributeDefinition(attrInfo.Attr);
                }                

                query.Selection.Add(def);
            }
        }

        private IDictionary<string, PropertyValues> GetListPropertyValues() {
            var res = new Dictionary<string, PropertyValues>(attributesToQuery.Count);

            foreach(var attrInfo in attributesToQuery) {
                if(!attrInfo.IsList) {
                    continue;
                }

                var propertyAlias = attrInfo.Attr;
                
                if(!attrInfo.Attr.StartsWith("Custom_")) {
                    propertyAlias = attrInfo.Prefix + propertyAlias;
                }
                
                if(res.ContainsKey(propertyAlias)) {
                    continue;
                }
                
                var propertyName = ResolvePropertyKey(propertyAlias);

                PropertyValues values;
                
                if(res.ContainsKey(propertyName)) {
                    values = res[propertyName];
                } else {
                    values = QueryPropertyValues(propertyName);
                    res.Add(propertyName, values);
                }

                if(!res.ContainsKey(propertyAlias)) {
                    res.Add(propertyAlias, values);
                }
            }

            return res;
        }

        public PropertyValues QueryPropertyValues(string propertyName) {
            var res = new PropertyValues();
            IAttributeDefinition nameDef;
            var query = GetPropertyValuesQuery(propertyName, out nameDef);

            foreach (var asset in services.Retrieve(query).Assets) {
                var name = asset.GetAttribute(nameDef).Value as string;
                res.Add(new ValueId(asset.Oid, name));
            }

            return res;
        }

        private Query GetPropertyValuesQuery(string propertyName, out IAttributeDefinition nameDef) {
            var assetType = metaModel.GetAssetType(propertyName);
            nameDef = assetType.GetAttributeDefinition(Entity.NameAttribute);

            IAttributeDefinition inactiveDef;

            var query = new Query(assetType);
            query.Selection.Add(nameDef);

            if (assetType.TryGetAttributeDefinition(Entity.InactiveAttribute, out inactiveDef)) {
                var filter = new FilterTerm(inactiveDef);
                filter.Equal("False");
                query.Filter = filter;
            }

            query.OrderBy.MajorSort(assetType.DefaultOrderBy, OrderBy.Order.Ascending);
            return query;
        }

        private static string ResolvePropertyKey(string propertyAlias) {
            switch (propertyAlias) {
                case "DefectStatus":
                    return "StoryStatus";
                case "DefectSource":
                    return "StorySource";
                case "ScopeBuildProjects":
                    return "BuildProject";
                case "TaskOwners":
                case "StoryOwners":
                case "DefectOwners":
                case "TestOwners":
                case "ThemeOwners":
                    return "Member";
                case "PrimaryWorkitemPriority":                      
                    return "WorkitemPriority";
            }

            return propertyAlias;
        }
    }
}