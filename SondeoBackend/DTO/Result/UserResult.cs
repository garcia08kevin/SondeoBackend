using SondeoBackend.Configuration;
using SondeoBackend.DTO.UserControl;
using System.Collections;

namespace SondeoBackend.DTO.Result
{
    public class UserResult
    {
        public bool Result { get; set; }
        public string Token { get; set; }
        public string Respose { get; set; }
        public UserDetail User { get; set; }
        public List<CustomUser> UserList { get; set; }
    }
}
