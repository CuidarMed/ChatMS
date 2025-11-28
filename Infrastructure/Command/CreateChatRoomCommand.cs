using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Command
{
    public class CreateChatRoomCommand : ICreateChatRoomCommand
    {
        private readonly AppDbContext _context;

        public CreateChatRoomCommand(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ChatRoom> ExecuteAsync(CreateChatRoomRequest request)
        {
            // SIEMPRE requerir AppointmentId para crear una sala de chat
            if (request.AppointmentId <= 0)
            {
                throw new InvalidOperationException("AppointmentId es requerido para crear una sala de chat");
            }

            // Buscar sala existente para esta consulta específica
            // IMPORTANTE: Solo buscar salas que tengan AppointmentId (no null) para evitar reutilizar salas antiguas
            // Y que coincidan EXACTAMENTE con el AppointmentId solicitado
            var existingRoom = await _context.ChatRooms
                .FirstOrDefaultAsync(r => r.DoctorID == request.DoctorId && 
                                         r.PatientId == request.PatientId && 
                                         r.AppointmentId == request.AppointmentId &&
                                         r.IsActive);
            
            if (existingRoom != null)
            {
                // Verificar que realmente coincida el AppointmentId
                if (existingRoom.AppointmentId != request.AppointmentId)
                {
                    // Si no coincide, no reutilizar esta sala
                    existingRoom = null;
                }
            }

            if (existingRoom != null)
            {
                return existingRoom;
            }

            // Crear nueva sala asociada a la consulta específica
            // Verificar nuevamente que AppointmentId tenga valor antes de crear
            if (request.AppointmentId <= 0)
            {
                throw new InvalidOperationException($"No se puede crear una sala sin AppointmentId válido. AppointmentId recibido: {request.AppointmentId}");
            }

            var chatRoom = new ChatRoom
            {
                DoctorID = request.DoctorId,
                PatientId = request.PatientId,
                AppointmentId = request.AppointmentId, // SIEMPRE asociar a la consulta específica
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Verificar que AppointmentId se asignó correctamente antes de guardar
            if (chatRoom.AppointmentId <= 0)
            {
                throw new InvalidOperationException($"Error: AppointmentId no se asignó correctamente a la sala. Valor: {chatRoom.AppointmentId}");
            }

            _context.ChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();

            // Verificar que se guardó correctamente
            var savedRoom = await _context.ChatRooms.FindAsync(chatRoom.Id);
            if (savedRoom == null || savedRoom.AppointmentId <= 0)
            {
                throw new InvalidOperationException($"Error: La sala se creó pero no tiene AppointmentId. Sala ID: {chatRoom.Id}");
            }

            return chatRoom;
        }
    }
}

