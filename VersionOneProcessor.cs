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
        public const string DefectType = "Defect";
        public const string PrimaryWorkitemType = "PrimaryWorkitem";
        public const string MemberType = "Member";
        public const string LinkType = "Link";
        public const string AttributeDefinitionType = "AttributeDefinition";

        private const string OwnersAttribute = "Owners";
        public const string AssetStateAttribute = "AssetState";
        private const string ScopeAttribute = "Scope";
        private const string ParentAttribute = "Parent";
        private const string IdAttribute = "ID";
        private const string StatusAttribute = "Status";
        private const string ParentAndUpAttribute = "ParentAndUp";
        private const string AssetTypeAttribute = "AssetType";
        private const string AssetAttribute = "Asset";
        private const string UrlAttribute = "URL";
        private const string OnMenuAttribute = "OnMenu";
        private const string ChangeDateUtcAttribute = "ChangeDateUTC";
        private const string SourceNameAttribute = "Source.Name";
        private const string NameAttribute = "Name";

        private IServices services;
        private IMetaModel metaModel;
        private readonly ILogger logger; 
        private readonly XmlElement configuration;

        private readonly IQueryBuilder queryBuilder;

        private IDictionary<string, PropertyValues> ListPropertyValues {
            get { return queryBuilder.ListPropertyValues; }
        }

        public VersionOneProcessor(XmlElement config, ILogger logger) {
            configuration = config;
            this.logger = logger;

            queryBuilder = new QueryBuilder();
        }

        private void Connect() {
            var connector = new V1Central(configuration);
            connector.Validate();
            services = connector.Services;
            metaModel = connector.MetaModel;

            queryBuilder.Setup(services, metaModel);
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
            var projectOid = Oid.FromToken(projectId, metaModel);
            var filter = GroupFilter.And(Filter.Closed(false), Filter.Equal(ScopeAttribute, projectOid));

            return queryBuilder
                .Query(PrimaryWorkitemType, filter)
                .Select(asset => new PrimaryWorkitem(asset, ListPropertyValues, queryBuilder.TypeResolver)).ToList();
        }

        public IList<PrimaryWorkitem> GetClosedWorkitemsByProjectId(string projectId) {
            var projectOid = Oid.FromToken(projectId, metaModel);
            var filter = GroupFilter.And(Filter.Closed(true), Filter.Equal(ScopeAttribute, projectOid));

            return queryBuilder
                .Query(PrimaryWorkitemType, filter)
                .Select(asset => new PrimaryWorkitem(asset, ListPropertyValues, queryBuilder.TypeResolver)).ToList();
        }

        public IList<FeatureGroup> GetFeatureGroupsByProjectId(string projectId, Filter filters, Filter childrenFilters) {
            var featureGroupType = metaModel.GetAssetType(FeatureGroupType);
            var ownersDefinition = featureGroupType.GetAttributeDefinition(OwnersAttribute);

            var projectOid = Oid.FromToken(projectId, metaModel);
            var filter = GroupFilter.And(Filter.Equal(ScopeAttribute, projectOid), Filter.Equal(ParentAttribute, string.Empty), filters);

            return queryBuilder.Query(FeatureGroupType, filter)
                .Select(asset => new FeatureGroup(
                    asset, ListPropertyValues, 
                    GetFeatureGroupStoryChildren(asset.Oid.Momentless.Token.ToString(), childrenFilters).Cast<Workitem>().ToList(), 
                    GetMembersByIds(asset.GetAttribute(ownersDefinition).ValuesList),
                    queryBuilder.TypeResolver))
                .ToList();
        }

        private IList<Member> GetMembersByIds(IList oids) {
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
            
            var members = queryBuilder.Query(MemberType, terms).Select(asset => new Member(asset)).ToList();
            return members;
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

        public IList<ValueId> GetWorkitemStatuses() {
            try {
                return queryBuilder.QueryPropertyValues("StoryStatus").ToList();
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

        // TODO change return type to ListValue
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

        public string GetWorkitemLink(Workitem workitem) {
            return string.Format("{0}assetdetail.v1?oid={1}", configuration["ApplicationUrl"].InnerText, workitem.Id);
        }

        public PropertyValues GetAvailableListValues(string typeToken, string fieldName) {
            try {
                var type = metaModel.GetAssetType(typeToken);
                var attributeDefinition = type.GetAttributeDefinition(fieldName);
                
                if(attributeDefinition.AttributeType != AttributeType.Relation) {
                    throw new VersionOneException("Not a Relation field");
                }

                var listTypeToken = attributeDefinition.RelatedAsset.Token;
                return queryBuilder.QueryPropertyValues(listTypeToken);
            } catch(MetaException) {
                throw new VersionOneException("Invalid type or field name");
            }
        }

        public IList<ValueId> GetWorkitemPriorities() {
            try {
                return queryBuilder.QueryPropertyValues("WorkitemPriority").ToList();
            } catch(Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }

        public bool ProjectExists(string projectId) {
            return GetProjectById(projectId) != null;
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
            queryBuilder.AddProperty(attr, prefix, isList);
        }

        public void AddListProperty(string fieldName, string typeToken) {
            queryBuilder.AddListProperty(fieldName, typeToken);
        }

        public void AddOptionalProperty(string attr, string prefix) {
            queryBuilder.AddOptionalProperty(attr, prefix);
        }

        // TODO check and remove Feature Group type condition, it looks redundant
        private IList<Story> GetFeatureGroupStoryChildren(string featureGroupParentToken, Filter filter) {
            var workitemType = metaModel.GetAssetType(StoryType);
            
            var parentTerm = new FilterTerm(workitemType.GetAttributeDefinition(ParentAndUpAttribute));
            parentTerm.Equal(featureGroupParentToken);
            var typeTerm = new FilterTerm(workitemType.GetAttributeDefinition(AssetTypeAttribute));
            typeTerm.NotEqual(FeatureGroupType);

            var terms = new AndFilterTerm(parentTerm, typeTerm);
            var customTerm = filter.GetFilter(workitemType);            
            terms.And(customTerm);
            
            return queryBuilder.Query(StoryType, terms)
                .Select(asset => new Story(asset, ListPropertyValues, queryBuilder.TypeResolver)).ToList();
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

            var nameTerm = new FilterTerm(linkType.GetAttributeDefinition(Entity.NameAttribute));
            nameTerm.Equal(linkTitle);

            var assetTerm = new FilterTerm(linkType.GetAttributeDefinition(AssetAttribute));
            assetTerm.Equal(assetOid);

            var query = new Query(linkType) {Filter = new AndFilterTerm(nameTerm, assetTerm)};
            var result = services.Retrieve(query).Assets;

            if(result.Any()) {
                logger.Log(LogMessage.SeverityType.Info,
                    string.Format("No need to create link - it already exists. Updating link with title {0} for asset {1}", linkTitle, assetOid));

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
                logger.Log(LogMessage.SeverityType.Info, string.Format("Creating new link with title {0} for asset {1}", title, asset.Oid));

                linkAsset = services.New(linkType, asset.Oid.Momentless);
                linkAsset.SetAttributeValue(linkType.GetAttributeDefinition(Entity.NameAttribute), title);
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

        //TODO refactor
        public IList<WorkitemFromExternalSystem> GetWorkitemsClosedSinceBySourceId(string sourceId, DateTime closedSince, string externalIdFieldName, string lastCheckedDefectId, Filter filters, out DateTime dateLastChange, out string lastChangedIdLocal) {
            var workitemType = metaModel.GetAssetType(PrimaryWorkitemType);
            
            var storyTerm = new FilterTerm(workitemType.GetAttributeDefinition(AssetTypeAttribute));
            storyTerm.Equal(StoryType);

            var defectTerm = new FilterTerm(workitemType.GetAttributeDefinition(AssetTypeAttribute));
            defectTerm.Equal(DefectType);

            var sourceTerm = new FilterTerm(workitemType.GetAttributeDefinition(SourceNameAttribute));
            sourceTerm.Equal(sourceId);
            
            var assetStateTerm = new FilterTerm(workitemType.GetAttributeDefinition(AssetStateAttribute));
            assetStateTerm.Equal(AssetState.Closed);

            AndFilterTerm terms;

            if(closedSince != DateTime.MinValue) {
                var changeDateTerm = new FilterTerm(workitemType.GetAttributeDefinition(ChangeDateUtcAttribute));
                changeDateTerm.Greater(closedSince);
                terms = new AndFilterTerm(sourceTerm, assetStateTerm, changeDateTerm);
            } else {
                terms = new AndFilterTerm(sourceTerm, assetStateTerm);
            }

            var orTerm = new OrFilterTerm(storyTerm, defectTerm);
            terms.And(orTerm);

            var customTerm = filters.GetFilter(workitemType);
            terms.And(customTerm);

            var workitems = queryBuilder.Query(PrimaryWorkitemType, terms).Select(asset => new Workitem(asset, ListPropertyValues, queryBuilder.TypeResolver)).ToList();

            // Return results
            dateLastChange = closedSince;
            lastChangedIdLocal = lastCheckedDefectId;

            IList<WorkitemFromExternalSystem> results = new List<WorkitemFromExternalSystem>();

            foreach(var asset in workitems) {
                var id = asset.Number;
                var changeDateUtc = asset.GetProperty<DateTime>(ChangeDateUtcAttribute);

                logger.Log(LogMessage.SeverityType.Debug, string.Format("Processing V1 Defect {0} closed at {1}", id, changeDateUtc));

                if(lastCheckedDefectId.Equals(id)) {
                    logger.Log(LogMessage.SeverityType.Debug, "\tSkipped because this ID was processed last time");
                    continue;
                }

                if(closedSince.CompareTo(changeDateUtc) == 0) {
                    logger.Log(LogMessage.SeverityType.Debug, "\tSkipped because the ChangeDate is equal the date/time we last checked for changes");
                    continue;
                }

                if((dateLastChange == DateTime.MinValue && changeDateUtc != DateTime.MinValue) || changeDateUtc.CompareTo(dateLastChange) > 0) {
                    logger.Log(LogMessage.SeverityType.Debug, "\tCaused an update to LastChangeID and dateLastChanged");
                    dateLastChange = changeDateUtc;
                    lastChangedIdLocal = id;
                }

                results.Add(new WorkitemFromExternalSystem(asset.Asset, ListPropertyValues, externalIdFieldName, queryBuilder.TypeResolver));
            }

            return results;
        }

        //TODO refactor
        public Workitem CreateWorkitem(string assetType, string title, string description, string projectId, string projectName, string externalFieldName, string externalId, string externalSystemName, string priorityId, string owners, string urlTitle, string url) {
            if(string.IsNullOrEmpty(title))
                throw new ArgumentException("Empty title");

            Oid projectOid;

            if(!string.IsNullOrEmpty(projectId)) {
                projectOid = Oid.FromToken(projectId, metaModel);
            } else if(!string.IsNullOrEmpty(projectName)) {
                var project = GetProjectByName(projectName);
                projectOid = project != null ? project.Oid.Momentless : Oid.Null;
            } else {
                logger.Log(LogMessage.SeverityType.Info, string.Format("Could not assign to project with ID '{0}'.  Used first accessible project instead.", projectId));
                var project = GetRootProject();
                projectOid = project != null ? project.Oid.Momentless : Oid.Null;
            }

            if(projectOid == Oid.Null) {
                throw new ArgumentException("Can't find proper project");
            }

            var source = GetSource(externalSystemName);

            if(source == null) {
                throw new ArgumentException("Can't find proper source");
            }

            var sourceOid = source.Oid.Momentless;
            var workitemType = metaModel.GetAssetType(assetType);
            var newWorkitem = services.New(workitemType, Oid.Null);

            newWorkitem.SetAttributeValue(workitemType.GetAttributeDefinition("Name"), title);
            newWorkitem.SetAttributeValue(workitemType.GetAttributeDefinition("Scope"), projectOid);
            newWorkitem.SetAttributeValue(workitemType.GetAttributeDefinition("Description"), description);
            newWorkitem.SetAttributeValue(workitemType.GetAttributeDefinition("Source"), sourceOid);
            newWorkitem.SetAttributeValue(workitemType.GetAttributeDefinition(externalFieldName), externalId);

            foreach(var ownerOid in GetOwnerOids(owners)) {
                newWorkitem.AddAttributeValue(workitemType.GetAttributeDefinition("Owners"), ownerOid);
            }

            if(!string.IsNullOrEmpty(priorityId)) {
                newWorkitem.SetAttributeValue(workitemType.GetAttributeDefinition("Priority"), Oid.FromToken(priorityId, metaModel));
            }

            services.Save(newWorkitem);

            if(!string.IsNullOrEmpty(url)) {
                var linkType = metaModel.GetAssetType("Link");
                var newlink = services.New(linkType, newWorkitem.Oid.Momentless);
                newlink.SetAttributeValue(linkType.GetAttributeDefinition("Name"), !string.IsNullOrEmpty(urlTitle) ? urlTitle : url);
                newlink.SetAttributeValue(linkType.GetAttributeDefinition("URL"), url);
                newlink.SetAttributeValue(linkType.GetAttributeDefinition("OnMenu"), true);
                services.Save(newlink);
            }

            return new Workitem(newWorkitem, ListPropertyValues, queryBuilder.TypeResolver);
        }

        //TODO refactor
        public bool CheckForDuplicate(string externalSystemName, string externalFieldName, string externalId, Filter filters) {
            var workitemType = metaModel.GetAssetType(PrimaryWorkitemType);

            var sourceTerm = new FilterTerm(workitemType.GetAttributeDefinition(SourceNameAttribute));
            sourceTerm.Equal(externalSystemName);

            var externalIdTerm = new FilterTerm(workitemType.GetAttributeDefinition(externalFieldName));
            externalIdTerm.Equal(externalId);

            var terms = new AndFilterTerm(sourceTerm, externalIdTerm);

            var storyTerm = new FilterTerm(workitemType.GetAttributeDefinition(AssetTypeAttribute));
            storyTerm.Equal(StoryType);

            var defectTerm = new FilterTerm(workitemType.GetAttributeDefinition(AssetTypeAttribute));
            defectTerm.Equal(DefectType);
            
            var orTerm = new OrFilterTerm(storyTerm, defectTerm);
            terms.And(orTerm);

            var result = queryBuilder.Query(PrimaryWorkitemType, terms).ToList();

            return (result.Count > 0);
        }

        //TODO refactor
        private Asset GetProjectByName(string projectName) {
            var scopeType = metaModel.GetAssetType(ScopeAttribute);
            var scopeName = scopeType.GetAttributeDefinition(NameAttribute);

            var scopeNameTerm = new FilterTerm(scopeName);
            scopeNameTerm.Equal(projectName);

            var scopeState = scopeType.GetAttributeDefinition(AssetStateAttribute);
            var scopeStateTerm = new FilterTerm(scopeState);
            scopeStateTerm.NotEqual(AssetState.Closed);

            var query = new Query(scopeType);
            query.Selection.Add(scopeName);
            var terms  = new AndFilterTerm(scopeNameTerm, scopeStateTerm);

            var result = queryBuilder.Query(ScopeAttribute, terms);

            return result.FirstOrDefault();
        }

        //TODO refactor
        private Asset GetRootProject() {
            var scopeType = metaModel.GetAssetType(ScopeAttribute);
            var scopeName = scopeType.GetAttributeDefinition(NameAttribute);

            var scopeState = scopeType.GetAttributeDefinition(AssetStateAttribute);
            var scopeStateTerm = new FilterTerm(scopeState);
            scopeStateTerm.NotEqual(AssetState.Closed);

            var scopeQuery = new Query(scopeType, scopeType.GetAttributeDefinition(ParentAttribute)) { Filter = scopeStateTerm };
            scopeQuery.Selection.Add(scopeName);

            var nameQueryResult = services.Retrieve(scopeQuery);

            return nameQueryResult.Assets.FirstOrDefault();
        }

        //TODO refactor
        private Asset GetSource(string sourceName) {
            var storySource = metaModel.GetAssetType("StorySource");

            var term = new FilterTerm(storySource.GetAttributeDefinition("Name"));
            term.Equal(sourceName);

            var sourceQuery = new Query(storySource) {Filter = term};
            var sources = services.Retrieve(sourceQuery).Assets;

            return sources.FirstOrDefault();
        }

        /// <summary>
        /// Attempts to match owners of the workitem in the external system to users in VersionOne.
        /// </summary>
        /// <param name="ownerNames">Comma seperated list of usernames.</param>
        /// <returns>Oids of matching users in VersionOne.</returns>
        //TODO refactor
        private IEnumerable<Oid> GetOwnerOids(string ownerNames) {
            var result = new List<Oid>();

            if(!string.IsNullOrEmpty(ownerNames)) {
                var memberType = metaModel.GetAssetType("Member");
                var ownerQuery = new Query(memberType);

                var terms = new List<IFilterTerm>();

                foreach(var ownerName in ownerNames.Split(',')) {
                    var term = new FilterTerm(memberType.GetAttributeDefinition("Username"));
                    term.Equal(ownerName);
                    terms.Add(term);
                }

                ownerQuery.Filter = new AndFilterTerm(terms.ToArray());

                var matches = services.Retrieve(ownerQuery).Assets;
                result.AddRange(matches.Select(owner => owner.Oid));
            }

            return result.ToArray();
        }
    }
}