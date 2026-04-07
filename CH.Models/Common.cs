using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CH.Models.Common
{
  public class KeyValue
  {
    public int Id { get; set; }
    public string Value { get; set; }
  }

  [TypescriptIgnore]
  public class GenericKeyValue<TKey, TValue>
  {
    public TKey Id { get; set; }
    public TValue Value { get; set; }
  }

  #region DTO helpers

  public class ModelError
  {
    public string PropertyName { get; set; }
    public string Description { get; set; }
  }

  public class ModelErrorList<T> : List<ModelError>
  {
    public void Add(Expression<Func<T, object>> property, string description)
    {
      this.Add(new ModelError()
      {
        PropertyName = property.GetPropertyName(),
        Description = description,
      });
    }
  }

  public class SaveResult<T>
  {
    public ModelErrorList<T> ModelErrors { get; set; }


    public SaveResult()
    {
      this.ModelErrors = new ModelErrorList<T>();
    }
  }

  #endregion


  [TypescriptIgnore]
  public class AuditChange
  {
    public DateTime DateCreated { get; set; }
    public string TableName { get; set; }
    public string FieldName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public string UserName { get; set; }
    public int TypeDescriptionId { get; set; }
    public string TypeDescription { get; set; }
  }

  [TypescriptIgnore]
  public class DatabaseUpdateSummary
  {
    public int InsertedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int SkippedCount { get; set; }
  }

  [TypescriptIgnore]
  public class CodeDef
  {
    // TODO: Flesh this out, use AutoMapper, replace manager return values of type Entities.CodeDef
  }

  [TypescriptIgnore]
  public class CodeDefType
  {
    // TODO: Flesh this out, use AutoMapper, replace manager return values of type Entities.CodeDefType
  }

  [TypescriptIgnore]
  public class FileDownloadResult
  {
    public bool Succeeded { get; set; }
    public byte[] FileContents { get; set; }
    public string ContentType { get; set; }
    public string FileName { get; set; }
  }

  [MyPhaTypescriptInclude]
  public class SupportConfiguration
  {
    public string Email { get; set; }
    public string Phone { get; set; }
    public string MailTo { get; set; }
    public string MailAddress1 { get; set; }
    public string MailAddress2 { get; set; }
    public string MailCity { get; set; }
    public string MailState { get; set; }
    public string MailZip { get; set; }
  }

  [MyPhaTypescriptInclude]
  public class ChatConfiguration
  {
    public TimeSpan MinTimeOfDay { get; set; }
    public string MaxTimeOfDayFormatted { get; set; }

    public TimeSpan MaxTimeOfDay { get; set; }
    public string MinTimeOfDayFormatted { get; set; }

    public string AllowedOnDays { get; set; }
    public string MinDayOfWeek { get; set; }
    public string MaxDayOfWeek { get; set; }

    public TimeSpan EstimatedResponseTime { get; set; }
    public string EstimatedResponseTimeFormatted { get; set; }

    public bool IsChatAvailable { get; set; }

    // TODO: Add these back if they become important for the client apps to know
    //public TimeSpan AbandonedSessionTimeout { get; set; }
    //public TimeSpan AbandonedSessionInterval { get; set; }
    //public TimeSpan SessionAlertDelay { get; set; }
    //public TimeSpan SessionAlertInterval { get; set; }
  }

}
