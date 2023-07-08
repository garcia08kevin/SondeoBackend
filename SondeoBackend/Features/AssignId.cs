using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using System.Web.WebPages;

namespace SondeoBackend.Configuration
{
    public class AssignId
    {
        private readonly UserManager<CustomUser> _userManager;

        public AssignId(UserManager<CustomUser> userManager) {
            _userManager = userManager;
        }
        public async Task<string> AssignSyncId(string lastId, string email)
        {

            string digitos = Regex.Match(lastId, @"\d+").Value;
            var alias = await UserAlias(email);
            if(alias.IsEmpty())
            {
                return string.Empty;
            }
            if (lastId.Equals("0"))
            {                                
                return $"{1}{alias}";
            }
            else
            {
                var numero = Convert.ToInt32(digitos);
                return $"{numero+1}{alias}";
            }            
        }

        public async Task<string> UserAlias(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return string.Empty;
            }
            return user.Alias;
        }
    }
}
