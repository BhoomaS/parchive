using CH.Models.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace CH.Models.Email
{
 
    public class EmailService
    {
      public int Id { get; set; }
      public string to { get; set; }
      public string cc { get; set; }
      public string subject { get; set; }
      public string message { get; set; }
      public string notes { get; set; }
      public string signature { get; set; }
      public string uploadedDocs { set; get; }
      public string selectionType { set; get; }
    }

    //[TypescriptInclude]
    public class EmailServiceResult : SaveResult<EmailService>
    {
      public bool Succeeded { get; set; }
      public string Message { get; set; }
      public bool Status { get; set; }

    }
  
}
