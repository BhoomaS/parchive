using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FastMember;

using CH.Data;
using CH.Models.Common;
using CH.Models.Enums;
using CH.Business.Services;
using System.Globalization;

namespace CH.Business
{
  public abstract class BaseManager
  {
    protected readonly AppDbContext Context;
    protected readonly IIdentityService IdentityService;
    protected readonly ICacheService CacheService;
    protected readonly IConfiguration Config;
    protected readonly IMapper Mapper;

    private static readonly Dictionary<Type, IEnumerable<PropertyInfo>> _Properties =
      new Dictionary<Type, IEnumerable<PropertyInfo>>();


    public BaseManager(
      AppDbContext context,
      IIdentityService identityService,
      ICacheService cacheService,
      IConfiguration config,
      IMapper mapper)
    {
      Context = context;
      IdentityService = identityService;
      CacheService = cacheService;
      Config = config;
      Mapper = mapper;
    }


    public IEnumerable<T> ConvertDataTableToList<T>(DataTable table) where T : class, new()
    {
      var objType = typeof(T);
      IEnumerable<PropertyInfo> properties;

      lock (_Properties)
      {
        if (!_Properties.TryGetValue(objType, out properties))
        {
          properties = objType.GetProperties().Where(property => property.CanWrite);
          _Properties.Add(objType, properties);
        }
      }

      // Remove any properties that are not part of the DataTable
      properties = properties.Where(o => table.Columns.Contains(o.Name)).ToList();

      //var list = new List<T>(table.Rows.Count);
      var list = new ConcurrentBag<T>();

      Parallel.ForEach<DataRow>(table.Rows.Cast<DataRow>(), row =>
      {
        var obj = new T();

        foreach (var prop in properties)
        {
          if (prop != null)
          {
            Type t = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            object cellValue = row[prop.Name];


            object propertyValue = null;

            if (cellValue != null && cellValue != DBNull.Value)
            {
              if (t == typeof(DateTime) || t == typeof(DateTime?)) // Handle DateTime conversion
              {
                string dateString = cellValue.ToString();
                string[] formats = { "yyyyMMdd", "M/d/yy HH:mm", "M/d/yy" }; // Added "M/d/yy" to handle missing time

                if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                  propertyValue = parsedDate;
                }
                else
                {
                  propertyValue = (cellValue == null || cellValue == DBNull.Value) ? null
                  : Convert.ChangeType(row[prop.Name], t);
                }
              }
              else
              {
                propertyValue = Convert.ChangeType(cellValue, t);
              }
            }

            //if (cellValue != null && cellValue != DBNull.Value)
            //{
            //  if (t == typeof(DateTime)) // Handle DateTime conversion
            //  {
            //    string dateString = cellValue.ToString();
            //    if (DateTime.TryParseExact(dateString, "yyyyMMdd",
            //        CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            //    {
            //      propertyValue = parsedDate;
            //    }
            //    else
            //    {
            //      propertyValue = (cellValue == null || cellValue == DBNull.Value) ? null
            //: Convert.ChangeType(row[prop.Name], t);
            //    }
            //  }
            //  else
            //  {
            //    propertyValue = Convert.ChangeType(cellValue, t);
            //  }
            //}

            var accessors = TypeAccessor.Create(objType);
            accessors[obj, prop.Name] = propertyValue;
          }
        }


        list.Add(obj);
      });

      return list;
    }


    #region Audit

    protected async Task<List<AuditChange>> GetAuditChangesAsync(int chMemberId)
    {
      var result = await Context.AuditChanges
          .Where(o => o.ChMemberId == chMemberId)
          .GroupJoin(Context.AuditChangeFields, o => o.Id, o => o.AuditChangeId, (x, y) => new { AuditChange = x, Fields = y })
          .SelectMany(x => x.Fields.DefaultIfEmpty(), (x, y) => new { x.AuditChange, Field = y })
          .Select(o => new AuditChange()
          {
            DateCreated = o.AuditChange.EditedTimestamp,
            TableName = o.AuditChange.TableName,
            FieldName = o.Field.FieldName,
            OldValue = o.Field.OldValue,
            NewValue = o.Field.NewValue,
            UserName = o.AuditChange.UserEditedBy.FullName,
            TypeDescriptionId = o.AuditChange.CodeAuditChangeTypeId
          }).ToListAsync();

      result.AddRange(await Context.AuditLogs
          .Where(o => o.CodeAuditTypeId == (int)AuditType.MemberView &&
              o.AdditionalInfo == chMemberId.ToString())
          .Select(o => new AuditChange()
          {
            DateCreated = o.DateCreated,
            TableName = "",
            UserName = o.User.FullName,
            TypeDescriptionId = o.CodeAuditTypeId
          })
          .ToListAsync());

      var codeDefs = await Context.CodeDefs
          .Select(o => new Entities.CodeDef()
          {
            Id = o.Id,
            DisplayName = o.DisplayName
          })
          .ToListAsync();

      foreach (var row in result)
      {
        row.TypeDescription = codeDefs.Where(x => x.Id == row.TypeDescriptionId).FirstOrDefault().DisplayName;

        if (row.FieldName != null)
        {

          if (row.FieldName.StartsWith("Code") && row.FieldName.EndsWith("Id"))
          {
            if (!String.IsNullOrWhiteSpace(row.OldValue))
            {
              row.OldValue = codeDefs.Where(c => c.Id == Convert.ToInt32(row.OldValue)).FirstOrDefault().DisplayName;
            }
            if (!String.IsNullOrWhiteSpace(row.NewValue))
            {
              row.NewValue = codeDefs.Where(c => c.Id == Convert.ToInt32(row.NewValue)).FirstOrDefault().DisplayName;
            }
            row.FieldName = row.FieldName.Replace("Code", "").Replace("Id", "");
          }
        }
      }

      return result;
    }

    protected async Task<bool> AddAuditLogAsync(int typeId, string additionalInfo = null, int? userId = null, IAppDbContext context = null)
    {
      context ??= Context;

      await context.AuditLogs.AddAsync(new Entities.AuditLog()
      {
        CodeAuditTypeId = typeId,
        AdditionalInfo = additionalInfo,
        CreatedBy = userId ?? IdentityService.UserId,
        DateCreated = DateTime.Now,
      });
      await context.SaveChangesAsync();
      return true;
    }

    #endregion


    #region Code Defs

    // TODO: Change this to use a model instead
    protected async Task<IEnumerable<Entities.CodeDef>> GetCodeDefsAsync(int id)
    {
      if (id <= 0)
      {
        throw new ArgumentNullException(paramName: nameof(id));
      }

      return await Context.CodeDefs.Where(cd => cd.CodeDefTypeId == id )
        .OrderBy(o => o.DisplayIndex)
        .ToListAsync();
    }

    protected async Task<IEnumerable<string>> GetCodeDefNamesAsync(int typeId)
    {
      return await Context.CodeDefs
        .Where(o => o.CodeDefTypeId == typeId && o.IsEnabled != false)
        .OrderBy(o => o.DisplayIndex)
        .ThenBy(o => o.DisplayName)
        .Select(o => o.DisplayName)
        .ToListAsync();
    }

    protected async Task<IEnumerable<object>> GetCodeDefItemsAsync(int typeId)
    {
      return await Context.CodeDefs
        .Where(o => o.CodeDefTypeId == typeId)
        .OrderBy(o => o.DisplayIndex)
        .ThenBy(o => o.DisplayName)
        .Select(o => new
        {
          id = o.Id,
          name = o.DisplayName,
        })
        .ToListAsync();
    }


    protected async Task<IEnumerable<object>> GetCodeDefListItemsAsync(int typeId)
    {
      var result = await Context.CodeDefs
        .Where(o => o.CodeDefTypeId == typeId)
        .OrderBy(o => o.DisplayIndex)
        .ThenBy(o => o.DisplayName)
        .Select(o => new
        {
          key = o.EnumKey,
          name = o.DisplayName,
        })
        .ToListAsync();

      return result;
    }

    #endregion


    #region Code Def Types

    // TODO: Change this to use a model instead
    protected async Task<Entities.CodeDefType> GetCodeDefTypesAsync(int id)
    {
      if (id <= 0)
      {
        throw new ArgumentNullException(paramName: nameof(id));
      }
      return await Context.CodeDefTypes.FirstOrDefaultAsync(predicate: b => b.Id == id);
    }

    #endregion


    #region Lookups

    protected async Task<IEnumerable<string>> GetEthnicitiesAsync()
    {
      var ethnicityTypes = await Context.Ethnicities
        .Select(e => e.EthnicityType)
        .OrderBy(e => e)
        .ToListAsync();
      return ethnicityTypes;
    }

    protected async Task<IEnumerable<KeyValue>> GetClientsAsync()
    {
      var clientNames = await Context.SnowflakeEmployers
      .Where(e => e.IsEnabled ?? false)
      .Select(e => new KeyValue()
      {
        //Id = e.ChEmployerId,
        Value = e.ClientName.ToUpper().Trim(),
      })
              .Distinct()
              .OrderBy(e => e.Value)
              .ToListAsync();
      return clientNames;
    }

    protected async Task<IEnumerable<KeyValue>> GetCarrierNamesAsync()
    {
      var carrierNames = await Context.SnowflakeMembers
        .Select(e => new KeyValue()
        {
          Value = e.CarrierName.ToUpper().Trim(),
        }).Distinct().OrderBy(e => e.Value).ToListAsync();
      return carrierNames;
    }


    protected async Task<IEnumerable<KeyValue>> GetGroupsAsync(string client)
    {
      var groups = await Context.SnowflakeEmployers
      .Where(e => e.ClientName == client && e.IsEnabled == true)
      .Select(e => new KeyValue()
      {
        Value = e.EmployerName.ToUpper().Trim(),
      }).Distinct()
         .OrderBy(e => e.Value)
         .ToListAsync();
      return groups;
    }


    protected async Task<IEnumerable<KeyValue>> GetCarrierNamesAsync(string group)
    {

      var ch_employer_id = Context.SnowflakeEmployers.FirstOrDefault(e => e.EmployerName.ToLower().Contains(group.ToLower()));

      var carrierNames = await Context.SnowflakeMembers
        .Where(e => e.ChEmployerId == ch_employer_id.ChEmployerId)
        .Select(e => new KeyValue()
        {
          Value = e.CarrierName.ToUpper().Trim(),
        }).Distinct().OrderBy(e => e.Value).ToListAsync();
      return carrierNames;
    }


    protected async Task<IEnumerable<KeyValue>> GetCarrierNamesBasedOnClientNameAsync(string client)
    {
      var carrierNames = await Context.SnowflakeMembers.
       Join(Context.SnowflakeEmployers, u => u.ChEmployerId, uir => uir.ChEmployerId,
       (u, uir) => new { u, uir })
       .Where(m => m.uir.ClientName.ToLower().Contains(client.ToLower()))
      .Select(m => new KeyValue()
      {
        Value = m.u.CarrierName.ToUpper().Trim(),
      }).Distinct().OrderBy(e => e.Value).ToListAsync();
      return carrierNames;

    }

    protected async Task<IEnumerable<KeyValue>> GetEmployersAsync()
    {
      var employers = await Context.SnowflakeEmployers
  .Where(e => e.IsEnabled ?? false)
  .Select(e => new KeyValue()
  {
    //Id = e.ChEmployerId,
    Value = e.EmployerName.ToUpper().Trim(),
  })
          .Distinct()
          .OrderBy(e => e.Value)
          .ToListAsync();
      return employers;
    }

    protected async Task<IEnumerable<string>> GetStatesAsync()
    {
      var stateCodes = await Context.States
        .Select(s => s.StateCode)
        .OrderBy(s => s)
        .ToListAsync();
      return stateCodes;
    }

    protected async Task<IEnumerable<string>> GetRelationshipsAsync()
    {
      var relationships = await Context.Relationships
        .Select(s => s.RelationshipType)
        .OrderBy(s => s)
        .ToListAsync();
      return relationships;
    }

    #endregion

  }
}
