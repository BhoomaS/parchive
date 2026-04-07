using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CH.Models;
using CH.Models.MasterEmployer;

namespace CH.Business.AutoMapper
{
	public partial class AppMappingProfile
	{
		private void CreateEmployerMaps()
		{
			CreateMap<Entities.SnowflakeMasterEmployer, MasterEmployerSummary>()
				// Flattening nav props
				.ForMember(o => o.EmployerDetailId, ex => ex.MapFrom(o => o.EmployerDetail.Id))
				.ForMember(o => o.DefaultUserAssignedId, ex => ex.MapFrom(o => o.EmployerDetail.DefaultUserAssignedId))
				.ForMember(o => o.DefaultUserAssignedFullName, ex => ex.MapFrom(o => o.EmployerDetail.DefaultUserAssigned.FullName));
			
      CreateMap<Entities.SnowflakeMasterEmployer, MasterEmployer>()
				// Flattening nav props
				.ForMember(o => o.EmployerDetailId, ex => ex.MapFrom(o => o.EmployerDetail.Id))
				.ForMember(o => o.DefaultUserAssignedId, ex => ex.MapFrom(o => o.EmployerDetail.DefaultUserAssignedId))
				.ForMember(o => o.DefaultUserAssignedFullName, ex => ex.MapFrom(o => o.EmployerDetail.DefaultUserAssigned.FullName))
				.ForMember(o => o.MyPhaRegistrationMinClinicalRisk, ex => ex.MapFrom(o => o.EmployerDetail.MyPhaRegistrationMinClinicalRisk))
				.ForMember(o => o.EmployerBenefits, ex => ex.MapFrom(o => o.EmployerDetail.EmployerBenefits));

			CreateMap<Entities.EmployerBenefit, EmployerBenefit>()
				.ForMember(o => o.EmployerBenefitId, ex => ex.MapFrom(o => o.Id))
				.ReverseMap();

		}

	}
}
