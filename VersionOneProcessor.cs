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

        private IDictionary<string, Oid> propertyOids;

        public VersionOneProcessor(XmlElement config, ILogger logger) {
            configuration = config;
            this.logger = logger;
        }

        private void Connect() {
            var connector = new V1Central(configuration);
            connector.Validate();
            services = connector.Services;
            metaModel = connector.MetaModel;
            LoadListValues();
        }

        public bool ValidateConnection() {
            try {
                Connect();                
            } catch(Exception ex) {
                logger.Log(LogMessage.SeverityType.Error, "Connection is not valid." + ex.Message);
                return false;
            }

            return true;
        }

        private void LoadListValues() {
            propertyOids = QueryPropertyOidValues("WorkitemPriority");
        }

        public IList<PrimaryWorkitem> GetWorkitemsByProjectId(string projectId) {
            var workitemType = metaModel.GetAssetType("PrimaryWorkitem");

            var projectOid = Oid.FromToken(projectId, metaModel);
            var scopeTerm = new FilterTerm(workitemType.GetAttributeDefinition("Scope"));
            scopeTerm.Equal(projectOid);

            var stateTerm = new FilterTerm(workitemType.GetAttributeDefinition("AssetState"));
            stateTerm.NotEqual(AssetState.Closed);

            return GetWorkitems("PrimaryWorkitem", new AndFilterTerm(scopeTerm, stateTerm)).Select(asset => new PrimaryWorkitem(asset, propertyOids)).ToList();
        }

        public IList<PrimaryWorkitem> GetClosedWorkitemsByProjectId(string projectId) {
            var workitemType = metaModel.GetAssetType("PrimaryWorkitem");

            var projectOid = Oid.FromToken(projectId, metaModel);
            var scopeTerm = new FilterTerm(workitemType.GetAttributeDefinition("Scope"));
            scopeTerm.Equal(projectOid);

            var stateTerm = new FilterTerm(workitemType.GetAttributeDefinition("AssetState"));
            stateTerm.Equal(AssetState.Closed);

            return GetWorkitems("PrimaryWorkitem", new AndFilterTerm(scopeTerm, stateTerm)).Select(asset => new PrimaryWorkitem(asset, propertyOids)).ToList();
        }

        public IList<FeatureGroup> GetFeatureGroupsByProjectId(string projectId) {
             var workitemType = metaModel.GetAssetType("Theme");

            var projectOid = Oid.FromToken(projectId, metaModel);
            var scopeTerm = new FilterTerm(workitemType.GetAttributeDefinition("Scope"));
            scopeTerm.Equal(projectOid);

            return GetWorkitems("Theme", scopeTerm).Select(asset => new FeatureGroup(asset)).ToList();
        }

        private AssetList GetWorkitems(string workitemTypeName, IFilterTerm filter) {
            try {
                var workitemType = metaModel.GetAssetType(workitemTypeName);
                var query = new Query(workitemType) { Filter = filter };

                query.Selection.Add(workitemType.GetAttributeDefinition(Workitem.NumberProperty));
                query.Selection.Add(workitemType.GetAttributeDefinition(Workitem.NameProperty));
                query.Selection.Add(workitemType.GetAttributeDefinition(Workitem.DescriptionProperty));
                query.Selection.Add(workitemType.GetAttributeDefinition(Workitem.PriorityProperty));
                query.Selection.Add(workitemType.GetAttributeDefinition(Workitem.StatusProperty));
                query.Selection.Add(workitemType.GetAttributeDefinition(Workitem.EstimateProperty));
                query.Selection.Add(workitemType.GetAttributeDefinition(Workitem.AssetTypeProperty));
                query.Selection.Add(workitemType.GetAttributeDefinition(Workitem.ParentNameProperty));
                query.Selection.Add(workitemType.GetAttributeDefinition(Workitem.TeamNameProperty));
                query.Selection.Add(workitemType.GetAttributeDefinition(Workitem.SprintNameProperty));
                query.Selection.Add(workitemType.GetAttributeDefinition(Workitem.OrderProperty));

                var assetList = services.Retrieve(query).Assets;

                return assetList;
            } catch (Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }       

        public virtual IList<string> GetAssetTypes() {
            return new[] { "Story", "Defect" };
        }

        public void UpdateWorkitemLinkAndReference(PrimaryWorkitem workitem, string cardId, string cardLink) {
            logger.Log(LogMessage.SeverityType.Info, "Updating V1 workitem reference and creating link");

            const string linkTitle = "LeanKitKanban Card";

            try {
                if(!string.IsNullOrEmpty(cardId)) {
                    var storyType = metaModel.GetAssetType("PrimaryWorkitem");
                    workitem.Asset.SetAttributeValue(storyType.GetAttributeDefinition("Reference"), cardId);
                    services.Save(workitem.Asset);
                    
                    logger.Log(LogMessage.SeverityType.Info, "Workitem reference updated");
                }

                if(!string.IsNullOrEmpty(cardLink)) {
                    AddLinkToAsset(workitem.Asset, cardLink, linkTitle);
                }
            } catch(Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }

        public void SaveWorkitems(IEnumerable<PrimaryWorkitem> workitems) {
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

        public void UpdateProject(string projectId, string boardLink) {
            const string linkTitle = "LeanKitKanban Board";

            try {
                if(!string.IsNullOrEmpty(boardLink)) {
                    var projectAsset = GetProjectById(projectId);
                    AddLinkToAsset(projectAsset, boardLink, linkTitle);
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

        public bool IsProjectExist(string projectId) {
            return GetProjectById(projectId) != null;
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
                        "No need to create link to LKK - it already exists. Updating link with title {0} for asset {1}",
                        linkTitle,
                        assetOid));

                return result.First();
            }

            return null;
        }

        private void AddLinkToAsset(Asset asset, string link, string title) {
            if(asset == null) {
                return;
            }

            var linkType = metaModel.GetAssetType("Link");

            var linkAsset = GetLinkByTitle(asset.Oid, title);
            if(linkAsset == null) {
                logger.Log(LogMessage.SeverityType.Info,
                    string.Format("Creating new link to LKK with title {0} for asset {1}", title, asset.Oid));

                linkAsset = services.New(linkType, asset.Oid.Momentless);
                linkAsset.SetAttributeValue(linkType.GetAttributeDefinition("Name"), title);
                linkAsset.SetAttributeValue(linkType.GetAttributeDefinition("OnMenu"), true);
            }

            linkAsset.SetAttributeValue(linkType.GetAttributeDefinition("URL"), link);

            services.Save(linkAsset);

            logger.Log(LogMessage.SeverityType.Info, string.Format("{0} link saved", title));
        }

        /// <summary>
        /// Get available property values. Note that value names may not be unique, so we cannot use IDictionary as return type.
        /// </summary>
        /// <param name="propertyName">Property name, ex. PrimaryWorkitem.Status</param>
        private IList<KeyValuePair<string, string>> QueryPropertyValues(string propertyName) {
            var res = new List<KeyValuePair<string, string>>();
            IAttributeDefinition nameDef;
            Query query = GetPropertyValuesQuery(propertyName, out nameDef);

            foreach(var asset in services.Retrieve(query).Assets) {
                var name = asset.GetAttribute(nameDef).Value as string;
                res.Add(new KeyValuePair<string, string>(name, asset.Oid.ToString()));
            }

            return res;
        }

        private IDictionary<string, Oid> QueryPropertyOidValues(string propertyName) {
            var res = new Dictionary<string, Oid>();
            IAttributeDefinition nameDef;
            Query query = GetPropertyValuesQuery(propertyName, out nameDef);

            foreach (var asset in services.Retrieve(query).Assets) {
                var name = asset.GetAttribute(nameDef).Value as string;
                if (name != null) {
                    res.Add(asset.Oid.Momentless.Token, asset.Oid);
                }
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
    }
}