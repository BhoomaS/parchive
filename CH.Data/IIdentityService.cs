using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CH.Data
{
    public interface IIdentityService
    {
        int? UserId { get; }
        int? ChMemberId { get; }
    }
}
