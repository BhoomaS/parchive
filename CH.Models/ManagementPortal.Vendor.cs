using CH.Models.Common;

namespace CH.Models.ManagementPortal.Vendor
{
	public class Vendor
	{
		public int Id { get; set; }
		public int EmployerId { get; set; }
		public string EmployerName { get; set; }
		public string VendorName { get; set; }
		public string Phone { get; set; }
		public string Website { get; set; }
		public string Email { get; set; }
		public string ContactName { get; set; }
		public string ClientName { get; set; }
		public bool IsEnabled { get; set; }
		public bool IsGlobalVendor { get; set; }
	}

	public class VendorUpdate
	{
		public int Id { get; set; }
		public int EmployerId { get; set; }
		public string VendorName { get; set; }
		public string Phone { get; set; }
		public string Website { get; set; }
		public string Email { get; set; }
		public string ContactName { get; set; }
		public bool IsEnabled { get; set; }
	}
}
