namespace Tvl.VisualStudio.Language.Jellequin
{
    using System;

    public static class Constants
    {
        /* The language name (used for the language service) and content type must be the same
         * due to the way Visual Studio internally registers file extensions and content types.
         */
        public const string JellequinLanguageName = "Jellequin";
        public const string JellequinContentType = JellequinLanguageName;
        public const string JellequinFileExtension = ".jell";

        // product registration
        public const int JellequinLanguageResourceId = 100;
        public const string JellequinLanguagePackageNameResourceString = "#110";
        public const string JellequinLanguagePackageDetailsResourceString = "#111";
        public const string JellequinLanguagePackageProductVersionString = "1.0";

        public const string JellequinLanguagePackageGuidString = "1782E1AA-0FBD-4982-B6A8-A1110D95CA58";
        public static readonly Guid JellequinLanguagePackageGuid = new Guid("{" + JellequinLanguagePackageGuidString + "}");

        public const string JellequinLanguageGuidString = "54098E6C-FC60-47F9-9621-0298FDB102EB";
        public static readonly Guid JellequinLanguageGuid = new Guid("{" + JellequinLanguageGuidString + "}");
    }
}
