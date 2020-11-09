# GrpcServiceGettingStarted
https://techmarday.com/Grpc-trong-dotnetcore

Tiếp theo bài viết: https://techmarday.com/tong-quan-grpc
Phần này mình sẽ đi vào phần thực hành: Source code sử dụng .net core 3.1

Yêu cầu: 
.net core, c# căn bản
Hiểu về các khái niệm liên quan đến grpc (có thể tham khảo bài viết tổng quan grpc trước khi xem bài này)

Tạo mới 1 solution tên là: GrpcServiceGettingStarted

1- Tạo booking service ở server side

Tạo mới 1 project chọn kiểu grpc service

Đặt tên project là BookingService

Mặc định sẽ có file Greeter.proto và GreeterService.cs. Các bạn xóa 2 file này đi

Nhớ xóa cả phần khai báo dependency trong startup.cs

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<BookingService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
Build lại sau khi xóa và chắc chắn không có lỗi

Demo này sẽ viết 2 methods của booking service cho server side
1 - GetBookingById

2 - GetListBookingStatusById

Tạo file Booking.proto

Các bạn có thể đọc thêm về Protocol buffer ở đây
https://developers.google.com/protocol-buffers/docs/overview

Có thể hiểu nôm na, sau khi tạo file với đuôi .proto thì protocol plugin sẽ tự động tạo ra các methods ở client và server side với tham số truyền vào và type trả về.

syntax = "proto3";

option csharp_namespace = "BookingService";

package Booking;

// The Booking service definition.
service Booking {
	rpc GetBookingById(BookingRequest) returns (BookingResponse);	
	rpc GetListBookingStatusById (BookingRequest) returns (stream BookingStatusResponse) {}
}

message BookingRequest{
	string Id = 1;
}

message BookingResponse{
	string Id = 1;
	string Description = 2;
	int32 Quantity = 3;
}

message BookingStatusResponse {
	string status = 1;
}
Code ở trên mình định nghĩ GetBookingById với parameter là BookingRequest và kiểu trả về là BookingResponse

rpc GetBookingById(BookingRequest) returns (BookingResponse); method này là kiểu Unary RPC, nghĩa là client gửi 1 request và server trả về 1 response tương ứng.

rpc GetListBookingStatusById (BookingRequest) returns (stream BookingStatusResponse) {} method này là kiểu Server streaming RPC, nghĩa là client gửi 1 request và server trả về stream message.

Sau khi định nghĩa parameter, kiểu trả về và các phương thức trong file .proto. Ta sẽ build project để tạo ra các methods tương ứng với method định nghĩa trong file .proto ở server side

Vào thư mục debug sẽ thấy file BookingGrpc.cs được tạo ra

Các bạn có thể xem các method tương ứng được tự động tạo ra

Tiếp theo ta sẽ tạo file BookingService.cs trong folder Service để override các method được tự động tạo ra từ file .proto

BookingService này kế thừa Booking.BookingBase
.net core cung cấp package Grpc.Core; để làm việc với Grpc

Viết code override methods

using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
namespace BookingService
{
    public class BookingService : Booking.BookingBase
    {
        private readonly ILogger<BookingService> _logger;

        public BookingService(ILogger<BookingService> logger)
        {
            _logger = logger;
        }

        public override Task<BookingResponse> GetBookingById(BookingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new BookingResponse
            {
                Id = request.Id,
                Description = "Description test",
                Quantity = 2
            });
        }

        public override async Task GetListBookingStatusById(BookingRequest request, IServerStreamWriter<BookingStatusResponse> responseStream,
            ServerCallContext context)
        {
            await responseStream.WriteAsync(
                new BookingStatusResponse { Status = "Cancelled" });
            await Task.Delay(500);
            await responseStream.WriteAsync(
                new BookingStatusResponse { Status = "In-progress" });
            await Task.Delay(1000);
            await responseStream.WriteAsync(
                new BookingStatusResponse { Status = "Succeed" });
        }
    }
}
ở Method GetListBookingStatusById kiểu trả về là stream nên ta phải sử dụng IServerStreamWriter<BookingStatusResponse> responseStream

Stream này sẽ trả về liên tục khi client gọi. Và khi clients nhận đủ dữ liệu trả về thì quá trình này kết thúc.

Khai báo BookingService. Để map endpoint của booking service ta thêm vào startup.cs

Run Booking service ở port 5001

Vậy là ta đã tạo thành công grpc cho booking service ở server side.

2. Tạo client app để call service ở server 

Tạo mới 1 console app bằng .net core

Đặt tên project là GrpcClientApp

Sau khi tạo xong ta vào nuget packet và cài đặt các packages sau:

rpc.Tools

Grpc.Net.Client

Google.Protobuf

Tạo ra folder Protos và copy file Booking.proto ở Booking service project vào folder này

Vào folder chứa GrpcClientApp project để chỉnh sửa file GRPCClientApp.csproj

Thêm GrpcServices="Client" 
Sau khi thêm vào build lại project. Đảm bảo build thành công và check lại file GRPCClientApp.csproj có đúng Config Client hay chưa
Sau khi build thành công ta vào folder debug sẽ thấy được class BookingGrpc.cs được tạo ra từ file .proto
Class này chứa các method tương ứng trong file .proto
Gọi booking service ở Main method của Program.cs

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
Vì đã host server ở port 5001, ta sẽ host client ở port 5002. Chỉnh file launchSetting.json
Test gọi service của server side từ client side

Chạy file BookingService.exe để start grpc ở server với port 5001
Chạy GRPCClientApp ở port 5002
Source code: https://github.com/TechMarDay/GrpcServiceGettingStarted
Link tham khảo:

https://docs.microsoft.com/en-us/aspnet/core/tutorials/grpc/grpc-start?view=aspnetcore-3.1&tabs=visual-studio
   
