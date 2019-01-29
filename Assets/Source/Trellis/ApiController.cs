/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class ApiController
    {
        public readonly AccountHttpController Accounts;
        public readonly AnchorsHttpController Anchors;
        public readonly AppHttpController Apps;
        public readonly AssetLibrariesHttpController AssetLibraries;
        public readonly CollaboratorsHttpController Collaborators;
        public readonly DevicesHttpController Devices;
        public readonly EmailAuthHttpController EmailAuths;
        public readonly FileHttpController Files;
        public readonly HoloAuthHttpController HoloAuths;
        public readonly MembersHttpController Members;
        public readonly NeighborhoodsHttpController Neighborhoods;
        public readonly OrganizationsHttpController Organizations;
        public readonly PublishedAppsHttpController PublishedApps;
        public readonly ScenesHttpController Scenes;
        public readonly ScriptsHttpController Scripts;
        public readonly SnapsHttpController Snaps;
        public readonly StorageHttpController Storages;
        public readonly UserHttpController Users;
        public readonly UtilitiesHttpController Utilities;
        public readonly VersioningHttpController Versionings;
        
        public ApiController(IHttpService http)
        {
            Accounts = new AccountHttpController(http);
            Anchors = new AnchorsHttpController(http);
            Apps = new AppHttpController(http);
            AssetLibraries = new AssetLibrariesHttpController(http);
            Collaborators = new CollaboratorsHttpController(http);
            Devices = new DevicesHttpController(http);
            EmailAuths = new EmailAuthHttpController(http);
            Files = new FileHttpController(http);
            HoloAuths = new HoloAuthHttpController(http);
            Members = new MembersHttpController(http);
            Neighborhoods = new NeighborhoodsHttpController(http);
            Organizations = new OrganizationsHttpController(http);
            PublishedApps = new PublishedAppsHttpController(http);
            Scenes = new ScenesHttpController(http);
            Scripts = new ScriptsHttpController(http);
            Snaps = new SnapsHttpController(http);
            Storages = new StorageHttpController(http);
            Users = new UserHttpController(http);
            Utilities = new UtilitiesHttpController(http);
            Versionings = new VersioningHttpController(http);
        }
    }
}