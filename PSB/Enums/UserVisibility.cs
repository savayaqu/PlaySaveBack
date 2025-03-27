using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSB.Enums
{
    public enum UserVisibility
    {
        Public = 1,
        Private = 2,
        FriendsOnly = 3
    }

    public static class UserVisibilityExtensions
    {
        public static string ToLabel(this UserVisibility visibility)
        {
            return visibility switch
            {
                UserVisibility.Public => "Public",
                UserVisibility.Private => "Private",
                UserVisibility.FriendsOnly => "Friends Only",
                _ => throw new ArgumentOutOfRangeException(nameof(visibility))
            };
        }
    }
}
