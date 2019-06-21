// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Cci
{
    internal class ResourceSection
    {
        internal ResourceSection(byte[] sectionBytes, uint[] relocations)
        {
            Debug.Assert(sectionBytes != null);
            Debug.Assert(relocations != null);

            SectionBytes = sectionBytes;
            Relocations = relocations;
        }

        internal readonly byte[] SectionBytes;
        //This is the offset into SectionBytes that should be modified.
        //It should have the section's RVA added to it.
        internal readonly uint[] Relocations;
    }

    /// <summary>
    /// A resource file formatted according to Win32 API conventions and typically obtained from a Portable Executable (PE) file.
    /// See the Win32 UpdateResource method for more details.
    /// </summary>
    internal interface IWin32Resource
    {
        /// <summary>
        /// A string that identifies what type of resource this is. Only valid if this.TypeId &lt; 0.
        /// </summary>
        string TypeName
        {
            get;
            // ^ requires this.TypeId < 0;
        }

        /// <summary>
        /// An integer tag that identifies what type of resource this is. If the value is less than 0, this.TypeName should be used instead.
        /// </summary>
        int TypeId
        {
            get;
        }

        /// <summary>
        /// The name of the resource. Only valid if this.Id &lt; 0.
        /// </summary>
        string Name
        {
            get;
            // ^ requires this.Id < 0; 
        }

        /// <summary>
        /// An integer tag that identifies this resource. If the value is less than 0, this.Name should be used instead.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The language for which this resource is appropriate.
        /// </summary>
        uint LanguageId { get; }

        /// <summary>
        /// The code page for which this resource is appropriate.
        /// </summary>
        uint CodePage { get; }

        /// <summary>
        /// The data of the resource.
        /// </summary>
        IEnumerable<byte> Data { get; }
    }
}
