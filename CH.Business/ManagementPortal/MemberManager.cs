using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using CH.Data;
using CH.Models.Enums;
using CH.Models.ManagementPortal.Member.Search;
using CH.Business.Services;
using CH.Entities;

namespace CH.Business.ManagementPortal
{
  public partial interface IMemberManager
  {
    Task<IEnumerable<MemberSearchResult>> SearchMembersAsync(MemberSearchRequest request);
    Task<bool> UpdateDoNotCallAsync(int chMemberId, bool value);

    Task<bool> UpdateNotReachableAsync(int chMemberId, bool value);
    Task<bool> UpdateVIPAsync(int chMemberId, bool value);

  }

  public partial class MemberManager : BaseManager, IMemberManager
  {
    private readonly ILogger<MemberManager> _logger;
    public MemberManager(
        AppDbContext context,
        IIdentityService identityService,
        ICacheService cacheService,
        IConfiguration config,
        IMapper mapper,
        ILogger<MemberManager> logger)
    : base(context, identityService, cacheService, config, mapper)
    { _logger = logger; }

    public async Task<IEnumerable<SearchesModel>> DownloadCareSearches()
    {
      var searches = await Context.SearchesTaskAsymc();
      return searches;
    }

    public async Task<IEnumerable<MemberSearchResult>> SearchMembersAsync(
        MemberSearchRequest request)
    {

      var query = Context.SnowflakeMembers.
        Join(Context.SnowflakeEmployers, m => m.ChEmployerId, e => e.ChEmployerId,
        (m, e) => new { m, e })
        .Where(x => x.e.IsEnabled == true && x.m.ChEmployerId == x.e.ChEmployerId && x.m.GroupId == x.e.GroupId).AsQueryable();



      //var query  = Context.SnowflakeMembers.AsQueryable();//.Find();

      //string sp = "SIMPLEPAY";

      var jobj = new Newtonsoft.Json.Linq.JObject();

      if (request.MemberId.HasValue)
      {
        query = query.Where(o => o.m.ChMemberId == request.MemberId);
        jobj.Add("memberId", request.MemberId);
      }
      if (!string.IsNullOrWhiteSpace(request.AlternateMemberId))
      {
        query = query.Where(o => o.m.AlternateMemberId.Contains(request.AlternateMemberId));
        //query = query.Where(o => o.AlternateMemberId == request.AlternateMemberId);

      }
      if (!string.IsNullOrWhiteSpace(request.Name))
      {
        query = query.Where(o =>
          o.m.FirstName.Contains(request.Name)
          || o.m.FirstNameEdited.Contains(request.Name)
          || o.m.LastName.Contains(request.Name)
          || o.m.LastNameEdited.Contains(request.Name)
          || o.m.MiddleName.Contains(request.Name)
          || o.m.MiddleNameEdited.Contains(request.Name));
        jobj.Add("name", request.Name);
      }

      if (!string.IsNullOrWhiteSpace(request.Phone))
      {
        query = query.Where(o =>
          o.m.CellPhone.Contains(request.Phone)
          || o.m.CellPhoneEdited.Contains(request.Phone)
          || o.m.WorkPhone.Contains(request.Phone)
          || o.m.WorkPhoneEdited.Contains(request.Phone)
          || o.m.HomePhone.Contains(request.Phone)
          || o.m.HomePhoneEdited.Contains(request.Phone)
          || o.m.Updated_Phone.Contains(request.Phone)
          || o.m.SelfReported_Phone.Contains(request.Phone));
        jobj.Add("phone", request.Phone);
      }

      if (!string.IsNullOrWhiteSpace(request.SelfReportedPhone))
      {
        query = query.Where(o =>
          o.m.SelfReported_Phone.Contains(request.SelfReportedPhone));
        jobj.Add("selfReportedPhone", request.SelfReportedPhone);
      }

      if (!string.IsNullOrWhiteSpace(request.Email))
      {
        query = query.Where(o =>
          o.m.EmailAddress.Contains(request.Email));
        jobj.Add("email", request.Email);
      }

      if (!string.IsNullOrWhiteSpace(request.SecondaryContact))
      {
        query = query.Where(o =>
           o.m.MemberDetail.SecondaryContact.Contains(request.SecondaryContact)
          );
        jobj.Add("secondaryContact", request.SecondaryContact);
      }

      if (!string.IsNullOrWhiteSpace(request.DOB))
      {
        DateTime dateValue;
        if (DateTime.TryParse(request.DOB, out dateValue))
        {
          query = query.Where(o => o.m.Dob == dateValue);
          jobj.Add("dob", dateValue.ToString("yyyy-MM-dd"));
        }
        //DateTime dateTime = DateTime.ParseExact(request.DOB, "yyyy-MM-dd", null);
        //query = query.Where(o => o.Dob == dateTime);
      }

      if (request.PHA.HasValue)
      {
        query = query.Where(o => o.m.MemberDetail.UserAssignedId == request.PHA.Value);
        jobj.Add("pha", request.PHA);
      }

      if (!string.IsNullOrWhiteSpace(request.Risk))
      {
        string op = null;
        string risk = request.Risk.Trim();
        if (risk.StartsWith(">="))
        {
          op = ">=";
          risk = risk.Substring(2).Trim();
        }
        else if (risk.StartsWith(">"))
        {
          op = ">";
          risk = risk.Substring(1).Trim();
        }
        else if (risk.StartsWith("<="))
        {
          op = "<=";
          risk = risk.Substring(2).Trim();
        }
        else if (risk.StartsWith("<"))
        {
          op = "<";
          risk = risk.Substring(1).Trim();
        }
        int riskValue;
        if (int.TryParse(risk, out riskValue))
        {
          if (op == ">=")
          {
            query = query.Where(o => o.m.ClinicalRisk >= riskValue);
            jobj.Add("risk", $">={riskValue}");
          }
          else if (op == ">")
          {
            query = query.Where(o => o.m.ClinicalRisk > riskValue);
            jobj.Add("risk", $">{riskValue}");
          }
          else if (op == "<=")
          {
            query = query.Where(o => o.m.ClinicalRisk <= riskValue);
            jobj.Add("risk", $"<={riskValue}");
          }
          else if (op == "<")
          {
            query = query.Where(o => o.m.ClinicalRisk < riskValue);
            jobj.Add("risk", $"<{riskValue}");
          }
          else
          {
            query = query.Where(o => o.m.ClinicalRisk == riskValue);
            jobj.Add("risk", $"={riskValue}");
          }
        }
        //query = query.Where(o => o.ClinicalRisk.Contains(request.Risk));
      }

      if (!string.IsNullOrWhiteSpace(request.Client))
      {
        query = query.Where(o => o.e.ClientName.ToLower().Contains(request.Client.ToLower()));
        jobj.Add("client", request.Client);
      }

      if (!string.IsNullOrWhiteSpace(request.GroupName))
      {
        var employer = Context.SnowflakeEmployers.FirstOrDefault(o => o.EmployerName.Contains(request.GroupName));
        //int indexValue = employer.GroupId.IndexOf('.');
        //var subGroupId = indexValue > 0 ? employer.GroupId.Substring(0, indexValue) : employer.GroupId;
        //query = query.Where(o => o.m.GroupId.Contains(subGroupId));
        var chEmployerId = employer.ChEmployerId;

        query = query.Where(o => o.m.ChEmployerId.Equals(chEmployerId));
        jobj.Add("groupName", request.GroupName);
      }

      if (!string.IsNullOrWhiteSpace(request.AlternateMemberId) && !string.IsNullOrWhiteSpace(request.GroupId))
      {
        query = query.Where(o => o.m.AlternateMemberId.Contains(request.AlternateMemberId) && o.m.GroupId.Contains(request.GroupId));
        jobj.Add("groupId", request.GroupId);
      }

      /* if (request.Client.HasValue)
       {
         query = query.Where(o => o.ChEmployerId == request.Client.Value);
         jobj.Add("client", request.Client.Value);
       }*/

      if (!string.IsNullOrWhiteSpace(request.Carrier))
      {
        query = query.Where(o => o.m.CarrierName == request.Carrier);
        jobj.Add("carrier", request.Carrier);
      }
      //if (!string.IsNullOrWhiteSpace(request.ClientId))
      //{
      //  query = query.Where(o => o.m.CLIENT_ID == request.ClientId);
      //  jobj.Add("clientId", request.ClientId);
      //}

      //DateTime? currentdate = DateTime.Now;
      //DateTime? DateofBirth = Convert.ToDateTime(request.DOB);
      //int ageInYears = currentdate.Value.Year - DateofBirth.Value.Year;     
      var result = await query
          .Select(o => new MemberSearchResult
          {
            Id = o.m.ChMemberId,
            MemberId = o.m.MemberId,
            FullName = o.m.FullName,
            // EmailAddress = o.EmailAddressEdited ?? o.EmailAddress,
            EmailAddress = o.m.EmailAddress.ToLower(),
            //Phone = o.CellPhone,
            //Phone = (o.CellPhone != null && o.CellPhone != " ") ? o.CellPhone : (o.HomePhone != null && o.HomePhone != "") ? o.HomePhone : (o.WorkPhone != null && o.WorkPhone != "") ? o.WorkPhone : (o.SelfReported_Phone != null && o.SelfReported_Phone != "") ? o.SelfReported_Phone : o.Updated_Phone,
            Phone = (o.m.CellPhone != "N/A") ? o.m.CellPhone : (o.m.HomePhone != "N/A") ? o.m.HomePhone : (o.m.WorkPhone != "N/A") ? o.m.WorkPhone : o.m.Updated_Phone,
            Dob = o.m.Dob,
            ClinicalRisk = o.m.ClinicalRisk,
            AssignedPha = o.m.MemberDetail.UserAssigned.FullName,
            DoNotCall = o.m.MemberDetail.Dnc,
            HasMyPhaAccount = o.m.ApplicationUsers.Any(),
            AlternateMemberId = o.m.AlternateMemberId,
            Relationship = o.m.Relationship,
            GroupId = o.m.GroupId,
            VIP = (bool)o.m.MemberDetail.Vip,
            Minor = (bool)o.m.MemberDetail.Minor,
            //TruncatedAlternateMemberId = ( o.CarrierName.Contains("SIMPLEPAY") || o.CarrierName.Contains("COUPE") ) ? o.AlternateMemberId.Remove((o.AlternateMemberId).Length - 1) : o.AlternateMemberId,
            TruncatedAlternateMemberId = o.m.AlternateMemberId,
            ClientEmployeeId = o.m.ClientEmployeeId,
            Carrier = o.m.CarrierName,
            SecondaryContact = o.m.MemberDetail.SecondaryContact.ToLower(),
            SelfReportedPhone = o.m.SelfReported_Phone,
            Engaged = o.m.MemberDetail.CodeProgramTypeId,
            CellPhone = o.m.CellPhone,
            WorkPhone = o.m.WorkPhone,
            HomePhone = o.m.HomePhone,
            CurrentStatus = o.m.CurrentStatus,
            ClientId = o.e.ClientId,
          })
          .ToListAsync();

      jobj.Add("results", result.Count);
      string auditJson = Newtonsoft.Json.JsonConvert.SerializeObject(jobj);

      await Context.AuditLogs.AddAsync(new Entities.AuditLog()
      {
        CodeAuditTypeId = (int)AuditType.MemberSearch,
        AdditionalInfo = auditJson,
        CreatedBy = IdentityService.UserId,
        DateCreated = DateTime.Now,
      });
      await Context.SaveChangesAsync();

      return result;
    }

    public async Task<bool> UpdateDoNotCallAsync(int chMemberId, bool value)
    {
      var memberDetail = await Context.MemberDetails
          .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId);
      if (memberDetail == null)
      {
        memberDetail = new Entities.MemberDetail()
        {
          ChMemberId = chMemberId,
        };
        await Context.MemberDetails.AddAsync(memberDetail);
      }

      memberDetail.Dnc = value;
      memberDetail.LastEditedTimestamp = DateTime.Now;
      memberDetail.UserLastEditedById = IdentityService.UserId;

      await Context.SaveChangesAsync();
      return true;
    }

    public async Task<bool> UpdateNotReachableAsync(int chMemberId, bool value)
    {
      var memberDetail = await Context.MemberDetails
          .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId);
      if (memberDetail == null)
      {
        memberDetail = new Entities.MemberDetail()
        {
          ChMemberId = chMemberId,
        };
        await Context.MemberDetails.AddAsync(memberDetail);
      }

      memberDetail.Minor = value;
      memberDetail.LastEditedTimestamp = DateTime.Now;
      memberDetail.UserLastEditedById = IdentityService.UserId;

      await Context.SaveChangesAsync();
      return true;
    }

    public async Task<bool> UpdateVIPAsync(int chMemberId, bool value)
    {
      var memberDetail = await Context.MemberDetails
          .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId);
      if (memberDetail == null)
      {
        memberDetail = new Entities.MemberDetail()
        {
          ChMemberId = chMemberId,
        };
        await Context.MemberDetails.AddAsync(memberDetail);
      }

      memberDetail.Vip = value;
      memberDetail.LastEditedTimestamp = DateTime.Now;
      memberDetail.UserLastEditedById = IdentityService.UserId;

      await Context.SaveChangesAsync();
      return true;
    }
  }
}