using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CH.Entities
{
    public partial class SnowflakeEmployerSource
    {
        public int CH_EMPLOYER_ID { get; set; }
        public string GROUP_ID { get; set; }
        public string EMPLOYER_NAME { get; set; }
        public bool IS_ENABLED { get; set; }
        public System.DateTime? DSS_CREATE_TIME { get; set; }
        public System.DateTime? DSS_UPDATE_TIME { get; set; }
    }
}
