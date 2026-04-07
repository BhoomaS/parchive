using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace CH.Business.AutoMapper
{
	// Got the approach to using the ProjectTo method here:
	// https://nodogmablog.bryanhogan.net/2020/05/automapper-projectto-instance-version/
	public partial class AppMappingProfile : Profile
	{
		public AppMappingProfile()
		{
			CreateMap<bool, bool?>().ConvertUsing(b => b);
			CreateMap<int, int?>().ConvertUsing(i => i);

			CreateAuthMaps();
			CreateMgmtPortalMemberOutreachMaps();
			CreateMyPhaAppMemberMaps();
			CreateEmployerMaps();
		}
	}
}
