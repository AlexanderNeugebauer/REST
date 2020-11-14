using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REST
{
   
   class ResponseContext
   {
      private string _version;   // HTTP protocol version
      private StatusCode _status;
      private Dictionary<string, string> _headers = new Dictionary<string, string>(); // HTTP headers
      private string _body;       // HTTP body
   }
}
