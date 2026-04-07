using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CH.Entities
{
    public partial class ApplicationRole : IdentityRole<int>
    {
    }
}
