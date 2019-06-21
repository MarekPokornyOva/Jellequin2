using System;

namespace JellequinVs.Diagnostics.Contracts
{
    public static class Contract
    {
        public static void Requires<TException>(bool condition, string userMessage) where TException:Exception
        {
            if (!condition)
                throw (Exception)typeof(TException).GetConstructor(new Type[] { typeof(string) })
                    .Invoke(new object[] { userMessage });
        }

        public static void Requires<TException>(bool condition) where TException : Exception
        {
            Requires<TException>(condition, "Something went wrong.");
        }
    }
}
