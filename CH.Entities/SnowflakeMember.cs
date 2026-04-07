using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CH.Entities
{
    public partial class SnowflakeMember
    {
        public string FullName
        {
            get
            {
                // return $"{FirstNameEdited ?? FirstName} {MiddleNameEdited ?? MiddleName} {LastNameEdited ?? LastName}".Replace("  ", " ");
                return $"{ FirstName} {MiddleName} {LastName}".Replace("  ", " ");
              }
        }
    }


    public partial class SnowflakeMemberSource
    {
        public int CH_MEMBER_ID { get; set; }
        public int CH_EMPLOYER_ID { get; set; }
        public string MEMBER_ID { get; set; }
        public string GROUP_ID { get; set; }
        public string FIRST_NAME { get; set; }
        public string MIDDLE_NAME { get; set; }
        public string LAST_NAME { get; set; }
        public string SSN { get; set; }
        public string GENDER { get; set; }
        public System.DateTime? DOB { get; set; }
        public string ADDRESS_1 { get; set; }
        public string ADDRESS_2 { get; set; }
        public string CITY { get; set; }
        public string COUNTY { get; set; }
        public string STATE { get; set; }
        public string ZIP_CODE { get; set; }
        public string EMAIL_ADDRESS { get; set; }
        public string MARITAL_STATUS { get; set; }
        public string CELL_PHONE { get; set; }
        public string WORK_PHONE { get; set; }
        public string HOME_PHONE { get; set; }
    public string Updated_Phone { get; set; } // Updated_PHONE (length: 100)
    public string SelfReported_Phone { get; set; }
    public string RACE { get; set; }
        public string ETHNIC_GROUP { get; set; }
        public string EDUCATION_LEVEL { get; set; }
        public string RELATIONSHIP { get; set; }
        public string AGE_RANGE { get; set; }
        public string OFFICE_LOCATION { get; set; }
        public string ALTERNATE_MEMBER_ID { get; set; }
        public string MARA_RISK { get; set; }
        public string CLINICAL_RISK { get; set; }
        public string CURRENT_STATUS { get; set; }
        public string COVERAGE_TYPE { get; set; }
        public string CARRIER_NAME { get; set; }
        public string PLAN_NAME { get; set; }
        public System.DateTime? EFFECTIVE_DATE { get; set; }
        public string DSS_RECORD_SOURCE { get; set; }
        public System.DateTime? DSS_LOAD_DATE { get; set; }
        public System.DateTime? DSS_CREATE_TIME { get; set; }
        public System.DateTime? DSS_UPDATE_TIME { get; set; }
        public string ClientEmployeeId { get; set; }
    }
}
