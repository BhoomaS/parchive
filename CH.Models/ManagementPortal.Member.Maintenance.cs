using System;
using System.Collections.Generic;
using CH.Models.Common;

namespace CH.Models.ManagementPortal.Member.Maintenance
{
	public class MaintenanceTable
	{
		public List<string> Columns { get; set; }
		public List<object[]> Rows { get; set; }
	}

	public class MaintenanceBiometric
	{
		public int? Id { get; set; }
		public long? SfId { get; set; }
		public DateTime? TestDate { get; set; }
		public string A1C { get; set; }
		public string BMI { get; set; }
		public string Systolic { get; set; }
		public string Diastolic { get; set; }
		public string Height { get; set; }
		public string Weight { get; set; }
		public string Waist { get; set; }
		public string Glucose { get; set; }
		public string HDL { get; set; }
		public string LDL { get; set; }
		public string Triglycerides { get; set; }
		public string Tobacco { get; set; }
		public string Fasting { get; set; }
		public string TotalCholesterol { get; set; }
		public string HdlRatio { get; set; }
		public string ALT { get; set; }
		public string AST { get; set; }
	}

	public class MaintenancePcp
	{
		public int? Id { get; set; }
		public long? SfId { get; set; }
		public string Address { get; set; }
		public string Zip { get; set; }
		public string NPI { get; set; }
		public DateTime? DateLastSeen { get; set; }
		public string Name { get; set; }
		public string Phone { get; set; }
		public string Specialty { get; set; }
		public string Fax { get; set; }
	}

	public class MaintenanceChronic
	{
		public int? Id { get; set; }
		public long? SfId { get; set; }
		public string Condition { get; set; }
		public string ICD { get; set; }
		public DateTime? Date { get; set; }
		public bool ExcludeReporting { get; set; }
		public bool ExcludeScoring { get; set; }
	}

	public class MaintenanceMedical
	{
		public int? Id { get; set; }
		public long? SfId { get; set; }
		public DateTime? ServiceDate { get; set; }
		public string Description { get; set; }
		public string Code { get; set; }
		public string Type { get; set; }
		public string Frequency { get; set; }
    public string ProviderName { get; set; }
    public bool ExcludeReporting { get; set; }
		public bool ExcludeScoring { get; set; }
	}

	public class MaintenanceAllergy
	{
		public int? Id { get; set; }
		public long? SfId { get; set; }
		public string Allergy { get; set; }
	}

	public class MaintenanceCareTeam
	{
		public int? Id { get; set; }
		public long? SfId { get; set; }
		public string Name { get; set; }
		public string Phone { get; set; }
		public string Specialty { get; set; }
		public string Fax { get; set; }
		public DateTime? ServiceDate { get; set; }
		public bool ExcludeReporting { get; set; }
		public bool Active { get; set; }
	}

	public class MaintenanceRecommendation
	{
		public int? Id { get; set; }
		public long? SfId { get; set; }
		public string Recommendation { get; set; }
    public int RecommendationPriority { get; set; }
    public string Plan { get; set; }
		public DateTime? Date { get; set; }
		public bool Completed { get; set; }
		public bool ExcludeReporting { get; set; }
    public string RecommendationCode { get; set; }
    public string BestResourceToMeetMetric { get; set; }
  }

	public class MaintenanceGoal
	{
		public int? Id { get; set; }
		public long? SfId { get; set; }
		public string Goal { get; set; }
		public string Plan { get; set; }
		public string PlanB { get; set; }
		public bool ExcludeReporting { get; set; }
	}

	public class MaintenanceProgram
	{
		public int? Id { get; set; }
		public long? SfId { get; set; }
		public string Name { get; set; }
		public DateTime? Date { get; set; }
		public string Completed { get; set; }
		public bool ExcludeReporting { get; set; }
	}

  public class AccoladeReferral
  {
    public string AccoladeId { get; set; }
    public DateTime? BirthDate { get; set; }
    public string Relationship { get; set; }
    public string Gender { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string ProgramName { get; set; }
    public string ProgramPartner { get; set; }
    public string MemberEngaged { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? ImportDate { get; set; }
  }

  public class AccoladeEngagement
  {
    public string AccoladeId { get; set; }
    public string CommunicationId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string Gender { get; set; }
    public string Relationship { get; set; }
    public string CommunicationChannel { get; set; }
    public DateTime? CommunicationDate { get; set; }
    public string CallerType { get; set; }
    public string SpeakingTo { get; set; }
    public string CommunicationDirection { get; set; }
    public string ClinicalEngagement { get; set; }
    public string CommunicationObjectiveCategory { get; set; }
    public string CommunicationObjective { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? ImportDate { get; set; }
  }

  public class AccoladeUMData
  {
    public string Company { get; set; }
    public string ParentGroup { get; set; }
    public string Group { get; set; }
    public string Plan { get; set; }
    public string IsuiteCaseId { get; set; }
    public string IsuiteReferenceNum { get; set; }
    public string Setting { get; set; }
    public string Service { get; set; }
    public string Urgency { get; set; }
    public DateTime? AdmitDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public DateTime? CaseClosedDate { get; set; }
    public DateTime? DecisionDate { get; set; }
    public string ActualDays { get; set; }
    public string UnitType { get; set; }
    public string UnitsRequested { get; set; }
    public string UnitsApproved { get; set; }
    public string UnitsNonCertified { get; set; }
    public string Decision { get; set; }
    public string DischargeDisposition { get; set; }
    public string NonCertReason { get; set; }
    public DateTime? NotificationDate { get; set; }
    public DateTime? ProcedureRequestedDate { get; set; }
    public string ProcedureCode { get; set; }
    public string ProcedureName { get; set; }
    public string PrimaryDiagnosisCode { get; set; }
    public string PrimaryDiagnosisDescription { get; set; }
    public string PlanMemberNum { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string AgeAtAdmission { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public string PrimaryPhone { get; set; }
    public string PrimaryExt { get; set; }
    public string PhysicianFirstName { get; set; }
    public string PhysicianLastName { get; set; }
    public string PhysicianCity { get; set; }
    public string PhysicianState { get; set; }
    public string PhysicianTaxId { get; set; }
    public string ServiceProviderName { get; set; }
    public string ServiceProviderCity { get; set; }
    public string ServiceProviderState { get; set; }
    public string InNetwork { get; set; }
    public string PhysicianReview { get; set; }
    public string ReviewType { get; set; }
    public string ReviewDate { get; set; }
    public string ReviewDecision { get; set; }
    public string CMGRLastName { get; set; }
    public string CMGRFirstName { get; set; }
    public string RNLocation { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? ImportDate { get; set; }
  }

}
