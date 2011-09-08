using System;
using System.Collections.Generic;
using VersionOne.ServerConnector.Entities;
using VersionOne.ServerConnector.Filters;

namespace VersionOne.ServerConnector {
    public interface IVersionOneProcessor {
        bool ValidateConnection();
        
        IList<PrimaryWorkitem> GetWorkitemsByProjectId(string projectId);
        IList<PrimaryWorkitem> GetClosedWorkitemsByProjectId(string projectId);
        IList<FeatureGroup> GetFeatureGroupsByProjectId(string projectId, Filter filters, Filter childrenFilters);
        
        void SaveWorkitems(IEnumerable<Workitem> workitems);
        void CloseWorkitem(PrimaryWorkitem workitem);
        void UpdateProject(string projectId, string link, string linkTitle);
        string GetWorkitemLink(Workitem workitem);
        void SetWorkitemStatus(PrimaryWorkitem workitem, string statusId);
        KeyValuePair<string, string> CreateWorkitemStatus(string statusName);
        IList<KeyValuePair<string, string>> GetWorkitemStatuses();
        IList<KeyValuePair<string, string>> GetWorkitemPriorities();

        PropertyValues GetAvailableListValues(string typeToken, string fieldName);
        
        bool ProjectExists(string projectId);
        bool AttributeExists(string typeName, string attributeName);
        
        void AddProperty(string attr, string prefix, bool isList);
        void AddListProperty(string fieldName, string typeToken);
        void AddOptionalProperty(string attr, string prefix);
        void AddLinkToWorkitem(Workitem workitem, string link, string title, bool onMenu);

        IList<WorkitemFromExternalSystem> GetWorkitemsClosedSinceBySourceId(string sourceId, DateTime closedSince, string externalIdFieldName, string lastCheckedDefectId, Filter filters,
            out DateTime dateLastChange, out string lastChangedIDLocal);

        bool CheckForDuplicate(string externalSystemName, string externalFieldName, string externalId, Filter filters);
        Workitem CreateWorkitem(string assetType, string title, string description, string projectId, string projectName, string externalFieldName, string externalId, string externalSystemName, string priorityId, string owners, string urlTitle, string url);
    }
}