using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;

using CH.Entities;
using CH.Models.Enums;

namespace CH.Data
{
  public partial class AppDbContext
  {
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;
    public DbContextOptions<AppDbContext> DbContextOptions => _dbContextOptions;

    private readonly IIdentityService _identityService;
    private bool AddAuditRecords { get; set; }
    private DateTime StartSaveTime { get; set; }


    public AppDbContext(DbContextOptions<AppDbContext> options, IIdentityService identityService)
: base(options)
    {
      _dbContextOptions = options;
      _identityService = identityService;
      this.AddAuditRecords = true;
    }


    private int? GetChMemberId(EntityEntry entry)
    {
      foreach (var prop in entry.Properties)
      {
        //if (prop.Metadata.Relational().ColumnName == "CH_MEMBER_ID")
        if (prop.Metadata.GetColumnName() == "CH_MEMBER_ID")
          return (int?)prop.CurrentValue ?? (int?)prop.OriginalValue;
      }
      return null;
    }

    private List<EntityEntry> AddAuditChangeRecords()
    {
      this.ChangeTracker.DetectChanges();

      var auditChanges = new List<AuditChange>();
      var delayedEntries = new List<EntityEntry>();

      DateTime now = this.StartSaveTime = DateTime.Now;
      // TODO: Changed this to accomodate Member user self-service password reset
      // Is there a better way of handling identity?
      int? userId = _identityService.UserId;
      if (userId == 0) userId = null; // Added this to allow self-registration
      if (userId != null)
      {
        foreach (var entry in this.ChangeTracker.Entries())
      {
        // Ignore changes to audit tables
        if (entry.Entity is AuditChange || entry.Entity is AuditChangeField ||
            entry.Entity is AuditLog ||
            entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
        {
          continue;
        }

        var keys = new List<string>();
        //string tableName = entry.Metadata.Relational().TableName;
        string tableName = entry.Metadata.GetTableName();

        if (entry.State == EntityState.Added)
        {
          delayedEntries.Add(entry);
          continue;
        }
        foreach (var prop in entry.Properties)
        {
          var fields = new List<AuditChangeField>();

          string oldValue = prop.OriginalValue == null ? null
              : prop.OriginalValue.ToString();
          string newValue = prop.CurrentValue == null ? null
              : prop.CurrentValue.ToString();

          if (prop.Metadata.IsPrimaryKey())
          {
            keys.Add(newValue);
          }

          if (entry.State == EntityState.Modified)
          {
            if (oldValue != newValue)
            {
              string fieldName = prop.Metadata.Name;
              if (fieldName.Contains("Timestamp", StringComparison.OrdinalIgnoreCase) ||
                  fieldName.Equals("UserLastEditedById", StringComparison.OrdinalIgnoreCase))
              {
                continue;
              }

              fields.Add(new AuditChangeField()
              {
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
              });
            }
          }

          if (fields.Count > 0)
          {
            var auditChange = new AuditChange()
            {
              CodeAuditChangeTypeId = (int)AuditChangeType.Update,
              TableName = tableName,
              RecordKey = string.Join(',', keys),
              UserEditedById = userId,
              EditedTimestamp = now,
              ChMemberId = GetChMemberId(entry),
            };
            foreach (var field in fields)
            {
              auditChange.AuditChangeFields.Add(field);
              field.AuditChange = auditChange;
            }
            auditChanges.Add(auditChange);
          }
        }

        if (entry.State == EntityState.Deleted)
        {
          auditChanges.Add(new AuditChange()
          {
            CodeAuditChangeTypeId = (int)AuditChangeType.Delete,
            TableName = tableName,
            UserEditedById = userId,
            EditedTimestamp = now,
            ChMemberId = GetChMemberId(entry),
            RecordKey = string.Join(',', keys),
            AdditionalInfo = JsonConvert.SerializeObject(entry.Entity, new JsonSerializerSettings()
            {
              ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            }),
          });
        }
      };

        if (auditChanges.Count > 0)
          this.AuditChanges.AddRange(auditChanges);
      }

      return delayedEntries;
    }

    private void ProcessDelayedEntries(List<EntityEntry> delayedEntries)
    {
      if (delayedEntries.Count == 0)
        return;

      var fields = new List<AuditChangeField>();

      int? userId = _identityService.UserId;
      if (userId == 0) userId = null; // Added this to allow self-registration


      if(userId != null)
      {
        DateTime now = this.StartSaveTime;

        var auditChanges = new List<AuditChange>();
        foreach (var entry in delayedEntries)
        {
          //string tableName = entry.Metadata.Relational().TableName;
          string tableName = entry.Metadata.GetTableName();

          var keys = new List<string>();
          foreach (var prop in entry.Properties)
          {
            if (prop.Metadata.IsPrimaryKey() && prop.CurrentValue != null)
              keys.Add(prop.CurrentValue.ToString());

            if (prop.CurrentValue != null)
            {
              string newValue = prop.CurrentValue.ToString();
              if (newValue != null)
              {
                string fieldName = prop.Metadata.Name;
                if (fieldName.Contains("Timestamp", StringComparison.OrdinalIgnoreCase) ||
                    fieldName.Equals("UserLastEditedById", StringComparison.OrdinalIgnoreCase) ||
                    fieldName.Equals("RecordDate", StringComparison.OrdinalIgnoreCase) ||
                    fieldName.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
                    fieldName.Equals("LastEditedTimestamp", StringComparison.OrdinalIgnoreCase) ||
                    fieldName.Equals("IgnoreFlag", StringComparison.OrdinalIgnoreCase) ||
                    fieldName.Equals("ChMemberId", StringComparison.OrdinalIgnoreCase))
                {
                  continue;
                }

                fields.Add(new AuditChangeField()
                {
                  FieldName = fieldName,
                  OldValue = null,
                  NewValue = newValue,
                });
              }

            }
          }

          if (fields.Count > 0)
          {
            var auditChange = new AuditChange()
            {
              CodeAuditChangeTypeId = (int)AuditChangeType.Insert,
              TableName = tableName,
              RecordKey = string.Join(',', keys),
              UserEditedById = userId,
              EditedTimestamp = now,
              ChMemberId = GetChMemberId(entry),
            };

            foreach (var field in fields)
            {
              auditChange.AuditChangeFields.Add(field);
              field.AuditChange = auditChange;
            }
            auditChanges.Add(auditChange);
          }
        }

        this.AuditChanges.AddRange(auditChanges);
        base.SaveChanges();
      }
      
    }

    //public override int SaveChanges()
    //{
    //    return base.SaveChanges();
    //}

    public int SaveChangesNoAudit()
    {
      bool priorAddAuditRecords = this.AddAuditRecords;
      this.AddAuditRecords = false;
      int result = SaveChanges();
      this.AddAuditRecords = priorAddAuditRecords;
      return result;
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
      var delayedEntries = new List<EntityEntry>();
      if (this.AddAuditRecords)
        delayedEntries = AddAuditChangeRecords();

      int result = base.SaveChanges(acceptAllChangesOnSuccess);

      ProcessDelayedEntries(delayedEntries);

      return result;
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default(CancellationToken))
    {
      var delayedEntries = new List<EntityEntry>();
      if (this.AddAuditRecords)
        delayedEntries = AddAuditChangeRecords();

      int result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
      ProcessDelayedEntries(delayedEntries);

      return result;
    }

    //public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
    //{
    //    return base.SaveChangesAsync(cancellationToken);
    //}

    public Task<int> SaveChangesAsync()
    {
      return base.SaveChangesAsync();
    }

    public AppDbContext Clone()
    {
      return new AppDbContext(DbContextOptions, _identityService);
    }
  }
}