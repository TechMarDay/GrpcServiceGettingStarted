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