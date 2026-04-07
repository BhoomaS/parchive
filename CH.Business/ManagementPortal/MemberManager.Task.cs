using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

using CH.Data;
using CH.Models.ManagementPortal.Member.Task;
using AngleSharp.Dom;

namespace CH.Business.ManagementPortal
{
    public partial interface IMemberManager
    {
        Task<IEnumerable<TaskSummary>> GetTasksAsync(int chMemberId);
        Task<IEnumerable<TaskSummary>> GetOverViewTasksAsync(int Id);
        Task<IEnumerable<TaskSummary>> GetOpenTasksAsync();
        Task<TaskSummary> SaveTaskAsync(int chMemberId, TaskDetail task);
        Task<bool> DeleteTaskAsync(int chMemberId, int taskId);
    }


    public partial class MemberManager : IMemberManager
    {
        private IQueryable<TaskSummary> ConvertTaskSummary(
            IQueryable<CH.Entities.TaskEntity> query)
        {
            return query
                .Select(o => new TaskSummary()
                {
                    TaskId = o.Id,
                    ChMemberId = o.ChMemberId,
                    MemberId = o.SnowflakeMember.MemberId,
                    Client=o.SnowflakeMember.GroupId,
                    AssignedId = o.UserAssignedId,
                    AssignedToName = o.UserAssigned.FullName,
                    StartDate = o.StartDate,
                    EndDate = o.EndDate,
                    TaskPriorityId = o.CodeTaskPriorityId,
                    PriorityName = o.CodeTaskPriority.DisplayName,
                    TaskTypeId = o.CodeTaskTypeId,
                    TaskTypeName = o.CodeTaskType.DisplayName,
                    TaskStatusId = o.CodeTaskStatusId,
                    TaskStatusName = o.CodeTaskStatus.DisplayName,
                    TaskNote = o.TaskNote,
                    UserInitial = o.UserInitial,
                    CreatedTimestamp = o.CreatedTimestamp,
                });
        }

        public async Task<TaskSummary> SaveTaskAsync(int chMemberId, TaskDetail model)
        {
            
            Entities.TaskEntity task = null;

            var phauser = await Context.ApplicationUsers.FirstOrDefaultAsync(o => o.Id == IdentityService.UserId);
            var fullName = phauser.FullName.TrimEnd();
            var names = fullName.Split(' ');
            string userInitial = " User(";
            string fN_initial = fullName.Substring(0, 1) != "J" ? String.Concat(userInitial, fullName.Substring(0, 1)) : String.Concat(userInitial, fullName.Substring(0, 2));
                  string lN_initial = fN_initial == " User(Ja" ? names[1].Substring(0, 2)
            : fN_initial == " User(Jo" && fullName != "Jorge Gonzalez" ? names[1].Substring(0, 3)
            : fN_initial == " User(M" && fullName == "Melissa Orosco" ? names[1].Substring(0, 3) : names[1].Substring(0, 1);
      userInitial = String.Concat(fN_initial, String.Concat(lN_initial, ") "));

      if (model.Id.HasValue)
            {
                task = await Context.TaskEntities
                    .FirstOrDefaultAsync(x => x.ChMemberId == chMemberId && x.Id == model.Id);
            }

            if (task == null)
            {
                task = new Entities.TaskEntity()
                {
                    ChMemberId = chMemberId,
                    RecordDate = DateTime.Now,
                    CreatedTimestamp = DateTime.Now,
                };
                await Context.TaskEntities.AddAsync(task);
            }

            task.CodeTaskTypeId = model.CodeTaskTypeId;
            task.CodeTaskPriorityId = model.CodeTaskPriorityId;
            task.StartDate = model.StartDate;
            task.EndDate = model.EndDate;
            task.UserAssignedId = model.UserAssignedId;
            task.CodeTaskStatusId = model.CodeTaskStatusId;
            task.TaskNote = model.TaskNote;
            task.UserInitial = userInitial;
            task.UserLastEditedById = IdentityService.UserId;
            task.LastEditedTimestamp = DateTime.Now;

            await Context.SaveChangesAsync();

            var query = Context.TaskEntities.Where(x => x.Id == task.Id);
            return await ConvertTaskSummary(query).FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteTaskAsync(int chMemberId, int taskId)
        {
            var success = false;

            if (taskId <= 0)
            {
                throw new ArgumentNullException(paramName: nameof(taskId));
            }
            // Get Task by taskId
            var task = await Context.TaskEntities.FirstOrDefaultAsync(x => x.Id == taskId && x.ChMemberId == chMemberId);

            // Validate if Task exists
            if (task == null)
            {
                return success;
            }

            // Remove the Task from repository
            Context.Remove(task);
            //if (task.CreatedTimestamp >= DateTime.Now.Subtract(
            //    Config.GetDeleteMaximumAge()))
            //{
            //    Context.Remove(task);
            //}
            //else
            //{
            //    return false;
            //}

            // Delete Task in database
            if (await Context.SaveChangesAsync() > 0)
            {
                success = true;
            }

            return success;
        }

        public async Task<IEnumerable<TaskSummary>> GetTasksAsync(int chMemberId)
        {
            if (chMemberId == 0)
            {
                throw new ArgumentNullException(paramName: nameof(chMemberId));
            }

            var query = Context.TaskEntities.Where(x => x.ChMemberId == chMemberId);
            var tasks = await ConvertTaskSummary(query).ToListAsync();
            return tasks;
        }

        public async Task<IEnumerable<TaskSummary>> GetOverViewTasksAsync(int Id)
        {
          if (Id == 0)
          {
            throw new ArgumentNullException(paramName: nameof(Id));
          }
          DateTime today = TimeZoneInfo.ConvertTime(DateTime.Today, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
          var query = Context.TaskEntities.Where(x => x.UserAssignedId == Id && x.CodeTaskStatusId != 45 && x.EndDate < today);
          var tasks = await ConvertTaskSummary(query).ToListAsync();
          return tasks;
        }

    public async Task<IEnumerable<TaskSummary>> GetOpenTasksAsync()
        {

      var query = (from t in Context.TaskEntities
                   join m in Context.SnowflakeMembers on t.ChMemberId equals m.ChMemberId
                   join e in Context.SnowflakeEmployers on m.ChEmployerId equals e.ChEmployerId
                   where t.CodeTaskStatusId != (int)Models.Enums.TaskStatus.Closed && t.CodeTaskStatusId != null 
                   && (m.CurrentStatus != "TERMED" || m.CurrentStatus != "Termed" || m.CurrentStatus != "COBRA")
                   && e.IsEnabled == true && m.GroupId == e.GroupId
                   orderby t.EndDate ascending
                   select t);
      //var query = Context.TaskEntities.Join(Context.SnowflakeMembers,t=>t.ChMemberId)
      //    .Where(x => (x.CodeTaskStatusId != (int)Models.Enums.TaskStatus.Closed) && x.CodeTaskStatusId!=null);
      var tasks = await ConvertTaskSummary(query).ToListAsync();
      
      foreach(var member in tasks)
      {
        var employers = Context.SnowflakeEmployers.Where(x => x.GroupId.Contains(member.Client)).Take(1).ToList();
       if(employers != null)
        {
          foreach(var e in employers)
          {
            member.Group = e.EmployerName;
            member.Client = e.ClientName;
          }
        }

      }
      //List<TaskSummary> lstTasks = new List<TaskSummary>();
      //foreach (var member in tasks)
      //{
       
      //  var status = await Context.SnowflakeMembers.FirstOrDefaultAsync(x => x.MemberId == member.MemberId);

      //  if (status.CurrentStatus != "TERMED" || status.CurrentStatus != "Termed" || status.CurrentStatus != "COBRA")
      //  {
      //    lstTasks.Add(status);
      //  }
      //}
      return tasks;
        }

        public async Task<bool> UpdatedTask(TaskDetail note)
        {
            var success = false;
            var updatedTask = await Context.TaskEntities.FirstOrDefaultAsync(x => x.Id == note.Id);

            if (updatedTask == null)
            {
                throw new ArgumentNullException(paramName: nameof(updatedTask));
            }

            updatedTask.CodeTaskTypeId = note.CodeTaskTypeId;
            updatedTask.CodeTaskPriorityId = note.CodeTaskPriorityId;
            updatedTask.StartDate = note.StartDate;
            updatedTask.EndDate = note.EndDate;
            updatedTask.UserAssignedId = note.UserAssignedId;
            updatedTask.CodeTaskStatusId = note.CodeTaskStatusId;
            updatedTask.RecordDate = DateTime.UtcNow;
            updatedTask.UserLastEditedById = IdentityService.UserId;
            updatedTask.LastEditedTimestamp = DateTime.UtcNow;

            Context.TaskEntities.Update(updatedTask);

            if(await Context.SaveChangesAsync() > 0)
            {
                success = true;
            }

            return success;
        }
    }
}
