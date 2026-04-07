using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using FastMember;
using Snowflake.Data.Client;
using EFCore.BulkExtensions;

using CH.Data;
using CH.Models.Common;
using CH.Business.Services;
using Microsoft.EntityFrameworkCore.Internal;

namespace CH.Business
{
  public interface ISnowflakeManager
  {
    Task<DataTable> GetSnowflakeDataTable(SnowflakeManager.TableDefinition tableDef,
        string whereClause, List<System.Data.IDbDataParameter> parameters);
    Task<DataTable> GetDataTableAsync(string tableName);
    Task<List<KeyValue>> GetSnowflakeEmployersAsync(string clientName="");
    Task<List<KeyValue>> GetAllClientsAsync();
    Task<List<string>> GetSnowflakeInsuranceCarriersAsync();
    Task<DatabaseUpdateSummary> UpdateEmployerTableAsync();
    Task<DatabaseUpdateSummary> UpdateMemberTableAsync();
  }

  public class SnowflakeManager : BaseManager, ISnowflakeManager
  {
    public class TableDefinition
    {
      public string ViewName { get; set; }
      public bool HasChMemberId { get; set; }
      public Type OverrideType { get; set; }
    }


    private static readonly Dictionary<Type, IEnumerable<PropertyInfo>> _Properties =
        new Dictionary<Type, IEnumerable<PropertyInfo>>();

    public static readonly Dictionary<string, TableDefinition> _TableLookups = CreateTableLookup();

    private static Dictionary<string, TableDefinition> CreateTableLookup()
    {
      var dict = new Dictionary<string, TableDefinition>();
      dict[SnowflakeTables.Biometrics] = new TableDefinition()
      {
        ViewName = "VIEW_CH_MEMBER_BIOMETRICS",
        HasChMemberId = true
      };
      dict[SnowflakeTables.PCP] = new TableDefinition()
      {
        ViewName = "VIEW_CH_MEMBER_PCP",
        HasChMemberId = true
      };
      dict[SnowflakeTables.Chronic] = new TableDefinition()
      {
        ViewName = "VIEW_CH_MEMBER_CHRONIC_CONDITIONS",
        HasChMemberId = true
      };
      dict[SnowflakeTables.MedicalInfo] = new TableDefinition()
      {
        ViewName = "VIEW_CH_MEMBER_MEDICAL_INFO",
        HasChMemberId = true
      };
      dict[SnowflakeTables.Allergies] = new TableDefinition()
      {
        ViewName = "VIEW_CH_MEMBER_ALLERGIES",
        HasChMemberId = false
      };
      dict[SnowflakeTables.CareTeam] = new TableDefinition()
      {
        ViewName = "VIEW_CH_MEMBER_CARE_TEAM",
        HasChMemberId = true
      };
      dict[SnowflakeTables.Recommendations] = new TableDefinition()
      {
        ViewName = "VIEW_CH_MEMBER_RECOMMENDATIONS",
        //ViewName = "VIEW_CH_MEMBER_RECOMMENDATIONS_TESTING",
        HasChMemberId = true
      };
      dict[SnowflakeTables.Goals] = new TableDefinition()
      {
        ViewName = "VIEW_CH_MEMBER_PERSONAL_GOALS_PLANS",
        HasChMemberId = false
      };
      dict[SnowflakeTables.Programs] = new TableDefinition()
      {
        ViewName = "VIEW_CH_MEMBER_PROGRAMS",
        HasChMemberId = false
      };
      dict[SnowflakeTables.AccoladeReferrals] = new TableDefinition()
      {
        ViewName = "VIEW_ACCOLADE_REFERRAL",
        HasChMemberId = true
      };
      dict[SnowflakeTables.AccoladeEngagements] = new TableDefinition()
      {
        ViewName = "VIEW_ACCOLADE_ENGAGEMENT",
        HasChMemberId = true
      };
      dict[SnowflakeTables.AccoladeUMDatas] = new TableDefinition()
      {
        ViewName = "VIEW_ACCOLADE_UMDATA",
        HasChMemberId = true
      };
      dict[SnowflakeTables.VoriHealth] = new TableDefinition()
      {
        ViewName = "VIEW_VORIHEALTH_ENGAGEMENT",
        HasChMemberId = true
      };
      ////dict["Vendor"] = "VIEW_CH_RESOURCE_VENDOR";
      return dict;
    }


    private readonly string _ConnectionString;


    public SnowflakeManager(AppDbContext context,
        IIdentityService identityService,
        ICacheService cacheService,
        IConfiguration config,
        IMapper mapper)
    : base(context, identityService, cacheService, config, mapper)
    {
      _ConnectionString = config.GetSnowflakeConnection();
    }

    private string GetBooleanYesNoJson(string value)
    {
      return "Y".Equals(value, StringComparison.OrdinalIgnoreCase) ? "Y" : "N";
    }

    private async Task<DataTable> GetSnowflakeDataTable(string commandText,
        List<System.Data.IDbDataParameter> parameters = null)
    {
      var dataTable = new DataTable();

      try
      {
        using (var conn = new SnowflakeDbConnection())
        {
          conn.ConnectionString = _ConnectionString;
          conn.Open();

          using (var cmd = conn.CreateCommand())
          {
            if (parameters != null)
              cmd.Parameters.AddRange(parameters.ToArray());

            cmd.CommandText = commandText;
            using (var reader = await cmd.ExecuteReaderAsync())
            {
              dataTable.Load(reader);
            }
          }
        }
      }
      catch (Exception ex)
      {
        string s = ex.ToString();
        s = null;
      }

      return dataTable;
    }

    public async Task<DataTable> GetSnowflakeDataTable(TableDefinition tableDef,
        string whereClause, List<System.Data.IDbDataParameter> parameters)
    {
      string commandText = $"select * from {tableDef.ViewName} {whereClause}";
      return await GetSnowflakeDataTable(commandText, parameters);
    }

    public async Task<DataTable> GetDataTableAsync(string tableName)
    {
      var tableDef = _TableLookups[tableName];
      return await GetSnowflakeDataTable(tableDef, string.Empty, null);
    }

    public async Task<List<KeyValue>> GetSnowflakeEmployersAsync(string clientName="")
    {
      var employers = new List<KeyValue>();


      var employeesAll = await Context.SnowflakeEmployers.Where(x => x.IsEnabled == true && (x.ClientName==clientName || clientName=="")).ToListAsync();
      var distictEmployees = employeesAll.GroupBy(x => x.ChEmployerId)
                         .Select(g => g.First())
                         .ToList();



      foreach (var em in distictEmployees)
      {
        employers.Add(new KeyValue()
        {
          Id = em.ChEmployerId,
          Value = em.EmployerName
        });
      }
      return employers.OrderBy(o => o.Value).ToList();
    }
    public async Task<List<KeyValue>> GetAllClientsAsync()
    {
      var employers = new List<KeyValue>();


      var employeesAll = await Context.SnowflakeEmployers.Where(x => x.IsEnabled ==true).ToListAsync();
      var distictEmployees = employeesAll.GroupBy(x => x.ClientId)
                         .Select(g => g.First())
                         .ToList();


      foreach (var em in distictEmployees)
      {
        employers.Add(new KeyValue()
        {
          //Id = em.ClientId,
          Value = em.ClientName
        });
      }

      return employers.OrderBy(o => o.Value).ToList();
    }
    //public async Task<List<KeyValue>> GetSnowflakeEmployersAsync()
    //{
    //  var employers = new List<KeyValue>();

    //  try
    //  {
    //    using (var conn = new SnowflakeDbConnection())
    //    {
    //      conn.ConnectionString = _ConnectionString;
    //      conn.Open();

    //      using (var cmd = conn.CreateCommand())
    //      {
    //        cmd.CommandText = $"select distinct CH_EMPLOYER_ID, EMPLOYER_NAME from VIEW_CH_EMPLOYER where IS_ENABLED=1 ";
    //        using (var reader = await cmd.ExecuteReaderAsync())
    //        {
    //          while (reader.Read())
    //          {
    //            employers.Add(new KeyValue()
    //            {
    //              Id = reader.GetInt32(0),
    //              Value = reader.GetString(1),
    //            });
    //          }
    //        }
    //      }
    //    }
    //  }
    //  catch (Exception ex)
    //  {
    //    string s = ex.ToString();
    //    s = null;
    //  }

    //  // These next steps shouldn't have to be done in a normal Prod environment,
    //  // however local & dev/test won't be automatically updated by WhereScape.
    //  // Normally, WhereScape calls the /api/member/UpdateMemberList API,
    //  // which calls UpdateEmployerTableAsync().
    //  var unknownEmployers = employers
    //    .Where(o => !Context.SnowflakeEmployers.Any(x => x.ChEmployerId == o.Id))
    //    .ToList();

    //  //if (unknownEmployers.Any())
    //    // Any employers found in Snowflake that do not currently exist in the application database?
    //   // await UpdateEmployerTableAsync();

    //  return employers.OrderBy(o => o.Value).ToList();
    //}

    //public async Task<List<KeyValue>> GetSnowflakeEmployersAsync()
    //{
    //  var employers = new List<KeyValue>();

    //  try
    //  {
    //    using (var conn = new SnowflakeDbConnection())
    //    {
    //      conn.ConnectionString = _ConnectionString;
    //      conn.Open();

    //      using (var cmd = conn.CreateCommand())
    //      {
    //        cmd.CommandText = $"select distinct CH_EMPLOYER_ID, EMPLOYER_NAME from VIEW_CH_EMPLOYER where IS_ENABLED=1 ";
    //        using (var reader = await cmd.ExecuteReaderAsync())
    //        {
    //          while (reader.Read())
    //          {
    //            employers.Add(new KeyValue()
    //            {
    //              Id = reader.GetInt32(0),
    //              Value = reader.GetString(1),
    //            });
    //          }
    //        }
    //      }
    //    }
    //  }
    //  catch (Exception ex)
    //  {
    //    string s = ex.ToString();
    //    s = null;
    //  }

    //  // These next steps shouldn't have to be done in a normal Prod environment,
    //  // however local & dev/test won't be automatically updated by WhereScape.
    //  // Normally, WhereScape calls the /api/member/UpdateMemberList API,
    //  // which calls UpdateEmployerTableAsync().
    //  var unknownEmployers = employers
    //    .Where(o => !Context.SnowflakeEmployers.Any(x => x.ChEmployerId == o.Id))
    //    .ToList();

    //  //if (unknownEmployers.Any())
    //    // Any employers found in Snowflake that do not currently exist in the application database?
    //   // await UpdateEmployerTableAsync();

    //  return employers.OrderBy(o => o.Value).ToList();
    //}

    public async Task<List<string>> GetSnowflakeInsuranceCarriersAsync()
    {
      var carriers = new List<string>();
      try
      {
        using (var conn = new SnowflakeDbConnection())
        {
          conn.ConnectionString = _ConnectionString;
          conn.Open();

          using (var cmd = conn.CreateCommand())
          {
            cmd.CommandText = $"select CARRIER_NAME from VIEW_CARRIER";
            using (var reader = await cmd.ExecuteReaderAsync())
            {
              while (reader.Read())
              {
                carriers.Add(reader.GetString(0));
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        string s = ex.ToString();
        s = null;
      }
      return carriers.OrderBy(o => o).ToList();
    }

    public async Task<DatabaseUpdateSummary> UpdateEmployerTableAsync()
    {
      var genericRepository = new GenericRepository<Entities.SnowflakeEmployer>(Context);
      var existing = (await genericRepository.GetAsync())
          .ToDictionary(o => o.ChEmployerId);

      var dataTable = await GetSnowflakeDataTable("select * from VIEW_CH_EMPLOYER order by DSS_CREATE_TIME desc");
      var snowflakeEmployers = ConvertDataTableToList<Entities.SnowflakeEmployerSource>(dataTable);

      var result = new DatabaseUpdateSummary();
      DateTime now = DateTime.Now;

      foreach (var snowflakeEmployer in snowflakeEmployers)
      {
        if (!existing.ContainsKey(snowflakeEmployer.CH_EMPLOYER_ID))
        {
          var newEmployer = new Entities.SnowflakeEmployer()
          {
            ChEmployerId = snowflakeEmployer.CH_EMPLOYER_ID,
            GroupId = snowflakeEmployer.GROUP_ID,
            EmployerName = snowflakeEmployer.EMPLOYER_NAME,
            IsEnabled = snowflakeEmployer.IS_ENABLED,
            DssCreateTime = snowflakeEmployer.DSS_CREATE_TIME,
            DssUpdateTime = snowflakeEmployer.DSS_UPDATE_TIME,
            LastSnowflakeTimestamp = now,
          };
          genericRepository.Insert(newEmployer);

          result.InsertedCount++;
        }
        else
        {
          var employer = existing[snowflakeEmployer.CH_EMPLOYER_ID];

          // Update the record from Snowflake.
          employer.GroupId = snowflakeEmployer.GROUP_ID;
          employer.EmployerName = snowflakeEmployer.EMPLOYER_NAME;
          employer.IsEnabled = snowflakeEmployer.IS_ENABLED;
          employer.DssCreateTime = snowflakeEmployer.DSS_CREATE_TIME;
          employer.DssUpdateTime = snowflakeEmployer.DSS_UPDATE_TIME;
          employer.LastSnowflakeTimestamp = now;

          result.UpdatedCount++;
        }
      }

      Context.SaveChangesNoAudit();

      return result;
    }

    public async Task<DatabaseUpdateSummary> UpdateMemberTableAsync()
    {
      var genericRepository = new GenericRepository<Entities.SnowflakeMember>(Context);
      var existing = (await genericRepository.GetAsync())
          .ToDictionary(o => o.ChMemberId);

      var dataTable = await GetSnowflakeDataTable("select * from VIEW_CH_MEMBER order by DSS_LOAD_DATE desc");
      var snowflakeMembers = ConvertDataTableToList<Entities.SnowflakeMemberSource>(dataTable);

      var result = new DatabaseUpdateSummary();
      DateTime now = DateTime.Now;

      var membersToInsert = new List<Entities.SnowflakeMember>();
      var membersToUpdate = new List<Entities.SnowflakeMember>();

      Func<string, int?> parseRisk = (textValue) =>
      {
        int parsedValue;
        if (int.TryParse(textValue, out parsedValue))
          return parsedValue;
        return null;
      };

      foreach (var snowflakeMember in snowflakeMembers)
      {
        if (!existing.ContainsKey(snowflakeMember.CH_MEMBER_ID))
        {
          var newMember = new Entities.SnowflakeMember()
          {
            ChMemberId = snowflakeMember.CH_MEMBER_ID,
            ChEmployerId = snowflakeMember.CH_EMPLOYER_ID,
            MemberId = snowflakeMember.MEMBER_ID,
            GroupId = snowflakeMember.GROUP_ID,
            FirstName = snowflakeMember.FIRST_NAME,
            MiddleName = snowflakeMember.MIDDLE_NAME,
            LastName = snowflakeMember.LAST_NAME,
            Ssn = snowflakeMember.SSN,
            Gender = snowflakeMember.GENDER,
            Dob = snowflakeMember.DOB,
            Address1 = snowflakeMember.ADDRESS_1,
            Address2 = snowflakeMember.ADDRESS_2,
            City = snowflakeMember.CITY,
            County = snowflakeMember.COUNTY,
            State = snowflakeMember.STATE,
            ZipCode = snowflakeMember.ZIP_CODE,
            EmailAddress = snowflakeMember.EMAIL_ADDRESS,
            MaritalStatus = snowflakeMember.MARITAL_STATUS,
            CellPhone = snowflakeMember.CELL_PHONE,
            WorkPhone = snowflakeMember.WORK_PHONE,
            HomePhone = snowflakeMember.HOME_PHONE,
            Race = snowflakeMember.RACE,
            EthnicGroup = snowflakeMember.ETHNIC_GROUP,
            EducationLevel = snowflakeMember.EDUCATION_LEVEL,
            Relationship = snowflakeMember.RELATIONSHIP,
            AgeRange = snowflakeMember.AGE_RANGE,
            OfficeLocation = snowflakeMember.OFFICE_LOCATION,
            AlternateMemberId = snowflakeMember.ALTERNATE_MEMBER_ID,
            MaraRisk = snowflakeMember.MARA_RISK,
            ClinicalRisk = parseRisk(snowflakeMember.CLINICAL_RISK),
            CurrentStatus = snowflakeMember.CURRENT_STATUS,
            CoverageType = snowflakeMember.COVERAGE_TYPE,
            CarrierName = snowflakeMember.CARRIER_NAME,
            PlanName = snowflakeMember.PLAN_NAME,
            EffectiveDate = snowflakeMember.EFFECTIVE_DATE,
            DssRecordSource = snowflakeMember.DSS_RECORD_SOURCE,
            DssLoadDate = snowflakeMember.DSS_LOAD_DATE,
            DssCreateTime = snowflakeMember.DSS_CREATE_TIME,
            DssUpdateTime = snowflakeMember.DSS_UPDATE_TIME,
            LastSnowflakeTimestamp = now,
            ClientEmployeeId =snowflakeMember.ClientEmployeeId
          };
          membersToInsert.Add(newMember);

          result.InsertedCount++;
        }
        else
        {
          var member = existing[snowflakeMember.CH_MEMBER_ID];

          // The following fields are considered read-only at the portal, so they can be
          // updated even if the record is "locked".
          member.MemberId = snowflakeMember.MEMBER_ID;
          member.FirstName = snowflakeMember.FIRST_NAME;
          member.MiddleName = snowflakeMember.MIDDLE_NAME;
          member.LastName = snowflakeMember.LAST_NAME;
          member.Ssn = snowflakeMember.SSN;
          member.GroupId = snowflakeMember.GROUP_ID;
          member.Dob = snowflakeMember.DOB;
          member.AgeRange = snowflakeMember.AGE_RANGE;
          member.Relationship = snowflakeMember.RELATIONSHIP;
          member.CurrentStatus = snowflakeMember.CURRENT_STATUS;
          member.MaraRisk = snowflakeMember.MARA_RISK;
          member.ClinicalRisk = parseRisk(snowflakeMember.CLINICAL_RISK);
          member.CarrierName = snowflakeMember.CARRIER_NAME;
          member.PlanName = snowflakeMember.PLAN_NAME;
          member.EffectiveDate = snowflakeMember.EFFECTIVE_DATE;
          member.CoverageType = snowflakeMember.COVERAGE_TYPE;
          member.ChEmployerId = snowflakeMember.CH_EMPLOYER_ID;

          member.DssRecordSource = snowflakeMember.DSS_RECORD_SOURCE;
          member.DssLoadDate = snowflakeMember.DSS_LOAD_DATE;
          member.DssCreateTime = snowflakeMember.DSS_CREATE_TIME;
          member.DssUpdateTime = snowflakeMember.DSS_UPDATE_TIME;
          member.LastSnowflakeTimestamp = now;

          // These fields are not editable in the portal, so also update these.
          member.EducationLevel = snowflakeMember.EDUCATION_LEVEL;
          member.OfficeLocation = snowflakeMember.OFFICE_LOCATION;
          member.AlternateMemberId = snowflakeMember.ALTERNATE_MEMBER_ID;
          member.MaritalStatus = snowflakeMember.MARITAL_STATUS;
          member.Race = snowflakeMember.RACE;

          if (member.IsModified)
          {
            result.SkippedCount++;
          }
          else
          {
            // Update the record from Snowflake if it hasn't been updated locally.
            member.Gender = snowflakeMember.GENDER;
            member.Address1 = snowflakeMember.ADDRESS_1;
            member.Address2 = snowflakeMember.ADDRESS_2;
            member.City = snowflakeMember.CITY;
            member.County = snowflakeMember.COUNTY;
            member.State = snowflakeMember.STATE;
            member.ZipCode = snowflakeMember.ZIP_CODE;
            member.EmailAddress = snowflakeMember.EMAIL_ADDRESS;
            member.CellPhone = snowflakeMember.CELL_PHONE;
            member.WorkPhone = snowflakeMember.WORK_PHONE;
            member.HomePhone = snowflakeMember.HOME_PHONE;
            member.EthnicGroup = snowflakeMember.ETHNIC_GROUP;

            result.UpdatedCount++;
          }

          membersToUpdate.Add(member);
        }
      }

      var bulkConfig = new BulkConfig()
      {
        BulkCopyTimeout = 0,
        UseTempDB = true,
      };

      using (var transaction = await Context.Database.BeginTransactionAsync())
      {
        if (membersToInsert.Count > 0)
        {
          await Context.BulkInsertAsync(membersToInsert, bulkConfig);
        }

        if (membersToUpdate.Count > 0)
        {
          await Context.BulkUpdateAsync(membersToUpdate, bulkConfig);
        }

        transaction.Commit();
      }

      return result;
    }


    //public IEnumerable<T> ConvertDataTableToList<T>(DataTable table) where T : class, new()
    //{
    //    var objType = typeof(T);
    //    IEnumerable<PropertyInfo> properties;

    //    lock (_Properties)
    //    {
    //        if (!_Properties.TryGetValue(objType, out properties))
    //        {
    //            properties = objType.GetProperties().Where(property => property.CanWrite);
    //            _Properties.Add(objType, properties);
    //        }
    //    }

    //    // Remove any properties that are not part of the DataTable
    //    properties = properties.Where(o => table.Columns.Contains(o.Name)).ToList();

    //    //var list = new List<T>(table.Rows.Count);
    //    var list = new System.Collections.Concurrent.ConcurrentBag<T>();

    //    Parallel.ForEach<DataRow>(table.Rows.Cast<DataRow>(), row =>
    //    {
    //        var obj = new T();

    //        foreach (var prop in properties)
    //        {
    //            if (prop != null)
    //            {
    //                Type t = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

    //                object cellValue = row[prop.Name];
    //                object propertyValue = (cellValue == null || cellValue == DBNull.Value) ? null
    //                    : Convert.ChangeType(row[prop.Name], t);

    //                var accessors = TypeAccessor.Create(objType);
    //                accessors[obj, prop.Name] = propertyValue;
    //            }
    //        }

    //        list.Add(obj);
    //    });

    //    return list;
    //}
  }
}
