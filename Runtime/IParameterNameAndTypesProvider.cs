using System;
using System.Collections.Generic;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.vrchat.avatar_parameters_driver
{
    [Obsolete("Use Narazaka.VRChat.AvatarParametersUtil.IParameterNameAndTypesProvider instead.")]
    public interface IParameterNameAndTypesProvider
    {

        IEnumerable<VRCExpressionParameters.Parameter> GetParameterNameAndTypes();
    }
}
