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

        [HttpGet("rooms/{chatRoomId}/user/{userId}")]
        [ProducesResponseType(typeof(ChatRoomRequest), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChatRoom(int chatRoomId, int userId)
        {
            try
            {
                var chatRoom = await getChatRoomService.GetChatRoomAsync(chatRoomId, userId);

                if (chatRoom == null)
                    return NotFound(new { message = "Sala de chat no encontrada" });

                return Ok(chatRoom);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("rooms/{chatRoomId}/messages")]
        [ProducesResponseType(typeof(ChatMessageRequest), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChatMessages(int chatRoomId, [FromBody] GetMessagesRequestDto request)
        {
            try
            {
                request.ChatRoomId = chatRoomId;
                var messages = await getChatMessagesService.GetChatMessagesAsync(request);
                return Ok(messages);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("rooms/{chatRoomId}/read")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkMessagesAsRead(int chatRoomId, [FromBody] MarkAsReadRequest request)
        {
            try
            {
                Console.WriteLine($"📨 [Controller] MarkMessagesAsRead llamado");
                Console.WriteLine($"📨 [Controller] chatRoomId: {chatRoomId}");
                Console.WriteLine($"📨 [Controller] request: {System.Text.Json.JsonSerializer.Serialize(request)}");
                Console.WriteLine($"📨 [Controller] request.UserId: {request?.UserId}");
                Console.WriteLine($"📨 [Controller] request.UserRole: {request?.UserRole}");

                // ✅ Validar que request no sea null
                if (request == null)
                {
                    Console.WriteLine("❌ [Controller] Request es NULL");
                    return BadRequest(new { message = "Request body es requerido" });
                }

                // ✅ Validar que UserId no sea 0
                if (request.UserId <= 0)
                {
                    Console.WriteLine($"❌ [Controller] UserId inválido: {request.UserId}");
                    return BadRequest(new { message = "UserId es requerido y debe ser mayor a 0" });
                }

                // ✅ Validar que UserRole no sea null/empty
                if (string.IsNullOrEmpty(request.UserRole))
                {
                    Console.WriteLine("❌ [Controller] UserRole es null o vacío");
                    return BadRequest(new { message = "UserRole es requerido" });
                }

                await markMessagesAsReadService.MarkMessagesAsReadAsync(chatRoomId, request.UserId, request.UserRole);

                Console.WriteLine("✅ [Controller] Mensajes marcados exitosamente");
                return Ok(new { message = "Mensajes marcados como leídos" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [Controller] Error: {ex.Message}");
                Console.WriteLine($"❌ [Controller] StackTrace: {ex.StackTrace}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", service = "ChatMS", timestamp = DateTime.UtcNow });
        }

    }
}
