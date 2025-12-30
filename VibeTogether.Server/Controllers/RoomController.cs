using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VibeTogether.Server.DTOs.Room;
using VibeTogether.Server.Services;
using VibeTogether.Server.Services.Interfaces;

namespace VibeTogether.Server.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly PresenceService _presenceService;

        public RoomController(IRoomService roomService, PresenceService presenceService)
        {
            _roomService = roomService;
            _presenceService = presenceService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var result = await _roomService.CreateRoomAsync(userId);
            return Ok(result);
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinRoom([FromBody] JoinRoomRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            await _roomService.JoinRoomAsync(userId, request.RoomCode);
            return Ok();
        }

        //[Authorize]
        //[HttpPost("activate/{roomId}")]
        //public IActionResult ActivateRoom(string roomId)
        //{
        //    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        //    _presenceService.SetActiveRoom(userId, roomId);
        //    Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" + _presenceService.GetActiveRoom(userId));
        //    return Ok();
        //}

    }
}
