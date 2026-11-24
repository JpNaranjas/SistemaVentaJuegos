use tienda_juegos;

-- Insertar juegos (15 registros)
insert into juegos (nombre, precio_unitario, categoria, marca, consola, stock, fecha_salida) values
('The Legend of Zelda Ocarina of Time', 1600.00, 'Videojuego', 'Nintendo', 'Nintendo Switch 2', 4, '2026-11-05'),
('Dragon Quest XI: Edición Definitiva', 860.00, 'Videojuego', 'Square Enix', 'PlayStation 4', 2, '2017-07-29'),
('Virus', 60.00, 'Juego de mesa', 'Tranjis Games', null, 5, null),
('Donkey Kong Bananza', 1500.00, 'Videojuego', 'Nintendo', 'Nintendo Switch 2', 8, '2025-07-17'),
('GTA VI', 1800.00, 'Videojuego', 'Rockstar', 'PlayStation 5', 12, '2026-11-19'),
('Catan', 750.00, 'Juego de mesa', 'Devir', null, 10, null),
('Monopoly', 500.00, 'Juego de mesa', 'Hasbro', null, 20, null),
('Super Mario Bros Wonder', 1200.00, 'Videojuego', 'Nintendo', 'Nintendo Switch', 15, '2023-10-20'),
('Elden Ring', 1400.00, 'Videojuego', 'Bandai Namco', 'PlayStation 5', 6, '2022-02-25'),
('Ajedrez Profesional', 300.00, 'Juego de mesa', 'Genérico', null, 25, null),
('Halo Infinite', 800.00, 'Videojuego', 'Xbox Game Studios', 'Xbox Series X', 3, '2021-12-08'),
('Uno', 150.00, 'Juego de mesa', 'Mattel', null, 30, null),
('Minecraft', 600.00, 'Videojuego', 'Mojang', 'Xbox Series X', 50, '2011-11-18'),
('DOS', 200.00, 'Juego de mesa', 'Mattel', null, 10, null),
('Jenga', 250.00, 'Juego de mesa', 'Hasbro', null, 18, null);

-- Insertar clientes (15 registros)
insert into clientes (nombre, correo, telefono, fecha_alta) values
('Juan Pérez', 'juan.perez@email.com', '555-0101', '2026-01-10'),
('María López', 'maria.lopez@email.com', '555-0102', '2026-01-15'),
('Carlos Ruiz', 'carlos.ruiz@gmail.com', '555-0103', '2026-02-01'),
('Ana Torres', 'ana.torres@yahoo.com', '555-0104', '2026-02-10'),
('Luis Gómez', 'luis.gomez@email.com', '555-0105', '2026-03-05'),
('Elena Díaz', 'elena.diaz@email.com', '555-0106', '2026-03-12'),
('Pedro Sánchez', 'pedro.sanchez@email.com', '555-0107', '2026-04-01'),
('Lucía Ramírez', 'lucia.ramirez@email.com', '555-0108', '2026-04-18'),
('Jorge Fernández', 'jorge.fernandez@email.com', '555-0109', '2026-05-02'),
('Sofía Castro', 'sofia.castro@email.com', '555-0110', '2026-05-20'),
('Diego Morales', 'diego.morales@email.com', '555-0111', '2026-06-01'),
('Valeria Ortiz', 'valeria.ortiz@email.com', '555-0112', '2026-06-15'),
('Andrés Silva', 'andres.silva@email.com', '555-0113', '2026-07-01'),
('Camila Vega', 'camila.vega@email.com', '555-0114', '2026-07-10'),
('Raúl Mendoza', 'raul.mendoza@email.com', '555-0115', '2026-08-05');

-- Insertar reservas (15 registros)
insert into reservas (cliente_id, fecha_reserva, estado, total) values
(1, '2026-10-01 10:00:00', 'Pendiente', 1600.00),
(2, '2026-10-02 11:30:00', 'Completada', 860.00),
(3, '2026-10-03 12:15:00', 'Cancelada', 0.00),
(4, '2026-10-04 09:45:00', 'Pendiente', 1800.00),
(5, '2026-10-05 14:20:00', 'Completada', 750.00),
(6, '2026-10-06 16:00:00', 'Pendiente', 1500.00),
(7, '2026-10-07 10:30:00', 'Completada', 500.00),
(8, '2026-10-08 13:10:00', 'Pendiente', 1200.00),
(9, '2026-10-09 15:45:00', 'Completada', 1400.00),
(10, '2026-10-10 11:00:00', 'Pendiente', 300.00),
(11, '2026-10-11 10:20:00', 'Completada', 800.00),
(12, '2026-10-12 12:00:00', 'Pendiente', 150.00),
(13, '2026-10-13 14:30:00', 'Completada', 600.00),
(14, '2026-10-14 09:00:00', 'Pendiente', 100.00),
(15, '2026-10-15 16:15:00', 'Completada', 250.00);

-- Insertar detalle_reserva (15 registros)
insert into detalle_reserva (reserva_id, producto_id, cantidad, precio_unitario) values
(1, 1, 1, 1600.00),
(2, 2, 1, 860.00),
(3, 6, 1, 750.00),
(4, 5, 1, 1800.00),
(5, 6, 1, 750.00),
(6, 4, 1, 1500.00),
(7, 7, 1, 500.00),
(8, 8, 1, 1200.00),
(9, 9, 1, 1400.00),
(10, 10, 1, 300.00),
(11, 11, 1, 800.00),
(12, 12, 1, 150.00),
(13, 13, 1, 600.00),
(14, 14, 1, 100.00),
(15, 15, 1, 250.00);

-- Insertar pagos (15 registros)
insert into pagos (cliente_id, producto_id, cantidad, fecha_pago, total, metodo_pago, estado) values
(1, 1, 1, '2026-10-01 10:15:00', 1600.00, 'Tarjeta', 'Pagado'),
(2, 2, 1, '2026-10-02 11:45:00', 860.00, 'Efectivo', 'Pagado'),
(3, 6, 1, '2026-10-03 12:20:00', 750.00, 'Transferencia', 'Cancelado'),
(4, 5, 1, '2026-10-04 10:00:00', 1800.00, 'Tarjeta', 'Pagado'),
(5, 6, 1, '2026-10-05 14:30:00', 750.00, 'Efectivo', 'Pagado'),
(6, 11, 1, '2026-10-06 16:10:00', 800.00, 'Transferencia', 'Pagado'),
(7, 6, 2, '2026-10-07 10:40:00', 1500.00, 'Tarjeta', 'Pagado'),
(8, 13, 1, '2026-10-08 13:20:00', 600.00, 'Efectivo', 'Pagado'),
(9, 10, 1, '2026-10-09 15:55:00', 300.00, 'Tarjeta', 'Pagado'),
(10, 14, 1, '2026-10-10 11:10:00', 100.00, 'Transferencia', 'Pagado'),
(11, 15, 3, '2026-10-11 10:30:00', 750.00, 'Efectivo', 'Pagado'),
(12, 9, 1, '2026-10-12 12:10:00', 1400.00, 'Tarjeta', 'Pagado'),
(13, 5, 1, '2026-10-13 14:40:00', 1800.00, 'Transferencia', 'Pendiente'),
(14, 7, 1, '2026-10-14 09:10:00', 500.00, 'Efectivo', 'Pagado'),
(15, 8, 2, '2026-10-15 16:25:00', 2400.00, 'Tarjeta', 'Pagado');