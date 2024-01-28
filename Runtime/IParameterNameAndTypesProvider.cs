using System.Collections.Generic;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.vrchat.avatar_parameters_driver
{
    public interface IParameterNameAndTypesProvider
    {
        IEnumerable<VRCExpressionParameters.Parameter> GetParameterNameAndTypes();
    }
}
