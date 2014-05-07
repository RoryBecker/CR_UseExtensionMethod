using System.ComponentModel.Composition;
using DevExpress.CodeRush.Common;

namespace CR_UseExtensionMethod
{
    [Export(typeof(IVsixPluginExtension))]
    public class CR_UseExtensionMethodExtension : IVsixPluginExtension { }
}