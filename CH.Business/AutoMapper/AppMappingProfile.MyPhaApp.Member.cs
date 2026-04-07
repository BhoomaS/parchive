using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CH.Models.MyPhaApp.Member;

namespace CH.Business.AutoMapper
{
  public partial class AppMappingProfile
  {
    private void CreateMyPhaAppMemberMaps()
    {
      CreateMap<Entities.SnowflakeMember, MemberDetail>()
        // Coalescing user-edited and Snowflake fields
        .ForMember(o => o.FirstName, ex => ex.MapFrom(o => o.FirstNameEdited ?? o.FirstName))
        .ForMember(o => o.MiddleName, ex => ex.MapFrom(o => o.MiddleNameEdited ?? o.MiddleName))
        .ForMember(o => o.LastName, ex => ex.MapFrom(o => o.LastNameEdited ?? o.LastName))
        .ForMember(o => o.Address1, ex => ex.MapFrom(o => o.Address1Edited ?? o.Address1))
        .ForMember(o => o.Address2, ex => ex.MapFrom(o => o.Address2Edited ?? o.Address2))
        .ForMember(o => o.City, ex => ex.MapFrom(o => o.CityEdited ?? o.City))
        .ForMember(o => o.State, ex => ex.MapFrom(o => o.StateEdited ?? o.State))
        .ForMember(o => o.ZipCode, ex => ex.MapFrom(o => o.ZipCodeEdited ?? o.ZipCode))
        // Use the Member's login email in case a PHA has changed their contact email in the management portal
        .ForMember(o => o.EmailAddress, ex => ex.MapFrom(o => o.ApplicationUsers.FirstOrDefault().Email))
        .ForMember(o => o.CellPhone, ex => ex.MapFrom(o => o.CellPhoneEdited ?? o.CellPhone))
        .ForMember(o => o.WorkPhone, ex => ex.MapFrom(o => o.WorkPhoneEdited ?? o.WorkPhone))
        .ForMember(o => o.HomePhone, ex => ex.MapFrom(o => o.HomePhoneEdited ?? o.HomePhone));

      CreateMap<Entities.SnowflakeMember, MemberSummary>()
        // Coalescing user-edited and Snowflake fields
        .ForMember(o => o.FirstName, ex => ex.MapFrom(o => o.FirstNameEdited ?? o.FirstName))
        .ForMember(o => o.MiddleName, ex => ex.MapFrom(o => o.MiddleNameEdited ?? o.MiddleName))
        .ForMember(o => o.LastName, ex => ex.MapFrom(o => o.LastNameEdited ?? o.LastName))
        .ForMember(o => o.Address1, ex => ex.MapFrom(o => o.Address1Edited ?? o.Address1))
        .ForMember(o => o.Address2, ex => ex.MapFrom(o => o.Address2Edited ?? o.Address2))
        .ForMember(o => o.City, ex => ex.MapFrom(o => o.CityEdited ?? o.City))
        .ForMember(o => o.State, ex => ex.MapFrom(o => o.StateEdited ?? o.State))
        .ForMember(o => o.ZipCode, ex => ex.MapFrom(o => o.ZipCodeEdited ?? o.ZipCode))
        // Use the Member's login email in case a PHA has changed their contact email in the management portal
        .ForMember(o => o.EmailAddress, ex => ex.MapFrom(o => o.ApplicationUsers.FirstOrDefault().Email))
        .ForMember(o => o.CellPhone, ex => ex.MapFrom(o => o.CellPhoneEdited ?? o.CellPhone))
        .ForMember(o => o.WorkPhone, ex => ex.MapFrom(o => o.WorkPhoneEdited ?? o.WorkPhone))
        .ForMember(o => o.HomePhone, ex => ex.MapFrom(o => o.HomePhoneEdited ?? o.HomePhone))
        // TODO: Consolidate logic for Detail & Summary?
        // Currently, the only diff is this below, Plan
        .ForMember(o => o.PlanName, ex => ex.MapFrom(o => o.PlanName));

      CreateMap<Entities.ApplicationUser, Pha>()
        .ForMember(o => o.Bio, ex => ex.MapFrom(o => o.PhaDetail.Bio))
        .ForMember(o => o.Title, ex => ex.MapFrom(o => o.PhaDetail.Title))
        .ForMember(o => o.HeadshotUrl, ex => ex.MapFrom(o => o.PhaDetail.HeadshotUrl));
    }

  }
}
