﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SondeoBackend.Context;
using SondeoBackend.Models;

namespace SondeoBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly DataContext _context;

        public NotificationsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Notifications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            if (_context.Notifications == null)
            {
                return NotFound();
            }
            return await _context.Notifications.ToListAsync();
        }

        [HttpGet]
        [Route("UltimaNotificacion")]
        public async Task<ActionResult<Notification>> GetLastNotifications()
        {
            var lastValue = _context.Notifications.Where(j => j.Vista == false).OrderByDescending(e => e.Id).FirstOrDefault();
            return lastValue;
        }

        // GET: api/Notifications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotification(int id)
        {
            if (_context.Notifications == null)
            {
                return NotFound();
            }
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            return notification;
        }

        [Route("MarcarComoLeido")]
        [HttpPost]
        public async Task<IActionResult> MarcarComoLeido(int id)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(id);
                notification.Vista = true;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotificationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [Route("NotificacionesNoLeidas")]
        [HttpGet]
        public async Task<int> NotificacionesNoLeidas ()
        {
            int cantidad = 0;
            var notificaciones = await _context.Notifications.ToListAsync();
            for (int i = 0; i < notificaciones.Count; i++)
            {
                if (!notificaciones[i].Vista)
                {
                    cantidad++;
                }
            }
            return cantidad;
        }

        // DELETE: api/Notifications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            if (_context.Notifications == null)
            {
                return NotFound();
            }
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NotificationExists(int id)
        {
            return (_context.Notifications?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
