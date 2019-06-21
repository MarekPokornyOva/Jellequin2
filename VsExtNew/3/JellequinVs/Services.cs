#pragma warning disable 169 // The field 'fieldname' is never used

namespace Tvl.VisualStudio.Language.Jellequin
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Utilities;

    public static class Services
    {
        [Export]
        [Name(Constants.JellequinContentType)]
        [BaseDefinition("code")]
        private static readonly ContentTypeDefinition JavaContentTypeDefinition;

        [Export]
        [FileExtension(Constants.JellequinFileExtension)]
        [ContentType(Constants.JellequinContentType)]
        private static readonly FileExtensionToContentTypeDefinition JavaFileExtensionToContentTypeDefinition;
    }
}
