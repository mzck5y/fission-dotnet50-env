using System;

namespace Fission.DotNetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FunctionNameAttribute : Attribute
    {
        #region Properties

        public string Name { get; init; }

        #endregion

        #region Constructors

        public FunctionNameAttribute(string name)
        {
            Name = name;
        }

        #endregion
    }
}
