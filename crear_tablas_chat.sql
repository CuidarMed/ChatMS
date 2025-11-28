-- Script para crear las tablas de ChatMS en la base de datos
-- Ejecutar este script en la base de datos remota si las migraciones no se aplican automáticamente
-- NOTA: La tabla Users ya existe (compartida con otros microservicios), no se crea aquí

-- Crear tabla ChatRooms (si no existe)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChatRooms]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChatRooms] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [PatientId] int NOT NULL,
        [DoctorID] int NOT NULL,
        [AppointmentId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_ChatRooms] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChatRooms_Users_DoctorID] FOREIGN KEY ([DoctorID]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChatRooms_Users_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE NO ACTION
    );
    
    -- Crear índices
    CREATE INDEX [IX_ChatRooms_DoctorID] ON [dbo].[ChatRooms] ([DoctorID]);
    CREATE INDEX [IX_ChatRooms_PatientId] ON [dbo].[ChatRooms] ([PatientId]);
    CREATE INDEX [IX_ChatRooms_AppointmentId] ON [dbo].[ChatRooms] ([AppointmentId]);
    
    -- Índice único filtrado
    CREATE UNIQUE NONCLUSTERED INDEX [IX_ChatRooms_DoctorID_PatientId_AppointmentId] 
    ON [dbo].[ChatRooms] ([DoctorID], [PatientId], [AppointmentId])
    WHERE [AppointmentId] IS NOT NULL;
END
GO

-- Crear tabla ChatMessages (si no existe)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChatMessages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChatMessages] (
        [id] int IDENTITY(1,1) NOT NULL,
        [ChatRoomId] int NOT NULL,
        [SenderId] int NOT NULL,
        [Message] nvarchar(2000) NOT NULL,
        [SendAt] datetime2 NOT NULL,
        [IsRead] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([id]),
        CONSTRAINT [FK_ChatMessages_ChatRooms_ChatRoomId] FOREIGN KEY ([ChatRoomId]) REFERENCES [dbo].[ChatRooms] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChatMessages_Users_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE NO ACTION
    );
    
    -- Crear índices
    CREATE INDEX [IX_ChatMessages_ChatRoomId] ON [dbo].[ChatMessages] ([ChatRoomId]);
    CREATE INDEX [IX_ChatMessages_SendAt] ON [dbo].[ChatMessages] ([SendAt]);
END
GO

-- Crear tabla de migraciones si no existe
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END
GO

-- Insertar la migración en el historial
IF NOT EXISTS (SELECT * FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = '20251123194549_UpdateChatRoomAppointmentIdRequiredAndFilteredIndex')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251123194549_UpdateChatRoomAppointmentIdRequiredAndFilteredIndex', '8.0.0');
END
GO

PRINT 'Tablas de ChatMS creadas exitosamente!';

