using System;
using System.Collections.Generic;
using System.Linq;
using VersionOne.SDK.APIClient;
using VersionOne.ServiceHost.Core.Logging;
using System.Xml;

namespace VersionOne.ServerConnector {
    public class VersionOneProcessor : IVersionOneProcessor {
        private IServices services;
        private IMetaModel metaModel;
        private readonly ILogger logger;
        private readonly XmlElement configuration;
        
        private readonly static LinkedList<AttributeInfo> AttributesToQuery = new LinkedList<AttributeInfo>();

        private Dictionary<string, PropertyValues> listPropertyValues;

        public VersionOneProcessor(XmlElement config, ILogger logger) {
            configuration = config;
            this.logger = logger;
        }

        private void Connect() {
            var connector = new V1Central(configuration);
            connector.Validate();
            services = connector.Services;
            metaModel = connector.MetaModel;
            listPropertyValues = GetListPropertyValues();
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
            var workitemType = metaModel.GetAssetType("PrimaryWorkitem");

            var projectOid = Oid.FromToken(projectId, metaModel);
            var scopeTerm = new FilterTerm(workitemType.GetAttributeDefinition("Scope"));
            scopeTerm.Equal(projectOid);

            var stateTerm = new FilterTerm(workitemType.GetAttributeDefinition("AssetState"));
            stateTerm.NotEqual(AssetState.Closed);

            return GetWorkitems("PrimaryWorkitem", new AndFilterTerm(scopeTerm, stateTerm)).Select(asset => new PrimaryWorkitem(asset, listPropertyValues)).ToList();
        }

        public IList<PrimaryWorkitem> GetClosedWorkitemsByProjectId(string projectId) {
            var workitemType = metaModel.GetAssetType("PrimaryWorkitem");

            var projectOid = Oid.FromToken(projectId, metaModel);
            var scopeTerm = new FilterTerm(workitemType.GetAttributeDefinition("Scope"));
            scopeTerm.Equal(projectOid);

            var stateTerm = new FilterTerm(workitemType.GetAttributeDefinition("AssetState"));
            stateTerm.Equal(AssetState.Closed);

            return GetWorkitems("PrimaryWorkitem", new AndFilterTerm(scopeTerm, stateTerm)).Select(asset => new PrimaryWorkitem(asset, listPropertyValues)).ToList();
        }

        public IList<FeatureGroup> GetFeatureGroupsByProjectId(string projectId) {
            var featureGroupType = metaModel.GetAssetType("Theme");

            var projectOid = Oid.FromToken(projectId, metaModel);
            var scopeTerm = new FilterTerm(featureGroupType.GetAttributeDefinition("Scope"));
            scopeTerm.Equal(projectOid);
            var assetTypeTerm = new FilterTerm(featureGroupType.GetAttributeDefinition("Parent"));
            assetTypeTerm.Equal("");

            return GetWorkitems("Theme", new AndFilterTerm(scopeTerm, assetTypeTerm)).Select(asset => new FeatureGroup(asset, listPropertyValues, GetFeatureGroupChildren(asset.Oid.Momentless.Token.ToString()))).ToList();
        }

        private AssetList GetWorkitems(string workitemTypeName, IFilterTerm filter) {
            try {
                var workitemType = metaModel.GetAssetType(workitemTypeName);
                var query = new Query(workitemType) { Filter = filter };

                AddSelection(query, workitemTypeName, workitemType);

                return services.Retrieve(query).Assets;
            } catch (Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }       

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
                var primaryWorkitemType = metaModel.GetAssetType("PrimaryWorkitem");
                var statusAttributeDefinition = primaryWorkitemType.GetAttributeDefinition("Status");

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

        public void AddProperty(string attr, string prefix, bool isList) {
            AttributesToQuery.AddLast(new AttributeInfo(attr, prefix, isList));
        }

        public IList<Workitem> GetFeatureGroupChildren(string featureGroupParentToken) {
            //http://integsrv01/VersionOne11/rest-1.v1/Data/Workitem?where=(Parent.ParentMeAndUp='Theme:1056';AssetType!='Theme')
            var workitemType = metaModel.GetAssetType("PrimaryWorkitem");
            var terms = new List<FilterTerm>();
            var term = new FilterTerm(workitemType.GetAttributeDefinition("ParentAndUp"));
            term.Equal(featureGroupParentToken);
            terms.Add(term);
            term = new FilterTerm(workitemType.GetAttributeDefinition("AssetType"));
            term.NotEqual("Theme");
            terms.Add(term);

            return GetWorkitems("PrimaryWorkitem", new AndFilterTerm(terms.ToArray())).
                    Select( asset => new Workitem(asset, listPropertyValues)).ToList();
        }

        private Asset GetProjectById(string projectId) {
            var scopeType = metaModel.GetAssetType("Scope");
            var scopeState = scopeType.GetAttributeDefinition("AssetState");

            var scopeStateTerm = new FilterTerm(scopeState);
            scopeStateTerm.NotEqual(AssetState.Closed);

            var query = new Query(Oid.FromToken(projectId, metaModel)) {Filter = scopeStateTerm};
            var result = services.Retrieve(query);

            return result.Assets.FirstOrDefault();
        }

        private Asset GetLinkByTitle(Oid assetOid, string linkTitle) {
            var linkType = metaModel.GetAssetType("Link");

            var nameTerm = new FilterTerm(linkType.GetAttributeDefinition("Name"));
            nameTerm.Equal(linkTitle);

            var assetTerm = new FilterTerm(linkType.GetAttributeDefinition("Asset"));
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

            var linkType = metaModel.GetAssetType("Link");

            var linkAsset = GetLinkByTitle(asset.Oid, title);
            if (linkAsset == null) {
                logger.Log(LogMessage.SeverityType.Info,
                    string.Format("Creating new link with title {0} for asset {1}", title, asset.Oid));

                linkAsset = services.New(linkType, asset.Oid.Momentless);
                linkAsset.SetAttributeValue(linkType.GetAttributeDefinition("Name"), title);
                linkAsset.SetAttributeValue(linkType.GetAttributeDefinition("OnMenu"), onMenu);
            }

            linkAsset.SetAttributeValue(linkType.GetAttributeDefinition("URL"), link);

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
            nameDef = assetType.GetAttributeDefinition("Name");

            IAttributeDefinition inactiveDef;

            var query = new Query(assetType);
            query.Selection.Add(nameDef);
            
            if(assetType.TryGetAttributeDefinition("Inactive", out inactiveDef)) {
                var filter = new FilterTerm(inactiveDef);
                filter.Equal("False");
                query.Filter = filter;
            }

            query.OrderBy.MajorSort(assetType.DefaultOrderBy, OrderBy.Order.Ascending);
            return query;
        }

        private static void AddSelection(Query query, string typePrefix, IAssetType type) {
            foreach (var attrInfo in AttributesToQuery) {
                if(attrInfo.Prefix != typePrefix) {
                    continue;
                }
                var def = type.GetAttributeDefinition(attrInfo.Attr);
                query.Selection.Add(def);
            }
        }

        private Dictionary<string, PropertyValues> GetListPropertyValues() {
            var res = new Dictionary<string, PropertyValues>(AttributesToQuery.Count);
            
            foreach (var attrInfo in AttributesToQuery) {
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
                    return "Member";
                case "PrimaryWorkitemPriority":                      
                    return "WorkitemPriority";
            }

            return propertyAlias;
        }
    }
}