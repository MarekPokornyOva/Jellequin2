﻿namespace Tvl.VisualStudio.Language.Jellequin.Project.PropertyPages
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PropertyPageArgumentException : ArgumentException
    {
        public PropertyPageArgumentException(string message)
            : base(message)
        {
        }

        protected PropertyPageArgumentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
