using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BookingService;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GRPCClientApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var paymentClient = new Booking.BookingClient(channel);

            Console.WriteLine("Welcome to the gRPC client");
            var booking = paymentClient.GetBookingById(new BookingRequest
            {
               Id = "00001"
            });
            Console.WriteLine($"Booking description: {booking.Description}");

            //Get streaming message
            var statuses = paymentClient.GetListBookingStatusById(new BookingRequest
            {
                Id = "00002"
            });

            while (await statuses.ResponseStream.MoveNext())
            {
                var statusReply = statuses.ResponseStream.Current.Status;
                Console.WriteLine($"Payment status: {statusReply}");
            }

            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
