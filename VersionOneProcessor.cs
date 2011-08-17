using System;
using System.Collections.Generic;
using System.Linq;
using VersionOne.SDK.APIClient;
using VersionOne.ServerConnector.Entities;
using VersionOne.ServerConnector.Filters;
using VersionOne.ServiceHost.Core.Logging;
using System.Xml;
using System.Collections;

namespace VersionOne.ServerConnector {
    // TODO extract hardcoded strings to constants
    // TODO this one is getting huge - it should be split
    public class VersionOneProcessor : IVersionOneProcessor {
        public const string FeatureGroupType = "Theme";
        public const string StoryType = "Story";
        public const string PrimaryWorkitemType = "PrimaryWorkitem";
        public const string MemberType = "Member";
        public const string LinkType = "Link";
        public const string AttributeDefinitionType = "AttributeDefinition";

        private const string OwnersAttribute = "Owners";
        private const string AssetStateAttribute = "AssetState";
        private const string ScopeAttribute = "Scope";
        private const string ParentAttribute = "Parent";
        private const string IdAttribute = "ID";
        private const string StatusAttribute = "Status";
        private const string ParentAndUpAttribute = "ParentAndUp";
        private const string AssetTypeAttribute = "AssetType";
        private const string NameAttribute = "Name";
        private const string AssetAttribute = "Asset";
        private const string UrlAttribute = "URL";
        private const string OnMenuAttribute = "OnMenu";
        private const string InactiveAttribute = "Inactive";

        private IServices services;
        private IMetaModel metaModel;
        private ILocalizer localizer;
        private readonly ILogger logger;
        private readonly XmlElement configuration;
        
        private readonly LinkedList<AttributeInfo> attributesToQuery = new LinkedList<AttributeInfo>();

        public IDictionary<string, PropertyValues> ListPropertyValues { get; private set; }

        public VersionOneProcessor(XmlElement config, ILogger logger) {
            configuration = config;
            this.logger = logger;
        }

        private void Connect() {
            var connector = new V1Central(configuration);
            connector.Validate();
            services = connector.Services;
            metaModel = connector.MetaModel;
            localizer = connector.Loc;
            ListPropertyValues = GetListPropertyValues();
        }

        public bool ValidateConnection() {
            try {
                Connect();                
            } catch(Exception ex) {
                logger.Log(LogMessage.SeverityType.Error, "Connection is not valid. " + ex.Message);
                return false;
            }

            return true;
        }

        public IList<PrimaryWorkitem> GetWorkitemsByProjectId(string projectId) {
            var workitemType = metaModel.GetAssetType(PrimaryWorkitemType);

            var projectOid = Oid.FromToken(projectId, metaModel);
            var scopeTerm = new FilterTerm(workitemType.GetAttributeDefinition(ScopeAttribute));
            scopeTerm.Equal(projectOid);

            var stateTerm = new FilterTerm(workitemType.GetAttributeDefinition(AssetStateAttribute));
            stateTerm.NotEqual(AssetState.Closed);

            return RetrieveData(PrimaryWorkitemType, new AndFilterTerm(scopeTerm, stateTerm)).Select(asset => new PrimaryWorkitem(asset, ListPropertyValues)).ToList();
        }

        //TODO we can remove this method using filter
        public IList<PrimaryWorkitem> GetClosedWorkitemsByProjectId(string projectId) {
            var workitemType = metaModel.GetAssetType(PrimaryWorkitemType);

            var projectOid = Oid.FromToken(projectId, metaModel);
            var scopeTerm = new FilterTerm(workitemType.GetAttributeDefinition(ScopeAttribute));
            scopeTerm.Equal(projectOid);

            var stateTerm = new FilterTerm(workitemType.GetAttributeDefinition(AssetStateAttribute));
            stateTerm.Equal(AssetState.Closed);

            return RetrieveData(PrimaryWorkitemType, new AndFilterTerm(scopeTerm, stateTerm)).Select(asset => new PrimaryWorkitem(asset, ListPropertyValues)).ToList();
        }

        public IList<FeatureGroup> GetFeatureGroupsByProjectId(string projectId, Filter filters, Filter childrenFilters) {
            var featureGroupType = metaModel.GetAssetType(FeatureGroupType);
            var ownersDefinition = featureGroupType.GetAttributeDefinition(OwnersAttribute);

            var projectOid = Oid.FromToken(projectId, metaModel);
            var scopeTerm = new FilterTerm(featureGroupType.GetAttributeDefinition(ScopeAttribute));
            scopeTerm.Equal(projectOid);
            var assetTypeTerm = new FilterTerm(featureGroupType.GetAttributeDefinition(ParentAttribute));
            assetTypeTerm.Equal(string.Empty);

            var terms = new AndFilterTerm(scopeTerm, assetTypeTerm);
            var customTerm = filters.GetFilter(featureGroupType);
            if (customTerm.HasTerms) {
                terms.And(customTerm);
            } 

            return RetrieveData(FeatureGroupType, terms).Select(asset => new FeatureGroup(asset, ListPropertyValues, GetFeatureGroupStoryChildren(asset.Oid.Momentless.Token.ToString(), childrenFilters).Cast<Workitem>().ToList(), GetMembersByIds(asset.GetAttribute(ownersDefinition).ValuesList))).ToList();
        }

        public IList<Member> GetMembersByIds(IList oids) {
            if (oids.Count == 0) {
                return new List<Member>();
            }
            var memberType = metaModel.GetAssetType(MemberType);

            var terms = new OrFilterTerm();
            foreach(var oid in oids) {
                var term = new FilterTerm(memberType.GetAttributeDefinition(IdAttribute));
                term.Equal(oid);
                terms.Or(term);
            }
            var members = RetrieveData(MemberType, terms).Select(asset => new Member(asset)).ToList();
            return members;
        }

        public IList<FieldInfo> GetFieldsList(string type) {
            var attrType = metaModel.GetAssetType(AttributeDefinitionType);
            var assetType = metaModel.GetAssetType(type);
            
            var termType = new FilterTerm(attrType.GetAttributeDefinition("Asset.AssetTypesMeAndDown.Name"));
            termType.Equal(type);
            IAttributeDefinition inactiveDef;
            FilterTerm termState = null;
            if (assetType.TryGetAttributeDefinition(InactiveAttribute, out inactiveDef)) {
                termState = new FilterTerm(inactiveDef);
                termState.Equal("False");
            }
            return RetrieveData(AttributeDefinitionType, new AndFilterTerm(termType, termState)).
                Select(x =>new FieldInfo(x, GetLocalizeString(assetType.DisplayName))).ToList();
        }

        public PropertyValues GetValuesForType(string typeName) {
            if (!ListPropertyValues.ContainsKey(typeName)) {
                ListPropertyValues.Add(typeName, QueryPropertyOidValues(typeName));
            }           
            return ListPropertyValues[typeName];
        }

        private AssetList RetrieveData(string workitemTypeName, IFilterTerm filter) {
            try {
                var workitemType = metaModel.GetAssetType(workitemTypeName);
                var query = new Query(workitemType) { Filter = filter};

                AddSelection(query, workitemTypeName, workitemType);
                return services.Retrieve(query).Assets;

            } catch (Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }

        private string GetLocalizeString(string resourceString) {
            return localizer.Resolve(resourceString);
        }

        // TODO remove this from library code
        public virtual IList<string> GetAssetTypes() {
            return new[] { "Story", "Defect" };
        }

        public void SaveWorkitems(IEnumerable<Workitem> workitems) {
            var assetList = new AssetList();

            if(workitems == null) {
                return;
            }

            assetList.AddRange(workitems.Select(workitem => workitem.Asset));
            services.Save(assetList);
        }

        public void CloseWorkitem(PrimaryWorkitem workitem) {
            try {
                var closeOperation = workitem.Asset.AssetType.GetOperation("Inactivate");
                services.ExecuteOperation(closeOperation, workitem.Asset.Oid);
            } catch(Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }

        public IList<KeyValuePair<string, string>> GetWorkitemStatuses() {
            try {
                return QueryPropertyValues("StoryStatus");
            } catch(Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }
        
        public void SetWorkitemStatus(PrimaryWorkitem workitem, string statusId) {
            try {
                var primaryWorkitemType = metaModel.GetAssetType(PrimaryWorkitemType);
                var statusAttributeDefinition = primaryWorkitemType.GetAttributeDefinition(StatusAttribute);

                workitem.Asset.SetAttributeValue(statusAttributeDefinition, Oid.FromToken(statusId, metaModel));
                services.Save(workitem.Asset);
            } catch(Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }

        public KeyValuePair<string, string> CreateWorkitemStatus(string statusName) {
            try {
                var primaryWorkitemStatusType = metaModel.GetAssetType("StoryStatus");
                var status = new Asset(primaryWorkitemStatusType);
                status.SetAttributeValue(primaryWorkitemStatusType.NameAttribute, statusName);
                services.Save(status);

                return new KeyValuePair<string, string>(statusName, status.Oid.ToString());
            } catch(Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }

        public void UpdateProject(string projectId, string link, string linkTitle) {
            try {
                if(!string.IsNullOrEmpty(link)) {
                    var projectAsset = GetProjectById(projectId);
                    AddLinkToAsset(projectAsset, link, linkTitle, true);
                }
            } catch(Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }

        public string GetWorkitemLink(PrimaryWorkitem workitem) {
            return string.Format("{0}assetdetail.v1?oid={1}", configuration["ApplicationUrl"].InnerText, workitem.Id);
        }

        public IList<KeyValuePair<string, string>> GetWorkitemPriorities() {
            try {
                return QueryPropertyValues("WorkitemPriority");
            } catch(Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }

        public bool ProjectExists(string projectId) {
            return GetProjectById(projectId) != null;
        }

        public bool TypeExists(string typeName) {
            try {
                var type = metaModel.GetAssetType(typeName);
                return type != null;
            } catch(MetaException) {
                return false;
            }
        }

        public bool AttributeExists(string typeName, string attributeName) {
            try {
                var type = metaModel.GetAssetType(typeName);
                var attributeDefinition = type.GetAttributeDefinition(attributeName);
                return attributeDefinition != null;
            } catch(MetaException) {
                return false;
            }
        }

        public void AddProperty(string attr, string prefix, bool isList) {
            attributesToQuery.AddLast(new AttributeInfo(attr, prefix, isList));
        }

        private IList<Story> GetFeatureGroupStoryChildren(string featureGroupParentToken, Filter filter) {
            var workitemType = metaModel.GetAssetType(StoryType);
            
            var parentTerm = new FilterTerm(workitemType.GetAttributeDefinition(ParentAndUpAttribute));
            parentTerm.Equal(featureGroupParentToken);
            var typeTerm = new FilterTerm(workitemType.GetAttributeDefinition(AssetTypeAttribute));
            typeTerm.NotEqual(FeatureGroupType);

            var terms = new AndFilterTerm(parentTerm, typeTerm);
            var customTerm = filter.GetFilter(workitemType);
            if (customTerm.HasTerms) {
                terms.And(customTerm);
            }
            return RetrieveData(StoryType, terms).
                    Select(asset => new Story(asset, ListPropertyValues)).ToList();
        }

        private Asset GetProjectById(string projectId) {
            var scopeType = metaModel.GetAssetType(ScopeAttribute);
            var scopeState = scopeType.GetAttributeDefinition(AssetStateAttribute);

            var scopeStateTerm = new FilterTerm(scopeState);
            scopeStateTerm.NotEqual(AssetState.Closed);

            var query = new Query(Oid.FromToken(projectId, metaModel)) {Filter = scopeStateTerm};
            var result = services.Retrieve(query);

            return result.Assets.FirstOrDefault();
        }

        private Asset GetLinkByTitle(Oid assetOid, string linkTitle) {
            var linkType = metaModel.GetAssetType(LinkType);

            var nameTerm = new FilterTerm(linkType.GetAttributeDefinition(NameAttribute));
            nameTerm.Equal(linkTitle);

            var assetTerm = new FilterTerm(linkType.GetAttributeDefinition(AssetAttribute));
            assetTerm.Equal(assetOid);

            var query = new Query(linkType) {Filter = new AndFilterTerm(nameTerm, assetTerm)};
            var result = services.Retrieve(query).Assets;

            if(result.Any()) {
                logger.Log(LogMessage.SeverityType.Info,
                    string.Format(
                        "No need to create link - it already exists. Updating link with title {0} for asset {1}",
                        linkTitle,
                        assetOid));

                return result.First();
            }

            return null;
        }

        private void AddLinkToAsset(Asset asset, string link, string title, bool onMenu) {
            if (asset == null) {
                return;
            }

            var linkType = metaModel.GetAssetType(LinkType);

            var linkAsset = GetLinkByTitle(asset.Oid, title);
            if (linkAsset == null) {
                logger.Log(LogMessage.SeverityType.Info,
                    string.Format("Creating new link with title {0} for asset {1}", title, asset.Oid));

                linkAsset = services.New(linkType, asset.Oid.Momentless);
                linkAsset.SetAttributeValue(linkType.GetAttributeDefinition(NameAttribute), title);
                linkAsset.SetAttributeValue(linkType.GetAttributeDefinition(OnMenuAttribute), onMenu);
            }

            linkAsset.SetAttributeValue(linkType.GetAttributeDefinition(UrlAttribute), link);

            services.Save(linkAsset);

            logger.Log(LogMessage.SeverityType.Info, string.Format("{0} link saved", title));
        }

        public void AddLinkToWorkitem(Workitem workitem, string link, string title, bool onMenu) {            
            try {
                if (!string.IsNullOrEmpty(link)) {
                    AddLinkToAsset(workitem.Asset, link, title, onMenu);
                }
            } catch (Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }

        /// <summary>
        /// Get available property values. Note that value names may not be unique, so we cannot use IDictionary as return type.
        /// </summary>
        /// <param name="propertyName">Property name, ex. PrimaryWorkitem.Status</param>
        private IList<KeyValuePair<string, string>> QueryPropertyValues(string propertyName) {
            IAttributeDefinition nameDef;
            var query = GetPropertyValuesQuery(propertyName, out nameDef);

            return services.Retrieve(query).Assets
                .Select(asset => new KeyValuePair<string, string>((string) asset.GetAttribute(nameDef).Value, asset.Oid.ToString()))
                .ToList();
        }

        private PropertyValues QueryPropertyOidValues(string propertyName) {
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
            nameDef = assetType.GetAttributeDefinition(NameAttribute);

            IAttributeDefinition inactiveDef;

            var query = new Query(assetType);
            query.Selection.Add(nameDef);

            if (assetType.TryGetAttributeDefinition(InactiveAttribute, out inactiveDef)) {
                var filter = new FilterTerm(inactiveDef);
                filter.Equal("False");
                query.Filter = filter;
            }

            query.OrderBy.MajorSort(assetType.DefaultOrderBy, OrderBy.Order.Ascending);
            return query;
        }

        private void AddSelection(Query query, string typePrefix, IAssetType type) {
            foreach (var attrInfo in attributesToQuery) {
                if(attrInfo.Prefix != typePrefix) {
                    continue;
                }
                var def = type.GetAttributeDefinition(attrInfo.Attr);
                query.Selection.Add(def);
            }
        }

        private Dictionary<string, PropertyValues> GetListPropertyValues() {
            var res = new Dictionary<string, PropertyValues>(attributesToQuery.Count);
            
            foreach (var attrInfo in attributesToQuery) {
                if (!attrInfo.IsList) {
                    continue;
                }

                var propertyAlias = attrInfo.Attr;
                if (!attrInfo.Attr.StartsWith("Custom_")) {
                    propertyAlias = attrInfo.Prefix + propertyAlias;
                }
                if(res.ContainsKey(propertyAlias)) {
                    continue;
                }
                var propertyName = ResolvePropertyKey(propertyAlias);

                PropertyValues values;
                if (res.ContainsKey(propertyName)) {
                    values = res[propertyName];
                } else {
                    values = QueryPropertyOidValues(propertyName);
                    res.Add(propertyName, values);
                }

                if (!res.ContainsKey(propertyAlias)) {
                    res.Add(propertyAlias, values);
                }
            }
            return res;
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