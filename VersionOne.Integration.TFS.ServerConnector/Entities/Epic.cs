using System.Collections.Generic;
using VersionOne.SDK.APIClient;

namespace VersionOne.Integration.Tfs.ServerConnector.Entities
{
	public class Epic : Workitem
	{
		public override string TypeToken
		{
			get { return VersionOneProcessor.EpicType; }
		}

		internal Epic(Asset asset, IDictionary<string, PropertyValues> listValues, IList<Member> owners, IEntityFieldTypeResolver typeResolver)
			: base(asset, listValues, owners, typeResolver) { }
	}
}