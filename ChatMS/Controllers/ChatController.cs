using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ICreateRoomService _createRoomService;
        private readonly IGetUserChatRoomsService _getUserChatRoomsService;
        private readonly IGetChatRoomService _getChatRoomService;
        private readonly IGetChatMessagesService _getChatMessagesService;
        private readonly ISendMessageService _sendMessageService;
        private readonly IMarkMessagesAsReadService _markMessagesAsReadService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            ICreateRoomService createRoomService,
            IGetUserChatRoomsService getUserChatRoomsService,
            IGetChatRoomService getChatRoomService,
            IGetChatMessagesService getChatMessagesService,
            ISendMessageService sendMessageService,
            IMarkMessagesAsReadService markMessagesAsReadService,
            ILogger<ChatController> logger)
        {
            _createRoomService = createRoomService;
            _getUserChatRoomsService = getUserChatRoomsService;
            _getChatRoomService = getChatRoomService;
            _getChatMessagesService = getChatMessagesService;
            _sendMessageService = sendMessageService;
            _markMessagesAsReadService = markMessagesAsReadService;
            _logger = logger;
        }

        // POST: api/Chat/create/room
        [HttpPost("create/room")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateChatRoomRequest request)
        {
            try
            {
                _logger.LogInformation("=== INICIO CreateRoom ===");
                _logger.LogInformation("Request recibido (raw): {@Request}", request);
                
                if (request == null)
                {
                    _logger.LogError("Request es null");
                    return BadRequest(new { message = "Request no puede ser null" });
                }
                
                _logger.LogInformation("DoctorId: {DoctorId}, PatientId: {PatientId}, AppointmentId: {AppointmentId}", 
                    request.DoctorId, request.PatientId, request.AppointmentId);
                
                if (request.DoctorId <= 0 || request.PatientId <= 0)
                {
                    _logger.LogWarning("DoctorId o PatientId inválidos: DoctorId={DoctorId}, PatientId={PatientId}", 
                        request.DoctorId, request.PatientId);
                    return BadRequest(new { message = "DoctorId y PatientId son requeridos y deben ser mayores a 0" });
                }

                // Validar que AppointmentId esté presente (requerido para asociar a consulta específica)
                if (request.AppointmentId <= 0)
                {
                    _logger.LogError("AppointmentId inválido o faltante. AppointmentId recibido: {AppointmentId}", request.AppointmentId);
                    return BadRequest(new { message = $"AppointmentId es requerido para asociar el chat a una consulta específica. Valor recibido: {request.AppointmentId}" });
                }
                
                _logger.LogInformation("Validaciones pasadas. Creando/buscando sala para AppointmentId: {AppointmentId}", request.AppointmentId);

                _logger.LogInformation("Creando/buscando sala de chat para AppointmentId: {AppointmentId}, DoctorId: {DoctorId}, PatientId: {PatientId}", 
                    request.AppointmentId, request.DoctorId, request.PatientId);

                var chatRoom = await _createRoomService.CreateChatRoomAsync(request);
                
                _logger.LogInformation("Sala de chat encontrada/creada: {ChatRoomId} para AppointmentId: {AppointmentId}", 
                    chatRoom.Id, request.AppointmentId);
                
                return Ok(chatRoom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear sala de chat");
                return StatusCode(500, new { message = "Error al crear sala de chat", error = ex.Message });
            }
        }

        // GET: api/Chat/rooms/user/{userId}
        [HttpGet("rooms/user/{userId}")]
        public async Task<IActionResult> GetUserChatRooms(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "UserId debe ser mayor a 0" });
                }

                var rooms = await _getUserChatRoomsService.GetUserChatRoomsAsync(userId);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener salas de chat del usuario {UserId}", userId);
                return StatusCode(500, new { message = "Error al obtener salas de chat", error = ex.Message });
            }
        }

        // GET: api/Chat/rooms/{chatRoomId}/user/{userId}
        [HttpGet("rooms/{chatRoomId}/user/{userId}")]
        public async Task<IActionResult> GetChatRoom(int chatRoomId, int userId)
        {
            try
            {
                if (chatRoomId <= 0 || userId <= 0)
                {
                    return BadRequest(new { message = "ChatRoomId y UserId deben ser mayores a 0" });
                }

                var room = await _getChatRoomService.GetChatRoomAsync(chatRoomId, userId);
                
                if (room == null)
                {
                    return NotFound(new { message = "Sala de chat no encontrada" });
                }

                return Ok(room);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sala de chat {ChatRoomId} para usuario {UserId}", chatRoomId, userId);
                return StatusCode(500, new { message = "Error al obtener sala de chat", error = ex.Message });
            }
        }

        // POST: api/Chat/rooms/{chatRoomId}/messages
        [HttpPost("rooms/{chatRoomId}/messages")]
        public async Task<IActionResult> GetChatMessages(int chatRoomId, [FromBody] GetChatMessagesRequest request)
        {
            try
            {
                if (chatRoomId <= 0)
                {
                    return BadRequest(new { message = "ChatRoomId debe ser mayor a 0" });
                }

                if (request == null || request.UserId <= 0)
                {
                    return BadRequest(new { message = "UserId es requerido" });
                }

                var skip = request.PageNumber > 0 ? (request.PageNumber - 1) * request.PageSize : 0;
                var take = request.PageSize > 0 ? request.PageSize : 50;

                var messages = await _getChatMessagesService.GetChatMessagesAsync(chatRoomId, request.UserId, skip, take);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mensajes de la sala {ChatRoomId}", chatRoomId);
                return StatusCode(500, new { message = "Error al obtener mensajes", error = ex.Message });
            }
        }

        // POST: api/Chat/rooms/{chatRoomId}/read
        [HttpPost("rooms/{chatRoomId}/read")]
        public async Task<IActionResult> MarkMessagesAsRead(int chatRoomId, [FromBody] int userId)
        {
            try
            {
                if (chatRoomId <= 0 || userId <= 0)
                {
                    return BadRequest(new { message = "ChatRoomId y UserId deben ser mayores a 0" });
                }

                await _markMessagesAsReadService.MarkMessagesAsReadAsync(chatRoomId, userId);
                return Ok(new { message = "Mensajes marcados como leídos" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar mensajes como leídos en sala {ChatRoomId}", chatRoomId);
                return StatusCode(500, new { message = "Error al marcar mensajes como leídos", error = ex.Message });
            }
        }

        // POST: api/Chat/send
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                if (request == null || request.ChatRoomID <= 0 || string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { message = "ChatRoomID y Message son requeridos" });
                }

                // Obtener el senderId del token JWT o del request
                // Por ahora, asumimos que viene en el request o lo obtenemos del token
                var senderId = GetUserIdFromToken(); // Implementar este método
                
                if (senderId <= 0)
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var message = await _sendMessageService.SendMessageAsync(request.ChatRoomID, senderId, request.Message);
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar mensaje");
                return StatusCode(500, new { message = "Error al enviar mensaje", error = ex.Message });
            }
        }

        private int GetUserIdFromToken()
        {
            // TODO: Implementar extracción del userId del token JWT
            // Por ahora, intentar obtenerlo del header Authorization
            var userIdClaim = User?.FindFirst("UserId")?.Value ?? User?.FindFirst("sub")?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return 0;
        }
    }

    // DTO para GetChatMessages
    public class GetChatMessagesRequest
    {
        public int ChatRoomId { get; set; }
        public int UserId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}

