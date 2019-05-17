using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iqviaPracticalExam.Areas.TweetCollector.Models
{
    public class iqviaTweetObject
    {
        public long id { get; set; }
        public DateTime stamp { get; set; } = DateTime.UtcNow;
        public string text { get; set; } = "";
    }
}
