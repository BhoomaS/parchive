using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using CH.Data;
using CH.Models.Common;
using CH.Business.Services;

namespace CH.Business
{
	public interface ILookupManager
	{
		Task<List<AuditChange>> GetAuditChangesAsync(int chMemberId);
		Task<bool> AddAuditLogAsync(int typeId, string additionalInfo = null, int? userId = null);
		// TODO: Change the two below to use models instead
		Task<IEnumerable<Entities.CodeDef>> GetCodeDefsAsync(int id);
		Task<Entities.CodeDefType> GetCodeDefTypesAsync(int id);
		Task<IEnumerable<string>> GetCodeDefNamesAsync(int typeId);
		Task<IEnumerable<object>> GetCodeDefItemsAsync(int typeId);
    Task<IEnumerable<object>> GetCodeDefListItemsAsync(int typeId);
    Task<IEnumerable<string>> GetEthnicitiesAsync();
		Task<IEnumerable<KeyValue>> GetEmployersAsync();
		Task<IEnumerable<string>> GetStatesAsync();
		Task<IEnumerable<string>> GetRelationshipsAsync();
    Task<IEnumerable<KeyValue>> GetCarrierNamesAsync();
    Task<IEnumerable<KeyValue>> GetGroupsAsync(string client);
    Task<IEnumerable<KeyValue>> getCarrierNameBasedOnClientAsync(string client);
    Task<IEnumerable<KeyValue>> GetCarrierNamesAsync(string group);
    Task<IEnumerable<KeyValue>> GetClientsAsync();
  }

	public class LookupManager : BaseManager, ILookupManager
	{
		public LookupManager(
			AppDbContext context,
			IIdentityService identityService,
			ICacheService cacheService,
			IConfiguration config,
			IMapper mapper)
			: base(context, identityService, cacheService, config, mapper)
		{

		}


		public new Task<List<AuditChange>> GetAuditChangesAsync(int chMemberId)
		{
			return base.GetAuditChangesAsync(chMemberId);
		}

		public new Task<bool> AddAuditLogAsync(int typeId, string additionalInfo = null, int? userId = null)
		{
			return base.AddAuditLogAsync(typeId, additionalInfo, userId);
		}

		// TODO: Change this to use a model instead
		public new Task<IEnumerable<Entities.CodeDef>> GetCodeDefsAsync(int id)
		{
			return base.GetCodeDefsAsync(id);
		}

		// TODO: Change this to use a model instead
		public new Task<Entities.CodeDefType> GetCodeDefTypesAsync(int id)
		{
			return base.GetCodeDefTypesAsync(id);
		}

		public new Task<IEnumerable<string>> GetCodeDefNamesAsync(int typeId)
		{
			return base.GetCodeDefNamesAsync(typeId);
		}

		public new Task<IEnumerable<object>> GetCodeDefItemsAsync(int typeId)
		{
			return base.GetCodeDefItemsAsync(typeId);
		}

    public new Task<IEnumerable<object>> GetCodeDefListItemsAsync(int typeId)
    {
      return base.GetCodeDefListItemsAsync(typeId);
    }

    public new Task<IEnumerable<string>> GetEthnicitiesAsync()
		{
			return base.GetEthnicitiesAsync();
		}

		public new Task<IEnumerable<KeyValue>> GetEmployersAsync()
		{
			return base.GetEmployersAsync();
		}

    public new Task<IEnumerable<KeyValue>> GetCarrierNamesAsync()
    {
      return base.GetCarrierNamesAsync();
    }

    public new Task<IEnumerable<KeyValue>> GetGroupsAsync(string client)
    {
      return base.GetGroupsAsync(client);
    }

    public new Task<IEnumerable<KeyValue>> getCarrierNameBasedOnClientAsync(string client)
    {
      return base.GetCarrierNamesBasedOnClientNameAsync(client);
    }

    public new Task<IEnumerable<KeyValue>> GetCarrierNamesAsync(string group)
    {
      return base.GetCarrierNamesAsync(group);
    }

    public new Task<IEnumerable<KeyValue>> GetClientsAsync()
    {
      return base.GetClientsAsync();
    }

    public new Task<IEnumerable<string>> GetStatesAsync()
		{
			return base.GetStatesAsync();
		}

		public new Task<IEnumerable<string>> GetRelationshipsAsync()
		{
			return base.GetRelationshipsAsync();
		}


		// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
		// TODO: Add / expose new BaseManager methods here.  //
	}

}
