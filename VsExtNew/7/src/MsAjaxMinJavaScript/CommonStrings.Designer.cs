﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Ajax.Utilities {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///    A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class CommonStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        internal CommonStrings() {
        }
        
        /// <summary>
        ///    Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MsAjaxMinJavaScript.CommonStrings", typeof(CommonStrings).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///    Overrides the current thread's CurrentUICulture property for all
        ///    resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to : .
        /// </summary>
        public static string ContextSeparator {
            get {
                return ResourceManager.GetString("ContextSeparator", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Fallback character encoding failed.
        /// </summary>
        public static string FallbackEncodingFailed {
            get {
                return ResourceManager.GetString("FallbackEncodingFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Invalid JSON JavaScript nodes encountered during output.
        /// </summary>
        public static string InvalidJSONOutput {
            get {
                return ResourceManager.GetString("InvalidJSONOutput", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to run-time.
        /// </summary>
        public static string Severity0 {
            get {
                return ResourceManager.GetString("Severity0", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to coding.
        /// </summary>
        public static string Severity1 {
            get {
                return ResourceManager.GetString("Severity1", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to code.
        /// </summary>
        public static string Severity2 {
            get {
                return ResourceManager.GetString("Severity2", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to performance.
        /// </summary>
        public static string Severity3 {
            get {
                return ResourceManager.GetString("Severity3", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to improper technique.
        /// </summary>
        public static string Severity4 {
            get {
                return ResourceManager.GetString("Severity4", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to unknown ({0}).
        /// </summary>
        public static string SeverityUnknown {
            get {
                return ResourceManager.GetString("SeverityUnknown", resourceCulture);
            }
        }
    }
}