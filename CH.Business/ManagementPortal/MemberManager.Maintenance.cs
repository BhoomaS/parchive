using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Snowflake.Data.Client;

using CH.Data;
using CH.Models.ManagementPortal.Member.Maintenance;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using Microsoft.AspNetCore.Http;
using System.Xml;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using CH.Entities;
using AutoMapper;
using System.Diagnostics;


namespace CH.Business.ManagementPortal
{
  public partial interface IMemberManager
  {
    Task<object> GetMemberTableJsonAsync(string tableName, int chMemberId, string? type);
    Task<object> GetPHSMemberTableJsonAsync(string tableName, int chMemberId, string? type);
    Task<object> SaveMaintenanceBiometricAsync(int chMemberId, MaintenanceBiometric record);
    Task<object> SaveMaintenancePcpAsync(int chMemberId, MaintenancePcp record);
    Task<object> SaveMaintenanceChronicAsync(int chMemberId, MaintenanceChronic record);
    Task<object> SaveMaintenanceMedicalAsync(int chMemberId, MaintenanceMedical record);
    Task<object> SaveMaintenanceAllergyAsync(int chMemberId, MaintenanceAllergy record);
    Task<object> SaveMaintenanceCareTeamAsync(int chMemberId, MaintenanceCareTeam record);
    Task<object> SaveMaintenanceRecommendationAsync(int chMemberId, MaintenanceRecommendation record);
    Task<object> SaveMaintenanceGoalAsync(int chMemberId, MaintenanceGoal record);
    Task<object> SaveMaintenanceProgramAsync(int chMemberId, MaintenanceProgram record);
    byte[] MaintenanceDownloadPHSAsync(int chMemberId);
  }


  public partial class MemberManager
  {
    private class VaccinationDefinition
    {
      public string Description { get; set; }
      public string Code { get; set; }
      public string Type { get; set; }
    }
    private class PreventiveDefinition
    {
      public string Description { get; set; }
      public string Code { get; set; }
      public string Type { get; set; }
    }

    private static List<VaccinationDefinition> _VaccinationDefinitions =
        new List<VaccinationDefinition>()
    {
            new VaccinationDefinition()
                { Description = "Flu Vaccine", Code = "90686", Type = "Vaccination" },
            new VaccinationDefinition()
                { Description = "Pneumonia Vaccine", Code = "90732", Type = "Vaccination" },
            new VaccinationDefinition()
                { Description = "Shingles Vaccine", Code = "90736", Type = "Vaccination" },
            new VaccinationDefinition()
                { Description = "Tetanus Vaccine", Code = "90715", Type = "Vaccination" }
    };
    private static List<PreventiveDefinition> _PreventiveDefinitions =
       new List<PreventiveDefinition>()
   {
            new PreventiveDefinition()
                { Description = "Colorectal Screening", Code = "3017F", Type = "Preventive" },
            new PreventiveDefinition()
                { Description = "Osteoporosis Screening", Code = "77080", Type = "Preventive" },
            new PreventiveDefinition()
                { Description = "Mammogram Screening", Code = "77067", Type = "Preventive" },
            new PreventiveDefinition()
                { Description = "Cervical Cancer Screening", Code = "88141", Type = "Preventive" },
            new PreventiveDefinition()
                { Description = "Glaucoma Screening", Code = "G0117", Type = "Preventive" },
            new PreventiveDefinition()
                { Description = "Cholesterol Testing", Code = "80061", Type = "Preventive" },
            new PreventiveDefinition()
                { Description = "Chlamydia Screening", Code = "87320", Type = "Preventive" },
            new PreventiveDefinition()
                { Description = "Eye Exam", Code = "Z0100", Type = "Preventive" }
   };

    public bool IsSnowflakeTable(string name)
    {
      bool res = false;
      if(name==SnowflakeTables.Biometrics||name==SnowflakeTables.PCP||name==SnowflakeTables.CareTeam||name==SnowflakeTables.MedicalInfo
        || name == SnowflakeTables.Chronic || name == SnowflakeTables.Recommendations || name == SnowflakeTables.Allergies || name == SnowflakeTables.Goals
        || name == SnowflakeTables.Programs || name== SnowflakeTables.AccoladeEngagements|| name == SnowflakeTables.AccoladeReferrals|| name ==SnowflakeTables.AccoladeUMDatas
        ||name== SnowflakeTables.VoriHealth)
      {
        return true;
      }
      return res;
    }

    public async Task<object> GetMemberTableJsonAsync(string tableName, int chMemberId, string? type)
    {
      var tableDef = SnowflakeManager._TableLookups[tableName];

      string whereClause = tableName == "MedicalInfo" ?
             (tableDef.HasChMemberId
            ? "where CH_MEMBER_ID = ? order by SERVICE_DATE desc limit 30"
            : "where MEMBER_ID in (select MEMBER_ID from VIEW_CH_MEMBER where CH_MEMBER_ID = ?) order by SERVICE_DATE desc limit 30")
            : tableName == "PCP" ?
             (tableDef.HasChMemberId
            ? "where CH_MEMBER_ID = ? order by PCP_DATE_LAST_SEEN desc limit 10"
            : "where MEMBER_ID in (select MEMBER_ID from VIEW_CH_MEMBER where CH_MEMBER_ID = ?) order by PCP_DATE_LAST_SEEN desc limit 10")
            : tableName == "CareTeam" ?
             (tableDef.HasChMemberId
            ? "where CH_MEMBER_ID = ? order by SERVICE_DATE desc limit 10"
            : "where MEMBER_ID in (select MEMBER_ID from VIEW_CH_MEMBER where CH_MEMBER_ID = ?) order by SERVICE_DATE desc limit 10")
            : tableName == "AccoladeEngagements" ?
             (tableDef.HasChMemberId
            ? "where CH_MEMBER_ID = ? order by COMMUNICATIONDATE desc limit 10"
            : "where MEMBER_ID in (select MEMBER_ID from VIEW_CH_MEMBER where CH_MEMBER_ID = ?) order by COMMUNICATIONDATE desc limit 10")
             : tableName == "AccoladeReferrals" ?
             (tableDef.HasChMemberId
            ? "where CH_MEMBER_ID = ? order by EFFECTIVE_DATE desc limit 10"
            : "where MEMBER_ID in (select MEMBER_ID from VIEW_CH_MEMBER where CH_MEMBER_ID = ?) order by EFFECTIVE_DATE desc limit 10")
             : tableName == "AccoladeUMDatas" ?
             (tableDef.HasChMemberId
            ? "where CH_MEMBER_ID = ? order by ADMIT_DATE desc limit 10"
            : "where MEMBER_ID in (select MEMBER_ID from VIEW_CH_MEMBER where CH_MEMBER_ID = ?) order by ADMIT_DATE desc limit 10")
            : (tableDef.HasChMemberId
            ? "where CH_MEMBER_ID = ?"
            : "where MEMBER_ID in (select MEMBER_ID from VIEW_CH_MEMBER where CH_MEMBER_ID = ?)");

      var parameters = new List<IDbDataParameter>();

      var memberParam = new SnowflakeDbParameter();
      memberParam.ParameterName = "1";
      memberParam.DbType = DbType.Int32;
      memberParam.Value = chMemberId;
      parameters.Add(memberParam);
      DataTable dataTable = new DataTable();

      if (!(IsSnowflakeTable(tableName)))
      {
        
        //dataTable = await new SnowflakeManager(this.Context,
        //       this.IdentityService, this.CacheService, this.Config, this.Mapper)
        //   .GetSnowflakeDataTable(tableDef, whereClause, parameters);
      }
      switch (tableName)
      {
        case SnowflakeTables.Biometrics:
          return await GetMemberBiometricJson(chMemberId, new DataTable());
        case SnowflakeTables.PCP:
          return await GetMemberPcpJson(chMemberId, new DataTable());
        case SnowflakeTables.Chronic:
          return await GetMemberChronicJson(chMemberId, new DataTable());
        case SnowflakeTables.MedicalInfo:
          return await GetMemberMedicalInfoJson(chMemberId, new DataTable(), type);
        case SnowflakeTables.Allergies:
          return await GetMemberAllergyJson(chMemberId, dataTable);
        case SnowflakeTables.CareTeam:
          return await GetMemberCareTeamJson(chMemberId, new DataTable());
        case SnowflakeTables.Recommendations:
          return await GetMemberRecommendationJson(chMemberId, new DataTable());
        case SnowflakeTables.Goals:
          return await GetMemberGoalJson(chMemberId, dataTable);
        case SnowflakeTables.Programs:
          return await GetMemberProgamJson(chMemberId, dataTable);
        case SnowflakeTables.AccoladeEngagements:
          return await GetMemberAccoladeEngagementJson(chMemberId, dataTable);
        case SnowflakeTables.AccoladeReferrals:
          return await GetMemberAccoladeReferralJson(chMemberId, dataTable);
        case SnowflakeTables.AccoladeUMDatas:
          return await GetMemberAccoladeUMDataJson(chMemberId, dataTable);
        case SnowflakeTables.VoriHealth:
          return await GetMemberVoriHealthJson(chMemberId, dataTable);
      }
      throw new ArgumentException();
    }


    public async Task<object> GetPHSMemberTableJsonAsync(string tableName, int chMemberId, string? type)
    {
      var tableDef = SnowflakeManager._TableLookups[tableName];

      //string whereClause = tableDef.HasChMemberId
      //        ? "where CH_MEMBER_ID = ?"
      //        : "where MEMBER_ID in (select MEMBER_ID from VIEW_CH_MEMBER where CH_MEMBER_ID = ?)";

      string whereClause = tableName == "MedicalInfo" ? 
              (tableDef.HasChMemberId
             ? "where CH_MEMBER_ID = ? order by SERVICE_DATE desc limit 30"
             : "where MEMBER_ID in (select MEMBER_ID from VIEW_CH_MEMBER where CH_MEMBER_ID = ?) order by SERVICE_DATE desc limit 30")
             : tableName == "PCP" ?
              (tableDef.HasChMemberId
             ? "where CH_MEMBER_ID = ? order by PCP_DATE_LAST_SEEN desc limit 10"
             : "where MEMBER_ID in (select MEMBER_ID from VIEW_CH_MEMBER where CH_MEMBER_ID = ?) order by PCP_DATE_LAST_SEEN desc limit 10")
             : tableName == "CareTeam" ?
              (tableDef.HasChMemberId
             ? "where CH_MEMBER_ID = ? order by SERVICE_DATE desc limit 10"
             : "where MEMBER_ID in (select MEMBER_ID from VIEW_CH_MEMBER where CH_MEMBER_ID = ?) order by SERVICE_DATE desc limit 10")
             : (tableDef.HasChMemberId
             ? "where CH_MEMBER_ID = ?"
             : "where MEMBER_ID in (select MEMBER_ID from VIEW_CH_MEMBER where CH_MEMBER_ID = ?)");

      var parameters = new List<IDbDataParameter>();

      var memberParam = new SnowflakeDbParameter();
      memberParam.ParameterName = "1";
      memberParam.DbType = DbType.Int32;
      memberParam.Value = chMemberId;
      parameters.Add(memberParam);
      DataTable dataTable = new DataTable();
      //if (!(IsSnowflakeTable(tableName)))
      //{
      //   dataTable = await new SnowflakeManager(this.Context,
      //        this.IdentityService, this.CacheService, this.Config, this.Mapper)
      //    .GetSnowflakeDataTable(tableDef, whereClause, parameters);
      //}
      switch (tableName)
      {
        case SnowflakeTables.Biometrics:
          return await GetPHSMemberBiometricJson(chMemberId, new DataTable());
        case SnowflakeTables.PCP:
          return await GetPHSMemberPcpJson(chMemberId, new DataTable());
        case SnowflakeTables.Chronic:
          return await GetPHSMemberChronicJson(chMemberId, new DataTable());
        case SnowflakeTables.MedicalInfo:
          return await GetPHSMemberMedicalInfoJson(chMemberId, new DataTable(), type);
        case SnowflakeTables.Allergies:
          return await GetPHSMemberAllergyJson(chMemberId, dataTable);
        case SnowflakeTables.CareTeam:
          return await GetPHSMemberCareTeamJson(chMemberId, new DataTable());
        case SnowflakeTables.Recommendations:
          return await GetPHSMemberRecommendationJson(chMemberId, new DataTable());
        case SnowflakeTables.Goals:
          return await GetPHSMemberGoalJson(chMemberId, dataTable);
          //case SnowflakeTables.Programs:
          //  return await GetPHSMemberProgamJson(chMemberId, dataTable);
      }
      throw new ArgumentException();
    }

    #region Biometric

    private async Task<object> GetPHSMemberBiometricJson(int chMemberId,
        DataTable snowflakeTable)
    {
      var config = new MapperConfiguration(cfg => cfg.CreateMap<SnowflakeMemberBiometricGlobal, SnowflakeMemberBiometric>());
      var mapper = config.CreateMapper();

      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberBiometricSource>(snowflakeTable)
      //    .Select(o => new Entities.SnowflakeMemberBiometric()
      //    {
      //      Id = o.PHA_BIOMETRIC_ID ?? 0,
      //      ChBiometricId = o.CH_BIOMETRIC_ID,
      //      ChMemberId = o.CH_MEMBER_ID,
      //      RecordId = o.RECORD_ID,
      //      BiometricTestDate = o.BIOMETRIC_TEST_DATE,
      //      FastingFlag = o.FASTING_FLAG,
      //      Glucose = o.GLUCOSE,
      //      A1C = o.A1C,
      //      BloodPressureSystolic = o.BLOOD_PRESSURE_SYSTOLIC,
      //      BloodPressureDiastolic = o.BLOOD_PRESSURE_DIASTOLIC,
      //      TotalCholesterol = o.TOTAL_CHOLESTEROL,
      //      TotalCholesterolHdlRatio = o.TOTAL_CHOLESTEROL_HDL_RATIO,
      //      Ldl = o.LDL,
      //      Hdl = o.HDL,
      //      Triglycerides = o.TRIGLYCERIDES,
      //      WaistCircumference = o.WAIST_CIRCUMFERENCE,
      //      Height = o.HEIGHT,
      //      Weight = o.WEIGHT,
      //      Bmi = o.BMI,
      //      Alt = o.ALT,
      //      Ast = o.AST,
      //      Tobacco = o.TOBACCO,
      //      DssRecordSource = o.DSS_RECORD_SOURCE,
      //      DssLoadDate = o.DSS_LOAD_DATE,
      //      DssCreateTime = o.DSS_CREATE_TIME,
      //      DssUpdateTime = o.DSS_UPDATE_TIME,
      //    })
      //    .ToList();
      var existingList = await Context.SnowflakeMemberBiometrics
          .Where(o => o.ChMemberId == chMemberId)
          .ToListAsync();
      var newlist = await Context.SnowflakeMemberBiometricGlobals
                .Where(o => o.ChMemberId == chMemberId)
                .ToListAsync();     
      //snowflakeList.RemoveAll(o => existingList
                                     //    .Any(e => e.ChBiometricId == o.ChBiometricId || e.Id == o.Id ));
                                     //var sflist = (List<SnowflakeMemberBiometric>)newlist.ToList() as new List<SnowflakeMemberBiometric>();
      List<SnowflakeMemberBiometric> snowflakeListNew = mapper.Map<List<SnowflakeMemberBiometric>>(newlist);

      existingList.RemoveAll(o => snowflakeListNew
          .Any(e => e.ChBiometricId == o.ChBiometricId || e.Id == o.Id));
      var combinedList = existingList.Concat(snowflakeListNew).OrderByDescending(o => o.BiometricTestDate).ToList();


      return combinedList.Select(o => GetMemberBiometricJson(o)).Take(1);
    }

    private object GetMemberBiometricJson(Entities.SnowflakeMemberBiometric entity)
    {
      return new object[]
      {
                entity.Id,
                entity.ChBiometricId,
                entity.BiometricTestDate?.ToString("d"),
                entity.A1C,
                entity.Bmi,
                entity.BloodPressureSystolic,
                entity.BloodPressureDiastolic,
                entity.Height,
                entity.Weight,
                entity.WaistCircumference,
                entity.Glucose,
                entity.Hdl,
                entity.Ldl,
                entity.Triglycerides,
                entity.Tobacco,
                entity.FastingFlag,
                entity.TotalCholesterol,
                entity.TotalCholesterolHdlRatio,
                entity.Alt,
                entity.Ast,
      };
    }

    private async Task<object> GetMemberBiometricJson(int chMemberId,
        DataTable snowflakeTable)
    {

      var config = new MapperConfiguration(cfg => cfg.CreateMap<SnowflakeMemberBiometricGlobal, SnowflakeMemberBiometric>());
      var mapper = config.CreateMapper();

      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberBiometricSource>(snowflakeTable)
      //    .Select(o => new Entities.SnowflakeMemberBiometric()
      //    {
      //      Id = o.PHA_BIOMETRIC_ID ?? 0,
      //      ChBiometricId = o.CH_BIOMETRIC_ID,
      //      ChMemberId = o.CH_MEMBER_ID,
      //      RecordId = o.RECORD_ID,
      //      BiometricTestDate = o.BIOMETRIC_TEST_DATE,
      //      FastingFlag = o.FASTING_FLAG,
      //      Glucose = o.GLUCOSE,
      //      A1C = o.A1C,
      //      BloodPressureSystolic = o.BLOOD_PRESSURE_SYSTOLIC,
      //      BloodPressureDiastolic = o.BLOOD_PRESSURE_DIASTOLIC,
      //      TotalCholesterol = o.TOTAL_CHOLESTEROL,
      //      TotalCholesterolHdlRatio = o.TOTAL_CHOLESTEROL_HDL_RATIO,
      //      Ldl = o.LDL,
      //      Hdl = o.HDL,
      //      Triglycerides = o.TRIGLYCERIDES,
      //      WaistCircumference = o.WAIST_CIRCUMFERENCE,
      //      Height = o.HEIGHT,
      //      Weight = o.WEIGHT,
      //      Bmi = o.BMI,
      //      Alt = o.ALT,
      //      Ast = o.AST,
      //      Tobacco = o.TOBACCO,
      //      DssRecordSource = o.DSS_RECORD_SOURCE,
      //      DssLoadDate = o.DSS_LOAD_DATE,
      //      DssCreateTime = o.DSS_CREATE_TIME,
      //      DssUpdateTime = o.DSS_UPDATE_TIME,
      //    })
      //    .ToList();
      var existingList = await Context.SnowflakeMemberBiometrics
          .Where(o => o.ChMemberId == chMemberId)
          .ToListAsync();
      var newlist = await Context.SnowflakeMemberBiometricGlobals
               .Where(o => o.ChMemberId == chMemberId)
               .ToListAsync();
     
      List<SnowflakeMemberBiometric> snowflakeListNew = mapper.Map<List<SnowflakeMemberBiometric>>(newlist);
      existingList.RemoveAll(o => snowflakeListNew
          .Any(e => e.ChBiometricId == o.ChBiometricId || e.Id == o.Id));

      //var combinedList = existingList.Concat(snowflakeList).ToList();
      var combinedList = existingList.Concat(snowflakeListNew).OrderByDescending(o => o.BiometricTestDate).ToList();

      return combinedList.Select(o => GetMemberBiometricJson(o)).ToList();
    }

    public async Task<object> SaveMaintenanceBiometricAsync(int chMemberId, MaintenanceBiometric record)
    {
      if (!record.TestDate.HasValue)
        return null;

      Entities.SnowflakeMemberBiometric entity = null;

      if (record.Id.HasValue)
      {
        entity = await Context.SnowflakeMemberBiometrics
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId && o.Id == record.Id);
      }
      if (entity == null && record.SfId.HasValue)
      {
        entity = await Context.SnowflakeMemberBiometrics
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId &&
                o.ChBiometricId == record.SfId);
      }

      if (entity == null)
      {
        entity = new Entities.SnowflakeMemberBiometric()
        {
          ChMemberId = chMemberId,
          ChBiometricId = record.SfId,
        };
        await Context.SnowflakeMemberBiometrics.AddAsync(entity);
      }

      entity.BiometricTestDate = record.TestDate;
      entity.A1C = record.A1C;
      entity.Bmi = record.BMI;
      entity.BloodPressureSystolic = record.Systolic;
      entity.BloodPressureDiastolic = record.Diastolic;
      entity.Height = record.Height;
      entity.Weight = record.Weight;
      entity.WaistCircumference = record.Waist;
      entity.Glucose = record.Glucose;
      entity.Hdl = record.HDL;
      entity.Ldl = record.LDL;
      entity.Triglycerides = record.Triglycerides;
      entity.Tobacco = record.Tobacco;
      entity.FastingFlag = record.Fasting;
      entity.TotalCholesterol = record.TotalCholesterol;
      entity.TotalCholesterolHdlRatio = record.HdlRatio;
      entity.Alt = record.ALT;
      entity.Ast = record.AST;

      entity.LastEditedTimestamp = DateTime.Now;
      entity.UserLastEditedById = IdentityService.UserId;

      await Context.SaveChangesAsync();

      return GetMemberBiometricJson(entity);
    }

    #endregion // Biometric


    #region PCP

    private async Task<object> GetPHSMemberPcpJson(int chMemberId,
        DataTable snowflakeTable)
    {
      var config = new MapperConfiguration(cfg => cfg.CreateMap<SnowflakeMemberPcpGlobal, SnowflakeMemberPcp>());
      var mapper = config.CreateMapper();
      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberPcpSource>(snowflakeTable)
      //    .Select(o => new Entities.SnowflakeMemberPcp()
      //    {
      //      Id = o.PHA_MEMBER_PCP_ID ?? 0,
      //      ChMemberPcpId = o.CH_MEMBER_PCP_ID,
      //      ChMemberId = o.CH_MEMBER_ID,
      //      PcpAddress = o.PCP_ADDRESS,
      //      PcpZipCode = o.PCP_ZIP_CODE,
      //      PcpNpi = o.PCP_NPI,
      //      PcpDateLastSeen = o.PCP_DATE_LAST_SEEN,
      //      PcpName = o.PCP_NAME,
      //      PcpPhone = o.PCP_PHONE,
      //      PcpSpecialty = o.PCP_SPECIALTY,
      //      PcpFax = o.PCP_FAX,
      //    }).OrderBy(o => o.PcpDateLastSeen)
      //    .ToList();

      var existingList = await Context.SnowflakeMemberPcps
          .Where(o => o.ChMemberId == chMemberId).OrderByDescending(o => o.PcpDateLastSeen).OrderBy(o => o.PcpName)
          .ToListAsync();
      var newlist = await Context.SnowflakeMemberPcpsGlobal
          .Where(o => o.ChMemberId == chMemberId).OrderByDescending(o => o.PcpDateLastSeen).OrderBy(o => o.PcpName)
          .ToListAsync();

      List<SnowflakeMemberPcp> snowflakeListNew = mapper.Map<List<SnowflakeMemberPcp>>(newlist);

      snowflakeListNew.RemoveAll(o => existingList
          .Any(e => (e.ChMemberPcpId == o.ChMemberPcpId && e.PcpDateLastSeen == o.PcpDateLastSeen)));


      existingList.RemoveAll(o => snowflakeListNew
          .Any(e => (e.ChMemberPcpId == o.ChMemberPcpId && e.PcpDateLastSeen > o.PcpDateLastSeen)
          ));

      snowflakeListNew.RemoveAll(o => existingList
          .Any(e => (e.ChMemberPcpId == o.ChMemberPcpId && e.PcpDateLastSeen > o.PcpDateLastSeen)
          ));

      var combinedList = existingList.Concat(snowflakeListNew).ToList();

      var pcp = combinedList.OrderByDescending(o => o.PcpDateLastSeen).ToList();

      return pcp.Select(o => GetMemberPcpJson(o)).Take(1).ToList();

    }

    private object GetMemberPcpJson(Entities.SnowflakeMemberPcp entity)
    {
      return new object[]
      {
                entity.Id,
                entity.ChMemberPcpId,
                entity.PcpAddress,
                entity.PcpZipCode,
                entity.PcpNpi,
                entity.PcpDateLastSeen?.ToString("d"),
                entity.PcpName,
                entity.PcpPhone,
                entity.PcpSpecialty,
                entity.PcpFax,
      };
    }

    private async Task<object> GetMemberPcpJson(int chMemberId,
        DataTable snowflakeTable)
    {
      var config = new MapperConfiguration(cfg => cfg.CreateMap<SnowflakeMemberPcpGlobal, SnowflakeMemberPcp>());
      var mapper = config.CreateMapper();
      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberPcpSource>(snowflakeTable)
      //    .Select(o => new Entities.SnowflakeMemberPcp()
      //    {
      //      Id = o.PHA_MEMBER_PCP_ID ?? 0,
      //      ChMemberPcpId = o.CH_MEMBER_PCP_ID,
      //      ChMemberId = o.CH_MEMBER_ID,
      //      PcpAddress = o.PCP_ADDRESS,
      //      PcpZipCode = o.PCP_ZIP_CODE,
      //      PcpNpi = o.PCP_NPI,
      //      PcpDateLastSeen = o.PCP_DATE_LAST_SEEN,
      //      PcpName = o.PCP_NAME,
      //      PcpPhone = o.PCP_PHONE,
      //      PcpSpecialty = o.PCP_SPECIALTY,
      //      PcpFax = o.PCP_FAX,
      //    })
      //   .ToList();
      var existingList = await Context.SnowflakeMemberPcps
          .Where(o => o.ChMemberId == chMemberId).OrderByDescending(o => o.PcpDateLastSeen).OrderBy(o => o.PcpName)
          .ToListAsync();
      var newlist = await Context.SnowflakeMemberPcpsGlobal
          .Where(o => o.ChMemberId == chMemberId).OrderByDescending(o => o.PcpDateLastSeen).OrderBy(o => o.PcpName)
          .ToListAsync();

      List<SnowflakeMemberPcp> snowflakeListNew = mapper.Map<List<SnowflakeMemberPcp>>(newlist);

      snowflakeListNew.RemoveAll(o => existingList
          .Any(e =>  (e.ChMemberPcpId == o.ChMemberPcpId && e.PcpDateLastSeen == o.PcpDateLastSeen)  ));


      existingList.RemoveAll(o => snowflakeListNew
          .Any(e => (e.ChMemberPcpId == o.ChMemberPcpId && e.PcpDateLastSeen > o.PcpDateLastSeen)
          ));

      snowflakeListNew.RemoveAll(o => existingList
          .Any(e => (e.ChMemberPcpId == o.ChMemberPcpId && e.PcpDateLastSeen > o.PcpDateLastSeen)
          ));

      var combinedList = existingList.Concat(snowflakeListNew).ToList();

      combinedList.OrderByDescending(o => o.PcpDateLastSeen).ToList();

      return combinedList.Select(o => GetMemberPcpJson(o)).Take(10).ToList();
    }

    public async Task<object> SaveMaintenancePcpAsync(int chMemberId, MaintenancePcp record)
    {
      if (string.IsNullOrWhiteSpace(record.Name) &&
          string.IsNullOrWhiteSpace(record.Specialty))
      {
        return null;
      }

      Entities.SnowflakeMemberPcp entity = null;

      if (record.Id.HasValue)
      {
        entity = await Context.SnowflakeMemberPcps
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId && o.Id == record.Id);
      }
      if (entity == null || record.Id.HasValue)
      {
        entity = await Context.SnowflakeMemberPcps
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId &&
                o.ChMemberPcpId == record.SfId);
      }

      if (entity == null)
      {
        entity = new Entities.SnowflakeMemberPcp()
        {
          ChMemberId = chMemberId,
          ChMemberPcpId = record.SfId,
        };
        await Context.SnowflakeMemberPcps.AddAsync(entity);
      }

      entity.PcpAddress = record.Address;
      entity.PcpZipCode = record.Zip;
      entity.PcpNpi = record.NPI;
      entity.PcpDateLastSeen = record.DateLastSeen;
      entity.PcpName = record.Name;
      entity.PcpPhone = record.Phone;
      entity.PcpSpecialty = record.Specialty;
      entity.PcpFax = record.Fax;

      entity.LastEditedTimestamp = DateTime.Now;
      entity.UserLastEditedById = IdentityService.UserId;

      await Context.SaveChangesAsync();

      return GetMemberPcpJson(entity);
    }

    #endregion // PCP


    #region Chronic

    private async Task<object> GetPHSMemberChronicJson(int chMemberId,
       DataTable snowflakeTable)
    {
      var config = new MapperConfiguration(cfg => cfg.CreateMap<SnowflakeMemberChronicConditionGlobal, SnowflakeMemberChronicCondition>());
      var mapper = config.CreateMapper();
      var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberChronicConditionSource>(snowflakeTable)
          .Select(o => new Entities.SnowflakeMemberChronicCondition()
          {
            Id = o.PHA_MEMBER_CHRONIC_CONDITION_ID ?? 0,
            ChMemberChronicConditionId = o.CH_MEMBER_CHRONIC_CONDITION_ID,
            ChMemberId = o.CH_MEMBER_ID,
            ChronicCondition = o.CHRONIC_CONDITION,
            IcdCode = o.ICD_CODE,
            ChronicConditionDate = o.CHRONIC_CONDITION_DATE,
            ProviderName = o.PROVIDER_NAME,
            ExcludeReporting = o.EXCLUDE_REPORTING,
            ExcludeScoring = o.EXCLUDE_SCORING,
          }).OrderByDescending(o => o.ChronicConditionDate)
          .ToList();

      var existingList = await Context.SnowflakeMemberChronicConditions
          .OrderByDescending(o => o.ChronicConditionDate)
          .Where(o => o.ChMemberId == chMemberId)
          .ToListAsync();
      var newlist = await Context.SnowflakeMemberChronicConditionsGlobal
         .Where(o => o.ChMemberId == chMemberId)
         .ToListAsync();
      List<SnowflakeMemberChronicCondition> snowflakeListNew = mapper.Map<List<SnowflakeMemberChronicCondition>>(newlist);

      snowflakeListNew.RemoveAll(o => existingList
          .Any(e => e.ChMemberChronicConditionId == o.ChMemberChronicConditionId || e.Id == o.Id));
      var combinedList = existingList.Concat(snowflakeListNew).OrderBy(o => o.ExcludeScoring).ToList();

      return combinedList.Select(o => GetPHSMemberChronicJson(o)).Take(10).ToList();
    }

    private object GetPHSMemberChronicJson(Entities.SnowflakeMemberChronicCondition entity)
    {
      return new object[]
      {
                entity.Id,
                entity.ChMemberChronicConditionId,
                entity.ChronicCondition,
                entity.IcdCode,
                entity.ProviderName,
                entity.ChronicConditionDate?.ToString("d"),
                entity.ExcludeReporting,
                entity.ExcludeScoring,
      };
    }

    private object GetMemberChronicJson(Entities.SnowflakeMemberChronicCondition entity)
    {
      return new object[]
      {
                entity.Id,
                entity.ChMemberChronicConditionId,
                entity.ChronicCondition,
                entity.IcdCode,
                entity.ChronicConditionDate?.ToString("d"),
                entity.ExcludeReporting,
                entity.ExcludeScoring,
      };
    }

    private async Task<object> GetMemberChronicJson(int chMemberId,
        DataTable snowflakeTable)
    {
      var config = new MapperConfiguration(cfg => cfg.CreateMap<SnowflakeMemberChronicConditionGlobal, SnowflakeMemberChronicCondition>());
      var mapper = config.CreateMapper();
      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberChronicConditionSource>(snowflakeTable)  
      //    .Select(o => new Entities.SnowflakeMemberChronicCondition()
      //    {
      //      Id = o.PHA_MEMBER_CHRONIC_CONDITION_ID ?? 0,
      //      ChMemberChronicConditionId = o.CH_MEMBER_CHRONIC_CONDITION_ID,
      //      ChMemberId = o.CH_MEMBER_ID,
      //      ChronicCondition = o.CHRONIC_CONDITION,
      //      IcdCode = o.ICD_CODE,
      //      ChronicConditionDate = o.CHRONIC_CONDITION_DATE,
      //      ProviderName = o.PROVIDER_NAME,
      //      ExcludeReporting = o.EXCLUDE_REPORTING,
      //      ExcludeScoring = o.EXCLUDE_SCORING,
      //    })
      //    .ToList();
      var existingList = await Context.SnowflakeMemberChronicConditions  
          .Where(o => o.ChMemberId == chMemberId)
          .ToListAsync();
       var newlist = await Context.SnowflakeMemberChronicConditionsGlobal  
          .Where(o => o.ChMemberId == chMemberId)
          .ToListAsync();
      List<SnowflakeMemberChronicCondition> snowflakeListNew = mapper.Map<List<SnowflakeMemberChronicCondition>>(newlist);

      snowflakeListNew.RemoveAll(o => existingList
          .Any(e => e.ChMemberChronicConditionId == o.ChMemberChronicConditionId || e.Id == o.Id));  


      var combinedList = existingList.Concat(snowflakeListNew).ToList(); 

      return combinedList.Select(o => GetMemberChronicJson(o)).ToList();
    }

    public async Task<object> SaveMaintenanceChronicAsync(int chMemberId, MaintenanceChronic record)
    {
      if (string.IsNullOrWhiteSpace(record.Condition))
        return null;

      Entities.SnowflakeMemberChronicCondition entity = null;

      if (record.Id.HasValue)
      {
        entity = await Context.SnowflakeMemberChronicConditions
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId && o.Id == record.Id);
      }
      if (entity == null && record.SfId.HasValue)
      {
        entity = await Context.SnowflakeMemberChronicConditions
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId &&
                o.ChMemberChronicConditionId == record.SfId);
      }

      if (entity == null)
      {
        entity = new Entities.SnowflakeMemberChronicCondition()
        {
          ChMemberId = chMemberId,
          ChMemberChronicConditionId = record.SfId,
        };
        await Context.SnowflakeMemberChronicConditions.AddAsync(entity);
      }

      entity.ChronicCondition = record.Condition;
      entity.IcdCode = record.ICD;
      entity.ChronicConditionDate = record.Date;
      entity.ExcludeReporting = record.ExcludeReporting;
      entity.ExcludeScoring = record.ExcludeScoring;

      entity.LastEditedTimestamp = DateTime.Now;
      entity.UserLastEditedById = IdentityService.UserId;

      await Context.SaveChangesAsync();

      return GetMemberChronicJson(entity);
    }

    #endregion // Chronic


    #region Medical Info
    private object GetPHSMemberMedicalInfoJson(Entities.SnowflakeMemberMedicalInfo entity)
    {
      return new object[]
      {
                entity.Id,
                entity.ChMemberMedicalInfoId,
                entity.ServiceDate?.ToString("d"),
                entity.Description,
                entity.Code,
                entity.Type,
                entity.Frequency,
                entity.ProviderName,
                entity.ExcludeReporting,
                entity.ExcludeScoring,

      };
    }
    private async Task<object> GetPHSMemberMedicalInfoJson(int chMemberId,
        DataTable snowflakeTable, string? type)
    {
      var config = new MapperConfiguration(cfg => cfg.CreateMap<SnowflakeMemberMedicalInfoGlobal, SnowflakeMemberMedicalInfo>());
      var mapper = config.CreateMapper();
      if (type == "undefined")
      {
        type = "'%'";
      }
      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberMedicalInfoSource>(snowflakeTable)
      //   .Select(o => new Entities.SnowflakeMemberMedicalInfo()
      //   {
      //     Id = o.PHA_MEMBER_MEDICAL_INFO_ID ?? 0,
      //     ChMemberMedicalInfoId = o.CH_MEMBER_MEDICAL_INFO_ID,
      //     ChMemberId = o.CH_MEMBER_ID,
      //     ServiceDate = o.SERVICE_DATE,
      //     Description = o.DESCRIPTION,
      //     Code = o.CODE,
      //     Type = o.TYPE,
      //     Frequency = o.FREQUENCY,
      //     ProviderName = o.PROVIDER_NAME,
      //     ExcludeReporting = o.EXCLUDE_REPORTING,
      //     ExcludeScoring = o.EXCLUDE_SCORING,
      //   })
      //   //.Where(o => o.ChMemberId == chMemberId)
      //   .Where(o => o.ChMemberId == chMemberId && (type == "'%'" || o.Type == type))
      //   .OrderByDescending(o => o.ServiceDate)
      //   .ToList();

      var existingList = await Context.SnowflakeMemberMedicalInfoes
         // .Where(o => o.ChMemberId == chMemberId)
          .Where(o => o.ChMemberId == chMemberId && (type == "'%'" || o.Type == type))
          .OrderByDescending(o => o.ServiceDate).OrderBy(o =>o.Description)
          .ToListAsync();
       var newlist = await Context.SnowflakeMemberMedicalInfoesGlobal
         // .Where(o => o.ChMemberId == chMemberId)
          .Where(o => o.ChMemberId == chMemberId && (type == "'%'" || o.Type == type))
          .OrderByDescending(o => o.ServiceDate).OrderBy(o => o.Description)
          .ToListAsync();
      List<SnowflakeMemberMedicalInfo> snowflakeListNew = mapper.Map<List<SnowflakeMemberMedicalInfo>>(newlist);

      snowflakeListNew.RemoveAll(o => existingList
          //.Any(e => (e.ChMemberMedicalInfoId == o.ChMemberMedicalInfoId || e.Id == o.Id)));
          .Any(e => (e.ChMemberMedicalInfoId == o.ChMemberMedicalInfoId || e.Id == o.Id) && (type == "'%'" || o.Type == type)));
     
        //var combinedList = existingList.Concat(snowflakeList).OrderBy(o => o.ExcludeScoring).ToList();
        var combinedList = existingList.Concat(snowflakeListNew).OrderByDescending(x=>x.ServiceDate).OrderBy(o => o.Description).ToList();

      //foreach (var vd in _VaccinationDefinitions)
      //  {
      //    if (!combinedList.Any(o => o.Type == vd.Type && o.Code == vd.Code))
      //    {
      //      combinedList.Add(new Entities.SnowflakeMemberMedicalInfo()
      //      {
      //        ChMemberId = chMemberId,
      //        Description = vd.Description,
      //        Code = vd.Code,
      //        Type = vd.Type,
      //      });
      //    }
      //  }
      //  foreach (var pd in _PreventiveDefinitions)
      //  {
      //    if (!combinedList.Any(o => o.Type == pd.Type && o.Code == pd.Code))
      //    {
      //      combinedList.Add(new Entities.SnowflakeMemberMedicalInfo()
      //      {
      //        ChMemberId = chMemberId,
      //        Description = pd.Description,
      //        Code = pd.Code,
      //        Type = pd.Type,
      //      });
      //    }
      //  }

        combinedList.RemoveAll(c => c.ExcludeReporting == true);

        combinedList.RemoveAll(o => combinedList
                  .Any(e => (e.Description == o.Description) && (e.ServiceDate > o.ServiceDate)));

        combinedList.RemoveAll(o => combinedList
                .Any(e => (e.Description == o.Description) && (e.ServiceDate != o.ServiceDate) && o.ServiceDate is null));

        combinedList.Select(o => o.Description).ToList();

        combinedList = combinedList.OrderByDescending(o => o.ServiceDate).ToList();

        return combinedList.Select(o => GetPHSMemberMedicalInfoJson(o)).ToList();
    }

    

    private object GetMemberMedicalInfoJson(Entities.SnowflakeMemberMedicalInfo entity)
    {
      return new object[]
      {
                entity.Id,
                entity.ChMemberMedicalInfoId,
                entity.ServiceDate?.ToString("d"),
                entity.Description,
                entity.Code,
                entity.Type,
                entity.Frequency,
                entity.ProviderName,
                entity.ExcludeReporting,
                entity.ExcludeScoring,
      };
    }

    private async Task<object> GetMemberMedicalInfoJson(int chMemberId,
        DataTable snowflakeTable, string? type)
    {
      var config = new MapperConfiguration(cfg => cfg.CreateMap<SnowflakeMemberMedicalInfoGlobal, SnowflakeMemberMedicalInfo>());
      var mapper = config.CreateMapper();
      if (type == "undefined")
      {
        type = "'%'";
      }
        
      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberMedicalInfoSource>(snowflakeTable)
      //    .Select(o => new Entities.SnowflakeMemberMedicalInfo()
      //    {
      //      Id = o.PHA_MEMBER_MEDICAL_INFO_ID ?? 0,
      //      ChMemberMedicalInfoId = o.CH_MEMBER_MEDICAL_INFO_ID,
      //      ChMemberId = o.CH_MEMBER_ID,
      //      ServiceDate = o.SERVICE_DATE,
      //      Description = o.DESCRIPTION,
      //      Code = o.CODE,
      //      Type = o.TYPE,
      //      Frequency = o.FREQUENCY,
      //      ProviderName = o.PROVIDER_NAME,
      //      ExcludeReporting = o.EXCLUDE_REPORTING,
      //      ExcludeScoring = o.EXCLUDE_SCORING,
      //    }).Where(o => o.ChMemberId == chMemberId && (type == "'%'" || o.Type == type))
      //    .ToList();
      
      var existingList = await Context.SnowflakeMemberMedicalInfoes
          .Where(o => o.ChMemberId == chMemberId && (type == "'%'" || o.Type == type)).OrderByDescending(o=>o.ServiceDate).Take(30)
          .ToListAsync();
      var newlist = await Context.SnowflakeMemberMedicalInfoesGlobal
          .Where(o => o.ChMemberId == chMemberId && (type == "'%'" || o.Type == type)).OrderByDescending(o => o.ServiceDate)
          .ToListAsync();
      List<SnowflakeMemberMedicalInfo> snowflakeListNew = mapper.Map<List<SnowflakeMemberMedicalInfo>>(newlist);

      snowflakeListNew.RemoveAll(o => existingList
          .Any(e => (e.ChMemberMedicalInfoId == o.ChMemberMedicalInfoId || e.Id == o.Id) && (type == "'%'" || o.Type == type) ));

      if (type == "Pharmacy")
      {

        var combinedList = existingList.Concat(snowflakeListNew).OrderByDescending(x=>x.ServiceDate).Take(30).ToList();
        combinedList.RemoveAll(c => c.ExcludeReporting == true);

        return combinedList.Select(o => GetMemberMedicalInfoJson(o)).ToList();
      }
      else
      {
        var combinedList = existingList.Concat(snowflakeListNew).ToList();

        //foreach (var vd in _VaccinationDefinitions)
        //{
        //  if (!combinedList.Any(o => o.Type == vd.Type && o.Code == vd.Code))
        //  {
        //    combinedList.Add(new Entities.SnowflakeMemberMedicalInfo()
        //    {
        //      ChMemberId = chMemberId,
        //      Description = vd.Description,
        //      Code = vd.Code,
        //      Type = vd.Type,
        //    });
        //  }
        //}
        //foreach (var pd in _PreventiveDefinitions)
        //{
        //  if (!combinedList.Any(o => o.Type == pd.Type && o.Code == pd.Code))
        //  {
        //    combinedList.Add(new Entities.SnowflakeMemberMedicalInfo()
        //    {
        //      ChMemberId = chMemberId,
        //      Description = pd.Description,
        //      Code = pd.Code,
        //      Type = pd.Type,
        //    });
        //  }
        //}

        combinedList.RemoveAll(o => combinedList
                .Any(e => (e.Description == o.Description) && (e.ServiceDate > o.ServiceDate)));

        combinedList.RemoveAll(o => combinedList
                .Any(e => (e.Description == o.Description) && (e.ServiceDate != o.ServiceDate) && o.ServiceDate is null));

        combinedList.Select(o => o.Description).ToList();

        combinedList= combinedList.OrderByDescending(o => o.ServiceDate).Take(30).ToList();

        return combinedList.Select(o => GetMemberMedicalInfoJson(o)).ToList();
      }


    }

    public async Task<object> SaveMaintenanceMedicalAsync(int chMemberId, MaintenanceMedical record)
    {
      if (string.IsNullOrWhiteSpace(record.Type))
        return null;

      Entities.SnowflakeMemberMedicalInfo entity = null;

      if (record.Id.HasValue)
      {
        entity = await Context.SnowflakeMemberMedicalInfoes
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId && o.Id == record.Id);
      }
      if (entity == null && record.SfId.HasValue)
      {
        entity = await Context.SnowflakeMemberMedicalInfoes
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId &&
                o.ChMemberMedicalInfoId == record.SfId);
      }

      if (entity == null)
      {
        entity = new Entities.SnowflakeMemberMedicalInfo()
        {
          ChMemberId = chMemberId,
          ChMemberMedicalInfoId = record.SfId,
        };
        await Context.SnowflakeMemberMedicalInfoes.AddAsync(entity);
      }

      entity.ServiceDate = record.ServiceDate;
      entity.Description = record.Description;
      entity.Code = record.Code;
      entity.Type = record.Type;
      if ("pharmacy".Equals(record.Type, StringComparison.OrdinalIgnoreCase))
        entity.Frequency = record.Frequency;
      entity.ExcludeReporting = record.ExcludeReporting;
      entity.ExcludeScoring = record.ExcludeScoring;

      entity.LastEditedTimestamp = DateTime.Now;
      entity.UserLastEditedById = IdentityService.UserId;

      await Context.SaveChangesAsync();
      return GetMemberMedicalInfoJson(entity);
    }

    #endregion // Medical Info


    #region Allergy

    private async Task<object> GetPHSMemberAllergyJson(int chMemberId,
        DataTable snowflakeTable)
    {
      var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberAllergySource>(snowflakeTable)
          .Select(o => new Entities.SnowflakeMemberAllergy()
          {
            Id = o.PHA_MEMBER_ALLERGY_ID ?? 0,
            ChMemberAllergyId = o.CH_MEMBER_ALLERGY_ID,
            ChMemberId = o.CH_MEMBER_ID,
            Allergies = o.ALLERGIES,
          }).OrderBy(o => o.Allergies)
          .ToList();

      var existingList = await Context.SnowflakeMemberAllergies
          .Where(o => o.ChMemberId == chMemberId).OrderBy(o => o.Allergies)
          .ToListAsync();

      snowflakeList.RemoveAll(o => existingList
          .Any(e => e.ChMemberAllergyId == o.ChMemberAllergyId || e.Id == o.Id));
      var combinedList = existingList.Concat(snowflakeList).ToList();

      return combinedList.Select(o => GetMemberAllergyJson(o)).TakeLast(10).ToList();
    }

    private object GetMemberAllergyJson(Entities.SnowflakeMemberAllergy entity)
    {
      return new object[]
      {
                entity.Id,
                entity.ChMemberAllergyId,
                entity.Allergies,
      };
    }

    private async Task<object> GetMemberAllergyJson(int chMemberId,
        DataTable snowflakeTable)
    {
      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberAllergySource>(snowflakeTable)
      //    .Select(o => new Entities.SnowflakeMemberAllergy()
      //    {
      //      Id = o.PHA_MEMBER_ALLERGY_ID ?? 0,
      //      ChMemberAllergyId = o.CH_MEMBER_ALLERGY_ID,
      //      ChMemberId = o.CH_MEMBER_ID,
      //      Allergies = o.ALLERGIES,
      //    })
      //    .ToList();

      var existingList = await Context.SnowflakeMemberAllergies
          .Where(o => o.ChMemberId == chMemberId)
          .ToListAsync();

      //snowflakeList.RemoveAll(o => existingList
      //    .Any(e => e.ChMemberAllergyId == o.ChMemberAllergyId || e.Id == o.Id));
      //var combinedList = existingList.Concat(snowflakeList).ToList();

      return existingList.Select(o => GetMemberAllergyJson(o)).ToList();
    }

    public async Task<object> SaveMaintenanceAllergyAsync(int chMemberId, MaintenanceAllergy record)
    {
      if (string.IsNullOrWhiteSpace(record.Allergy) && !record.Id.HasValue)
        return null;

      Entities.SnowflakeMemberAllergy entity = null;

      if (record.Id.HasValue)
      {
        entity = await Context.SnowflakeMemberAllergies
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId && o.Id == record.Id);
      }
      if (entity == null && record.SfId.HasValue)
      {
        entity = await Context.SnowflakeMemberAllergies
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId &&
                o.ChMemberAllergyId == record.SfId);
      }

      if (entity == null)
      {
        entity = new Entities.SnowflakeMemberAllergy()
        {
          ChMemberId = chMemberId,
          ChMemberAllergyId = record.SfId,
        };
        await Context.SnowflakeMemberAllergies.AddAsync(entity);
      }

      entity.Allergies = record.Allergy;

      entity.LastEditedTimestamp = DateTime.Now;
      entity.UserLastEditedById = IdentityService.UserId;

      await Context.SaveChangesAsync();

      return GetMemberAllergyJson(entity);
    }

    #endregion // Allergy


    #region Care Team

    private async Task<object> GetPHSMemberCareTeamJson(int chMemberId,
      DataTable snowflakeTable)
    {
      var config = new MapperConfiguration(cfg => cfg.CreateMap<SnowflakeMemberCareTeamGlobal, SnowflakeMemberCareTeam>());
      var mapper = config.CreateMapper();
      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberCareTeamSource>(snowflakeTable)
      //    .Select(o => new Entities.SnowflakeMemberCareTeam()
      //    {
      //      Id = o.PHA_MEMBER_CARE_TEAM_ID ?? 0,
      //      ChMemberCareTeamId = o.CH_MEMBER_CARE_TEAM_ID,
      //      ProviderId = o.PROVIDER_ID,
      //      ChMemberId = o.CH_MEMBER_ID,
      //      ProviderName = o.PROVIDER_NAME,
      //      ProviderPhone = o.PROVIDER_PHONE,
      //      ProviderSpecialty = o.PROVIDER_SPECIALTY,
      //      ProviderFax = o.PROVIDER_FAX,
      //      ServiceDate = o.SERVICE_DATE,
      //      ExcludeReporting = o.EXCLUDE_REPORTING,
      //      Active = o.ACTIVE,
      //    }).OrderByDescending(o => o.ServiceDate)
      //    .ToList();
      var existingList = await Context.SnowflakeMemberCareTeams
          .Where(o => o.ChMemberId == chMemberId).OrderByDescending(o => o.ServiceDate).Take(10)
          .ToListAsync();
       var newlist = await Context.SnowflakeMemberCareTeamsGlobal
          .Where(o => o.ChMemberId == chMemberId).OrderByDescending(o => o.ServiceDate).Take(10)
          .ToListAsync();
      List<SnowflakeMemberCareTeam> snowflakeListNew = mapper.Map<List<SnowflakeMemberCareTeam>>(newlist);

      snowflakeListNew.RemoveAll(o => existingList
          .Any(e => e.ChMemberCareTeamId == o.ChMemberCareTeamId || e.Id == o.Id));
      var combinedList = existingList.Concat(snowflakeListNew).ToList();
      combinedList.RemoveAll(c => c.ExcludeReporting == true);

      return combinedList.Select(o => GetMemberCareTeamJson(o)).Take(10).ToList();
    }

    private object GetMemberCareTeamJson(Entities.SnowflakeMemberCareTeam entity)
    {
      return new object[]
      {
                entity.Id,
                entity.ChMemberCareTeamId,
                entity.ProviderName,
                entity.ProviderPhone,
                entity.ProviderSpecialty,
                entity.ProviderFax,
                entity.ServiceDate?.ToString("d"),
                entity.ExcludeReporting,
                entity.Active,
      };
    }

    private async Task<object> GetMemberCareTeamJson(int chMemberId,
        DataTable snowflakeTable)
    {
      var config = new MapperConfiguration(cfg => cfg.CreateMap<SnowflakeMemberCareTeamGlobal, SnowflakeMemberCareTeam>());
      var mapper = config.CreateMapper();
      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberCareTeamSource>(snowflakeTable)
      //    .Select(o => new Entities.SnowflakeMemberCareTeam()
      //    {
      //      Id = o.PHA_MEMBER_CARE_TEAM_ID ?? 0,
      //      ChMemberCareTeamId = o.CH_MEMBER_CARE_TEAM_ID,
      //      ProviderId = o.PROVIDER_ID,
      //      ChMemberId = o.CH_MEMBER_ID,
      //      ProviderName = o.PROVIDER_NAME,
      //      ProviderPhone = o.PROVIDER_PHONE,
      //      ProviderSpecialty = o.PROVIDER_SPECIALTY,
      //      ProviderFax = o.PROVIDER_FAX,
      //      ServiceDate = o.SERVICE_DATE,
      //      ExcludeReporting = o.EXCLUDE_REPORTING,
      //      Active = o.ACTIVE,
      //    })
      //    .ToList();
      var existingList = await Context.SnowflakeMemberCareTeams
          .Where(o => o.ChMemberId == chMemberId).OrderByDescending(o => o.ServiceDate).Take(10)
          .ToListAsync();
       var newlist = await Context.SnowflakeMemberCareTeamsGlobal
          .Where(o => o.ChMemberId == chMemberId).OrderByDescending(o => o.ServiceDate).Take(10)
          .ToListAsync();
      List<SnowflakeMemberCareTeam> snowflakeListNew = mapper.Map<List<SnowflakeMemberCareTeam>>(newlist);

      snowflakeListNew.RemoveAll(o => existingList
          .Any(e => e.ChMemberCareTeamId == o.ChMemberCareTeamId || e.Id == o.Id));
      var combinedList = existingList.Concat(snowflakeListNew).ToList();

      return combinedList.Select(o => GetMemberCareTeamJson(o)).Take(10).ToList();
    }

    public async Task<object> SaveMaintenanceCareTeamAsync(int chMemberId, MaintenanceCareTeam record)
    {
      if (string.IsNullOrWhiteSpace(record.Name) && string.IsNullOrWhiteSpace(record.Specialty))
      {
        return null;
      }

      Entities.SnowflakeMemberCareTeam entity = null;

      if (record.Id.HasValue)
      {
        entity = await Context.SnowflakeMemberCareTeams
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId && o.Id == record.Id);
      }
      if (entity == null && record.SfId.HasValue)
      {
        entity = await Context.SnowflakeMemberCareTeams
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId &&
                o.ChMemberCareTeamId == record.SfId);
      }

      if (entity == null)
      {
        entity = new Entities.SnowflakeMemberCareTeam()
        {
          ChMemberId = chMemberId,
          ChMemberCareTeamId = record.SfId,
        };
        await Context.SnowflakeMemberCareTeams.AddAsync(entity);
      }

      entity.ProviderName = record.Name;
      entity.ProviderPhone = record.Phone;
      entity.ProviderSpecialty = record.Specialty;
      entity.ProviderFax = record.Fax;
      entity.ServiceDate = record.ServiceDate;
      entity.ExcludeReporting = record.ExcludeReporting;
      entity.Active = record.Active;

      entity.LastEditedTimestamp = DateTime.Now;
      entity.UserLastEditedById = IdentityService.UserId;

      await Context.SaveChangesAsync();

      return GetMemberCareTeamJson(entity);
    }

    #endregion // Care Team


    #region Recommendation

    private async Task<object> GetPHSMemberRecommendationJson(int chMemberId,
        DataTable snowflakeTable)
    {
      DateTime minDate = DateTime.Today.AddYears(-1);
      DateTime pastFourWeeks = DateTime.Today.AddDays(-28);
      var config = new MapperConfiguration(cfg => cfg.CreateMap<SnowflakeMemberRecommendationGlobal, SnowflakeMemberRecommendation>());
      var mapper = config.CreateMapper();
      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberRecommendationSource>(snowflakeTable)
      //    .Select(o => new Entities.SnowflakeMemberRecommendation()
      //    {
      //      Id = o.PHA_MEMBER_RECOMMENDATION_ID ?? 0,
      //      ChMemberRecommendationId = o.CH_MEMBER_RECOMMENDATION_ID,
      //      ChMemberId = o.CH_MEMBER_ID,
      //      Recommendation = o.RECOMMENDATION,
      //      RecommendationPriority = o.RECOMMENDATION_PRIORITY,
      //      Plan = o.PLAN,
      //      RecommendationDate = o.RECOMMENDATION_DATE,
      //      Completed = o.COMPLETED,
      //      ExcludeReporting = o.EXCLUDE_REPORTING,
      //      RecommendationCode = o.RECOMMENDATION_CODE,
      //      BestResourceToMeetMetric = o.BEST_RESOURCE_TO_MEET_METRIC
      //    })
      //    .Where(o => o.RecommendationDate >= minDate)
      //    .ToList();
            var existingList = await Context.SnowflakeMemberRecommendations
                .Where(o => o.ChMemberId == chMemberId)
               // .Where(o => o.RecommendationDate >= minDate)
                .Where(o => o.LastEditedTimestamp >= pastFourWeeks)
                .ToListAsync();
            var newlist = await Context.SnowflakeMemberRecommendationsGlobal
                .Where(o => o.ChMemberId == chMemberId)
                .Where(o => o.RecommendationDate >= minDate)
                .ToListAsync();
      List<SnowflakeMemberRecommendation> snowflakeListNew = mapper.Map<List<SnowflakeMemberRecommendation>>(newlist);

      snowflakeListNew.RemoveAll(o => existingList
               .Any(e => e.Recommendation == o.Recommendation));

      var combinedList = existingList.Concat(snowflakeListNew).OrderByDescending(o => o.RecommendationDate)
         .OrderBy(o => o.RecommendationPriority).OrderBy(o => o.Completed).ToList();

      //snowflakeListNew.RemoveAll(o => existingList
      //          .Any(e => e.ChMemberRecommendationId == o.ChMemberRecommendationId || e.Id == o.Id));

      //      var combinedList = existingList.Concat(snowflakeListNew).OrderByDescending(o => o.RecommendationDate).OrderBy(o => o.RecommendationPriority).OrderBy(o => o.Completed).ToList();

          
      //    combinedList.RemoveAll(o => combinedList
      //              .Any(e => (e.Recommendation == o.Recommendation) && (e.RecommendationDate > o.RecommendationDate)));

        combinedList.RemoveAll(c => c.ExcludeReporting == true);

        combinedList.OrderBy(o => o.RecommendationPriority).ToList();

          return combinedList.Select(o => GetMemberRecommendationJson(o)).Take(10).ToList();
    }

    private object GetMemberRecommendationJson(Entities.SnowflakeMemberRecommendation entity)
    {
      return new object[]
      {
                entity.Id,
                entity.ChMemberRecommendationId,
                entity.Recommendation,
                entity.RecommendationPriority,
                entity.Plan,
                entity.RecommendationDate?.ToString("d"),
                entity.Completed,
                entity.ExcludeReporting,
                entity.RecommendationCode,
                entity.BestResourceToMeetMetric

      };
    }

    private async Task<object> GetMemberRecommendationJson(int chMemberId,
        DataTable snowflakeTable)
    {
      DateTime minDate = DateTime.Today.AddYears(-1);
      DateTime pastFourWeeks = DateTime.Today.AddDays(-28);
      var config = new MapperConfiguration(cfg => cfg.CreateMap<SnowflakeMemberRecommendationGlobal, SnowflakeMemberRecommendation>());
      var mapper = config.CreateMapper();
      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberRecommendationSource>(snowflakeTable)
      //    .Select(o => new Entities.SnowflakeMemberRecommendation()
      //    {
      //      Id = o.PHA_MEMBER_RECOMMENDATION_ID ?? 0,
      //      ChMemberRecommendationId = o.CH_MEMBER_RECOMMENDATION_ID,
      //      ChMemberId = o.CH_MEMBER_ID,
      //      Recommendation = o.RECOMMENDATION,
      //      RecommendationPriority = o.RECOMMENDATION_PRIORITY,
      //      Plan = o.PLAN,
      //      RecommendationDate = o.RECOMMENDATION_DATE,
      //      Completed = o.COMPLETED,
      //      ExcludeReporting = o.EXCLUDE_REPORTING,
      //      RecommendationCode = o.RECOMMENDATION_CODE,
      //      BestResourceToMeetMetric = o.BEST_RESOURCE_TO_MEET_METRIC
      //    })
      //    .Where(o => o.RecommendationDate >= minDate)
      //    .ToList();
            var existingList = await Context.SnowflakeMemberRecommendations
                .Where(o => o.ChMemberId == chMemberId)
                //.Where(o => o.RecommendationDate >= minDate)
                .Where(o => o.LastEditedTimestamp >= pastFourWeeks)
                .ToListAsync();
            var newlist = await Context.SnowflakeMemberRecommendationsGlobal
                .Where(o => o.ChMemberId == chMemberId)
                .Where(o => o.RecommendationDate >= minDate)
                .ToListAsync();

      List<SnowflakeMemberRecommendation> snowflakeListNew = mapper.Map<List<SnowflakeMemberRecommendation>>(newlist);

      //existingList.RemoveAll(o => snowflakeListNew
      //          .Any(e => e.Recommendation == o.Recommendation));

      snowflakeListNew.RemoveAll(o => existingList
                .Any(e => e.Recommendation == o.Recommendation));

      //snowflakeListNew.RemoveAll(o => existingList
      //          .Any(e => e.ChMemberRecommendationId == o.ChMemberRecommendationId || e.Id == o.Id ));

      //snowflakeListNew.RemoveAll(o => existingList
      //          .Any(e => e.ChMemberRecommendationId == o.ChMemberRecommendationId || e.Id == o.Id ));

      var combinedList = existingList.Concat(snowflakeListNew).OrderByDescending(o => o.RecommendationDate)
          .OrderBy(o => o.RecommendationPriority).OrderBy(o => o.Completed).ToList();

      //combinedList.RemoveAll(o => combinedList
      //               .Any(e => (e.Recommendation == o.Recommendation) && (e.RecommendationDate > o.RecommendationDate)));

      combinedList.OrderBy(o => o.RecommendationPriority).ToList();

      return combinedList.Select(o => GetMemberRecommendationJson(o)).ToList();
    }


    public async Task<object> SaveMaintenanceRecommendationAsync(int chMemberId, MaintenanceRecommendation record)
    {
      if (string.IsNullOrWhiteSpace(record.Recommendation) &&
          string.IsNullOrWhiteSpace(record.Plan))
      {
        return null;
      }

      Entities.SnowflakeMemberRecommendation entity = null;

      if (record.Id.HasValue)
      {
        entity = await Context.SnowflakeMemberRecommendations
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId && o.Id == record.Id);
      }
      if (entity == null && record.SfId.HasValue)
      {
        entity = await Context.SnowflakeMemberRecommendations
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId &&
                o.ChMemberRecommendationId == record.SfId);
      }
      //if (entity == null && record.RecommendationPriority.HasValue)
      //{
      //  entity = await Context.SnowflakeMemberRecommendations
      //      .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId &&
      //          o.RecommendationPriority == record.RecommendationPriority);
      //}

      if (entity == null)
      {
        entity = new Entities.SnowflakeMemberRecommendation()
        {
          ChMemberId = chMemberId,
          ChMemberRecommendationId = record.SfId,
          RecommendationPriority = record.RecommendationPriority
        };
        await Context.SnowflakeMemberRecommendations.AddAsync(entity);
      }

      entity.Recommendation = record.Recommendation;
      entity.Plan = record.Plan;
      entity.RecommendationDate = record.Date;
      entity.BestResourceToMeetMetric = record.BestResourceToMeetMetric;
      entity.RecommendationPriority = record.RecommendationPriority;
      entity.Completed = record.Completed;
      entity.ExcludeReporting = record.ExcludeReporting;

      entity.LastEditedTimestamp = DateTime.Now;
      entity.UserLastEditedById = IdentityService.UserId;
      entity.RecommendationCode = record.RecommendationCode;

      await Context.SaveChangesAsync();
          
      return GetMemberRecommendationJson(entity);
    }

    #endregion // Recommendation


    #region Goal

    private async Task<object> GetPHSMemberGoalJson(int chMemberId,
     DataTable snowflakeTable)
    {
      var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberPersonalGoalsPlanSource>(snowflakeTable)
          .Select(o => new Entities.SnowflakeMemberPersonalGoalsPlan()
          {
            Id = o.PHA_MEMBER_PERSONAL_GOAL_PLAN_ID ?? 0,
            ChMemberPersonalGoalPlanId = o.CH_MEMBER_PERSONAL_GOAL_PLAN_ID,
            ChMemberId = o.CH_MEMBER_ID,
            PersonalGoals = o.PERSONAL_GOALS,
            PersonalPlan = o.PERSONAL_PLAN,
            PlanB = o.PLAN_B,
            ExcludeReporting = o.EXCLUDE_REPORTING,
          })
          .ToList();

      var existingList = await Context.SnowflakeMemberPersonalGoalsPlans
          .Where(o => o.ChMemberId == chMemberId)
          .ToListAsync();

      snowflakeList.RemoveAll(o => existingList
          .Any(e => e.ChMemberPersonalGoalPlanId == o.ChMemberPersonalGoalPlanId || e.Id == o.Id));
      var combinedList = existingList.Concat(snowflakeList).ToList();

      return combinedList.Select(o => GetMemberGoalJson(o)).TakeLast(10).ToList();
    }

    private object GetMemberGoalJson(Entities.SnowflakeMemberPersonalGoalsPlan entity)
    {
      return new object[]
      {
                entity.Id,
                entity.ChMemberPersonalGoalPlanId,
                entity.PersonalGoals,
                entity.PersonalPlan,
                entity.PlanB,
                entity.ExcludeReporting,
      };
    }

    private async Task<object> GetMemberGoalJson(int chMemberId,
        DataTable snowflakeTable)
    {
      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberPersonalGoalsPlanSource>(snowflakeTable)
      //    .Select(o => new Entities.SnowflakeMemberPersonalGoalsPlan()
      //    {
      //      Id = o.PHA_MEMBER_PERSONAL_GOAL_PLAN_ID ?? 0,
      //      ChMemberPersonalGoalPlanId = o.CH_MEMBER_PERSONAL_GOAL_PLAN_ID,
      //      ChMemberId = o.CH_MEMBER_ID,
      //      PersonalGoals = o.PERSONAL_GOALS,
      //      PersonalPlan = o.PERSONAL_PLAN,
      //      PlanB = o.PLAN_B,
      //      ExcludeReporting = o.EXCLUDE_REPORTING,
      //    })
      //    .ToList();

      var existingList = await Context.SnowflakeMemberPersonalGoalsPlans
          .Where(o => o.ChMemberId == chMemberId)
          .ToListAsync();

      //snowflakeList.RemoveAll(o => existingList
      //    .Any(e => e.ChMemberPersonalGoalPlanId == o.ChMemberPersonalGoalPlanId || e.Id == o.Id));
      //var combinedList = existingList.Concat(snowflakeList).ToList();

      return existingList.Select(o => GetMemberGoalJson(o)).ToList();
    }

    public async Task<object> SaveMaintenanceGoalAsync(int chMemberId, MaintenanceGoal record)
    {
      if (string.IsNullOrWhiteSpace(record.Goal) &&
          string.IsNullOrWhiteSpace(record.Plan))
      {
        return null;
      }

      Entities.SnowflakeMemberPersonalGoalsPlan entity = null;

      if (record.Id.HasValue)
      {
        entity = await Context.SnowflakeMemberPersonalGoalsPlans
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId && o.Id == record.Id);
      }
      if (entity == null && record.SfId.HasValue)
      {
        entity = await Context.SnowflakeMemberPersonalGoalsPlans
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId &&
                o.ChMemberPersonalGoalPlanId == record.SfId);
      }

      if (entity == null)
      {
        entity = new Entities.SnowflakeMemberPersonalGoalsPlan()
        {
          ChMemberId = chMemberId,
          ChMemberPersonalGoalPlanId = record.SfId,
        };
        await Context.SnowflakeMemberPersonalGoalsPlans.AddAsync(entity);
      }

      entity.PersonalGoals = record.Goal;
      entity.PersonalPlan = record.Plan;
      entity.PlanB = record.PlanB;
      entity.ExcludeReporting = record.ExcludeReporting;

      entity.LastEditedTimestamp = DateTime.Now;
      entity.UserLastEditedById = IdentityService.UserId;

      await Context.SaveChangesAsync();

      return GetMemberGoalJson(entity);
    }

    #endregion // Goal


    #region Program

    //private async Task<object> GetPHSMemberProgamJson(int chMemberId,
    //   DataTable snowflakeTable)
    //{
    //  var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberProgramSource>(snowflakeTable)
    //      .Select(o => new Entities.SnowflakeMemberProgram()
    //      {
    //        Id = o.PHA_MEMBER_PROGRAM_ID ?? 0,
    //        ChMemberProgramId = o.CH_MEMBER_PROGRAM_ID,
    //        ChMemberId = o.CH_MEMBER_ID,
    //        ProgramName = o.PROGRAM_NAME,
    //        ProgramDate = o.PROGRAM_DATE,
    //        Completed = o.COMPLETED,
    //        ExcludeReporting = o.EXCLUDE_REPORTING,
    //      })
    //      .ToList();

    //  var existingList = await Context.SnowflakeMemberPrograms
    //      .Where(o => o.ChMemberId == chMemberId)
    //      .ToListAsync();

    //  snowflakeList.RemoveAll(o => existingList
    //      .Any(e => e.ChMemberProgramId == o.ChMemberProgramId || e.Id == o.Id));
    //  var combinedList = existingList.Concat(snowflakeList).ToList();

    //  return combinedList.Select(o => GetMemberProgamJson(o)).TakeLast(10).ToList();
    //}

    static object GetMemberProgamJson(Entities.SnowflakeMemberProgram entity)
    {
      return new object[]
      {
                entity.Id,
                entity.ChMemberProgramId,
                entity.ProgramName,
                entity.ProgramDate?.ToString("d"),
                entity.Completed,
                entity.ExcludeReporting,
      };
    }

    private async Task<object> GetMemberProgamJson(int chMemberId,
        DataTable snowflakeTable)
    {
      var snowflakeList = ConvertDataTableToList<Entities.SnowflakeMemberProgramSource>(snowflakeTable)
          .Select(o => new Entities.SnowflakeMemberProgram()
          {
            Id = o.PHA_MEMBER_PROGRAM_ID ?? 0,
            ChMemberProgramId = o.CH_MEMBER_PROGRAM_ID,
            ChMemberId = o.CH_MEMBER_ID,
            ProgramName = o.PROGRAM_NAME,
            ProgramDate = o.PROGRAM_DATE,
            Completed = o.COMPLETED,
            ExcludeReporting = o.EXCLUDE_REPORTING,
          })
          .ToList();

      var existingList = await Context.SnowflakeMemberPrograms
          .Where(o => o.ChMemberId == chMemberId)
          .ToListAsync();

      snowflakeList.RemoveAll(o => existingList
          .Any(e => e.ChMemberProgramId == o.ChMemberProgramId || e.Id == o.Id));
      var combinedList = existingList.Concat(snowflakeList).ToList();

      return combinedList.Select(o => GetMemberProgamJson(o)).ToList();
    }

    public async Task<object> SaveMaintenanceProgramAsync(int chMemberId, MaintenanceProgram record)
    {
      if (string.IsNullOrWhiteSpace(record.Name))
        return null;

      Entities.SnowflakeMemberProgram entity = null;

      if (record.Id.HasValue)
      {
        entity = await Context.SnowflakeMemberPrograms
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId && o.Id == record.Id);
      }
      if (entity == null && record.SfId.HasValue)
      {
        entity = await Context.SnowflakeMemberPrograms
            .FirstOrDefaultAsync(o => o.ChMemberId == chMemberId &&
                o.ChMemberProgramId == record.SfId);
      }

      if (entity == null)
      {
        entity = new Entities.SnowflakeMemberProgram()
        {
          ChMemberId = chMemberId,
          ChMemberProgramId = record.SfId,
        };
        await Context.SnowflakeMemberPrograms.AddAsync(entity);
      }

      entity.ProgramName = record.Name;
      entity.ProgramDate = record.Date;
      entity.Completed = record.Completed;
      entity.ExcludeReporting = record.ExcludeReporting;

      entity.LastEditedTimestamp = DateTime.Now;
      entity.UserLastEditedById = IdentityService.UserId;

      await Context.SaveChangesAsync();

      return GetMemberProgamJson(entity);
    }



    #endregion // Program

    #region AccoladeEngagement
    static object GetMemberAccoladeEngagementJson(dynamic data)
    {
      return new 
      {
        data.FirstName,
        data.LastName,
        data.CommunicationDate,
        data.CallerType,
        data.CommunicationDirection,
        data.CommunicationObjectiveCategory,
        data.CommunicationObjective,
        data.ChMemberId,
        data.FileReceivedDate,
        data.LastEditedTimestamp

        //data.AccoladeId,
        //data.CommunicationId,
        //data.BirthDate,
        //data.Gender,
        //data.Relationship,
        //data.CommunicationChannel,
        //data.SpeakingTo,
        //data.ClinicalEngagement,
        //data.ReceivedDate,
        //data.ImportDate
      };
    }
    private async Task<object> GetMemberAccoladeEngagementJson(int chMemberId,
        DataTable snowflakeTable)
    {
      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeAccoladeEngagementSource>(snowflakeTable)
      //    .Select(o => new
      //    {
      //      AccoladeId = o.ACCOLADE_ID,
      //      CommunicationId = o.COMMUNICATIONID,
      //      FirstName = o.FIRSTNAME,
      //      LastName = o.LASTNAME,
      //      BirthDate = o.BIRTHDATE,
      //      Gender = o.GENDER,
      //      Relationship = o.RELATIONSHIP,
      //      CommunicationChannel = o.COMMUNICATIONCHANNEL,
      //      CommunicationDate = o.COMMUNICATIONDATE,
      //      CallerType = o.CALLERTYPE,
      //      SpeakingTo = o.SPEAKINGTO,
      //      CommunicationDirection = o.COMMUNICATIONDIRECTION,
      //      ClinicalEngagement = o.CLINICALENGAGEMENT,
      //      CommunicationObjectiveCategory = o.COMMOBJVCATETGORY,
      //      CommunicationObjective = o.COMMOBJECTIVE,
      //      ReceivedDate = o.RECEIVED_DATE,
      //      ImportDate = o.IMPORT_DATE
      //    }).OrderByDescending(o => o.CommunicationDate).ToList();

      var AccoladeEngagements = await Context.AccoladeEngagements.Where(x => x.ChMemberId == chMemberId).OrderByDescending(x=>x.CommunicationDate).Take(10).ToListAsync();
      return AccoladeEngagements.Select(o => GetMemberAccoladeEngagementJson(o)).ToList();
    }

    #endregion //AccoladeEngagement

    #region AccoladeReferral

    static object GetMemberAccoladeReferralJson(dynamic data)
    {
      return new  
      {
        data.ChMemberId,
        //data.BirthDate,
        //data.RelationshipCode,
        //data.Gender,
        data.LastName,
        data.FirstName,
        data.EffectiveDate,
        data.ProgramName,
        data.ProgramPartner,
        data.MemberEngaged,
        data.FileReceivedDate,
        data.LastEditedTimestamp
      };
    }
    private async Task<object> GetMemberAccoladeReferralJson(int chMemberId,
        DataTable snowflakeTable)
    {
      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeAccoladeReferralSource>(snowflakeTable)
      //  .Select(o => new
      //  {
      //    AccoladeId = o.ACCOLADE_ID,
      //    BirthDate = o.BIRTH_DATE,
      //    RelationshipCode = o.RELATIONSHIP_CODE,
      //    Gender = o.GENDER,
      //    LastName = o.LAST_NAME,
      //    FirstName = o.FIRST_NAME,
      //    EffectiveDate = o.EFFECTIVE_DATE,
      //    ProgramName = o.PROGRAM_NAME,
      //    ProgramPartner = o.PROGRAM_PARTNER,
      //    MemberEngaged = o.MEMBER_ENGAGED,
      //    ReceivedDate = o.RECEIVED_DATE,
      //    ImportDate = o.IMPORT_DATE
      //  }).OrderByDescending(o => o.EffectiveDate).ToList();

      var AccoladeList = Context.AccoladeReferrals.Where(x => x.ChMemberId == chMemberId).OrderByDescending(x => x.EffectiveDate).Take(10).ToList();

      return AccoladeList.Select(o => GetMemberAccoladeReferralJson(o)).ToList();
    }

    #endregion //AccoladeReferral


    #region AccoladeUMData

    static object GetMemberAccoladeUMDataJson(dynamic data)
    {
      return new 
      {
        data.Setting,
        data.Service,
        data.Urgency,
        data.ChMemberId,
        data.AdmitDate,
        data.DischargeDate,
        data.CaseClosedDate,
        data.DecisionDate,
        data.Decision,
        data.PrimaryDiagnosisDescription,
        data.PlanMemberNum,
        data.PhysicianFirstName,
        data.PhysicianLastName,
        data.FirstName,
        data.LastName,
        data.ServiceProviderName,
        data.FileReceivedDate,
        data.LastEditedTimestamp,

        //data.Company,
        //data.ParentGroup,
        //data.Group,
        //data.Plan,
        //data.IsuiteCaseId,
        //data.IsuiteReferenceNum,
       
       
        //data.ActualDays,
        //data.UnitType,
        //data.UnitsRequested,
        //data.UnitsApproved,
        //data.UnitsNonCertified,
        //data.DischargeDisposition,
        //data.NonCertReason,
        //data.NotificationDate,
        //data.ProcedureRequestedDate,
        //data.ProcedureCode,
        //data.ProcedureName,
        //data.PrimaryDiagnosisCode,
        //data.FirstName,
        //data.LastName,
        //data.Gender,
        //data.DateOfBirth,
        //data.AgeAtAdmission,
        //data.Address1,
        //data.Address2,
        //data.City,
        //data.State,
        //data.PostalCode,
        //data.Country,
        //data.PrimaryPhone,
        //data.PrimaryExt,
       
        //data.PhysicianCity,
        //data.PhysicianState,
        //data.PhysicianTaxId,
        //data.ServiceProviderCity,
        //data.ServiceProviderState,
        //data.InNetwork,
        //data.PhysicianReview,
        //data.ReviewType,
        //data.ReviewDate,
        //data.ReviewDecision,
        //data.CMGRLastName,
        //data.CMGRFirstName,
        //data.RNLocation,
        //data.ReceivedDate,
        //data.ImportDate
      };
    }
    private async Task<object> GetMemberAccoladeUMDataJson(int chMemberId,
        DataTable snowflakeTable)
    {
      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeAccoladeUMDataSource>(snowflakeTable)
      //    .Select(o => new 
      //    {
      //      Company = o.COMPANY,
      //      ParentGroup = o.PARENT_GROUP,
      //      Group = o.GROUP_,
      //      Plan = o.PLAN,
      //      IsuiteCaseId = o.ISUITE_CASE_ID,
      //      IsuiteReferenceNum = o.ISUITE_REFERENCE_NUM,
      //      Setting = o.SETTING,
      //      Service = o.SERVICE,
      //      Urgency = o.URGENCY,
      //      AdmitDate = o.ADMIT_DATE,
      //      DischargeDate = o.DISCHARGE_DATE,
      //      CaseClosedDate = o.CASE_CLOSED_DATE,
      //      DecisionDate = o.DECISION_DATE,
      //      ActualDays = o.ACTUAL_DAYS,
      //      UnitType = o.UNIT_TYPE,
      //      UnitsRequested = o.UNITS_REQUESTED,
      //      UnitsApproved = o.UNITS_APPROVED,
      //      UnitsNonCertified = o.UNITS_NON_CERTIFIED,
      //      Decision = o.DECISION,
      //      DischargeDisposition = o.DISCHARGE_DISPOSITION,
      //      NonCertReason = o.NON_CERT_REASON,
      //      NotificationDate = o.NOTIFICATION_DATE,
      //      ProcedureRequestedDate = o.PROCEDURE_REQUESTED_DATE,
      //      ProcedureCode = o.PROCEDURE_CODE,
      //      ProcedureName = o.PROCEDURE_NAME,
      //      PrimaryDiagnosisCode = o.PRIMARY_DIAGNOSIS_CODE,
      //      PrimaryDiagnosisDescription = o.PRIMARY_DIAGNOSIS_NAME,
      //      PlanMemberNum = o.PLAN_MEMBER_NUM,
      //      FirstName = o.MEMBER_FIRST_NAME,
      //      LastName = o.MEMBER_LAST_NAME,
      //      Gender = o.GENDER,
      //      DateOfBirth = o.DATE_OF_BIRTH,
      //      AgeAtAdmission = o.AGE_AT_ADMISSION,
      //      Address1 = o.ADDRESS1,
      //      Address2 = o.ADDRESS2,
      //      City = o.CITY,
      //      State = o.STATE,
      //      PostalCode = o.POSTAL_CODE,
      //      Country = o.COUNTRY,
      //      PrimaryPhone = o.PRIMARY_PHONE,
      //      PrimaryExt = o.PRIMARY_EXT,
      //      PhysicianFirstName = o.PHYSICIAN_FIRST_NAME,
      //      PhysicianLastName = o.PHYSICIAN_LAST_NAME,
      //      PhysicianCity = o.PHYSICIAN_CITY,
      //      PhysicianState = o.PHYSICIAN_STATE,
      //      PhysicianTaxId = o.PHYSICIAN_TAX_ID,
      //      ServiceProviderName = o.SERVICE_PROVIDER_NAME,
      //      ServiceProviderCity = o.SERVICE_PROVIDER_CITY,
      //      ServiceProviderState = o.SERVICE_PROVIDER_STATE,
      //      InNetwork = o.IN_NETWORK,
      //      PhysicianReview = o.PHYSICIAN_REVIEW,
      //      ReviewType = o.REVIEW_TYPE,
      //      ReviewDate = o.REVIEW_DATE,
      //      ReviewDecision = o.REVIEW_DECISION,
      //      CMGRLastName = o.CMGR_LAST_NAME,
      //      CMGRFirstName = o.CMGR_FIRST_NAME,
      //      RNLocation = o.RN_LOCATION,
      //      ReceivedDate = o.RECEIVED_DATE,
      //      ImportDate = o.IMPORT_DATE
      //    }).OrderByDescending(o => o.AdmitDate).ToList();

      var AccoladeUMData =await Context.AccoladeUMDatas.Where(x => x.ChMemberId == chMemberId).OrderByDescending(x => x.AdmitDate).Take(10).ToListAsync();

      return AccoladeUMData.Select(o => GetMemberAccoladeUMDataJson(o)).ToList();
    }

    #endregion //AccoladeUMData

    #region VoriHealth

    static object GetMemberVoriHealthJson(dynamic data)
    {
      return new
      {
        data.ChMemberId,
        data.Status,
        data.StatusDate,
        data.RecentVisitDate,
        data.Reason,
        data.FileReceivedDate,
        data.LastEditedTimestamp
      };
    }
    private async Task<object> GetMemberVoriHealthJson(int chMemberId,
        DataTable snowflakeTable)
    {
      //var snowflakeList = ConvertDataTableToList<Entities.SnowflakeVoriHealthSource>(snowflakeTable)
      //  .Select(o => new
      //  {
      //    AccoladeId = o.CH_MEMBER_ID,
      //    Status = o.STATUS,
      //    StatusDate = o.STATUS_DATE,
      //    RecentVisitDate = o.RECENT_VISIT_DATE,
      //    Reason = o.REASON
      //  })
      //  .ToList();
      var VoriHealth = await Context.VoriHealthEngagements.Where(x => x.ChMemberId == chMemberId).OrderByDescending(x => x.StatusDate).Take(10).ToListAsync();
      return VoriHealth.Select(o => GetMemberVoriHealthJson(o)).ToList();
    }

    #endregion //VoriHealth


    #region "Download PHS in PDF"


    public byte[] MaintenanceDownloadPHSAsync(int chMemberId)
    {
      var entity = Context.SnowflakeMembers
                .Where(o => o.ChMemberId == chMemberId)
                .ToList();
      // Encoding u8 = Encoding.UTF8;
      //    byte[] dataAsBytes = entity.SelectMany(s => Text.Encoding.UTF8.GetBytes(s))
      //.ToArray();
      //BinaryFormatter bf = new BinaryFormatter();
      //using (var ms = new MemoryStream())
      //{
      //  bf.Serialize(ms, entity);
      //  return ms.ToArray();
      //}
      Encoding u8 = Encoding.UTF8;
      byte[] result = entity.SelectMany(x => u8.GetBytes(x.ToString())).ToArray();

      return result;


    }
    public static void jsonStringToCSV(string jsonContent, string filename)
    {
      //used NewtonSoft json nuget package
      XmlNode xml = JsonConvert.DeserializeXmlNode("{records:{record:" + jsonContent + "}}");
      XmlDocument xmldoc = new XmlDocument();
      xmldoc.LoadXml(xml.InnerXml);
      XmlReader xmlReader = new XmlNodeReader(xml);
      DataSet dataSet = new DataSet();
      dataSet.ReadXml(xmlReader);
      if (dataSet.Tables.Count != 0)
      {
        var dt = dataSet.Tables[0];

        StringBuilder sb = new StringBuilder();

        IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                          Select(column => column.ColumnName);
        sb.AppendLine(string.Join("\t", columnNames));

        foreach (DataRow row in dt.Rows)
        {
          IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
          sb.AppendLine(string.Join("\t", fields));
        }
        System.IO.File.WriteAllText(filename, sb.ToString());

      }
    }

  }



  #endregion


}
