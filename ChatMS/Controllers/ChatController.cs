using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ICreateRoomService createRoomService;
        private readonly IGetChatRoomService getChatRoomService;
        private readonly IGetUserChatRoomsService getUserChatRoomsService;
        private readonly IGetChatMessagesService getChatMessagesService;
        private readonly ISendMessageService sendMessageService;
        private readonly IMarkMessagesAsReadService markMessagesAsReadService;

        public ChatController(ICreateRoomService createRoomService,IGetChatRoomService getChatRoomService,
            IGetUserChatRoomsService getUserChatRoomsService,IGetChatMessagesService getChatMessagesService,
            ISendMessageService sendMessageService,IMarkMessagesAsReadService markMessagesAsReadService)
        {
            this.createRoomService = createRoomService;
            this.getChatRoomService = getChatRoomService;
            this.getUserChatRoomsService = getUserChatRoomsService;
            this.getChatMessagesService = getChatMessagesService;
            this.sendMessageService = sendMessageService;
            this.markMessagesAsReadService = markMessagesAsReadService;
        }

        [HttpPost("create/room")]
        [ProducesResponseType(typeof(ChatRoomRequest), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateChatRoom([FromBody] CreateChatRoomRequest request)
        {
            try
            {
                var chatRoom = await createRoomService.CreateChatRoomAsync(request);
                return Ok(chatRoom);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("rooms/user/{id}")]
        [ProducesResponseType(typeof(IEnumerable<ChatRoomRequest>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserChatRooms(int id)
        {
            try
            {
                var chatRooms = await getUserChatRoomsService.GetUserChatRoomsAsync(id);
                return Ok(chatRooms);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
