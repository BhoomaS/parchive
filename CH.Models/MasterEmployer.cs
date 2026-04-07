using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CH.Models.Common;

namespace CH.Models.MasterEmployer
{
	[TypescriptInclude]
	public class MasterEmployerSummary
	{
		public int ChEmployerId { get; set; } // CH_EMPLOYER_ID (Primary key)
                                          //public string GroupId { get; set; } // GROUP_ID (length: 100)
    public string ClientName { get; set; }
    public string EmployerName { get; set; } // EMPLOYER_NAME (length: 100)
		//public DateTime? DssCreateTime { get; set; } // DSS_CREATE_TIME
		//public DateTime? DssUpdateTime { get; set; } // DSS_UPDATE_TIME
		//public DateTime? LastSnowflakeTimestamp { get; set; } // LastSnowflakeTimestamp
		public bool? IsEnabled { get; set; } // IS_ENABLED

		public int? EmployerDetailId { get; set; }
		public int? DefaultUserAssignedId { get; set; }
		public string DefaultUserAssignedFullName { get; set; }
	}

	[ManagementPortalTypescriptInclude]
	public class MasterEmployer
	{
		// TODO: Add required attributes
		public int ChEmployerId { get; set; } // CH_EMPLOYER_ID (Primary key)
                                          //public string GroupId { get; set; } // GROUP_ID (length: 100)
    public string ClientName { get; set; }
    public string EmployerName { get; set; } // EMPLOYER_NAME (length: 100)
		//public DateTime? DssCreateTime { get; set; } // DSS_CREATE_TIME
		//public DateTime? DssUpdateTime { get; set; } // DSS_UPDATE_TIME
		//public DateTime? LastSnowflakeTimestamp { get; set; } // LastSnowflakeTimestamp
		public bool? IsEnabled { get; set; } // IS_ENABLED

		public int? EmployerDetailId { get; set; }
		public int? DefaultUserAssignedId { get; set; }
		public string DefaultUserAssignedFullName { get; set; }
		public decimal? MyPhaRegistrationMinClinicalRisk { get; set; }

		public IEnumerable<EmployerBenefit> EmployerBenefits { get; set; }

    public MasterEmployer()
		{
			this.EmployerBenefits = new List<EmployerBenefit>();
		}
	}

	[TypescriptInclude]
	public class EmployerBenefit
	{
		// TODO: Add required attributes
		public int EmployerBenefitId { get; set; } // Id (Primary key)
		public int EmployerDetailId { get; set; } // EmployerDetailId
		public string Title { get; set; } // Title (length: 500)
		public string Subtitle { get; set; } // Subtitle (length: 500)
		public string Description { get; set; } // Description (length: 4000)
		public string Url { get; set; } // Url (length: 4000)
		public int? DisplayOrder { get; set; } // DisplayOrder
		public bool IsEnabled { get; set; } // IsEnabled
	}

	[ManagementPortalTypescriptInclude]
	public class MasterEmployerFilter
	{
		// TODO: Add properties as needed
		public int? ChEmployerId { get; set; }
	}

	[ManagementPortalTypescriptInclude]
	public class SaveMasterEmployerResult : Common.SaveResult<MasterEmployer>
	{
		public MasterEmployer Detail { get; set; }
	}



}
