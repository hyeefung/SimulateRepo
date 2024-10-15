using System.Text;

using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;

try
{
    string listItemIdToUpdate = "41";

    using (ClientContext client = new ClientContext("https://example.sharepoint.com/sites/ExampleSite/"))
    {
        client.ExecutingWebRequest += (sender, e) =>
        {
            e.WebRequestExecutor.WebRequest.Headers.Add("Authorization", "Bearer XXX");
            e.WebRequestExecutor.WebRequest.UserAgent = "TST|Example|1.0";
        };

        // 1. Get List Id by ListName
        var list = client.Web.Lists.GetByTitle("TestList");

        Action getByTitleAction = () =>
        {
            client.Load(
                list,
                l => l.Id,
                l => l.EnableFolderCreation,
                l => l.ParentWeb,
                l => l.Hidden,
                l => l.BaseType,
                l => l.Title,
                l => l.Description,
                l => l.EnableFolderCreation,
                l => l.DefaultDisplayFormUrl,
                l => l.RootFolder.ServerRelativeUrl,
                l => l.EnableAttachments,
                l => l.BaseTemplate,
                l => l.OnQuickLaunch,
                l => l.ContentTypesEnabled,
                l => l.ContentTypes,
                l => l.Fields.Include(f => f.Id, f => f.EntityPropertyName, f => f.InternalName, f => f.Title, f => f.FieldTypeKind, f => f.ReadOnlyField, f => f.Required));
        };

        getByTitleAction.Invoke();
        client.ExecuteQuery();

        // 2. Query list items
        var query = new CamlQuery();
        var sb = new StringBuilder();
        sb.Append("<View Scope='FilesOnly'>");
        sb.Append("<Query><Where><In><FieldRef Name='ID' /><Values>");
        sb.Append($"<Value Type='Counter'>{listItemIdToUpdate}</Value>");
        sb.AppendLine("</Values></In></Where></Query></View>");
        query.ViewXml = sb.ToString();

        var listItems = list.GetItems(query);
        Action getItemsAction = () =>
        {
            client.Load(listItems, items => items.Include(
                i => i.Id,
                i => i.ContentType,
                i => i.DisplayName,
                i => i.File.CheckOutType,
                i => i.File.CheckedOutByUser,
                i => i.FileSystemObjectType,
                i => i.EffectiveBasePermissions,
                i => i.ParentList,
                i => i.ParentList.DefaultDisplayFormUrl,
                i => i.File));
        };

        getItemsAction.Invoke();
        client.ExecuteQuery();

        var listItem = listItems.First();

        // 3. Update MyMetadata & MyMultiMetadata
        Field myMetadataField = list.Fields.GetByInternalNameOrTitle("MyMetadata");
        Field myMultiMmetadataField = list.Fields.GetByInternalNameOrTitle("MyMultiMetadata");
        TaxonomyField txtMyMetadataField = client.CastTo<TaxonomyField>(myMetadataField);
        TaxonomyField txtMyMultiMetadataField = client.CastTo<TaxonomyField>(myMultiMmetadataField);

        TaxonomyFieldValueCollection tfvc = new TaxonomyFieldValueCollection(client, null, txtMyMultiMetadataField);
        tfvc.PopulateFromLabelGuidPairs("320eb8c5-6f83-43a2-b947-abc7376ffc7d;b8dadff8-a6c3-4137-b0f2-9eefab1f5b2f");
        txtMyMultiMetadataField.SetFieldValueByValueCollection(listItem, tfvc);

        var tfv = new TaxonomyFieldValue
        {
            TermGuid = "9db20e93-c21b-4244-b683-d4a6cd53e98e",
            //Label = "9db20e93-c21b-4244-b683-d4a6cd53e98e"
        };
        txtMyMetadataField.SetFieldValueByValue(listItem, tfv);

        listItem.Update();
        client.ExecuteQuery();
    }
}
catch(Exception ex)
{
    Console.Write(ex.ToString());
}