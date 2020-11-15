using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace REST
{
   [Serializable]
   public class HttpException : Exception
   {
      public StatusCode StatusCode { get; set; }
      public HttpException(StatusCode statusCode)
      {
         StatusCode = statusCode;
      }
   }
}
