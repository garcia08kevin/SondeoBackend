﻿using System.Collections;

namespace SondeoBackend.Models
{
    public class AuthResult
    {
        public string Token { get; set; }
        public bool Result { get; set; }
        public List<string> Errors { get; set; }
        public IEnumerable Contenido { get; set; }
    }
}
