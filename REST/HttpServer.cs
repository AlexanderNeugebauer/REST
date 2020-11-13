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
   class HttpServer
   {
      private TcpListener server;
      private Int32 port;
      private IPAddress localAddr;
      public HttpServer()
      {
         port = 80;
         localAddr = IPAddress.Parse("127.0.0.1");
         server = new TcpListener(localAddr, port);
      }

      public void run()
      {
         try
         {
            // Start listening for client requests.
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data;

            // Enter the listening loop.
            while (true)
            {
               Console.Write("Waiting for a connection... ");

               // Perform a blocking call to accept requests.
               // You could also use server.AcceptSocket() here.
               TcpClient client = server.AcceptTcpClient();
               Console.WriteLine("Connected!");

               data = "";

               // Get a stream object for reading and writing
               NetworkStream stream = client.GetStream();

               int i;

               // Loop to receive all the data sent by the client.
               
               while (stream.DataAvailable)
               {
                  i = stream.Read(bytes, 0, bytes.Length);
                  // Translate data bytes to a ASCII string.
                  data += System.Text.Encoding.ASCII.GetString(bytes, 0, i);
               }
               Console.WriteLine("Received: {0}", data);
               // Process the data sent by the client.


               // Send back a response.
               byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
               stream.Write(msg, 0, msg.Length);
               Console.WriteLine("Sent: {0}", data);
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
            server.Stop();
         }

         Console.WriteLine("\nHit enter to continue...");
         Console.Read();
      }
   }



}
