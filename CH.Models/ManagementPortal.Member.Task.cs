using System;
using CH.Models.Common;
using Newtonsoft.Json;

namespace CH.Models.ManagementPortal.Member.Task
{
	public class TaskDetail
	{
		[JsonProperty("id")]
		public int? Id { get; set; }
		[JsonProperty("assignedId")]
		public int? UserAssignedId { get; set; }
		[JsonProperty("startDate")]
		public DateTime? StartDate { get; set; }
		[JsonProperty("endDate")]
		public DateTime? EndDate { get; set; }
		[JsonProperty("taskTypeId")]
		public int? CodeTaskTypeId { get; set; }
		[JsonProperty("taskPriorityId")]
		public int? CodeTaskPriorityId { get; set; }
		[JsonProperty("taskStatusId")]
		public int? CodeTaskStatusId { get; set; }
		[JsonProperty("taskNote")]
		public string TaskNote { get; set; }
    [JsonProperty("userInitial")]
    public string UserInitial { get; set; }
  }

	public class TaskSummary
	{
		public int? TaskId { get; set; }
		public int? ChMemberId { get; set; }
    public string? Client { get; set; }
    public string? Group { get; set; }
    public string? MemberId { get; set; }
		public int? AssignedId { get; set; }
		public string? AssignedToName { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public int? TaskTypeId { get; set; }
		public string? TaskTypeName { get; set; }
		public int? TaskPriorityId { get; set; }
		public string PriorityName { get; set; }
		public int? TaskStatusId { get; set; }
		public string? TaskStatusName { get; set; }
		public string? TaskNote { get; set; }
    public string? UserInitial { get; set; }
    public DateTime? CreatedTimestamp { get; set; }
	}


  public partial class Appointment
  {
    [Newtonsoft.Json.JsonProperty("id", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public long Id { get; set; }

    [Newtonsoft.Json.JsonProperty("firstName", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string FirstName { get; set; }

    [Newtonsoft.Json.JsonProperty("lastName", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string LastName { get; set; }

    [Newtonsoft.Json.JsonProperty("phone", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Phone { get; set; }

    [Newtonsoft.Json.JsonProperty("email", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Email { get; set; }

    /// <summary>Date</summary>
    [Newtonsoft.Json.JsonProperty("date", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Date { get; set; }

    [Newtonsoft.Json.JsonProperty("time", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public DateTime Time { get; set; }

    [Newtonsoft.Json.JsonProperty("endTime", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string EndTime { get; set; }

    [Newtonsoft.Json.JsonProperty("dateCreated", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string DateCreated { get; set; }

    [Newtonsoft.Json.JsonProperty("datetime", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    [Newtonsoft.Json.JsonConverter(typeof(DateFormatConverter))]
    public System.DateTimeOffset Datetime { get; set; }

    [Newtonsoft.Json.JsonProperty("datetimeCreated", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    [Newtonsoft.Json.JsonConverter(typeof(DateFormatConverter))]
    public System.DateTimeOffset DatetimeCreated { get; set; }

    [Newtonsoft.Json.JsonProperty("price", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Price { get; set; }

    [Newtonsoft.Json.JsonProperty("priceSold", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string PriceSold { get; set; }

    //[Newtonsoft.Json.JsonProperty("paid", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    //public AppointmentPaid Paid { get; set; }

    [Newtonsoft.Json.JsonProperty("amountPaid", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string AmountPaid { get; set; }

    [Newtonsoft.Json.JsonProperty("type", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Type { get; set; }

    [Newtonsoft.Json.JsonProperty("appointmentTypeID", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public long AppointmentTypeId { get; set; }

    [Newtonsoft.Json.JsonProperty("addonIDs", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public System.Collections.Generic.ICollection<int> AddonIDs { get; set; }

    [Newtonsoft.Json.JsonProperty("classID", Required = Newtonsoft.Json.Required.AllowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string ClassId { get; set; }

    [Newtonsoft.Json.JsonProperty("category", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Category { get; set; }

    [Newtonsoft.Json.JsonProperty("duration", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Duration { get; set; }

    [Newtonsoft.Json.JsonProperty("calendar", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Calendar { get; set; }

    [Newtonsoft.Json.JsonProperty("calendarID", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public int CalendarId { get; set; }

    [Newtonsoft.Json.JsonProperty("subCalendarID", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public int SubCalendarId { get; set; }

    [Newtonsoft.Json.JsonProperty("canClientCancel", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public bool CanClientCancel { get; set; }

    [Newtonsoft.Json.JsonProperty("canceled", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public bool Canceled { get; set; }

    [Newtonsoft.Json.JsonProperty("canClientReschedule", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public bool CanClientReschedule { get; set; }

    [Newtonsoft.Json.JsonProperty("location", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Location { get; set; }

    [Newtonsoft.Json.JsonProperty("certificate", Required = Newtonsoft.Json.Required.AllowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Certificate { get; set; }

    [Newtonsoft.Json.JsonProperty("confirmationPage", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string ConfirmationPage { get; set; }

    [Newtonsoft.Json.JsonProperty("notes", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Notes { get; set; }

    [Newtonsoft.Json.JsonProperty("noShow", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public bool NoShow { get; set; }

    [Newtonsoft.Json.JsonProperty("timezone", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string Timezone { get; set; }

    [Newtonsoft.Json.JsonProperty("calendarTimezone", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string CalendarTimezone { get; set; }

    ///// <summary>Labels</summary>
    //[Newtonsoft.Json.JsonProperty("labels", Required = Newtonsoft.Json.Required.AllowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    //public System.Collections.Generic.ICollection<Label> Labels { get; set; }

    //[Newtonsoft.Json.JsonProperty("forms", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    //public System.Collections.Generic.ICollection<Form> Forms { get; set; }

    [Newtonsoft.Json.JsonProperty("formsText", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public string FormsText { get; set; }

    [Newtonsoft.Json.JsonProperty("isVerified", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
    public bool IsVerified { get; set; }

    [Newtonsoft.Json.JsonProperty("scheduledBy")]
    public string ScheduledBy { get; set; }

    [Newtonsoft.Json.JsonExtensionData]
    public System.Collections.Generic.IDictionary<string, object> AdditionalProperties { get; set; } = new System.Collections.Generic.Dictionary<string, object>();
  }
  internal class DateFormatConverter : Newtonsoft.Json.Converters.IsoDateTimeConverter
  {
    public DateFormatConverter()
    {
      DateTimeFormat = "yyyy-MM-dd";
    }
  }
  public enum Direction
  {
    [System.Runtime.Serialization.EnumMember(Value = @"ASC")]
    ASC = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"DESC")]
    DESC = 1,

  }

}
