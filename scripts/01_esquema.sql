drop database if exists tienda_juegos;
create database tienda_juegos;
use tienda_juegos;

-- 1. Tabla: juegos
create table juegos (
    producto_id int auto_increment primary key,
    nombre varchar(100) not null unique,
    precio_unitario decimal(10,2) not null,
    categoria enum('Videojuego', 'Juego de mesa', 'Otros') not null,
    marca varchar(50) not null,
    consola varchar(50) null,
    stock int not null default 0,
    constraint chk_juegos_stock check (stock >= 0),
    constraint chk_juegos_precio check (precio_unitario > 0)
);

alter table juegos add column fecha_salida date;

-- 2. Tabla: clientes
create table clientes (
    cliente_id int auto_increment primary key,
    nombre varchar(100) not null,
    correo varchar(100) not null unique,
    telefono varchar(15) not null,
    fecha_alta date not null default (current_date)
);

-- 3. Tabla: reservas (Cabecera de reserva)
create table reservas (
    reserva_id int auto_increment primary key,
    cliente_id int not null,
    fecha_reserva datetime not null default current_timestamp,
    estado enum('Pendiente', 'Completada', 'Cancelada') not null default 'Pendiente',
    total decimal(10,2) not null default 0.00,
    constraint fk_reservas_cliente foreign key (cliente_id) 
        references clientes(cliente_id) 
        on update cascade on delete restrict,
    constraint chk_reservas_total check (total >= 0)
);

-- 4. Tabla: detalle_reserva (Relación N:M entre reservas y juegos)
create table detalle_reserva (
    reserva_id int not null,
    producto_id int not null,
    cantidad int not null default 1,
    precio_unitario decimal(10,2) not null,
    primary key (reserva_id, producto_id),
    constraint fk_detalle_reserva foreign key (reserva_id) 
        references reservas(reserva_id) 
        on update cascade on delete cascade,
    constraint fk_detalle_producto foreign key (producto_id) 
        references juegos(producto_id) 
        on update cascade on delete restrict,
    constraint chk_detalle_cantidad check (cantidad > 0)
);

-- 5. Tabla: pagos (Ventas generales independientes)
create table pagos (
    pago_id int auto_increment primary key,
    cliente_id int not null,
    producto_id int not null,
    cantidad int not null default 1,
    fecha_pago datetime not null default current_timestamp,
    total decimal(10,2) not null default 0.00,
    metodo_pago enum('Efectivo', 'Tarjeta', 'Transferencia') not null,
    estado enum('Pagado', 'Pendiente', 'Cancelado') not null default 'Pagado',
    constraint fk_pagos_cliente foreign key (cliente_id) 
        references clientes(cliente_id) 
        on update cascade on delete restrict,
    constraint fk_pagos_producto foreign key (producto_id) 
        references juegos(producto_id) 
        on update cascade on delete restrict,
    constraint chk_pagos_total check (total >= 0),
    constraint chk_pagos_cantidad check (cantidad > 0)
);

-- 02_datos.sql (Mínimo 15 registros por tabla)
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

-- Insertar pagos / ventas directas (15 registros)
-- Ahora sabemos exactamente qué producto se vendió y cuántos
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

-- Consulta de los productos de Nintendo
select * from juegos where marca like '%Nintendo%';
-- Consulta de los productos de Hasbro
select * from juegos where marca like '%Hasbro%';
-- Consulta de los 3 productos más caros
select nombre, categoria, marca, precio_unitario, stock 
from juegos order by precio_unitario desc limit 3;

-- ==========================================
-- TRIGGER DE REGLA DE NEGOCIO
-- Valida que la fecha de reserva sea máximo 1 mes antes de la salida del juego
-- ==========================================
delimiter //
create trigger tr_validar_fecha_reserva
before insert on detalle_reserva
for each row
begin
    declare v_fecha_salida date;
    
    select fecha_salida into v_fecha_salida 
    from juegos 
    where producto_id = new.producto_id;
    
    if v_fecha_salida is not null then
        if current_date() < date_sub(v_fecha_salida, interval 1 month) then
            signal sqlstate '45000' 
            set message_text = 'Error: Solo se puede reservar con un máximo de 1 mes de anticipación a la fecha de salida.';
        end if;
    end if;
end;
//
delimiter ;

-- ==========================================
-- 5. SQL PROCEDURAL (Competencias 5)
-- ==========================================

-- 5.1 STORED PROCEDURES (Se necesitan 4 para 2 integrantes)

-- Procedimiento 1: Registrar una venta directa (con parámetro de salida)
delimiter //
create procedure sp_registrar_venta_directa(
    in p_cliente_id int,
    in p_producto_id int,
    in p_cantidad int,
    in p_metodo_pago varchar(20),
    out p_mensaje varchar(100)
)
begin
    declare v_precio decimal(10,2);
    declare v_stock int;
    declare v_total decimal(10,2);
    
    -- Manejador de errores
    declare exit handler for sqlexception
    begin
        rollback;
        set p_mensaje = 'Error: Ocurrió un problema al registrar la venta.';
    end;

    start transaction;
    
    -- Obtener precio y stock actual del producto
    select precio_unitario, stock into v_precio, v_stock 
    from juegos where producto_id = p_producto_id for update;
    
    if v_stock >= p_cantidad then
        set v_total = v_precio * p_cantidad;
        
        -- Insertar el pago
        insert into pagos (cliente_id, producto_id, cantidad, total, metodo_pago, estado)
        values (p_cliente_id, p_producto_id, p_cantidad, v_total, p_metodo_pago, 'Pagado');
        
        -- Actualizar el stock
        update juegos set stock = stock - p_cantidad where producto_id = p_producto_id;
        
        commit;
        set p_mensaje = concat('Venta exitosa. Total: $', v_total);
    else
        rollback;
        set p_mensaje = 'Error: No hay stock suficiente.';
    end if;
end;
//
delimiter ;

-- Procedimiento 2: Registrar una reserva completa (Cabecera y Detalle)
delimiter //
create procedure sp_registrar_reserva(
    in p_cliente_id int,
    in p_producto_id int,
    in p_cantidad int
)
begin
    declare v_precio decimal(10,2);
    declare v_reserva_id int;
    
    declare exit handler for sqlexception
    begin
        rollback;
    end;

    start transaction;
    
    select precio_unitario into v_precio from juegos where producto_id = p_producto_id;
    
    -- Crear la cabecera de la reserva
    insert into reservas (cliente_id, estado, total) 
    values (p_cliente_id, 'Pendiente', (v_precio * p_cantidad));
    
    set v_reserva_id = last_insert_id();
    
    -- Crear el detalle (Aquí se disparará tu trigger de validación de fecha)
    insert into detalle_reserva (reserva_id, producto_id, cantidad, precio_unitario)
    values (v_reserva_id, p_producto_id, p_cantidad, v_precio);
    
    commit;
end;
//
delimiter ;

-- Procedimiento 3: Actualizar el stock de un juego
delimiter //
create procedure sp_actualizar_stock(
    in p_producto_id int,
    in p_nuevo_stock int
)
begin
    update juegos set stock = p_nuevo_stock where producto_id = p_producto_id;
end;
//
delimiter ;

-- Procedimiento 4: Cambiar estado de una reserva
delimiter //
create procedure sp_cambiar_estado_reserva(
    in p_reserva_id int,
    in p_nuevo_estado varchar(20)
)
begin
    update reservas set estado = p_nuevo_estado where reserva_id = p_reserva_id;
end;
//
delimiter ;


-- 5.2 FUNCIONES (Se necesitan 2 para 2 integrantes)

-- Función 1: Calcular el total de ingresos por cliente
delimiter //
create function fn_ingresos_cliente(p_cliente_id int) 
returns decimal(10,2)
deterministic
begin
    declare v_total decimal(10,2);
    select ifnull(sum(total), 0.00) into v_total 
    from pagos 
    where cliente_id = p_cliente_id and estado = 'Pagado';
    return v_total;
end;
//
delimiter ;

-- Función 2: Verificar si un juego tiene stock disponible
delimiter //
create function fn_hay_stock(p_producto_id int, p_cantidad int) 
returns varchar(20)
deterministic
begin
    declare v_stock int;
    select stock into v_stock from juegos where producto_id = p_producto_id;
    
    if v_stock >= p_cantidad then
        return 'Disponible';
    else
        return 'Agotado';
    end if;
end;
//
delimiter ;


-- 5.3 TRIGGERS (Se necesitan 2, ya tienes 1, falta este)

-- Trigger 2: Bitácora de cambios de stock
create table bitacora_stock (
    id_bitacora int auto_increment primary key,
    producto_id int not null,
    stock_anterior int not null,
    stock_nuevo int not null,
    fecha_cambio datetime default current_timestamp,
    usuario varchar(50)
);

delimiter //
create trigger tr_bitacora_stock
after update on juegos
for each row
begin
    if old.stock <> new.stock then
        insert into bitacora_stock (producto_id, stock_anterior, stock_nuevo, usuario)
        values (old.producto_id, old.stock, new.stock, current_user());
    end if;
end;
//
delimiter ;


-- ==========================================
-- 3. SEGURIDAD (Competencia 3) - Script 03_seguridad.sql
-- ==========================================

-- 3.4 Crear Roles
create role 'rol_administrador';
create role 'rol_operador';

-- 3.3 Otorgar Privilegios a los Roles
-- Administrador: Todo el control
grant all privileges on tienda_juegos.* to 'rol_administrador';

-- Operador: Solo puede ver y vender, no borrar ni modificar estructura
grant select on tienda_juegos.juegos to 'rol_operador';
grant select on tienda_juegos.clientes to 'rol_operador';
grant insert on tienda_juegos.pagos to 'rol_operador';
grant insert on tienda_juegos.reservas to 'rol_operador';
grant insert on tienda_juegos.detalle_reserva to 'rol_operador';
grant execute on procedure tienda_juegos.sp_registrar_venta_directa to 'rol_operador';

-- 3.2 Crear Usuarios y Asignarles Roles
create user 'admin_tienda'@'localhost' identified by 'Admin123!';
create user 'operador_tienda'@'localhost' identified by 'Oper123!';

grant 'rol_administrador' to 'admin_tienda'@'localhost';
grant 'rol_operador' to 'operador_tienda'@'localhost';


-- ==========================================
-- 4. CONCURRENCIA (Competencia 4) - Script 06_concurrencia.sql
-- ==========================================

-- 4.5 Demostración de Transacción con ROLLBACK
-- (Puedes poner esto como un script de prueba o documentarlo en tu reporte)

start transaction;
-- Intento de venta de un juego
update juegos set stock = stock - 1 where producto_id = 1;
insert into pagos (cliente_id, producto_id, cantidad, total, metodo_pago, estado)
values (1, 1, 1, 1600.00, 'Efectivo', 'Pagado');

-- Simulamos un error forzado
rollback;
-- El stock vuelve a la normalidad y el pago no se registra.

use tienda_juegos;

-- 4.4 Demostración de Niveles de Aislamiento
-- NOTA: Para probar esto, debes abrir dos pestañas o ventanas de tu gestor de BD (como MySQL Workbench) y ejecutar el código paso a paso.

-- ==========================================
-- PRUEBA DE LECTURA NO REPETIBLE (READ COMMITTED)
-- ==========================================

-- SESIÓN 1:
set session transaction isolation level read committed;
start transaction;
select stock from juegos where producto_id = 1; -- Supongamos que lee 4

/*
-- SESIÓN 2:
set session transaction isolation level read committed;
start transaction;
update juegos set stock = 10 where producto_id = 1;
commit;*/

-- SESIÓN 1 (Vuelve a consultar en la misma transacción):
select stock from juegos where producto_id = 1; -- Leerá 10. (Ocurrió una lectura no repetible)
commit;



-- ==========================================
-- PRUEBA DE BLOQUEO (SERIALIZABLE)
-- ==========================================

-- SESIÓN 1:
set session transaction isolation level serializable;
start transaction;
select * from juegos where producto_id = 1;
/*
-- SESIÓN 2:
set session transaction isolation level serializable;
start transaction;
update juegos set stock = 5 where producto_id = 1; -- Esta consulta se bloqueará y esperará a que la Sesión 1 haga commit o rollback.
*/


-- ==========================================
-- 4.5 Commit y Rollback
-- ==========================================

-- Demostración de transacción exitosa (Commit)
start transaction;
update juegos set stock = stock - 1 where producto_id = 2;
insert into pagos (cliente_id, producto_id, cantidad, total, metodo_pago, estado)
values (2, 2, 1, 860.00, 'Efectivo', 'Pagado');
commit;

-- Demostración de Rollback (Simulando un error en C#)
start transaction;
update juegos set stock = stock - 1 where producto_id = 3;
-- Supongamos que aquí el sistema falla o el usuario cancela
rollback;
-- El stock del producto 3 se mantiene intacto y no se insertó nada.

-- ==========================================================
-- 07_app_vistas_y_permisos.sql
-- Ejecutar DESPUÉS de tu script completo (con un usuario root/DBA)
-- ==========================================================
use tienda_juegos;

-- ---------- 2.6 VISTAS usadas por la app ----------
create or replace view v_reservas as
select r.reserva_id, c.nombre as cliente, j.nombre as juego, d.cantidad,
       d.precio_unitario, r.total, r.estado, r.fecha_reserva
from reservas r
inner join clientes c on c.cliente_id = r.cliente_id
inner join detalle_reserva d on d.reserva_id = r.reserva_id
inner join juegos j on j.producto_id = d.producto_id;

create or replace view v_ventas as
select p.pago_id, c.nombre as cliente, j.nombre as juego, p.cantidad,
       p.total, p.metodo_pago, p.estado, p.fecha_pago
from pagos p
inner join clientes c on c.cliente_id = p.cliente_id
inner join juegos j on j.producto_id = p.producto_id;

create or replace view v_inventario_marca as
select marca, count(*) as juegos, sum(stock) as unidades,
       sum(stock * precio_unitario) as valor_inventario
from juegos
group by marca;

-- ---------- Corrección: sp_registrar_reserva debe RE-LANZAR el error ----------
-- (si no, el mensaje del trigger de fecha nunca llega a la aplicación)
drop procedure if exists sp_registrar_reserva;
delimiter //
create procedure sp_registrar_reserva(
    in p_cliente_id int,
    in p_producto_id int,
    in p_cantidad int
)
begin
    declare v_precio decimal(10,2);
    declare v_reserva_id int;

    declare exit handler for sqlexception
    begin
        rollback;
        resignal;
    end;

    start transaction;
    select precio_unitario into v_precio from juegos where producto_id = p_producto_id;

    insert into reservas (cliente_id, estado, total)
    values (p_cliente_id, 'Pendiente', (v_precio * p_cantidad));
    set v_reserva_id = last_insert_id();

    insert into detalle_reserva (reserva_id, producto_id, cantidad, precio_unitario)
    values (v_reserva_id, p_producto_id, p_cantidad, v_precio);

    commit;
end;
//
delimiter ;

-- ---------- 3.3 Privilegios extra para el perfil OPERADOR ----------
grant execute on procedure tienda_juegos.sp_registrar_reserva to 'rol_operador';
grant execute on function  tienda_juegos.fn_hay_stock         to 'rol_operador';

-- Roles activos por defecto al iniciar sesión
set default role 'rol_administrador' to 'admin_tienda'@'localhost';
set default role 'rol_operador'      to 'operador_tienda'@'localhost';

-- Ejemplo de REVOKE (3.3): el operador no debe poder insertar pagos directo, solo por SP
revoke insert on tienda_juegos.pagos from 'rol_operador';

create user 'VaneRoll'@'localhost' identified by 'All2well';
create user 'JpNaranjas'@'localhost' identified by 'ViceCity';
create user 'DagoProfe'@'localhost' identified by 'DagoFest2026';
grant 'rol_administrador' to 'VaneRoll'@'localhost';
grant 'rol_operador' to 'JpNaranjas'@'localhost';
grant 'rol_administrador' to 'DagoProfe'@'localhost';
set default role 'rol_administrador' to 'VaneRoll'@'localhost';
set default role 'rol_administrador' to 'DagoProfe'@'localhost';
set default role 'rol_operador'      to 'JpNaranjas'@'localhost';

FLUSH PRIVILEGES;
