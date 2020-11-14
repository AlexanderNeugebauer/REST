using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;

namespace REST
{
   [Flags]
   enum Method
   {
      GET,
      POST,
      PUT,
      DELETE
   }
   class HttpServer
   {
      private class Endpoint
      {
         private string _path;
         private Method _verb;
         private Func<string> _evnt;

         public Endpoint(string path, Method verb, Func<string> func)
         {
            _path = path;
            _verb = verb;
            _evnt = func;
         }
         public string Signature()
         {
            return Signature(_verb, _path);
         }

         public void setEvent(Func<string> func)
         {
            _evnt = func;
         }

         public static string Signature(Method verb, string path)
         {
            return $"{verb.ToString()} {path}";
         }
      }

      private TcpListener _server;
      private Int32 _port;
      private IPAddress _localAddr;
      private Endpoint[] _endpoints;

      public HttpServer()
      {
         _port = 80;
         _localAddr = IPAddress.Parse("127.0.0.1");
         _server = new TcpListener(_localAddr, _port);

      }

      /// <summary>
      /// starts running the HTTP Server
      /// </summary>
      public void Run()
      {
         try
         {
            // Start listening for client requests.
            _server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data;

            // Enter the listening loop.
            while (true)
            {
               Console.Write("Waiting for a connection... ");

               // Perform a blocking call to accept requests.
               // You could also use server.AcceptSocket() here.
               TcpClient client = _server.AcceptTcpClient();
               Console.WriteLine("Connected!");

               data = "";

               // Get a stream object for reading and writing
               NetworkStream stream = client.GetStream();

               int i;

               // Loop to receive all the data sent by the client.

               do
               {
                  i = stream.Read(bytes, 0, bytes.Length);
                  // Translate data bytes to a ASCII string.
                  data += System.Text.Encoding.ASCII.GetString(bytes, 0, i);
               } while (stream.DataAvailable);
               // Process the data sent by the client.
               try
               {
                  RequestContext rc = new RequestContext(data);
                  Console.WriteLine(rc.ToString());
               
                  // Send back a response.

                  byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                  stream.Write(msg, 0, msg.Length);
               }
               catch (Exception e)
               {
                  throw; // TODO return 400
               }
               // Shutdown and end connection
               client.Close();
            }
         }
         catch (SocketException e)
         {
            Console.WriteLine("SocketException: {0}", e);
         }
         finally
         {
            // Stop listening for new clients.
            _server.Stop();
         }

         Console.WriteLine("\nHit enter to continue...");
         Console.Read();
      }

      /// <summary>
      /// Registers a function as endpoint for given path.
      /// </summary>
      /// <param name="path">Accepts values in {} as wildcards. e.g /messages/{msgNum}/</param>
      /// <param name="endPoint"></param>
      public void RegisterEndpoint(string path, Method verb, Func<string> func)
      {
         // check if endpoint already exists
         foreach (var ep in _endpoints) // ToList to make changes inside foeach despite IEnumberable
         {
            if (ep.Signature().Equals(Endpoint.Signature(verb, path)))
            {
               ep.setEvent(func);
               return;
            }
         }
         _endpoints.Append(new Endpoint(path, verb, func));
      }

   }
}
