use tienda_juegos;

-- 3.4 Crear Roles
create role 'rol_administrador';
create role 'rol_operador';

-- 3.3 Otorgar Privilegios a los Roles
grant all privileges on tienda_juegos.* to 'rol_administrador';

grant select on tienda_juegos.juegos to 'rol_operador';
grant select on tienda_juegos.clientes to 'rol_operador';
grant insert on tienda_juegos.pagos to 'rol_operador';
grant insert on tienda_juegos.reservas to 'rol_operador';
grant insert on tienda_juegos.detalle_reserva to 'rol_operador';

-- 3.2 Crear Usuarios y Asignarles Roles
create user 'admin_tienda'@'localhost' identified by 'Admin123!';
create user 'operador_tienda'@'localhost' identified by 'Oper123!';
create user 'VaneRoll'@'localhost' identified by 'All2well';
create user 'JpNaranjas'@'localhost' identified by 'ViceCity';
create user 'DagoProfe'@'localhost' identified by 'DagoFest2026';

grant 'rol_administrador' to 'admin_tienda'@'localhost';
grant 'rol_administrador' to 'VaneRoll'@'localhost';
grant 'rol_administrador' to 'DagoProfe'@'localhost';
grant 'rol_operador' to 'operador_tienda'@'localhost';
grant 'rol_operador' to 'JpNaranjas'@'localhost';

-- Asignar roles por defecto
set default role 'rol_administrador' to 'admin_tienda'@'localhost';
set default role 'rol_administrador' to 'VaneRoll'@'localhost';
set default role 'rol_administrador' to 'DagoProfe'@'localhost';
set default role 'rol_operador' to 'operador_tienda'@'localhost';
set default role 'rol_operador' to 'JpNaranjas'@'localhost';