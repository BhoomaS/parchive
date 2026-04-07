using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CH.Business.AutoMapper;
using CH.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CH.Business
{
	public static class StartupHelpers
	{
		#region Configure Services

		public static void ConfigureServices(IServiceCollection services, IConfiguration config)
		{
			ConfigureIdentity(services, config);
			ConfigureJwtAuthentication(services, config);

			services.AddDbContext<AppDbContext>(cfg =>
			{
				cfg.UseSqlServer(config.GetConnectionString("AppConnection"));
			});
		}

		private static void ConfigureIdentity(IServiceCollection services, IConfiguration config)
		{
			var lockoutOptions = new LockoutOptions()
			{
				AllowedForNewUsers = config.GetAllowedForNewUsers(),
				DefaultLockoutTimeSpan = config.GetDefaultLockoutTimeSpan(),
				MaxFailedAccessAttempts = config.GetMaxFailedAccessAttempts(),
			};

			services.AddIdentity<Entities.ApplicationUser, Entities.ApplicationRole>(cfg =>
					{
						cfg.User.RequireUniqueEmail = true;
						cfg.Password.RequiredLength = 6;
						cfg.Password.RequiredUniqueChars = 3;
						// These are set to false because of the CustomPasswordValidator added below,
						// which checks the count of matched sets instead of forcing a specific set.
						cfg.Password.RequireDigit = false;
						cfg.Password.RequireLowercase = false;
						cfg.Password.RequireNonAlphanumeric = false;
						cfg.Password.RequireUppercase = false;
						cfg.Lockout = lockoutOptions;
					})
					.AddEntityFrameworkStores<AppDbContext>()
					.AddPasswordValidator<CustomPasswordValidator>()
					.AddDefaultTokenProviders(); // Used for password reset token generation
		}

		private static void ConfigureJwtAuthentication(IServiceCollection services, IConfiguration config)
		{
			var key = Encoding.UTF8.GetBytes(config.GetJwtTokenKey());
			services.AddAuthentication(x =>
			{
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(x =>
				{
					x.RequireHttpsMetadata = false;
					x.SaveToken = false;
					x.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(key),
						ValidateIssuer = false,
						ValidateAudience = false,
						ClockSkew = TimeSpan.Zero
					};
					x.Events = new JwtBearerEvents
					{
						OnMessageReceived = context =>
						{
							var accessToken = context.Request.Query["access_token"];

							// If the request is for our hub...
							var path = context.HttpContext.Request.Path;

							// TODO: Debugging
							//System.Diagnostics.Trace.WriteLine($"MessageReceived: {accessToken}, path: {path}");

							if (!string.IsNullOrEmpty(accessToken) &&
									(path.StartsWithSegments("/chathub")))
							{
								// Read the token out of the query string
								context.Token = accessToken;
							}
							return Task.CompletedTask;
						}
					};
				})
			//.AddCookie()
			;
		}

		#endregion
	}
}
