using System;

namespace HintServiceMeow.Core.Interface
{
    internal interface IPlayerContext : IEquatable<IPlayerContext>
    {
        /// <summary>
        /// Return if the player is still valid(i.e. not disconnected).
        /// </summary>
        /// <returns></returns>
        bool IsValid();
    }
}
