using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CH.Data;
using CH.Models.ManagementPortal.Vendor;
using Microsoft.AspNetCore.Builder;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CH.Business.ManagementPortal
{
	public interface IVendorManager
	{
		Task<List<Vendor>> GetVendorListAsync();
    Task<List<Vendor>> GetVendorsListAsync(int chMemberId);
    Task<Vendor> SaveVendorAsync(VendorUpdate model);
	}


	public class VendorManager : IVendorManager
	{
		private readonly AppDbContext _context;
		private readonly IIdentityService _authService;


		public VendorManager(AppDbContext context, IIdentityService authService)
		{
			_context = context ?? throw new ArgumentNullException(paramName: nameof(context));
			_authService = authService;
		}


		private IQueryable<Vendor> ConvertVendor(IQueryable<Entities.Vendor> query)
		{
      // var chemp = _context.SnowflakeEmployers.ToList();
      return query
        .Select(o => new Vendor()
        {
          Id = o.Id,
          EmployerId = o.ChEmployerId ?? -1,
          EmployerName = o.SnowflakeMasterEmployer.EmployerName ?? VendorPage.GlobalVendor,
          VendorName = o.VendorName,
          Phone = o.Phone,
          Website = o.Website,
          Email = o.Email,
          ContactName = o.ContactName,
          IsEnabled = o.IsEnabled,
          ClientName = o.SnowflakeMasterEmployer.ClientName.ToUpper(),
          IsGlobalVendor = (o.SnowflakeMasterEmployer.EmployerName ?? VendorPage.GlobalVendor) == VendorPage.GlobalVendor
        });
		}

		public async Task<List<Vendor>> GetVendorListAsync()
		{
      //var query = _context.Vendors;
     

      var query = (from v in _context.Vendors
                   from e in _context.SnowflakeMasterEmployers
                   where (v.ChEmployerId == e.ChEmployerId && e.IsEnabled == true) || (v.ChEmployerId == null)
                   select v);

      return await ConvertVendor(query).ToListAsync();
		}
    public async Task<List<Vendor>> GetVendorsListAsync(int chMemberId)
    {
      int chEmployerId = await _context.SnowflakeMembers
       .Where(o => o.ChMemberId == chMemberId)
       .Select(o => o.ChEmployerId)
       .FirstOrDefaultAsync();
      var query = _context.Vendors.Where(o=>o.ChEmployerId== chEmployerId);
      return await ConvertVendor(query).ToListAsync();
    }

    public async Task<Vendor> SaveVendorAsync(VendorUpdate model)
		{
			Entities.Vendor vendor = null;
      Entities.Vendor vendorNew = null;
      var empId = model.EmployerId <= 0 ? (int?)null : model.EmployerId;
     

      if (model.Id != 0)
      {
        vendor = await _context.Vendors.Where(o => o.Id == model.Id)
          .FirstOrDefaultAsync();
        vendorNew = await _context.Vendors.Where(o => o.ChEmployerId == empId && o.VendorName.ToLower() == model.VendorName.ToLower())
          .FirstOrDefaultAsync();

        if (vendor != null && vendorNew == null)
        {
          vendor.ChEmployerId = model.EmployerId <= 0 ? (int?)null : model.EmployerId;
          vendor.VendorName = model.VendorName;
          vendor.Phone = model.Phone;
          vendor.Website = model.Website;
          vendor.Email = model.Email;
          vendor.ContactName = model.ContactName;
          vendor.IsEnabled = model.IsEnabled;
          vendor.UserLastEditedById = _authService.UserId;
          vendor.LastEditedTimestamp = DateTime.Now;

          await _context.SaveChangesAsync();
        }

        if (vendor != null || vendorNew == null)
        {
          //vendor.ChEmployerId = model.EmployerId <= 0 ? (int?)null : model.EmployerId;
          //vendor.VendorName = model.VendorName;
          vendor.Phone = model.Phone;
          vendor.Website = model.Website;
          vendor.Email = model.Email;
          vendor.ContactName = model.ContactName;
          vendor.IsEnabled = model.IsEnabled;
          vendor.UserLastEditedById = _authService.UserId;
          vendor.LastEditedTimestamp = DateTime.Now;

          await _context.SaveChangesAsync();
        }
      }


      if (model.Id == 0)
      {
        vendor = await _context.Vendors.Where(o => o.ChEmployerId == empId && o.VendorName.ToLower() == model.VendorName.ToLower())
          .FirstOrDefaultAsync();
        if (vendor == null)
        {
          vendor = new Entities.Vendor();
          await _context.Vendors.AddAsync(vendor);

          vendor.ChEmployerId = model.EmployerId <= 0 ? (int?)null : model.EmployerId;
          vendor.VendorName = model.VendorName;
          vendor.Phone = model.Phone;
          vendor.Website = model.Website;
          vendor.Email = model.Email;
          vendor.ContactName = model.ContactName;
          vendor.IsEnabled = model.IsEnabled;
          vendor.UserLastEditedById = _authService.UserId;
          vendor.LastEditedTimestamp = DateTime.Now;

          await _context.SaveChangesAsync();
        }
        else
        {
          return null;
        }
      }

        var query = _context.Vendors.Where(o => o.Id == vendor.Id);
        return await ConvertVendor(query).FirstOrDefaultAsync();
     
		}
	}
}
