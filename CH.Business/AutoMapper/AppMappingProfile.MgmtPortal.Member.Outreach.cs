using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

using CH.Models.ManagementPortal.Member.Outreach;

namespace CH.Business.AutoMapper
{
  public partial class AppMappingProfile
  {
    private void CreateMgmtPortalMemberOutreachMaps()
    {
      CreateMap<Entities.SnowflakeMember, Demographic>()
        .ForMember(o => o.Id, ex => ex.MapFrom(o => o.ChMemberId))

       // Coalescing user-edited and Snowflake fields
       // .ForMember(o => o.FirstName, ex => ex.MapFrom(o => o.FirstNameEdited ?? o.FirstName))
       .ForMember(o => o.FirstName, ex => ex.MapFrom(o =>  o.FirstName))
        .ForMember(o => o.MiddleName, ex => ex.MapFrom(o =>  o.MiddleName))
        .ForMember(o => o.LastName, ex => ex.MapFrom(o =>  o.LastName))
        .ForMember(o => o.Gender, ex => ex.MapFrom(o =>  o.Gender))
        .ForMember(o => o.Address1, ex => ex.MapFrom(o =>  o.Address1))
        .ForMember(o => o.Address2, ex => ex.MapFrom(o => o.Address2))
        .ForMember(o => o.City, ex => ex.MapFrom(o =>  o.City))
        .ForMember(o => o.State, ex => ex.MapFrom(o =>  o.State))
        .ForMember(o => o.ZipCode, ex => ex.MapFrom(o => o.ZipCode))
        .ForMember(o => o.EmailAddress, ex => ex.MapFrom(o =>  o.EmailAddress))
        .ForMember(o => o.CellPhone, ex => ex.MapFrom(o =>  o.CellPhone))
        .ForMember(o => o.WorkPhone, ex => ex.MapFrom(o => o.WorkPhone))
        .ForMember(o => o.HomePhone, ex => ex.MapFrom(o => o.HomePhone))
        .ForMember(o => o.Ethnicity, ex => ex.MapFrom(o =>  o.EthnicGroup))
         .ForMember(o => o.RelationClass, ex => ex.MapFrom(o =>  o.Relationship))
        .ForMember(o => o.UpdatedPhone, ex => ex.MapFrom(o =>  o.Updated_Phone))
        .ForMember(o => o.SelfReportedPhone, ex => ex.MapFrom(o =>  o.SelfReported_Phone))

        // Flattening
        .ForMember(o => o.Ssn, ex => ex.MapFrom(o => o.Ssn.Length >= 9 ? o.Ssn.Substring(o.Ssn.Length - 4, 4) : null))
        .ForMember(o => o.PrefLanguageId, ex => ex.MapFrom(o => o.MemberDetail.CodePreferredLanguageId))
        .ForMember(o => o.PrefContactTimeId, ex => ex.MapFrom(o => o.MemberDetail.CodePreferredContactTimeId))
        .ForMember(o => o.PrefContactMethodId, ex => ex.MapFrom(o => o.MemberDetail.CodePreferredContactMethodId))
        .ForMember(o => o.SecondaryContact, ex => ex.MapFrom(o => o.MemberDetail.SecondaryContact))
        .ForMember(o => o.Client, ex => ex.MapFrom(o => o.SnowflakeMasterEmployer.ClientName))
        .ForMember(o => o.Group, ex => ex.MapFrom(o => o.SnowflakeMasterEmployer.EmployerName))
        .ForMember(o => o.LearningStyleId, ex => ex.MapFrom(o => o.MemberDetail.CodeLearningStyleId))
        .ForMember(o => o.HasMyPhaAccount, ex => ex.MapFrom(o => o.ApplicationUsers.Any()));
    }
  }
}
