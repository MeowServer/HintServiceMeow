using HintServiceMeow.Core.Interface;
using System;

namespace HintServiceMeow.Core.Models
{
    internal class ReferenceHubContext : IPlayerContext
    {
        public ReferenceHub ReferenceHub { get; }

        public ReferenceHubContext(ReferenceHub referenceHub)
        {
            ReferenceHub = referenceHub ?? throw new ArgumentNullException(nameof(referenceHub), "ReferenceHub cannot be null");
        }

        public bool IsValid() => ReferenceHub != null && ReferenceHub.connectionToClient != null;

        public bool Equals(IPlayerContext other)
        {
            if (other is ReferenceHubContext otherContext)
            {
                return ReferenceHub == otherContext.ReferenceHub;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is IPlayerContext context)
            {
                return Equals(context);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ReferenceHub?.GetHashCode() ?? 0;
        }

        public static bool operator ==(ReferenceHubContext left, ReferenceHubContext right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(ReferenceHubContext left, ReferenceHubContext right)
        {
            return !(left == right);
        }
    }
}
