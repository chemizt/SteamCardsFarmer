﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPI.Data.Types {
    public class SSGame {
        [Key]
        public String Title { get; set; }
        public Double Price { get; set; }
        public String Link { get; set; }
    }
}
