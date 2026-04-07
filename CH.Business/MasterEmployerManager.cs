using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

using CH.Data;
using CH.Models.MasterEmployer;
using CH.Business.Services;
using Microsoft.EntityFrameworkCore.Internal;
using AngleSharp.Dom;

namespace CH.Business
{
  public interface IMasterEmployerManager
  {
    Task<IEnumerable<MasterEmployerSummary>> GetMasterEmployersAsync(MasterEmployerFilter filter);
    Task<MasterEmployer> GetMasterEmployerAsync(int chEmployerId);
    Task<SaveMasterEmployerResult> SaveMasterEmployerAsync(MasterEmployer model);
  }

  public class MasterEmployerManager : BaseManager, IMasterEmployerManager
  {
    public MasterEmployerManager(AppDbContext context,
      IIdentityService identityService,
      ICacheService cacheService,
      IConfiguration config,
      IMapper mapper)
      : base(context, identityService, cacheService, config, mapper)
    { }


    private IQueryable<Entities.SnowflakeMasterEmployer> QuerySnowflakeMasterEmployers(
      MasterEmployerFilter filter)
    {
      var query = Context.SnowflakeMasterEmployers.AsQueryable();

      if (filter?.ChEmployerId != null)
        query = query.Where(o => o.ChEmployerId == filter.ChEmployerId.Value).Take(1);

      // TODO: Add more filters

      return query;
    }

    public async Task<IEnumerable<MasterEmployerSummary>> GetMasterEmployersAsync(
        MasterEmployerFilter filter)
    {
      return await Mapper.ProjectTo<MasterEmployerSummary>(QuerySnowflakeMasterEmployers(filter))
        .ToListAsync();
    }

    public async Task<MasterEmployer> GetMasterEmployerAsync(int chEmployerId)
    {
      var filter = new MasterEmployerFilter()
      {
        ChEmployerId = chEmployerId
      };

      var query = QuerySnowflakeMasterEmployers(filter);

      var result = await Mapper.ProjectTo<MasterEmployer>(query)
        .SingleOrDefaultAsync();

      result.EmployerBenefits = await Mapper.ProjectTo<EmployerBenefit>(query
        .SelectMany(o => o.EmployerDetail.EmployerBenefits)
        .Where(o => o.IsDeleted == false))
        .ToListAsync();

      //result.EmployerBenefits = await Mapper.ProjectTo<EmployerBenefit>(query
      //	.Select(o => o.EmployerDetail.EmployerBenefits).AsQueryable())
      //	.ToListAsync();

      return result;
    }

    public async Task<SaveMasterEmployerResult> SaveMasterEmployerAsync(MasterEmployer model)
    {
      //string[] care = { "BUTLER COUNTY", "IMMAN", "IMMANUEL" ,"MEMPHIS OBSTETRICS","HARLAN BAKERIES","CADWELL INDUSTRIES","GLENDORA","SIGGP","YAMADA",
      //                  "ABILITY BEYOND","COUPEBCSMN","18322","18365","AAMCI","ARAPAHOE LIBRARY DISTRICT","JIM ELLIS","TOTEM","VP HOLDINGS","18604","COUPEHRTNH","MOSAIC"};
      //string[] noncare = { "39824", "HYDRO", "DURA", "10000645", "17403", "914465" };
      //var carelist = new List<string>(care);
      //var noncarelist = new List<string>(noncare);

      var nonEngagedCareCoordinationItems = Context.EmployerPhaMappings
          .Where(o => o.ProgramType.Equals("NonEngagedCareCoordination") && o.GroupName.Contains(model.EmployerName)).ToList();

      var nonEngagedItems = Context.EmployerPhaMappings
          .Where(o => o.ProgramType.Equals("NonEngaged") && o.GroupName.Contains(model.EmployerName)).ToList();

      var result = new SaveMasterEmployerResult()
      {
        Detail = model,
      };

      //int indexValue = model.GroupId.IndexOf('.');
      //var subGroupId = indexValue > 0 ? model.GroupId.Substring(0, indexValue) : model.GroupId;

      //assign selected PHA to the selected Client
      var members = Context.MemberDetails.
        Join(Context.SnowflakeMembers, u => u.ChMemberId, uir => uir.ChMemberId,
        (u, uir) => new { u, uir })
        //.Where(m => m.uir.GroupId == subGroupId)
        //.Where(m => m.uir.GroupId.Contains(subGroupId))
        .Where(m=> m.uir.ChEmployerId == model.ChEmployerId)
        .ToList();

        foreach (var member in members)
        {
          //set programtypeid if programtypeid is null
          if (nonEngagedCareCoordinationItems != null)
          {
            foreach (var nonEngagedCareCoordination in nonEngagedCareCoordinationItems)
            {
              if(nonEngagedCareCoordination.ChEmployerId== model.ChEmployerId && (member.u.CodeProgramTypeId == null) && (member.u.UserAssigned == null))
              //if (nonEngagedCareCoordination.GroupId.Contains(subGroupId) && (member.u.CodeProgramTypeId == null) && (member.u.UserAssigned == null))
              {
                member.u.CodeProgramTypeId = 120;  //NonEngagedCareCoordination
              }
            }
          }

          //set programtypeid if programtypeid is null
          if (nonEngagedItems != null)
          {
            foreach (var nonEngaged in nonEngagedItems)
            {
              if(nonEngaged.ChEmployerId == model.ChEmployerId && (member.u.CodeProgramTypeId == null) && (member.u.UserAssigned == null))
              //if (nonEngaged.GroupId.Contains(subGroupId) && (member.u.CodeProgramTypeId == null) && (member.u.UserAssigned == null))
              {
                member.u.CodeProgramTypeId = 12;   //NonEngaged
              }
            }
          }
          member.u.UserAssignedId = model.DefaultUserAssignedId;
        }

        await Context.SaveChangesAsync();



      //assign selected PHA to the selected Client in EmployerPhaMappings Table
      var clients = Context.EmployerPhaMappings
        .Where(o => o.ChEmployerId.Equals(model.ChEmployerId)).ToList();
        //.Where(o => o.GroupName.Contains(model.EmployerName)).ToList();

      if (clients != null)
      {
        foreach (var client in clients)
        {
          client.UserAssignedId = model.DefaultUserAssignedId;
        }

      }   
      await Context.SaveChangesAsync();

      ////assign selected PHA to the selected Client in CareFiles Table
      var res = await Context.EmployerPhaReAssign(model.DefaultUserAssignedId, model.EmployerName.ToUpper().Replace("'","").Trim());
      await Context.SaveChangesAsync();


      //update Employer status in Snowflake.CH_EMPLOYER 
      var employers = Context.SnowflakeEmployers.Where(o => o.ChEmployerId.Equals(model.ChEmployerId)).ToList();
      if (employers != null)
      {
        foreach (var employer in employers)
        {
          employer.IsEnabled = model.IsEnabled;
        }

      }
      await Context.SaveChangesAsync();


      // Validate
      if (result.ModelErrors.Count == 0)
      {
        await using var context = Context.Clone();

        var entities = context.SnowflakeMasterEmployers
          .Include(o => o.EmployerDetail)
          .Where(o => o.EmployerName.Contains(model.EmployerName)).ToList();

        foreach (var entity in entities)
        {

          // TODO: Do work
          // Name probably should be managed by the Snowflake workflow
          // If this can be changed, maybe add EmployerNameEdited, etc.
          //entity.EmployerName = model.EmployerName;

          entity.EmployerDetail ??= new Entities.EmployerDetail()
          { CreatedTimestamp = DateTimeOffset.Now };

          entity.EmployerDetail.DefaultUserAssignedId = model.DefaultUserAssignedId;
          entity.EmployerDetail.MyPhaRegistrationMinClinicalRisk = model.MyPhaRegistrationMinClinicalRisk;

          entity.EmployerDetail.LastEditedTimestamp = DateTimeOffset.Now;
          entity.EmployerDetail.UserLastEditedById = IdentityService.UserId;
          entity.IsEnabled = model.IsEnabled;
          var existingEmployerBenefits = await context.EmployerBenefits
            //.Where(o => !o.IsDeleted && o.EmployerDetail.ChEmployerId == model.ChEmployerId)
            .Where(o => !o.IsDeleted && o.EmployerDetail.ChEmployerId == entity.ChEmployerId)
            .ToListAsync();



          var matchingBenefits = model.EmployerBenefits
            .GroupJoin(existingEmployerBenefits,
              l => l.EmployerBenefitId,
              r => r.Id,
              (l, r) => new
              {
                UpdatedEb = l,
                ExistingEb = r.FirstOrDefault(),
              })
            .ToList();

          matchingBenefits.ForEach(o =>
          {
            if (o.ExistingEb == null)
            {
              // New benefit
              entity.EmployerDetail.EmployerBenefits.Add(new Entities.EmployerBenefit()
              {
                Title = o.UpdatedEb.Title,
                Subtitle = o.UpdatedEb.Subtitle,
                Description = o.UpdatedEb.Description,
                Url = o.UpdatedEb.Url,
                DisplayOrder = o.UpdatedEb.DisplayOrder,
                IsEnabled = o.UpdatedEb.IsEnabled,
                IsDeleted = false,
              });
            }
            else
            {
              // Existing benefit
              o.ExistingEb.Title = o.UpdatedEb.Title;
              o.ExistingEb.Subtitle = o.UpdatedEb.Subtitle;
              o.ExistingEb.Description = o.UpdatedEb.Description;
              o.ExistingEb.Url = o.UpdatedEb.Url;
              o.ExistingEb.DisplayOrder = o.UpdatedEb.DisplayOrder;
              o.ExistingEb.IsEnabled = o.UpdatedEb.IsEnabled;
              o.ExistingEb.LastEditedTimestamp = DateTimeOffset.Now;
              o.ExistingEb.UserLastEditedById = IdentityService.UserId;
            }
          });

          // Detect missing benefits, implied remove
          var deleteBenefits = existingEmployerBenefits.Where(o =>
              !model.EmployerBenefits.Any(x => x.EmployerBenefitId == o.Id))
            .ToList();

          foreach (var ben in deleteBenefits)
            // Removed benefit
            ben.IsDeleted = true;

          if (result.ModelErrors.Count == 0)
            await context.SaveChangesAsync();
        }
      }


      if (result.ModelErrors.Count == 0)
        result.Detail = await GetMasterEmployerAsync(model.ChEmployerId);

      return result;
    }


  }
}
