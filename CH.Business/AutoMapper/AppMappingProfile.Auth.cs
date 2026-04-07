using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CH.Models.Auth;

namespace CH.Business.AutoMapper
{
	public partial class AppMappingProfile
	{
		private void CreateAuthMaps()
		{
			CreateMap<Entities.ApplicationUser, ApplicationUser>()
				.ForMember(o => o.IsLocked, ex => ex.MapFrom(o => o.LockoutEnd.HasValue
					&& o.LockoutEnd > DateTimeOffset.Now))

				.ReverseMap();

			CreateMap<Entities.ApplicationUser, PhaUser>()
				.ForMember(o => o.IsLocked, ex => ex.MapFrom(o => o.LockoutEnd.HasValue
					&& o.LockoutEnd > DateTimeOffset.Now))
				.ForMember(o => o.Bio, ex => ex.MapFrom(o => o.PhaDetail.Bio))
				.ForMember(o => o.HeadshotUrl, ex => ex.MapFrom(o => o.PhaDetail.HeadshotUrl))
				.ForMember(o => o.Title, ex => ex.MapFrom(o => o.PhaDetail.Title));

			CreateMap<Entities.ApplicationUser, MemberUser>()
				.ForMember(o => o.IsLocked, ex => ex.MapFrom(o => o.LockoutEnd.HasValue
					&& o.LockoutEnd > DateTimeOffset.Now))
				.ForMember(o => o.Dob, ex => ex.MapFrom(o => o.SnowflakeMember.Dob))
				.ForMember(o => o.UserAssignedId, ex => ex.MapFrom(o => o.SnowflakeMember.MemberDetail.UserAssignedId))
				.ForMember(o => o.UserAssignedFullName,
					ex => ex.MapFrom(o => o.SnowflakeMember.MemberDetail.UserAssigned != null ? o.SnowflakeMember.MemberDetail.UserAssigned.FullName : null))
				.ForMember(o => o.Dnc, ex => ex.MapFrom(o => o.SnowflakeMember.MemberDetail.Dnc))
        .ForMember(o => o.Vip, ex => ex.MapFrom(o => o.SnowflakeMember.MemberDetail.Vip))
        .ForMember(o => o.Minor, ex => ex.MapFrom(o => o.SnowflakeMember.MemberDetail.Minor))
        .ForMember(o => o.ChMemberId, ex => ex.MapFrom(o => o.SnowflakeMember.MemberDetail.ChMemberId));
		}
	}
}
