/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class ApiController
    {
        public readonly UserHttpController Users;
        public readonly VersioningHttpController Versionings;
        public readonly AccountHttpController Accounts;
        public readonly EmailAuthHttpController EmailAuths;
        public readonly HoloAuthHttpController HoloAuths;
        public readonly OrganizationsHttpController Organizations;
        public readonly MembersHttpController Members;
        public readonly DevicesHttpController Devices;
        public readonly SnapsHttpController Snaps;
        public readonly UtilitiesHttpController Utilities;
        public readonly StorageHttpController Storages;
        public readonly NeighborhoodsHttpController Neighborhoods;
        public readonly FileHttpController Files;
        public readonly AppHttpController Apps;
        public readonly AnchorsHttpController Anchors;
        public readonly CollaboratorsHttpController Collaborators;
        public readonly ScenesHttpController Scenes;
        public readonly AssetLibrariesHttpController AssetLibraries;
        public readonly ScriptLibrariesHttpController ScriptLibraries;
        public readonly PublishedAppsHttpController PublishedApps;
        
        public ApiController(IHttpService http)
        {
            Users = new UserHttpController(http);
            Versionings = new VersioningHttpController(http);
            Accounts = new AccountHttpController(http);
            EmailAuths = new EmailAuthHttpController(http);
            HoloAuths = new HoloAuthHttpController(http);
            Organizations = new OrganizationsHttpController(http);
            Members = new MembersHttpController(http);
            Devices = new DevicesHttpController(http);
            Snaps = new SnapsHttpController(http);
            Utilities = new UtilitiesHttpController(http);
            Storages = new StorageHttpController(http);
            Neighborhoods = new NeighborhoodsHttpController(http);
            Files = new FileHttpController(http);
            Apps = new AppHttpController(http);
            Anchors = new AnchorsHttpController(http);
            Collaborators = new CollaboratorsHttpController(http);
            Scenes = new ScenesHttpController(http);
            AssetLibraries = new AssetLibrariesHttpController(http);
            ScriptLibraries = new ScriptLibrariesHttpController(http);
            PublishedApps = new PublishedAppsHttpController(http);
        }
    }
}