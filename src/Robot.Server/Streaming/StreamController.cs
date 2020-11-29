namespace Robot.Server.Streaming
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Robot.Devices.Camera.DirectShow;

    public sealed class StreamController : Controller
    {
        [Route("stream")]
        public async Task<IActionResult> StreamAsync()
        {
            var videoStream = new VideoStream(
                camera: new DirectShowCamera(),
                cancellationToken: HttpContext.RequestAborted);

            return File(videoStream.GetStream(), "video/webm");
        }
    }
}
