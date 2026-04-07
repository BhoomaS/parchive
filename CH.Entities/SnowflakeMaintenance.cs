using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Xml.Linq;

namespace CH.Entities
{
  public class SnowflakeMemberAllergySource
  {
    public int CH_MEMBER_ALLERGY_ID { get; set; }
    public int? PHA_MEMBER_ALLERGY_ID { get; set; }
    public int CH_MEMBER_ID { get; set; }
    public string ALLERGIES { get; set; }
  }

  public class SnowflakeMemberBiometricSource
  {
    public int CH_BIOMETRIC_ID { get; set; }
    public int? PHA_BIOMETRIC_ID { get; set; } // TODO: Add to Snowflake view
    public string RECORD_ID { get; set; }
    public int CH_MEMBER_ID { get; set; }
    public DateTime? BIOMETRIC_TEST_DATE { get; set; }
    public string FASTING_FLAG { get; set; }
    public string GLUCOSE { get; set; }
    public string A1C { get; set; }
    public string BLOOD_PRESSURE_SYSTOLIC { get; set; }
    public string BLOOD_PRESSURE_DIASTOLIC { get; set; }
    public string TOTAL_CHOLESTEROL { get; set; }
    public string TOTAL_CHOLESTEROL_HDL_RATIO { get; set; }
    public string LDL { get; set; }
    public string HDL { get; set; }
    public string TRIGLYCERIDES { get; set; }
    public string WAIST_CIRCUMFERENCE { get; set; }
    public string HEIGHT { get; set; }
    public string WEIGHT { get; set; }
    public string BMI { get; set; }
    public string ALT { get; set; }
    public string AST { get; set; }
    public string TOBACCO { get; set; }
    public string DSS_RECORD_SOURCE { get; set; }
    public DateTime? DSS_LOAD_DATE { get; set; }
    public DateTime? DSS_CREATE_TIME { get; set; }
    public DateTime? DSS_UPDATE_TIME { get; set; }
  }

  public class SnowflakeMemberCareTeamSource
  {
    public int CH_MEMBER_CARE_TEAM_ID { get; set; }
    public int? PHA_MEMBER_CARE_TEAM_ID { get; set; }
    public int CH_MEMBER_ID { get; set; }
    public string PROVIDER_NAME { get; set; }
    public string PROVIDER_SPECIALTY { get; set; }
    public string PROVIDER_PHONE { get; set; }
    public string PROVIDER_FAX { get; set; }
    public DateTime? SERVICE_DATE { get; set; }
    public string PROVIDER_ID { get; set; }
    public bool EXCLUDE_REPORTING { get; set; }
    public bool ACTIVE { get; set; }
  }

  public class SnowflakeMemberChronicConditionSource
  {
    public int CH_MEMBER_CHRONIC_CONDITION_ID { get; set; }
    public int? PHA_MEMBER_CHRONIC_CONDITION_ID { get; set; }
    public int CH_MEMBER_ID { get; set; }
    public string CHRONIC_CONDITION { get; set; }
    public string ICD_CODE { get; set; }
    public DateTime? CHRONIC_CONDITION_DATE { get; set; }
    public string PROVIDER_NAME { get; set; }
    public bool EXCLUDE_REPORTING { get; set; }
    public bool EXCLUDE_SCORING { get; set; }
  }

  public class SnowflakeMemberMedicalInfoSource
  {
    public long CH_MEMBER_MEDICAL_INFO_ID { get; set; }
    public int? PHA_MEMBER_MEDICAL_INFO_ID { get; set; }
    public int CH_MEMBER_ID { get; set; }
    public DateTime? SERVICE_DATE { get; set; }
    public string CODE { get; set; }
    public string DESCRIPTION { get; set; }
    public string TYPE { get; set; }
    public string FREQUENCY { get; set; }
    public string PROVIDER_NAME { get; set; }
    public bool EXCLUDE_REPORTING { get; set; }
    public bool EXCLUDE_SCORING { get; set; }
  }

  public class SnowflakeMemberPcpSource
  {
    public int CH_MEMBER_PCP_ID { get; set; }
    public int? PHA_MEMBER_PCP_ID { get; set; }
    public int CH_MEMBER_ID { get; set; }
    public string PCP_NAME { get; set; }
    public string PCP_SPECIALTY { get; set; }
    public string PCP_NPI { get; set; }
    public string PCP_ADDRESS { get; set; }
    public string PCP_ZIP_CODE { get; set; }
    public DateTime? PCP_DATE_LAST_SEEN { get; set; }
    public string PCP_PHONE { get; set; }
    public string PCP_FAX { get; set; }
    public int? UPDATED_BY_PHA { get; set; }
  }

  public class SnowflakeMemberPersonalGoalsPlanSource
  {
    public int CH_MEMBER_PERSONAL_GOAL_PLAN_ID { get; set; }
    public int? PHA_MEMBER_PERSONAL_GOAL_PLAN_ID { get; set; }
    public int CH_MEMBER_ID { get; set; }
    public string PERSONAL_GOALS { get; set; }
    public string PERSONAL_PLAN { get; set; }
    public string PLAN_B { get; set; }
    public bool EXCLUDE_REPORTING { get; set; }
  }

  public class SnowflakeMemberProgramSource
  {
    public int CH_MEMBER_PROGRAM_ID { get; set; }
    public int? PHA_MEMBER_PROGRAM_ID { get; set; }
    public int CH_MEMBER_ID { get; set; }
    public string PROGRAM_NAME { get; set; }
    public DateTime? PROGRAM_DATE { get; set; }
    public string COMPLETED { get; set; }
    //public string REFERRED { get; set; }
    public bool EXCLUDE_REPORTING { get; set; }
  }

  public class SnowflakeMemberRecommendationSource
  {
    public int CH_MEMBER_RECOMMENDATION_ID { get; set; }
    public int? PHA_MEMBER_RECOMMENDATION_ID { get; set; }
    public int CH_MEMBER_ID { get; set; }
    public string RECOMMENDATION { get; set; }
    public int RECOMMENDATION_PRIORITY { get; set; }
    public string PLAN { get; set; }
    public DateTime? RECOMMENDATION_DATE { get; set; }
    public bool COMPLETED { get; set; }
    public bool EXCLUDE_REPORTING { get; set; }
    public string RECOMMENDATION_CODE { get; set; }
    public string BEST_RESOURCE_TO_MEET_METRIC { get; set; }
  }

  public class SnowflakeAccoladeEngagementSource
  {
    public string CH_MEMBER_ID { get; set; }
    public string ACCOLADE_ID { get; set; }
    public string COMMUNICATIONID { get; set; }
    public string FIRSTNAME { get; set; }
    public string LASTNAME { get; set; }
    public DateTime? BIRTHDATE { get; set; }
    public string GENDER { get; set; }
    public string RELATIONSHIP { get; set; }
    public string COMMUNICATIONCHANNEL { get; set; }
    public DateTime? COMMUNICATIONDATE { get; set; }
    public string CALLERTYPE { get; set; }
    public string SPEAKINGTO { get; set; }
    public string COMMUNICATIONDIRECTION { get; set; }
    public string CLINICALENGAGEMENT { get; set; }
    public string COMMOBJVCATETGORY { get; set; }
    public string COMMOBJECTIVE { get; set; }
    public DateTime? RECEIVED_DATE { get; set; }
    public DateTime? IMPORT_DATE { get; set; }
  }

  public class SnowflakeAccoladeReferralSource
  {
    public string CH_MEMBER_ID { get; set; }
    public string ACCOLADE_ID { get; set; }
    public DateTime? BIRTH_DATE { get; set; }
    public string RELATIONSHIP_CODE { get; set; }
    public string GENDER { get; set; }
    public string LAST_NAME { get; set; }
    public string FIRST_NAME { get; set; }
    public DateTime? EFFECTIVE_DATE { get; set; }
    public string PROGRAM_NAME { get; set; }
    public string PROGRAM_PARTNER { get; set; }
    public string MEMBER_ENGAGED { get; set; }
    public DateTime? RECEIVED_DATE { get; set; }
    public DateTime? IMPORT_DATE { get; set; }
  }


  public class SnowflakeAccoladeUMDataSource
  {
    public string CH_MEMBER_ID { get; set; }
    public string COMPANY { get; set; }
    public string PARENT_GROUP { get; set; }
    public string GROUP_ { get; set; }
    public string PLAN { get; set; }
    public string ISUITE_CASE_ID { get; set; }
    public string ISUITE_REFERENCE_NUM { get; set; }
    public string SETTING { get; set; }
    public string SERVICE { get; set; }
    public string URGENCY { get; set; }
    public DateTime? ADMIT_DATE { get; set; }
    public DateTime? DISCHARGE_DATE { get; set; }
    public DateTime? CASE_CLOSED_DATE { get; set; }
    public DateTime? DECISION_DATE { get; set; }
    public string ACTUAL_DAYS { get; set; }
    public string UNIT_TYPE { get; set; }
    public string UNITS_REQUESTED { get; set; }
    public string UNITS_APPROVED { get; set; }
    public string UNITS_NON_CERTIFIED { get; set; }
    public string DECISION { get; set; }
    public string DISCHARGE_DISPOSITION { get; set; }
    public string NON_CERT_REASON { get; set; }
    public DateTime? NOTIFICATION_DATE { get; set; }
    public DateTime? PROCEDURE_REQUESTED_DATE { get; set; }
    public string PROCEDURE_CODE { get; set; }
    public string PROCEDURE_NAME { get; set; }
    public string PRIMARY_DIAGNOSIS_CODE { get; set; }
    public string PRIMARY_DIAGNOSIS_NAME { get; set; }
    public string PLAN_MEMBER_NUM { get; set; }
    public string MEMBER_FIRST_NAME { get; set; }
    public string MEMBER_LAST_NAME { get; set; }
    public string GENDER { get; set; }
    public DateTime? DATE_OF_BIRTH { get; set; }
    public string AGE_AT_ADMISSION { get; set; }
    public string ADDRESS1 { get; set; }
    public string ADDRESS2 { get; set; }
    public string CITY { get; set; }
    public string STATE { get; set; }
    public string POSTAL_CODE { get; set; }
    public string COUNTRY { get; set; }
    public string PRIMARY_PHONE { get; set; }
    public string PRIMARY_EXT { get; set; }
    public string PHYSICIAN_FIRST_NAME { get; set; }
    public string PHYSICIAN_LAST_NAME { get; set; }
    public string PHYSICIAN_CITY { get; set; }
    public string PHYSICIAN_STATE { get; set; }
    public string PHYSICIAN_TAX_ID { get; set; }
    public string SERVICE_PROVIDER_NAME { get; set; }
    public string SERVICE_PROVIDER_CITY { get; set; }
    public string SERVICE_PROVIDER_STATE { get; set; }
    public string IN_NETWORK { get; set; }
    public string PHYSICIAN_REVIEW { get; set; }
    public string REVIEW_TYPE { get; set; }
    public string REVIEW_DATE { get; set; }
    public string REVIEW_DECISION { get; set; }
    public string CMGR_LAST_NAME { get; set; }
    public string CMGR_FIRST_NAME { get; set; }
    public string RN_LOCATION { get; set; }
    public DateTime? RECEIVED_DATE { get; set; }
    public DateTime? IMPORT_DATE { get; set; }

  }

  public class SnowflakeVoriHealthSource
  {
    public string CH_MEMBER_ID { get; set; }
    //public string MEMBER_ID { get; set; }
   // public string FIRST_NAME { get; set; }
    //public string LAST_NAME { get; set; }
    //public string GENDER { get; set; }
    //public DateTime? DATE_OF_BIRTH{ get; set; }
    //public string EMAIL { get; set; }
    //public string PHONE_NUMBER { get; set; } 
    //public string EMR_ID { get; set; }
    //public string VENDOR_ID { get; set; }
    //public string GROUP_ID { get; set; }
    //public DateTime? ACCOUNT_CREATION_DATE { get; set; }
    //public DateTime? FIRST_VISIT_DATE { get; set; }
    public DateTime? RECENT_VISIT_DATE { get; set; }
    //public string DEVICE { get; set; }
    public string STATUS { get; set; }
    public DateTime? STATUS_DATE { get; set; }
    public string REASON { get; set; }
    //public string SOURCEFILE_NAME { get; set; }
    //public DateTime? FILE_RECEIVED_DATE { get; set; }
    //public DateTime? IMPORT_TIMESTAMP { get; set; }
  }

}