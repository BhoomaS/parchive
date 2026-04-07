using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CH.Business
{
	public class CustomPasswordValidator : IPasswordValidator<Entities.ApplicationUser>
	{
		protected readonly IConfiguration _config;

		public CustomPasswordValidator(IConfiguration config)
		{
			_config = config;
		}

		protected enum CharacterSet
		{
			CapitalLetter = 0,
			SmallLetter = 1,
			Digit = 2,
			Punctuation = 3,
		}

		protected bool IsInCharacterSet(CharacterSet charSet, char ch)
		{
			switch (charSet)
			{
				case CharacterSet.CapitalLetter:
					return (ch >= 'A') && (ch <= 'Z');
				case CharacterSet.SmallLetter:
					return (ch >= 'a') && (ch <= 'z');
				case CharacterSet.Digit:
					return (ch >= '0') && (ch <= '9');
				case CharacterSet.Punctuation:
					return "!@#$%^&*()_+-='\";:[{]}\\|.>,</?`~".IndexOf(ch) >= 0;
			}
			return false;
		}

		protected bool IsLongEnough(string password, int minLength)
		{
			return password != null && password.Length >= minLength;
		}

		protected bool SpansEnoughCharacterSets(string password, int minSets)
		{
			if (password == null)
				return false;

			int matchedSets = 0;
			foreach (CharacterSet charSet in Enum.GetValues(typeof(CharacterSet)))
			{
				foreach (char ch in password)
				{
					if (IsInCharacterSet(charSet, ch))
					{
						matchedSets++;
						break;
					}
				}
			}
			return matchedSets >= minSets;
		}

		public Task<IdentityResult> ValidateAsync(
			UserManager<Entities.ApplicationUser> manager,
			Entities.ApplicationUser user, string password)
		{
			if (IsLongEnough(password, manager.Options.Password.RequiredLength) &&
				SpansEnoughCharacterSets(password, _config.GetRequiredPasswordSets()))
			{
				return Task.FromResult(IdentityResult.Success);
			}
			return Task.FromResult(IdentityResult.Failed(new IdentityError()
			{
				Code = "CustomPassword",
				Description = "Password does not pass complexity rules",
			}));
		}

	}

}
