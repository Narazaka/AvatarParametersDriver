using System;
using System.Collections.Generic;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.vrchat.avatar_parameters_driver
{
#if AvatarParametersDriver_HAS_AvatarMenuCreator_OLD
    [Obsolete("Use Narazaka.VRChat.AvatarParametersUtil.IParameterNameAndTypesProvider instead.")]
    public interface IParameterNameAndTypesProvider
    {

        IEnumerable<VRCExpressionParameters.Parameter> GetParameterNameAndTypes();
    }
#else
    [Obsolete("Use Narazaka.VRChat.AvatarParametersUtil.IParameterNameAndTypesProvider instead.")]
    public interface IParameterNameAndTypesProvider : Narazaka.VRChat.AvatarParametersUtil.IParameterNameAndTypesProvider
    {
    }
#endif
}
