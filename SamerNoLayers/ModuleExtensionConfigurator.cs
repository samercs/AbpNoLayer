using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;

namespace SamerNoLayers;

public static class ModuleExtensionConfigurator
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        OneTimeRunner.Run(() =>
        {
            
            ConfigureExtraProperties();
        });
    }

    private static void ConfigureExtraProperties()
    {
        ObjectExtensionManager.Instance.Modules()
            .ConfigureIdentity(identity =>
            {
                identity.ConfigureUser(user =>
                {
                    user.AddOrUpdateProperty<Guid>(ApplicationConsts.ProfilePictureId);
                });
            });
    }
}