using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using VersionOne.SDK.APIClient;
using VersionOne.ServerConnector.Entities;
using VersionOne.ServerConnector.Filters;
using VersionOne.ServiceHost.Core.Logging;
using System.Xml;
using System.Collections;

namespace VersionOne.ServerConnector {
    // TODO extract hardcoded strings to constants
    // TODO this one is getting huge - it should be split
    // TODO change attribute to property in field names and move them to entity classes
    public class VersionOneProcessor : IVersionOneProcessor {
        public const string FeatureGroupType = "Theme";
        public const string StoryType = "Story";
        public const string DefectType = "Defect";
        public const string PrimaryWorkitemType = "PrimaryWorkitem";
        public const string MemberType = "Member";
        public const string LinkType = "Link";
        public const string AttributeDefinitionType = "AttributeDefinition";

        public const string SystemAdminRoleName = "Role.Name'System Admin";
        public const string SystemAdminRoleId = "Role:1";
        
        public const string OwnersAttribute = "Owners";
        public const string AssetStateAttribute = "AssetState";
        public const string AssetTypeAttribute = "AssetType";

        public const string DeleteOperation = "Delete";
        public const string InactivateOperation = "Inactivate";
        public const string ReactivateOperation = "Reactivate";

        public const string WorkitemPriorityType = "WorkitemPriority";
        public const string WorkitemSourceType = "StorySource";
        public const string WorkitemStatusType = "StoryStatus";

        private const string IdAttribute = "ID";
        private const string AssetAttribute = "Asset";

        private IServices services;
        private IMetaModel metaModel;
        private readonly ILogger logger; 
        private readonly XmlElement configuration;

        private IQueryBuilder queryBuilder;

        private IDictionary<string, PropertyValues> ListPropertyValues {
            get { return queryBuilder.ListPropertyValues; }
        }

        [Inject]
        public VersionOneProcessor(VersionOneSettings settings, ILogger logger) : this(settings.ToXmlElement(), logger) { }

        [Inject]
        public VersionOneProcessor(VersionOneSettings settings) : this(settings, null) { }

        [Inject]
        public VersionOneProcessor(XmlElement config, ILogger logger) {
            configuration = config;
            this.logger = logger;

            queryBuilder = new QueryBuilder();
        }

        [Inject]
        public VersionOneProcessor(XmlElement config) : this(config, null) { }

        private void Connect() {
            var connector = new V1Central(configuration);
            connector.Validate();
            services = connector.Services;
            metaModel = connector.MetaModel;

            queryBuilder.Setup(services, metaModel, connector.Loc);
        }

        protected internal void Connect(IServices testServices, IMetaModel testMetaData, IQueryBuilder testQueryBuilder) {
            services = testServices;
            metaModel = testMetaData;
            queryBuilder = testQueryBuilder;
        }

        public bool ValidateConnection() {
            try {
                Connect();                
            } catch(Exception ex) {
                logger.MaybeLog(LogMessage.SeverityType.Error, "Connection is not valid. " + ex.Message);
                return false;
            }

            return true;
        }

        public Member GetLoggedInMember() {
            return GetMembers(Filter.Empty()).Where(item => item.Asset.Oid.Token.Equals(services.LoggedIn)).FirstOrDefault();
        }

        public ICollection<Member> GetMembers(IFilter filter) {
            return queryBuilder.Query(MemberType, filter).Select(item => new Member(item)).ToList();
        } 

        // TODO use GetPrimaryWorkitems()
        public IList<PrimaryWorkitem> GetWorkitemsByProjectId(string projectId) {
            var projectOid = Oid.FromToken(projectId, metaModel);
            var filter = GroupFilter.And(Filter.Closed(false), Filter.Equal(Workitem.ScopeProperty, projectOid));

            return queryBuilder
                .Query(PrimaryWorkitemType, filter)
                .Select(asset => new PrimaryWorkitem(asset, ListPropertyValues, queryBuilder.TypeResolver)).ToList();
        }

        // TODO use GetPrimaryWorkitems()
        public IList<PrimaryWorkitem> GetClosedWorkitemsByProjectId(string projectId) {
            var projectOid = Oid.FromToken(projectId, metaModel);
            var filter = GroupFilter.And(Filter.Closed(true), Filter.Equal(Workitem.ScopeProperty, projectOid));

            return queryBuilder
                .Query(PrimaryWorkitemType, filter)
                .Select(asset => new PrimaryWorkitem(asset, ListPropertyValues, queryBuilder.TypeResolver)).ToList();
        }

        // TODO make this Story-agnostic. In case of criteria based ex. on Story-only custom fields current filter approach won't let an easy solution.
        public IList<FeatureGroup> GetFeatureGroups(IFilter filter, IFilter childrenFilter) {
            var featureGroupType = metaModel.GetAssetType(FeatureGroupType);
            var ownersDefinition = featureGroupType.GetAttributeDefinition(OwnersAttribute);

            return queryBuilder.Query(FeatureGroupType, filter)
                .Select(asset => new FeatureGroup(
                    asset, ListPropertyValues, 
                    GetWorkitems(StoryType, GroupFilter.And(Filter.Equal(Entity.ParentAndUpProperty, asset.Oid.Momentless.Token.ToString()), childrenFilter)), 
                    GetMembersByIds(asset.GetAttribute(ownersDefinition).ValuesList),
                    queryBuilder.TypeResolver))
                .ToList();
        }

        // TODO avoid ancient non generic collections
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

        public void SaveWorkitems(ICollection<Workitem> workitems) {
            if(workitems == null || workitems.Count == 0) {
                return;
            }

            foreach(var workitem in workitems) {
                try {
                    services.Save(workitem.Asset);
                } catch(V1Exception ex) {
                    logger.MaybeLog(LogMessage.SeverityType.Error, string.Format(queryBuilder.Localize(GetMessageFromException(ex)) + " '{0}' {2} ({1}).", workitem.Name, workitem.Number, workitem.TypeName));
                } catch (Exception ex) {
                    logger.MaybeLog(LogMessage.SeverityType.Error, "Internal error: " + ex.Message);
                }
            }
        }

        private static string GetMessageFromException(V1Exception exception) {
            var message = exception.Message;

            return message.Split(':')[0];
        }

        public void CloseWorkitem(PrimaryWorkitem workitem) {
            try {
                var closeOperation = workitem.Asset.AssetType.GetOperation(InactivateOperation);
                services.ExecuteOperation(closeOperation, workitem.Asset.Oid);
            } catch (V1Exception ex) {
                throw new VersionOneException(queryBuilder.Localize(ex.Message));
            } catch(Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }

        public IList<ValueId> GetWorkitemStatuses() {
            try {
                return queryBuilder.QueryPropertyValues(WorkitemStatusType).ToList();
            } catch (V1Exception ex) {
                throw new VersionOneException(queryBuilder.Localize(ex.Message));
            } catch(Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }

        public ValueId CreateWorkitemStatus(string statusName) {
            try {
                var primaryWorkitemStatusType = metaModel.GetAssetType(WorkitemStatusType);
                var status = new Asset(primaryWorkitemStatusType);
                status.SetAttributeValue(primaryWorkitemStatusType.NameAttribute, statusName);
                services.Save(status);

                return new ValueId(status.Oid.Momentless, statusName);
            } catch (V1Exception ex) {
                throw new VersionOneException(queryBuilder.Localize(ex.Message));
            } catch(Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }

        // TODO refactor
        public void UpdateProject(string projectId, Link link) {
            try {
                if(link != null && !string.IsNullOrEmpty(link.Url)) {
                    var projectAsset = GetProjectById(projectId);
                    AddLinkToAsset(projectAsset, link);
                }
            } catch (V1Exception ex) {
                throw new VersionOneException(queryBuilder.Localize(ex.Message));
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
                return queryBuilder.QueryPropertyValues(WorkitemPriorityType).ToList();
            } catch (V1Exception ex) {
                throw new VersionOneException(queryBuilder.Localize(ex.Message));
            } catch(Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }

        // TODO get rid of it
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
            if (!string.IsNullOrEmpty(attr)) {
                queryBuilder.AddOptionalProperty(attr, prefix);
            }
        }

        // TODO use filters
        private Asset GetProjectById(string projectId) {
            var scopeType = metaModel.GetAssetType(Workitem.ScopeProperty);
            var scopeState = scopeType.GetAttributeDefinition(AssetStateAttribute);

            var scopeStateTerm = new FilterTerm(scopeState);
            scopeStateTerm.NotEqual(AssetState.Closed);

            var query = new Query(Oid.FromToken(projectId, metaModel)) {Filter = scopeStateTerm};
            var result = services.Retrieve(query);

            return result.Assets.FirstOrDefault();
        }

        private List<Asset> GetAssetLinks(Oid assetOid, IFilter filter) {
            var fullFilter = GroupFilter.And(filter, Filter.Equal(AssetAttribute, assetOid.Momentless));

            return queryBuilder.Query(LinkType, fullFilter);
        }

        public List<Link> GetWorkitemLinks(Workitem workitem, IFilter filter) {
            return GetAssetLinks(Oid.FromToken(workitem.Id, metaModel), filter).Select(x => new Link(x)).ToList();
        }

        public void AddLinkToWorkitem(Workitem workitem, Link link) {
            try {
                if (link != null && !string.IsNullOrEmpty(link.Url)) {
                    AddLinkToAsset(workitem.Asset, link);
                }
            } catch (V1Exception ex) {
                throw new VersionOneException(queryBuilder.Localize(ex.Message));
            } catch (Exception ex) {
                throw new VersionOneException(ex.Message);
            }
        }

        private void AddLinkToAsset(Asset asset, Link link) {
            if (asset == null) {
                return;
            }

            var linkType = metaModel.GetAssetType(LinkType);

            var existedLinks = GetAssetLinks(asset.Oid, Filter.Equal(Link.UrlProperty, link.Url));

            if(existedLinks.Count > 0) {
                logger.MaybeLog(LogMessage.SeverityType.Debug, string.Format("No need to create link - it already exists."));
                return;
            }

            logger.MaybeLog(LogMessage.SeverityType.Info, string.Format("Creating new link with title {0} for asset {1}", link.Title, asset.Oid));

            var linkAsset = services.New(linkType, asset.Oid.Momentless);
            linkAsset.SetAttributeValue(linkType.GetAttributeDefinition(Entity.NameProperty), link.Title);
            linkAsset.SetAttributeValue(linkType.GetAttributeDefinition(Link.OnMenuProperty), link.OnMenu);
            linkAsset.SetAttributeValue(linkType.GetAttributeDefinition(Link.UrlProperty), link.Url);

            services.Save(linkAsset);
            logger.MaybeLog(LogMessage.SeverityType.Info, string.Format("{0} link saved", link.Title));
        }

        public IList<Workitem> GetPrimaryWorkitems(IFilter filter) {
            return GetWorkitems(PrimaryWorkitemType, filter);
        }

        public IList<Workitem> GetWorkitems(string type, IFilter filter) {
            var workitemType = metaModel.GetAssetType(type);
            var terms = filter.GetFilter(workitemType);

            return queryBuilder.Query(type, terms).Select(asset => Workitem.Create(asset, ListPropertyValues, queryBuilder.TypeResolver)).ToList();
        }

        //TODO refactor
        public Workitem CreateWorkitem(string assetType, string title, string description, string projectToken, 
                                       string externalFieldName, string externalId, string externalSystemName, 
                                       string priorityId, string owners) {
            if(string.IsNullOrEmpty(title)) {
                throw new ArgumentException("Empty title");
            }

            var projectOid = Oid.FromToken(projectToken, metaModel);
            var source = GetSourceByName(externalSystemName);
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

            //TODO refactor
            //NOTE Save doesn't return all the needed data, therefore we need another query
            return GetWorkitems(newWorkitem.AssetType.Token, Filter.Equal("ID", newWorkitem.Oid.Momentless.Token)).FirstOrDefault();
        }

        private ValueId GetSourceByName(string externalSystemName) {
            var sourceValues = queryBuilder.QueryPropertyValues(WorkitemSourceType);
            var source = sourceValues.Where(item => string.Equals(item.Name, externalSystemName)).FirstOrDefault();

            if(source == null) {
                throw new ArgumentException("Can't find proper source");
            }

            return source;
        }

        //TODO refactor
        /*
        private Asset GetProjectByName(string projectName) {
            var scopeType = metaModel.GetAssetType(Entity.ScopeProperty);
            var scopeName = scopeType.GetAttributeDefinition(Entity.NameProperty);

            var scopeNameTerm = new FilterTerm(scopeName);
            scopeNameTerm.Equal(projectName);

            var scopeState = scopeType.GetAttributeDefinition(AssetStateAttribute);
            var scopeStateTerm = new FilterTerm(scopeState);
            scopeStateTerm.NotEqual(AssetState.Closed);

            var query = new Query(scopeType);
            query.Selection.Add(scopeName);
            var terms  = new AndFilterTerm(scopeNameTerm, scopeStateTerm);

            var result = queryBuilder.Query(Entity.ScopeProperty, terms);

            return result.FirstOrDefault();
        }*/

        public string GetProjectTokenByName(string projectName) {
            var project = GetProjectByName(projectName);
            return project != null ? project.Oid.Momentless.Token : null;
        }

        private Asset GetProjectByName(string projectName) {
            var scopeType = metaModel.GetAssetType(Workitem.ScopeProperty);
            var scopeName = scopeType.GetAttributeDefinition(Entity.NameProperty);

            var filter = GroupFilter.And(
                Filter.Equal(Entity.NameProperty, projectName),
                Filter.Closed(false)
                );

            var query = new Query(scopeType);
            query.Selection.Add(scopeName);

            var result = queryBuilder.Query(Workitem.ScopeProperty, filter);

            return result.FirstOrDefault();
        }

        public string GetRootProjectToken() {
            var project = GetRootProject();

            return project == null ? null : project.Oid.Momentless.Token;
        }

        //TODO refactor
        private Asset GetRootProject() {
            var scopeType = metaModel.GetAssetType(Workitem.ScopeProperty);
            var scopeName = scopeType.GetAttributeDefinition(Entity.NameProperty);

            var scopeState = scopeType.GetAttributeDefinition(AssetStateAttribute);
            var scopeStateTerm = new FilterTerm(scopeState);
            scopeStateTerm.NotEqual(AssetState.Closed);

            var scopeQuery = new Query(scopeType, scopeType.GetAttributeDefinition(Entity.ParentProperty)) { Filter = scopeStateTerm };
            scopeQuery.Selection.Add(scopeName);

            var nameQueryResult = services.Retrieve(scopeQuery);

            return nameQueryResult.Assets.FirstOrDefault();
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