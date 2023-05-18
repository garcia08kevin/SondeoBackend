using Microsoft.AspNetCore.SignalR;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SondeoBackend.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int tipo { get; set; }
        public string Mensaje { get; set; }
        public DateTime fecha { get; set; }
        public int Identificacion { get; set; }
        [Required]
        [DefaultValue(false)]
        public bool Vista { get; set; }        
    }
    public class Hubs : Hub
    {
        public async Task SendMessage(string message)
        {
            await base.OnConnectedAsync();
            await Clients.All.SendAsync("Notificacion", message);
        }
        public async Task SendMessageInt(int value)
        {
            await base.OnConnectedAsync();
            await Clients.All.SendAsync("nroNotificaciones", value);
        }
    }
}
