using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CH.Entities
{
    public partial class SnowflakeMasterEmployerSource
    {
        public int CH_EMPLOYER_ID { get; set; }
        public string EMPLOYER_NAME { get; set; }
        public bool IS_ENABLED { get; set; }
        public string CLIENT_ID { get; set; }
        public string CLIENT_NAME { get; set; }
  }
}
