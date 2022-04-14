using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace SamerNoLayers;

[Dependency(ReplaceServices = true)]
public class SamerNoLayersBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "SamerNoLayers";
}
