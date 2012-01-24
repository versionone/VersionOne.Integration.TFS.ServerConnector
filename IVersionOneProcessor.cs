using System.Collections.Generic;
using VersionOne.ServerConnector.Entities;
using VersionOne.ServerConnector.Filters;

namespace VersionOne.ServerConnector {
    public interface IVersionOneProcessor {
        bool ValidateConnection();

        Member GetLoggedInMember();
        ICollection<Member> GetMembers(IFilter filter);
        
        IList<PrimaryWorkitem> GetWorkitemsByProjectId(string projectId);
        IList<PrimaryWorkitem> GetClosedWorkitemsByProjectId(string projectId);
        IList<FeatureGroup> GetFeatureGroups(IFilter filters, IFilter childrenFilters);
        
        void SaveWorkitems(ICollection<Workitem> workitems);
        void CloseWorkitem(PrimaryWorkitem workitem);
        void UpdateProject(string projectId, Link link);
        string GetWorkitemLink(Workitem workitem);
        ValueId CreateWorkitemStatus(string statusName);
        IList<ValueId> GetWorkitemStatuses();
        IList<ValueId> GetWorkitemPriorities();

        PropertyValues GetAvailableListValues(string typeToken, string fieldName);
        
        bool ProjectExists(string projectId);
        bool AttributeExists(string typeName, string attributeName);
        
        void AddProperty(string attr, string prefix, bool isList);
        void AddListProperty(string fieldName, string typeToken);
        void AddOptionalProperty(string attr, string prefix);
        void AddLinkToWorkitem(Workitem workitem, Link link);

        IList<Workitem> GetWorkitems(string type, IFilter filter);
        IList<Workitem> GetPrimaryWorkitems(IFilter filter);

        Workitem CreateWorkitem(string assetType, string title, string description, string projectToken, 
                                                        string externalFieldName, string externalId, string externalSystemName, string priorityId,
                                                        string owners);

        string GetProjectTokenByName(string projectName);
        string GetRootProjectToken();
    }
}